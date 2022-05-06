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
        private static DateTime lastExecutionTime = DateTime.MinValue;
        
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
                string localIP = IPUtils.GetLocalAddress().ToString();

                TimeSpan timeSpan = DateTime.Now.Subtract(lastExecutionTime);
                if (timeSpan.TotalMinutes < 1)
                {
                    string ratelimited = $"rate limited (wait {Mathf.RoundToInt(60 - (float)timeSpan.TotalSeconds)} seconds)";

                    string output = GetServerInfoText(server, localIP, ratelimited, ratelimited, full);
                    window.SendLocalChatMessage(output, ChatMessageType.CommandOutputMessage);
                }
                else
                {
                    string pending = "pending...";

                    string output = GetServerInfoText(server, localIP, pending, pending, full);
                    ChatMessage message = window.SendLocalChatMessage(output, ChatMessageType.CommandOutputMessage);

                    IPUtils.GetPortStatus(server.Port, (ip, port) =>
                    {
                        string newOutput = GetServerInfoText(server, localIP, $"{ip}:{server.Port}", port, full);
                        message.Text = newOutput;
                    });
                    lastExecutionTime = DateTime.Now;
                }
            }else if (Multiplayer.Session.Network is IClient client)
            {
                string output = GetClientInfoText(client, full);
                window.SendLocalChatMessage(output, ChatMessageType.CommandOutputMessage);
            }
            
        }

        private static string GetServerInfoText(IServer server, string localIP, string wanIP, string portStatus, bool full)
        {
            StringBuilder sb = new StringBuilder("Server info:");

            sb.Append($"\n  Local IP address: {FormatCopyString($"{localIP}:{server.Port}")}");
            sb.Append($"\n  WAN IP address: {FormatCopyString(wanIP, true, IPFilter)}");
            sb.Append($"\n  Port status: {portStatus}");
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
            StringBuilder sb = new StringBuilder("Client info:");

            string ipAddress = client.ServerEndpoint.ToString();

            sb.Append($"\n  Host IP address: {FormatCopyString(ipAddress, true, IPFilter)}");
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
            
            string[] parts = ip.Split(':');
            string safeIp = ip;
            if (parts.Length == 2)
            {
                safeIp = $"{ReplaceChars(parts[0], "0123456789", '*')}:{parts[1]}";
            }
            else
            {
                safeIp = ReplaceChars(safeIp, "0123456789", '*');
            }

            return safeIp;
        }

        private static string NgrokAddressFilter(string address)
        {
            if (!NebulaModel.Config.Options.StreamerMode) return address;

            return Regex.Replace(address, @"\w", "*");
        }

        private static string ReplaceChars(string s, string targetSymbols, char newVal)
        {
            StringBuilder sb = new StringBuilder(s);
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