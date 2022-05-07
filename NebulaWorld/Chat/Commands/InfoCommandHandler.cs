using BepInEx.Bootstrap;
using NebulaModel.DataStructures;
using NebulaModel.Networking;
using NebulaModel.Utils;
using NebulaWorld.MonoBehaviours.Local;
using System;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;
using static NebulaWorld.Chat.CopyTextChatLinkHandler;

namespace NebulaWorld.Chat.Commands
{
    public class InfoCommandHandler : IChatCommandHandler
    {
        public void Execute(ChatWindow window, string[] parameters)
        {
            if (!Multiplayer.IsActive)
            {
                window.SendLocalChatMessage("This command can only be used in multiplayer!", ChatMessageType.CommandErrorMessage);
                return;
            }
            
            bool full = parameters.Length > 0 && parameters[0].Equals("full");

            if (Multiplayer.Session.Network is IServer server)
            {
                string output = GetServerInfoText(
                    server, 
                    new IPUtils.IpInfo {
                        LANAddress = "Pending...",
                        WANv4Address = "Pending...",
                        WANv6Address = "Pending...",
                        PortStatus = "Pending...",
                        DataState = IPUtils.DataState.Unset
                    }, 
                    full
                );
                ChatMessage message = window.SendLocalChatMessage(output, ChatMessageType.CommandOutputMessage);

                // This will cause the temporary (Pending...) info to be dynamically replaced with the correct info once it is in
                IPUtils.GetIPInfo(server.Port).ContinueWith(async (ipInfo) =>
                {
                    string newOutput = GetServerInfoText(server, await ipInfo, full);
                    message.Text = newOutput;
                });
            }
            else if (Multiplayer.Session.Network is IClient client)
            {
                string output = GetClientInfoText(client, full);
                window.SendLocalChatMessage(output, ChatMessageType.CommandOutputMessage);
            }
            
        }

        private static string GetServerInfoText(IServer server, IPUtils.IpInfo ipInfo, bool full)
        {
            StringBuilder sb = new("Server info:");

            string lan = ipInfo.LANAddress;
            if (IPUtils.IsIPv4(lan))
            {
                lan = $"{FormatCopyString($"{ipInfo.LANAddress}:{server.Port}")}";
            }
            sb.Append($"\n  Local IP address: {lan}");

            string wanv4 = ipInfo.WANv4Address;
            if(IPUtils.IsIPv4(wanv4))
            {
                wanv4 = $"{FormatCopyString($"{ipInfo.WANv4Address}:{server.Port}", true, IPFilter)}";
            }
            sb.Append($"\n  WANv4 IP address: {wanv4}");

            string wanv6 = ipInfo.WANv6Address;
            if (IPUtils.IsIPv6(wanv6))
            {
                wanv6 = $"{FormatCopyString($"{ipInfo.WANv6Address}:{server.Port}", true, IPFilter)}";
            }
            sb.Append($"\n  WANv6 IP address: {wanv6}");

            if (server.NgrokAddress != null)
            {
                if (server.NgrokActive)
                {
                    sb.Append($"\n  Ngrok address: {FormatCopyString(server.NgrokAddress, true, NgrokAddressFilter)}");
                } else
                {
                    sb.Append($"\n  Ngrok address: Tunnel Inactive!");
                }
                
            }

            sb.Append($"\n  Port status: {ipInfo.PortStatus}");
            sb.Append($"\n  Data State: {ipInfo.DataState}");
            TimeSpan timeSpan = DateTime.Now.Subtract(Multiplayer.Session.StartTime);
            sb.Append($"\n  Uptime: {(int) Math.Round(timeSpan.TotalHours)}:{timeSpan.Minutes}:{timeSpan.Seconds} up");

            sb.Append("\n\nGame info:");
            sb.Append($"\n  Game Version: {GameConfig.gameVersion.ToFullString()}");
            sb.Append($"\n  Mod Version: {ThisAssembly.AssemblyFileVersion}");

            if (full)
            {
                sb.Append("\n\nMods installed:");
                foreach (var kv in Chainloader.PluginInfos)
                {
                    sb.Append($"\n  {kv.Key} - {kv.Value.Metadata.Version}");
                }
            }
            else
            {
                sb.Append($"\nUse '{ChatCommandRegistry.CommandPrefix}info full' to see mod list.");
            }

            return sb.ToString();
        }

        private static string GetClientInfoText(IClient client, bool full)
        {
            StringBuilder sb = new("Client info:");

            string ipAddress = client.ServerEndpoint.ToString();

            sb.Append($"\n  Host IP address: {FormatCopyString(ipAddress, true)}");
            sb.Append($"\n  Game Version: {GameConfig.gameVersion.ToFullString()}");
            sb.Append($"\n  Mod Version: {ThisAssembly.AssemblyFileVersion}");

            if (full)
            {
                sb.Append("\n\nMods installed:");
                foreach (var kv in Chainloader.PluginInfos)
                {
                    sb.Append($"\n  {kv.Key} - {kv.Value.Metadata.Version}");
                }
            }
            else
            {
                sb.Append($"\nUse '{ChatCommandRegistry.CommandPrefix}info full' to see mod list.");
            }

            return sb.ToString();
        }

        private static string IPFilter(string ip)
        {
            if (!NebulaModel.Config.Options.StreamerMode) return ip;

            if (!ip.Contains("]:")) {
                string[] parts = ip.Split(':');
                string safeIp = ip;
                if (parts.Length == 2)
                {
                    safeIp = $"{Regex.Replace(parts[0], @"\w", "*")}:{parts[1]}";
                }
                else
                {
                    safeIp = Regex.Replace(safeIp, @"\w", "*");
                }
                return safeIp;
            } else
            {
                string[] parts = ip.Split(new string[] { "]:" }, StringSplitOptions.None);
                string safeIp = ip;
                if (parts.Length == 2)
                {
                    safeIp = $"{Regex.Replace(parts[0], @"\w", "*")}]:{parts[1]}";
                }
                else
                {
                    safeIp = Regex.Replace(safeIp, @"\w", "*");
                }
                return safeIp;
            }
        }

        private static string NgrokAddressFilter(string address)
        {
            if (!NebulaModel.Config.Options.StreamerMode) return address;

            return Regex.Replace(address, @"\w", "*");
        }

        private static string ReplaceChars(string s, string targetSymbols, char newVal)
        {
            StringBuilder sb = new(s);
            for (int i = 0; i < sb.Length; i++)
            {
                if (targetSymbols.Contains(sb[i]))
                {
                    sb[i] = newVal;
                }
            }
            return sb.ToString();
        }

        public string GetDescription()
        {
            return "Get information about server";
        }
        
        public string[] GetUsage()
        {
            return new string[] { "[full]" };
        }
    }
}