using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Sockets;
using System.Net;
using System.Text;

namespace AIT_Network_Core
{
    public class UDP_Receiver
    {
        private IPEndPoint _sendingEndPoint;
        private UdpClient _udpClient;
        private int _timeOut;

        public UDP_Receiver(IPEndPoint listenEndPoint, IPEndPoint sendingEndPoint, int timeOut = 0)
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

        public byte[] Listen()
    {
            if (_udpClient == null) { throw new Exception("UDP Client is null"); }
            IPEndPoint remoteEndPoint = new IPEndPoint(IPAddress.Any, 0);
            Byte[] data = _udpClient.Receive(ref remoteEndPoint);
        return data;
    }

    void OnDestroy() { CleanUp(); }
    void OnDisable() { CleanUp(); }
    // be certain to catch ALL possibilities of exit in your environment,
    // or else the thread will typically live on beyond the app quitting.

    void CleanUp()
    {
        Console.WriteLine("Cleanup for receiver...");

        // note, consider carefully that it may not be running
        if (_udpClient != null)
        {
            _udpClient.Close();
        }
        Console.WriteLine(", receiver client correctly stopped");
    }
}
}