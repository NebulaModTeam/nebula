using System;
using System.Diagnostics;
using System.Text.RegularExpressions;
using WebSocketSharp;

namespace NebulaModel.Logger
{
    public static class Log
    {
        public static string LastInfoMsg { get; set; }
        public static string LastWarnMsg { get; set; }
        public static string LastErrorMsg { get; set; }

        private static ILogger logger;

        public static void Init(ILogger logger)
        {
            Log.logger = logger;
        }

        [Conditional("DEBUG")]
        public static void Debug(string message)
        {
            logger.LogDebug(message);
        }

        [Conditional("DEBUG")]
        public static void Debug(object message)
        {
            Debug(message?.ToString());
        }

        public static void Info(string message)
        {
            logger.LogInfo(message);
            LastInfoMsg = message;
        }

        public static void Info(object message)
        {
            Info(message?.ToString());
        }

        public static void Warn(string message)
        {
            logger.LogWarning(message);
        }

        public static void Warn(object message)
        {
            Warn(message?.ToString());
        }

        public static void WarnInform(string message)
        {
            Warn(message);
            LastWarnMsg = message;
        }

        public static void Error(string message)
        {
            logger.LogError(message);
            LastErrorMsg = message;
            if (UIFatalErrorTip.instance != null)
            {
                UIFatalErrorTip.instance.ShowError("[Nebula Error] " + message, "");
            }
        }

        public static void Error(Exception ex)
        {
            Error(ex?.ToString());
        }

        public static void Error(string message, Exception ex)
        {
            Error(message);
            Error(ex);
        }

        public static void SocketOutput(LogData data, string _)
        {
            string log = data.ToString();
            string ipv4Regex = "\\d{1,3}\\.\\d{1,3}\\.\\d{1,3}\\.\\d{1,3}";
            string ipv6Regex = "([0-9a-fA-F]{1,4}:){7,7}[0-9a-fA-F]{1,4}";
            log = Regex.Replace(log, ipv4Regex, "(IPv4 Address)");
            log = Regex.Replace(log, ipv6Regex, "(IPv6 Address)");

            if (data.Level >= LogLevel.Warn)
            {
                Warn(log);
            }
            else
            {
                Info(log);
            }
        }
    }
}
