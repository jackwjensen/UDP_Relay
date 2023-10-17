using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading;
using System.IO.IsolatedStorage;
using System.Threading.Tasks;
using System.Diagnostics;

namespace UDP_Relay_Core
{
    public class UDP_Relay: IDisposable
    {
        private bool _isRunning = false;
        public bool IsRunning { get => _isRunning; }
        private bool _isStopping = false;
        public bool IsStopping { get => _isStopping; }

        private List<Task> _relayTasks;
        private CancellationTokenSource _cancellationTokenSource;
        private bool disposedValue;

        /// <summary>
        /// Constructor
        /// </summary>
        public UDP_Relay()
        {
            _relayTasks = new List<Task>();
        }

        /// <summary>
        /// Sends data to a remote end point.
        /// </summary>
        /// <param name="remoteEndPoint">Target to receive data.</param>
        /// <param name="data">The data to send.</param>
        /// <exception cref="Exception">Error happened.</exception>
        public void Send(IPEndPoint remoteEndPoint, Byte[] data)
        {
            try
            {
                using (UdpClient udpClient = new UdpClient(remoteEndPoint.AddressFamily)
                {
                    EnableBroadcast = true,
                })
                {
                    int sentBytes = udpClient.Send(data, data.Length, remoteEndPoint);
                    if (sentBytes != data.Length)
                    {
                        throw new Exception("Number of sent bytes does not match data length!");
                    }
                }
            }
            catch (SocketException ex)
            {
                if (ex.ErrorCode != 10060)
                {
                    Logger.Log("Socket exception: " + remoteEndPoint, ex);
                    throw;
                }
                else
                {
                    Logger.LogTrace("Timeout when trying to send: " + remoteEndPoint);
                }
            }
        }

        /// <summary>
        /// Receives data from a local end point.
        /// </summary>
        /// <param name="localEndPoint">The local interface and port to listen on.</param>
        /// <param name="timeOut">0 = no timeout, positive value for timeout in milliseconds.</param>
        /// <param name="receiveEndPoint">Optional remote end point to receive data from - will limit to only receive from that end point.</param>
        /// <returns>Data received or empty byte[] if timeout.</returns>
        public byte[] Receive(IPEndPoint localEndPoint, int timeOut = 0, IPEndPoint receiveEndPoint = null)
        {
            Byte[] data = new byte[0];
            try
            {
                using (UdpClient udpClient = new UdpClient(localEndPoint))
                {
                    if (timeOut > 0)
                    {
                        udpClient.Client.SendTimeout = timeOut;
                        udpClient.Client.ReceiveTimeout = timeOut;
                    }
                    if (receiveEndPoint == null)
                    { // If no receive endpoint given set to any address depending on address family
                        if (localEndPoint.AddressFamily == AddressFamily.InterNetworkV6)
                        { // Set to IPv6 any address
                            receiveEndPoint = new IPEndPoint(IPAddress.IPv6Any, 0);
                        }
                        else
                        { // Set to IPv4 any address
                            receiveEndPoint = new IPEndPoint(IPAddress.Any, 0);
                        }
                    }
                    data = udpClient.Receive(ref receiveEndPoint);
                }
            }
            catch (SocketException ex)
            {
                if (ex.ErrorCode != 10060)
                {
                    Logger.Log("Socket exception: " + localEndPoint, ex);
                    throw;
                }
                else
                {
                    Logger.LogTrace("Timeout when trying to send: " + localEndPoint);
                }
            }
            return data;
        }

        /// <summary>
        /// Starts relaying data between a local end point and a remote end point.
        /// </summary>
        /// <param name="localEndPoint">End point to listen on.</param>
        /// <param name="relayEndPoint">End point to send received data to.</param>
        public void StartRelaying(IPEndPoint localEndPoint, IPEndPoint relayEndPoint)
        {
            _isRunning = true;
            if (_relayTasks == null)
            {
                _relayTasks = new List<Task>();
            }
            if (_cancellationTokenSource == null)
            {
                _cancellationTokenSource = new CancellationTokenSource();
            }
            Task t = Task.Run(() => RelayAsync(localEndPoint, relayEndPoint, _cancellationTokenSource.Token), _cancellationTokenSource.Token);
            _relayTasks.Add(t);
        }

        /// <summary>
        /// The actual task that relays data between a local end point and a remote end point.
        /// </summary>
        /// <param name="localEndPoint">End point to listen on.</param>
        /// <param name="relayEndPoint">End point to send received data to.</param>
        /// <param name="cancellationToken">Token for canceling the task.</param>
        /// <returns>Task for relaying data.</returns>
        private async Task RelayAsync(IPEndPoint localEndPoint, IPEndPoint relayEndPoint, CancellationToken cancellationToken)
        {
            try
            {
                string tName = localEndPoint + " - " + relayEndPoint;
                Logger.LogDebug("Relay task started: " + tName);

                using (UdpClient udpClient = new UdpClient(localEndPoint))
                {
                    Logger.LogDebug("Relay task listening: " + tName);
                    while (!IsStopping && !cancellationToken.IsCancellationRequested)
                    {
                        try
                        {
                            // Receive data
                            UdpReceiveResult result = await udpClient.ReceiveAsync()
                                .WithCancellation(cancellationToken)
                                .ConfigureAwait(false)
                                ;
                            byte[] data = result.Buffer;

                            string message = Encoding.UTF8.GetString(data);
                            Logger.LogDebug("Received " + tName + ": " + message);

                            // Send data
                            int sentBytes = await udpClient.SendAsync(data, data.Length, relayEndPoint)
                                .WithCancellation(cancellationToken)
                                .ConfigureAwait(false)
                                ;
                            if (sentBytes != data.Length)
                            {
                                throw new Exception("Received and sent data does not have same length! " + tName);
                            }
                            Logger.LogDebug("Sent " + tName + ": " + message);
                        }
                        catch (OperationCanceledException)
                        {
                            Logger.LogTrace("Task was canceled: " + tName);
                            udpClient.Close();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }
        }

        /// <summary>
        /// Stops relaying data.
        /// </summary>
        public void StopRelaying()
        {
            _isStopping = true;
            if (_cancellationTokenSource != null)
            {
                _cancellationTokenSource.Cancel();
            }

            if (_relayTasks != null)
            {
                foreach (Task t in _relayTasks)
                {
                    t.Wait();
                }
                _relayTasks.Clear();
            }

            _isRunning = false;
            _isStopping = false;
        }
        
        /// <inheritdoc />
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects)
                    Logger.LogTrace("Disposing relay...");
                    StopRelaying();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                _relayTasks = null;
                _isRunning = false;
                _isStopping = false;
                Logger.LogTrace("Relay disposed");
                disposedValue = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~UDP_Relay()
        // {
        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //     Dispose(disposing: false);
        // }

        /// <inheritdoc />
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}