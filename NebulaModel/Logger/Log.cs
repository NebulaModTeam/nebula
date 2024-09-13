#region

using System;
using System.Text.RegularExpressions;
using WebSocketSharp;

#endregion

namespace NebulaModel.Logger;

public static class Log
{
    private static ILogger logger;
    public static string LastInfoMsg { get; set; }
    public static string LastWarnMsg { get; set; }
    public static string LastErrorMsg { get; set; }

    public static void Init(ILogger logger)
    {
        Log.logger = logger;
    }

    public static void Debug(string message)
    {
        logger.LogDebug(message);
    }

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
            // Test if current code is executing on the main unity thread
            if (BepInEx.ThreadingHelper.Instance.InvokeRequired)
            {
                // ShowError has Unity API and needs to call on the main thread
                BepInEx.ThreadingHelper.Instance.StartSyncInvoke(() =>
                {
                    // We just want to use the window to show the error message, so leave GameMain.errored as the original value
                    var tmp = GameMain.errored;
                    UIFatalErrorTip.instance.ShowError("[Nebula Error] " + message, "");
                    GameMain.errored = tmp;
                });
                return;
            }
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
        var log = data.ToString();
        const string Ipv4Regex = @"\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}";
        const string Ipv6Regex = "([0-9a-fA-F]{1,4}:){7,7}[0-9a-fA-F]{1,4}";
        log = Regex.Replace(log, Ipv4Regex, "(IPv4 Address)");
        log = Regex.Replace(log, Ipv6Regex, "(IPv6 Address)");

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
