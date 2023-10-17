// See https://aka.ms/new-console-template for more information
using Microsoft.Extensions.Logging;
using System.Net;
using System.Text;
using System.Xml;
using UDP_Relay_Core;

try
{
    //*********************************************************************************************************************
    // Test sender
    // This code is used to test the UDP relay and the RelayConsol.
    // It sends UDP packets to the relay server and gets a response in return.
    //*********************************************************************************************************************

    // Add logging beyond console
    var loggerFactory = LoggerFactory.Create(
        builder => builder
                    .AddFile("UDP_TestSender.log")
                    .AddDebug()
                    .SetMinimumLevel(LogLevel.Debug)
    );
    var log = loggerFactory.CreateLogger<Program>();
    Logger.AddLogger(log);

    // Get settings from XML file
    Logger.Log("Test Sender started");
    XML_Wrapper xML_Wrapper = new XML_Wrapper(@"Settings_TestSender.xml");

    IPEndPoint localEndPoint = xML_Wrapper.GetIPEndPoint(@"/TestSender/LocalIP", @"/TestSender/LocalPort");
    Logger.LogDebug("Local IPEndPoint: " + localEndPoint.ToString());

    IPEndPoint serverEndPoint = xML_Wrapper.GetIPEndPoint(@"/TestSender/ServerIP", @"/TestSender/ServerPort");
    Logger.LogDebug("Server IPEndPoint: " + serverEndPoint);

    int timeOut = xML_Wrapper.GetInt(@"/TestSender/TimeOut");
    Logger.LogDebug("Timeout: " + timeOut);

    Console.WriteLine("Press a key to start sending");
    Console.ReadKey();

    // Start sending and receiving UDP packets
    UDP_Relay relay = new UDP_Relay();
    for (int i = 0; i < 100; i++)
    {
        string sendMessage = "A" + i;
        byte[] sendData = Encoding.UTF8.GetBytes(sendMessage);
        relay.Send(serverEndPoint, sendData);
        Logger.Log("Data sent: " + sendMessage);

        //sendMessage = "B" + i;
        //sendData = Encoding.UTF8.GetBytes(sendMessage);
        //relay.Send(serverEndPoint, sendData);
        //Logger.Log("Data sent: " + sendMessage);

        //sendMessage = "C" + i;
        //sendData = Encoding.UTF8.GetBytes(sendMessage);
        //relay.Send(serverEndPoint, sendData);
        //Logger.Log("Data sent: " + sendMessage);

        byte[] receivedData = relay.Receive(localEndPoint, timeOut, null);
        string receivedMessage = Encoding.UTF8.GetString(receivedData);
        Logger.Log("Received: " + receivedMessage);

        Thread.Sleep(100); // Throttle the sending to see effect on number of packets received/lost
    };
    Logger.Log("Sending loop finished - press a key to exit");
    Console.ReadKey();
}
catch (Exception ex)
{
    Logger.Log(ex);
    Console.ReadKey();
}