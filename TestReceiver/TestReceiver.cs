// See https://aka.ms/new-console-template for more information
using Microsoft.Extensions.Logging;
using System.Net;
using System.Reflection;
using System.Text;
using System.Xml;
using UDP_Relay_Core;

try
{
    //*********************************************************************************************************************
    // Test receiver
    // This code is used to test the UDP Relay Service and the Relay Console.
    // It waits for UDP packets from the relay server.
    // It sends a response to the relay server and writes the received data to the console.
    //*********************************************************************************************************************

    // Add logging beyond console
    var loggerFactory = LoggerFactory.Create(
        builder => builder
                    .AddFile("UDP_TestReceiver.log")
                    .AddDebug()
                    .SetMinimumLevel(LogLevel.Debug)
    );
    var log = loggerFactory.CreateLogger<Program>();
    Logger.AddLogger(log);

    // Get settings from XML file
    Logger.Log("TestReceiver started");
    XML_Wrapper xML_Wrapper = new XML_Wrapper(@"Settings_TestReceiver.xml");

    IPEndPoint listeningEndPoint = xML_Wrapper.GetIPEndPoint(@"/TestReceiver/ListeningIP", @"/TestReceiver/ListeningPort");
    Logger.LogDebug("Listening EndPoint: " + listeningEndPoint.ToString());

    IPEndPoint sendingEndPoint = xML_Wrapper.GetIPEndPoint(@"/TestReceiver/SendingIP", @"/TestReceiver/SendingPort");
    Logger.LogDebug("Sending EndPoint: " + sendingEndPoint.ToString());

    int timeOut = xML_Wrapper.GetInt(@"/TestReceiver/TimeOut");
    Logger.LogDebug("Timeout: " + timeOut);

    // Start receiving UDP packets and sending responses
    UDP_Relay relay = new UDP_Relay();
    Logger.Log("Waiting to receive data...");
    for (int i = 0; i < 100; i++)
    {
        byte[] receivedData = relay.Receive(listeningEndPoint, timeOut, null);
        string receivedMessage = Encoding.UTF8.GetString(receivedData);
        Logger.Log("Received: " + receivedMessage + " from: " + listeningEndPoint);
        string sendMessage = receivedMessage;

        byte[] sendData = Encoding.UTF8.GetBytes(sendMessage);
        relay.Send(sendingEndPoint, sendData);
        Logger.Log("Data sent: " + sendMessage + " to " + sendingEndPoint);
    };
    Logger.Log("Receiving loop finished - press a key to exit");
    Console.ReadKey();
}
catch (Exception ex)
{
    Logger.Log(ex);
    Console.ReadKey();
}