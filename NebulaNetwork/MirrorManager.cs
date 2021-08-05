using Mirror;
using NebulaModel;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace NebulaNetwork
{
    public class MirrorManager
    {
        public static string DefaultScheme = "tcp4";
        public static NetworkManager SetupMirror(Type networkManagerType, Uri uri)
        {
            NebulaModel.Logger.Log.Debug($"uri: {uri}");
            NebulaModel.Logger.Log.Debug($"uri original string: {uri.OriginalString}");
            NebulaModel.Logger.Log.Debug($"uri host: {uri.Host}");
            NebulaModel.Logger.Log.Debug($"uri port: {uri.Port}");
            NebulaModel.Logger.Log.Debug($"uri scheme: {uri.Scheme}");

            int MaxMessageSize = Config.Options.MaxMessageSize * 1024 * 1024; // 50 MB default
            int Timeout = (int)TimeSpan.FromSeconds(Config.Options.Timeout).TotalMilliseconds;
            GameObject mirrorRoot = new GameObject();
            mirrorRoot.SetActive(false);
            mirrorRoot.name = "Mirror Networking";
            NetworkManager NetworkManager = mirrorRoot.AddComponent(networkManagerType) as NetworkManager;
            NetworkManager.autoCreatePlayer = false;
            NetworkManager.networkAddress = uri.Host;
            NetworkManager.serverTickRate = Config.Options.TickRate; // 30 Hz default
#if DEBUG
            mirrorRoot.AddComponent(typeof(NetworkManagerHUD));
#endif
            var transports = new List<Transport>();

            // Telepathy
            TelepathyTransport telepathy = mirrorRoot.AddComponent<TelepathyTransport>();
            telepathy.clientMaxMessageSize = MaxMessageSize;
            telepathy.serverMaxMessageSize = MaxMessageSize;
            telepathy.port = (ushort)uri.Port;
            telepathy.SendTimeout = Timeout;
            telepathy.ReceiveTimeout = Timeout;
            if (Config.Options.TransportLayer == "telepathy") transports.Add(telepathy);

            // Kcp
            kcp2k.KcpTransport kcp = mirrorRoot.AddComponent<kcp2k.KcpTransport>();
            kcp.debugLog = true;
#if DEBUG
            kcp.statisticsGUI = true;
#endif
            kcp.ReceiveWindowSize = (uint)MaxMessageSize;
            kcp.SendWindowSize = (uint)MaxMessageSize;
            kcp.Port = (ushort)uri.Port;
            kcp.Timeout = Timeout;
            if (Config.Options.TransportLayer == "kcp") transports.Add(kcp);

            // EOS
            if (Config.Options.EOSEnabled && GameObject.Find("Epic Online Services"))
            {
                EpicTransport.EosTransport eosTransport = mirrorRoot.AddComponent<EpicTransport.EosTransport>();
                eosTransport.maxFragments = MaxMessageSize / 1159; // max packet size is 1159 bytes
                eosTransport.timeout = Config.Options.Timeout;
                transports.Add(eosTransport);
            }


            // Multiplex (Multiple transports)
            MultiplexTransport multiplex = mirrorRoot.AddComponent<MultiplexTransport>();
            multiplex.transports = transports.ToArray();

            mirrorRoot.SetActive(true);
            Transport.activeTransport = multiplex;

            return NetworkManager;
        }
    }
}
