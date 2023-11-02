using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Diagnostics.Eventing.Reader;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using UDP_Relay_Core;
using Microsoft.Extensions.Logging.EventLog;
using Microsoft.Extensions.Logging;


//*********************************************************************************************************************
// Relay service
// This is the main code for the UDP relay. It starts the relaying of UDP packets between a client and a server.
// This version runs as a windows service application.
// Either this or the Relay console must run for the application to work.
//*********************************************************************************************************************

namespace UDP_Relay_Service
{
    public partial class UDP_Relay_Service : ServiceBase
    {
        /// <summary>
        /// Create a new instance of the UDP Relay Service.
        /// </summary>
        public UDP_Relay_Service()
        {
            InitializeComponent();
            
            System.IO.Directory.SetCurrentDirectory(System.AppDomain.CurrentDomain.BaseDirectory);

            // Add logging beyond console
            var loggerFactory = LoggerFactory.Create(
                builder => builder
                            .AddFile("UDP_Relay_Service.log")
                            .AddDebug()
                            .AddEventLog(new EventLogSettings()
                            {
                                SourceName = "UDP_Relay_Service",
                                LogName = "Application",
                                Filter = (x, y) => y >= LogLevel.Information
                            })
                            .SetMinimumLevel(LogLevel.Debug)
            );
            var log = loggerFactory.CreateLogger<UDP_Relay_Service>();
            Logger.AddLogger(log);
        }

        private UDP_Relay relay;

        /// <summary>
        /// Starts the UDP relay service.
        /// </summary>
        /// <param name="args">Not used</param>
        protected override void OnStart(string[] args)
        {
            try
            { 
                // Get settings from XML file
                Logger.Log("UDP Relay Service starting");
                XML_Wrapper xML_Wrapper = new XML_Wrapper(@"Settings_UDP_Relay_Service.xml");

                IPEndPoint listeningClientEndPoint = xML_Wrapper.GetIPEndPoint(@"/UDP_Relay_Service/ListeningClientIP", @"/UDP_Relay_Service/ListeningClientPort");
                Logger.LogDebug("Listening Client EndPoint: " + listeningClientEndPoint.ToString());

                IPEndPoint listeningServerEndPoint = xML_Wrapper.GetIPEndPoint(@"/UDP_Relay_Service/ListeningServerIP", @"/UDP_Relay_Service/ListeningServerPort");
                Logger.LogDebug("Listening Server EndPoint: " + listeningServerEndPoint);

                IPEndPoint sendingDlientEndPoint = xML_Wrapper.GetIPEndPoint(@"/UDP_Relay_Service/SendingClientIP", @"/UDP_Relay_Service/SendingClientPort");
                Logger.LogDebug("Sending Client EndPoint: " + sendingDlientEndPoint);

                IPEndPoint sendingServerEndPoint = xML_Wrapper.GetIPEndPoint(@"/UDP_Relay_Service/SendingServerIP", @"/UDP_Relay_Service/SendingServerPort");
                Logger.LogDebug("Sending Server EndPoint: " + sendingServerEndPoint);

                int timeOut = xML_Wrapper.GetInt(@"/UDP_Relay_Service/TimeOut");
                Logger.LogDebug("Timeout: " + timeOut);

                // Start relaying UDP packets
                Logger.LogDebug("Relaying of UDP packets started");
                relay = new UDP_Relay();
                relay.StartRelaying(listeningClientEndPoint, sendingServerEndPoint);
                relay.StartRelaying(listeningServerEndPoint, sendingDlientEndPoint);

                Logger.Log("UDP Relay Service started");
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
                base.ExitCode = ex.HResult;
                base.Stop();
            }
        }

        /// <summary>
        /// Stops the UDP relay service.
        /// </summary>
        protected override void OnStop()
        {
            try
            {
                Logger.LogTrace("UDP Relay Service stopping");
                if (relay != null && !relay.IsStopping)
                {
                    relay.StopRelaying();
                    Logger.Log("UDP Relay Service stopped");
                    relay.Dispose();
                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
                base.ExitCode = ex.HResult;
            }
        }
    }
}
