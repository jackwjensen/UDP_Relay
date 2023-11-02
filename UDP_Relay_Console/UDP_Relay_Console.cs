// See https://aka.ms/new-console-template for more information
using Microsoft.Extensions.Logging;
using System.Net;
using System.Text;
using System.Xml;
using UDP_Relay_Core;

try
{
    //*********************************************************************************************************************
    // Relay Console
    // This is the main code for the UDP relay. It starts the relaying of UDP packets between a client and a server.
    // This version runs as a console application. Either this or the Relay service must run for the application to work.
    //*********************************************************************************************************************

    // Add logging beyond console
    var loggerFactory = LoggerFactory.Create(
        builder => builder
                    .AddFile("UDP_Relay_Consol.log")
                    .AddDebug()
                    .SetMinimumLevel(LogLevel.Debug)
    );
    var log = loggerFactory.CreateLogger<Program>();
    Logger.AddLogger(log);

    // Get settings from XML file
    Logger.Log("UDP Relay Console started");
    XML_Wrapper xML_Wrapper = new XML_Wrapper(@"Settings_UDP_Relay_Console.xml");

    IPEndPoint listeningClientEndPoint = xML_Wrapper.GetIPEndPoint(@"/UDP_Relay_Console/ListeningClientIP", @"/UDP_Relay_Console/ListeningClientPort");
    Logger.LogDebug("Listening Client EndPoint: " + listeningClientEndPoint);

    IPEndPoint sendingClientEndPoint = xML_Wrapper.GetIPEndPoint(@"/UDP_Relay_Console/SendingClientIP", @"/UDP_Relay_Console/SendingClientPort");
    Logger.LogDebug("Sending Client EndPoint: " + sendingClientEndPoint);

    IPEndPoint listeningServerEndPoint = xML_Wrapper.GetIPEndPoint(@"/UDP_Relay_Console/ListeningServerIP", @"/UDP_Relay_Console/ListeningServerPort");
    Logger.LogDebug("Listening Server EndPoint: " + listeningServerEndPoint);

    IPEndPoint sendingServerEndPoint = xML_Wrapper.GetIPEndPoint(@"/UDP_Relay_Console/SendingServerIP", @"/UDP_Relay_Console/SendingServerPort");
    Logger.LogDebug("Sending Server EndPoint: " + sendingServerEndPoint);

    int timeOut = xML_Wrapper.GetInt(@"/UDP_Relay_Console/TimeOut");
    Logger.LogDebug("Timeout: " + timeOut);

    // Start relaying UDP packets
    Logger.Log("Relaying of UDP packets started");
    UDP_Relay relay = new UDP_Relay();
    relay.StartRelaying(listeningClientEndPoint, sendingServerEndPoint);
    relay.StartRelaying(listeningServerEndPoint, sendingClientEndPoint);
    Thread.Sleep(100);
    Console.WriteLine("Pres any key to stop relaying");

    Console.ReadKey();    
    relay.StopRelaying();
    Logger.Log("Relaying of UDP packets stopped");

    Console.ReadKey();
    relay.Dispose();
    Logger.LogDebug("Relay disposed");

    Console.ReadKey();
}
catch (Exception ex)
{
    Logger.Log(ex.Message);
    Console.ReadKey();
}