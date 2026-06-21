using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;

namespace UDP_Relay_Core
{
    public static class Logger
    {
        private static List<ILogger> _loggerList = new List<ILogger>();

        /// <summary>
        /// When true, messages are also written to <see cref="Console"/> in addition to any
        /// registered <see cref="ILogger"/>s. Defaults to false so the library stays quiet when
        /// embedded elsewhere; the console host applications opt in by setting it to true at startup.
        /// </summary>
        public static bool WriteToConsole { get; set; } = false;

        public static void AddLogger(ILogger logger)
        {
            _loggerList.Add(logger);
        }

        public static void Log(string message)
        {
            if (WriteToConsole) Console.WriteLine(message);
            foreach (ILogger logger in _loggerList)
            {
                logger.LogInformation(message);
            }
        }

        public static void LogDebug(string message)
        {
            if (WriteToConsole) Console.WriteLine(message);
            foreach (ILogger logger in _loggerList)
            {
                logger.LogDebug(message);
            }
        }

        public static void LogTrace(string message)
        {
            foreach (ILogger logger in _loggerList)
            {
                logger.LogTrace(message);
            }
        }

        public static void Log(string message, Exception ex)
        {
            if (WriteToConsole) Console.WriteLine(message + Environment.NewLine + ex.Message);
            foreach (ILogger logger in _loggerList)
            {
                logger.LogError(ex, message);
            }
        }

        public static void Log(Exception ex)
        {
            if (WriteToConsole) Console.WriteLine(ex.Message);
            foreach (ILogger logger in _loggerList)
            {
                logger.LogError(ex, "Unhandled exception");
            }
        }
    }
}
