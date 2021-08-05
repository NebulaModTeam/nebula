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

            const int MaxMessageSize = 50 * 1024 * 1024; // 50 MB
            GameObject mirrorRoot = new GameObject();
            mirrorRoot.SetActive(false);
            mirrorRoot.name = "Mirror Networking";
            NetworkManager NetworkManager = mirrorRoot.AddComponent(networkManagerType) as NetworkManager;
            NetworkManager.autoCreatePlayer = false;
            NetworkManager.networkAddress = uri.Host;
#if DEBUG
            mirrorRoot.AddComponent(typeof(NetworkManagerHUD));
#endif
            var transports = new List<Transport>();

            // Telepathy
            TelepathyTransport telepathy = mirrorRoot.AddComponent<TelepathyTransport>();
            telepathy.clientMaxMessageSize = MaxMessageSize;
            telepathy.serverMaxMessageSize = MaxMessageSize;
            telepathy.port = (ushort)uri.Port;
            telepathy.SendTimeout = (int)TimeSpan.FromSeconds(30).TotalMilliseconds;
            if (Config.Options.TransportLayer == "telepathy") transports.Add(telepathy);

            // Kcp
            kcp2k.KcpTransport kcp = mirrorRoot.AddComponent<kcp2k.KcpTransport>();
            kcp.debugLog = true;
#if DEBUG
            kcp.statisticsGUI = true;
#endif
            kcp.ReceiveWindowSize = MaxMessageSize;
            kcp.SendWindowSize = MaxMessageSize;
            kcp.Port = (ushort)uri.Port;
            if (Config.Options.TransportLayer == "kcp") transports.Add(kcp);

            // EOS
            if (Config.Options.EOSEnabled && GameObject.Find("Epic Online Services"))
            {
                EpicTransport.EosTransport eosTransport = mirrorRoot.AddComponent<EpicTransport.EosTransport>();
                eosTransport.maxFragments = MaxMessageSize / 1159; // max packet size is 1159 bytes
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
