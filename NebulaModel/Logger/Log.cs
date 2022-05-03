using System;
using System.Diagnostics;

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
            LastWarnMsg = message;
        }

        public static void Warn(object message)
        {
            Warn(message?.ToString());
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
    }
}
