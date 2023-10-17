using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace UDP_Relay_Core
{
    public static class Logger
    {
        private static List<ILogger> _loggerList = new List<ILogger>();

        public static void AddLogger(ILogger logger)
        {
            _loggerList.Add(logger);
        }

        public static void Log(string message)
        {
            Console.WriteLine(message);
            foreach (ILogger logger in _loggerList)
            {
                logger.LogInformation(message);
            }
        }

        public static void LogDebug(string message)
        {
            Console.WriteLine(message);
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
            Console.WriteLine(message + Environment.NewLine + ex.Message);
            foreach (ILogger logger in _loggerList)
            {
                logger.LogError(ex, message);
            }
        }

        public static void Log(Exception ex)
        {
            Console.WriteLine(ex.Message);
            foreach (ILogger logger in _loggerList)
            {
                logger.LogError(ex, null);
            }
        }           
    }
}
