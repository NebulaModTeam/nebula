#region

using System;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using BepInEx.Bootstrap;
using NebulaModel;
using NebulaModel.DataStructures.Chat;
using NebulaModel.Networking;
using NebulaModel.Utils;
using NebulaWorld.MonoBehaviours.Local.Chat;
using static NebulaWorld.Chat.ChatLinks.CopyTextChatLinkHandler;

#endregion

namespace NebulaWorld.Chat.Commands;

public class InfoCommandHandler : IChatCommandHandler
{
    private static readonly string[] s_separator = ["]:"];

    public void Execute(ChatWindow window, string[] parameters)
    {
        if (!Multiplayer.IsActive)
        {
            window.SendLocalChatMessage("This command can only be used in multiplayer!".Translate(),
                ChatMessageType.CommandErrorMessage);
            return;
        }

        var full = parameters.Length > 0 && parameters[0].Equals("full");

        switch (Multiplayer.Session.Network)
        {
            case IServer server:
                {
                    var output = GetServerInfoText(
                        server,
                        new IPUtils.IpInfo
                        {
                            LANAddress = "Pending...".Translate(),
                            WANv4Address = "Pending...".Translate(),
                            WANv6Address = "Pending...".Translate(),
                            PortStatus = "Pending...".Translate(),
                            DataState = IPUtils.DataState.Unset
                        },
                        full
                    );
                    var message = window.SendLocalChatMessage(output, ChatMessageType.CommandOutputMessage);

                    // This will cause the temporary (Pending...) info to be dynamically replaced with the correct info once it is in
                    IPUtils.GetIPInfo(server.Port).ContinueWith(async ipInfo =>
                    {
                        var newOutput = GetServerInfoText(server, await ipInfo, full);
                        message.Text = newOutput;
                    });
                    break;
                }
            case IClient client:
                {
                    var output = GetClientInfoText(client, full);
                    window.SendLocalChatMessage(output, ChatMessageType.CommandOutputMessage);
                    break;
                }
        }
    }

    public string GetDescription()
    {
        return "Get information about server".Translate();
    }

    public string[] GetUsage()
    {
        return ["[full]"];
    }

    public static string GetServerInfoText(IServer server, IPUtils.IpInfo ipInfo, bool full)
    {
        StringBuilder sb = new("Server info:".Translate());

        var lan = ipInfo.LANAddress;
        if (IPUtils.IsIPv4(lan))
        {
            lan = $"{FormatCopyString($"{ipInfo.LANAddress}:{server.Port}")}";
        }
        sb.Append("\n  ").Append("Local IP address: ".Translate()).Append(lan);

        var wanv4 = ipInfo.WANv4Address;
        if (IPUtils.IsIPv4(wanv4))
        {
            wanv4 = $"{FormatCopyString($"{ipInfo.WANv4Address}:{server.Port}", true, IPFilter)}";
        }
        sb.Append("\n  ").Append("WANv4 IP address: ".Translate()).Append(wanv4);

        var wanv6 = ipInfo.WANv6Address;
        if (IPUtils.IsIPv6(wanv6))
        {
            wanv6 = $"{FormatCopyString($"{ipInfo.WANv6Address}:{server.Port}", true, IPFilter)}";
        }
        sb.Append("\n  ").Append("WANv6 IP address: ".Translate()).Append(wanv6);

        if (server.NgrokEnabled)
        {
            if (server.NgrokActive)
            {
                sb.Append("\n ").Append("Ngrok address: ".Translate())
                    .Append(FormatCopyString(server.NgrokAddress, true, NgrokAddressFilter));
            }
            else
            {
                sb.Append("\n  ").Append("Ngrok address: Tunnel Inactive!".Translate());
            }

            if (!string.IsNullOrWhiteSpace(server.NgrokLastErrorCode))
            {
                sb.Append($" ({FormatCopyString(server.NgrokLastErrorCode)}){FormatCopyString(server.NgrokLastErrorCodeDesc)}");
            }
        }

        sb.Append("\n  ").Append("Port status: ".Translate()).Append(ipInfo.PortStatus);
        sb.Append("\n  ").Append("Data state: ".Translate()).Append(ipInfo.DataState);
        var timeSpan = DateTime.Now.Subtract(Multiplayer.Session.StartTime);
        sb.Append("\n  ").Append("Uptime: ".Translate())
            .Append($"{(int)Math.Round(timeSpan.TotalHours)}:{timeSpan.Minutes}:{timeSpan.Seconds}");

        sb.Append("\n\n").Append("Game info:".Translate());
        sb.Append("\n  ").Append("Game Version: ".Translate()).Append(GameConfig.gameVersion.ToFullString());
        sb.Append("\n  ").Append("Mod Version: ".Translate()).Append(ThisAssembly.AssemblyFileVersion);

        if (full)
        {
            sb.Append("\n\n").Append("Mods installed:".Translate());
            var index = 1;
            foreach (var kv in Chainloader.PluginInfos)
            {
                sb.Append($"\n[{index++:D2}] {kv.Value.Metadata.Name} - {kv.Value.Metadata.Version}");
            }
        }
        else
        {
            sb.Append('\n').Append("Use '/info full' to see mod list.".Translate());
        }

        return sb.ToString();
    }

    private static string GetClientInfoText(IClient client, bool full)
    {
        StringBuilder sb = new("Client info:".Translate());

        var ipAddress = client.ServerEndpoint.ToString();

        sb.Append("\n  ").Append("Host IP address: ".Translate()).Append(FormatCopyString(ipAddress, true));
        sb.Append("\n  ").Append("Game Version: ".Translate()).Append(GameConfig.gameVersion.ToFullString());
        sb.Append("\n  ").Append("Mod Version: ".Translate()).Append(ThisAssembly.AssemblyFileVersion);
        sb.Append("\n  ").Append("Server UPS: ".Translate()).Append(Multiplayer.Session.State.GetServerUPS().ToString("F2"));

        if (full)
        {
            sb.Append("\n\n").Append("Mods installed:".Translate());
            var index = 1;
            foreach (var kv in Chainloader.PluginInfos)
            {
                sb.Append($"\n[{index++:D2}] {kv.Value.Metadata.Name} - {kv.Value.Metadata.Version}");
            }
        }
        else
        {
            sb.Append('\n').Append("Use '/info full' to see mod list.".Translate());
        }

        return sb.ToString();
    }

    private static string IPFilter(string ip)
    {
        if (!Config.Options.StreamerMode)
        {
            return ip;
        }

        if (!ip.Contains("]:"))
        {
            var parts = ip.Split(':');
            var safeIp = ip;
            safeIp = parts.Length == 2
                ? $"{Regex.Replace(parts[0], @"\w", "*")}:{parts[1]}"
                : Regex.Replace(safeIp, @"\w", "*");
            return safeIp;
        }
        else
        {
            var parts = ip.Split(s_separator, StringSplitOptions.None);
            var safeIp = ip;
            safeIp = parts.Length == 2
                ? $"{Regex.Replace(parts[0], @"\w", "*")}]:{parts[1]}"
                : Regex.Replace(safeIp, @"\w", "*");
            return safeIp;
        }
    }

    private static string NgrokAddressFilter(string address)
    {
        return !Config.Options.StreamerMode ? address : Regex.Replace(address, @"\w", "*");
    }

    //TODO: Unused?
    private static string ReplaceChars(string s, string targetSymbols, char newVal)
    {
        StringBuilder sb = new(s);
        for (var i = 0; i < sb.Length; i++)
        {
            if (targetSymbols.Contains(sb[i]))
            {
                sb[i] = newVal;
            }
        }
        return sb.ToString();
    }
}
