using BepInEx;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace NebulaModel.Utils
{
    public static class IPUtils
    {
        public static IPAddress GetLocalAddress()
        {
            using (Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, 0))
            {
                socket.Connect("8.8.8.8", 65530);
                IPEndPoint endPoint = socket.LocalEndPoint as IPEndPoint;
                return endPoint.Address;
            }
        }

        public static async Task GetPortStatus(int port, Action<string, string> resultCallback)
        {
            try
            {
                string response = await GetAsync($"https://ifconfig.co/port/{port}");
                Dictionary<string, object> jObject = MiniJson.Deserialize(response) as Dictionary<string, object>;
                string ip = (string) jObject["ip"]; 
                bool reachable = (bool) jObject["reachable"]; 
                ThreadingHelper.Instance.StartSyncInvoke(() =>
                {
                    resultCallback.Invoke(ip, reachable ? "Open" : "Closed");
                });
            }
            catch (Exception e)
            {
                ThreadingHelper.Instance.StartSyncInvoke(() =>
                {
                    resultCallback.Invoke($"Unknown ({e.Message})", $"Unknown ({e.Message})");
                });
            }
        }
        
        public static async Task<string> GetAsync(string uri)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);
            request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;

            using (HttpWebResponse response = (HttpWebResponse)await request.GetResponseAsync())
            {
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    using (Stream stream = response.GetResponseStream())
                    using (StreamReader reader = new StreamReader(stream))
                    {
                        return await reader.ReadToEndAsync();
                    }
                }

                throw new Exception("Can't connect to port checker service!");
            }
        }
    }
}