using Mirror;
using NebulaModel;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace NebulaNetwork
{
    public class MirrorManager
    {
        public static string DefaultScheme { get; protected set; } = "tcp4";
        public static NetworkManager SetupMirror(Type networkManagerType, Uri uri)
        {
            NebulaModel.Logger.Log.Debug($"uri: {uri}");
            NebulaModel.Logger.Log.Debug($"uri original string: {uri.OriginalString}");
            NebulaModel.Logger.Log.Debug($"uri host: {uri.Host}");
            NebulaModel.Logger.Log.Debug($"uri port: {uri.Port}");
            NebulaModel.Logger.Log.Debug($"uri scheme: {uri.Scheme}");

            bool IsHost = networkManagerType == typeof(HostManager);

            int MaxMessageSize = Config.Options.MaxMessageSize * 1024 * 1024; // 50 MB default
            int Timeout = (int)TimeSpan.FromSeconds(Config.Options.Timeout).TotalMilliseconds;
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
            if (!IsHost || Config.Options.TransportLayer == "telepathy")
            {
                TelepathyTransport telepathy = mirrorRoot.AddComponent<TelepathyTransport>();
                telepathy.clientMaxMessageSize = MaxMessageSize;
                telepathy.serverMaxMessageSize = MaxMessageSize;
                telepathy.port = (ushort)uri.Port;
                telepathy.SendTimeout = Timeout;
                telepathy.ReceiveTimeout = Timeout;
                transports.Add(telepathy);
            }

            /*
            // Kcp [SHOULD BE LAST TRANSPORT BEFORE MULTIPLEX]
            if (!IsHost || Config.Options.TransportLayer == "kcp")
            {
                kcp2k.KcpTransport kcp = mirrorRoot.AddComponent<kcp2k.KcpTransport>();
                kcp.debugLog = true;
#if DEBUG
                kcp.statisticsGUI = true;
#endif
                kcp.ReceiveWindowSize = Math.Min(65535, (uint)MaxMessageSize);
                kcp.SendWindowSize = Math.Min(65535, (uint)MaxMessageSize);
                kcp.Port = (ushort)uri.Port;
                kcp.Timeout = Timeout;
                transports.Add(kcp);
            }
            */

            // Multiplex (Multiple transports)
            MultiplexTransport multiplex = mirrorRoot.AddComponent<MultiplexTransport>();
            multiplex.transports = transports.ToArray();

#if DEBUG
            LatencySimulation latencySimulation = mirrorRoot.AddComponent<LatencySimulation>();
            latencySimulation.wrap = multiplex;
            latencySimulation.reliableLatency = Config.Options.ReliableLatency;
            latencySimulation.unreliableLoss = Config.Options.UnreliableLoss;
            latencySimulation.unreliableLatency = Config.Options.UnreliableLatency;
            latencySimulation.unreliableScramble = Config.Options.UnreliableScramble;
#endif

            mirrorRoot.SetActive(true);
#if DEBUG
            if(Config.Options.SimulateLatency)
            {
                Transport.activeTransport = latencySimulation;
            }
            else
            {
                Transport.activeTransport = multiplex;
            }
#else
            Transport.activeTransport = multiplex;
#endif

            return NetworkManager;
        }
    }
}
