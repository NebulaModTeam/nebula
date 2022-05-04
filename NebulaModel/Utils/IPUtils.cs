using BepInEx;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Timers;
using static NebulaModel.Utils.IPUtils;

namespace NebulaModel.Utils
{
    public static class IPUtils
    {
        static readonly HttpClient client = new();

        public enum DataState
        {
            Unset,
            Fresh,
            Cached
        }

        public enum Status
        {
            None,
            Unsupported,
            Unavailable
        }

        public enum PortStatus
        {
            Open,
            Closed
        }

        public struct IpInfo
        {
            public string LANAddress = null;
            public string WANv4Address = null;
            public string WANv6Address = null;
            public string PortStatus = null;
            public DataState DataState = DataState.Unset;
            public IpInfo() { }
        }

        static IpInfo ipInfo;

        static readonly Timer timer;

        static IPUtils()
        {
            timer = new Timer()
            {
                Enabled = false,
                Interval = TimeSpan.FromMinutes(1).TotalMilliseconds,
            };
            timer.Elapsed += (s, e) => { timer.Stop(); };
        }

        public static string GetLocalAddress()
        {
            using Socket socket = new(AddressFamily.InterNetwork, SocketType.Dgram, 0);
            socket.Connect("8.8.8.8", 65530);
            IPEndPoint endPoint = socket.LocalEndPoint as IPEndPoint;
            return endPoint.Address.ToString();
        }

        public static async Task<string> GetWANv4Address()
        {
            try
            {
                string response = await client.GetStringAsync("https://api.ipify.org");

                if(IsIPv4(response))
                {
                    return response;
                }

                return Status.Unsupported.ToString();
            }
            catch(Exception e)
            {
                Logger.Log.Warn(e);
                return ipInfo.WANv4Address ?? Status.Unavailable.ToString();
            }
        }

        public static async Task<string> GetWANv6Address()
        {
            try
            {
                string response = await client.GetStringAsync("https://api64.ipify.org");

                if(IsIPv6(response))
                {
                    return $"[{response}]";
                }

                return Status.Unsupported.ToString();
            }
            catch (Exception e)
            {
                Logger.Log.Warn(e);
                return ipInfo.WANv6Address ?? Status.Unavailable.ToString();
            }
        }

        public static async Task<string> GetPortStatus(ushort port)
        {
            try
            {
                string response = await client.GetStringAsync($"https://ifconfig.co/port/{port}");
                Dictionary<string, object> jObject = MiniJson.Deserialize(response) as Dictionary<string, object>;
                if (!IsIPv4((string)jObject["ip"]))
                {
                    return Status.Unsupported.ToString();
                }
                return (bool)jObject["reachable"] ? PortStatus.Open.ToString() : PortStatus.Closed.ToString();
            }
            catch (Exception e)
            {
                Logger.Log.Warn(e);
                return ipInfo.PortStatus ?? Status.Unavailable.ToString();
            }
        }

        public static async Task<IpInfo> GetIPInfo(ushort port = default)
        {
            if(timer.Enabled && ipInfo.DataState != DataState.Unset)
            {
                return ipInfo;
            }

            var rawInfo = new IpInfo()
            {
                LANAddress = GetLocalAddress().ToString(),
                WANv4Address = await GetWANv4Address(),
                WANv6Address = await GetWANv6Address(),
                DataState = DataState.Fresh
            };

            rawInfo.PortStatus = await GetPortStatus(port);

            ipInfo = rawInfo;
            ipInfo.DataState = DataState.Cached;
            timer.Start();

            return rawInfo;
        }

        public static bool IsIPv6(string ip)
        {
            if (IPAddress.TryParse(ip, out IPAddress ipAddress))
            {
                return ipAddress.AddressFamily == AddressFamily.InterNetworkV6;
            }
            return false;
        }

        public static bool IsIPv4(string ip)
        {
            if (IPAddress.TryParse(ip, out IPAddress ipAddress))
            {
                return ipAddress.AddressFamily == AddressFamily.InterNetwork;
            }
            return false;
        }

        public static async Task<bool> IsIPv6Supported()
        {
            return IsIPv6(await GetWANv6Address());
        }
    }
}