using System.Net;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using UDP_Relay_Core;

namespace UDP_Relay_Service;

/// <summary>
/// Hosts the bidirectional UDP relay as a background service: reads the XML settings,
/// starts the two one-way relay tasks, and stops them again on shutdown.
/// </summary>
public class RelayWorker : BackgroundService
{
    public RelayWorker(ILogger<RelayWorker> logger)
    {
        // Route UDP_Relay_Core's static logger into the host's logging pipeline
        // (file + Event Log). The service has no console, so WriteToConsole stays off.
        Logger.AddLogger(logger);
    }

    private UDP_Relay? _relay;

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            string settingsPath = Path.Combine(AppContext.BaseDirectory, "Settings_UDP_Relay_Service.xml");
            Logger.Log("UDP Relay Service starting");
            XML_Wrapper settings = new XML_Wrapper(settingsPath);

            IPEndPoint listeningClient = settings.GetIPEndPoint("/UDP_Relay_Service/ListeningClientIP", "/UDP_Relay_Service/ListeningClientPort");
            IPEndPoint listeningServer = settings.GetIPEndPoint("/UDP_Relay_Service/ListeningServerIP", "/UDP_Relay_Service/ListeningServerPort");
            IPEndPoint sendingClient = settings.GetIPEndPoint("/UDP_Relay_Service/SendingClientIP", "/UDP_Relay_Service/SendingClientPort");
            IPEndPoint sendingServer = settings.GetIPEndPoint("/UDP_Relay_Service/SendingServerIP", "/UDP_Relay_Service/SendingServerPort");

            _relay = new UDP_Relay();
            _relay.StartRelaying(listeningClient, sendingServer); // device -> remote server
            _relay.StartRelaying(listeningServer, sendingClient); // server reply -> device
            Logger.Log("UDP Relay Service started");
        }
        catch (Exception ex)
        {
            Logger.Log(ex);
            throw; // surface startup failure so the host stops the service
        }

        return Task.CompletedTask;
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        Logger.LogTrace("UDP Relay Service stopping");
        if (_relay != null)
        {
            _relay.StopRelaying();
            Logger.Log("UDP Relay Service stopped");
            _relay.Dispose();
        }
        await base.StopAsync(cancellationToken);
    }
}
