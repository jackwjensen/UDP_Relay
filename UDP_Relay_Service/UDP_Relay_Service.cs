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

                IPEndPoint localClientEndPoint = xML_Wrapper.GetIPEndPoint(@"/UDP_Relay_Service/LocalClientIP", @"/UDP_Relay_Service/LocalClientPort");
                Logger.LogDebug("Local ClientEndPoint: " + localClientEndPoint.ToString());

                IPEndPoint localServerEndPoint = xML_Wrapper.GetIPEndPoint(@"/UDP_Relay_Service/LocalServerIP", @"/UDP_Relay_Service/LocalServerPort");
                Logger.LogDebug("Local ServerEndPoint: " + localServerEndPoint);

                IPEndPoint clientEndPoint = xML_Wrapper.GetIPEndPoint(@"/UDP_Relay_Service/ClientIP", @"/UDP_Relay_Service/ClientPort");
                Logger.LogDebug("Client EndPoint: " + clientEndPoint);

                IPEndPoint serverEndPoint = xML_Wrapper.GetIPEndPoint(@"/UDP_Relay_Service/ServerIP", @"/UDP_Relay_Service/ServerPort");
                Logger.LogDebug("Server EndPoint: " + serverEndPoint);

                int timeOut = xML_Wrapper.GetInt(@"/UDP_Relay_Service/TimeOut");
                Logger.LogDebug("Timeout: " + timeOut);

                // Start relaying UDP packets
                Logger.LogDebug("Relaying of UDP packets started");
                relay = new UDP_Relay();
                relay.StartRelaying(localClientEndPoint, serverEndPoint);
                relay.StartRelaying(localServerEndPoint, clientEndPoint);

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
