using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Diagnostics;
using System.Threading;

namespace AIT_Network_Core
{
    public class UDP_Sender
    {
        private IPEndPoint _sendingEndPoint;
        private UdpClient _udpClient;
        private int _timeOut;

        public UDP_Sender(IPEndPoint listenEndPoint, IPEndPoint sendingEndPoint, int timeOut = 0)
        {
            _sendingEndPoint = sendingEndPoint;
            _timeOut = timeOut;

            _udpClient = new UdpClient(listenEndPoint)
            {
                EnableBroadcast = true,
            };
            if (_timeOut > 0)
            {
                _udpClient.Client.SendTimeout = _timeOut;
                _udpClient.Client.ReceiveTimeout = _timeOut;
            }
        }


            public void Send(Byte[] data)
        {            
            if (_udpClient == null) { throw new Exception("UDP Cleint is null!"); }
            int sentBytes = _udpClient.Send(data, data.Length, _sendingEndPoint);
            if (sentBytes != data.Length)
            {
                throw new Exception("Number of sent bytes does not match data length!");
            }
        }

        void OnDestroy() { CleanUp(); }
        void OnDisable() { CleanUp(); }
        // be certain to catch ALL possibilities of exit in your environment,
        // or else the thread will typically live on beyond the app quitting.

        void CleanUp()
        {
            Console.WriteLine("Cleanup for sender...");

            // note, consider carefully that it may not be running
            if (_udpClient != null)
            {
                _udpClient.Close();
            }
            Console.WriteLine(", sender client correctly stopped");
        }
    }
}