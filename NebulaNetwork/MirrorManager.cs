using Mirror;
using NebulaModel;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace NebulaNetwork
{
    public class MirrorManager
    {
        public static string DefaultScheme { get; protected set; } = "enet";
        public static NetworkManager SetupMirror(Type networkManagerType, Uri uri)
        {
            NebulaModel.Logger.Log.Debug($"uri: {uri}");
            NebulaModel.Logger.Log.Debug($"uri original string: {uri.OriginalString}");
            NebulaModel.Logger.Log.Debug($"uri host: {uri.Host}");
            NebulaModel.Logger.Log.Debug($"uri port: {uri.Port}");
            NebulaModel.Logger.Log.Debug($"uri scheme: {uri.Scheme}");

            bool IsHost = networkManagerType == typeof(HostManager);

            int multiplier = 1;
            char endChar = Config.Options.MaxMessageSize[Config.Options.MaxMessageSize.Length - 1];
            switch (endChar)
            {
                case 'G':
                    multiplier = 1024 * 1024 * 1024;
                    break;
                case 'M':
                    multiplier = 1024 * 1024;
                    break;
                case 'K':
                    multiplier = 1024;
                    break;
                case 'B':
                default:
                    multiplier = 1;
                    break;
            }
            int MaxMessageSize = 50 * 1024 * 1024;
            if (int.TryParse(Config.Options.MaxMessageSize.TrimEnd(endChar), out int result))
            {
                MaxMessageSize = result * multiplier;
            }

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
                telepathy.clientReceiveQueueLimit = Config.Options.QueueLimit;
                telepathy.clientSendQueueLimit = Config.Options.QueueLimit;
                telepathy.serverReceiveQueueLimitPerConnection = Config.Options.QueueLimit;
                telepathy.serverSendQueueLimitPerConnection = Config.Options.QueueLimit;
                transports.Add(telepathy);
            }

            // Ignorance
            if (!IsHost || Config.Options.TransportLayer == "ignorance")
            {
                IgnoranceTransport.Ignorance ignorance = mirrorRoot.AddComponent<IgnoranceTransport.Ignorance>();
                ignorance.port = uri.Port;
                ignorance.LogType = IgnoranceTransport.IgnoranceLogType.Verbose;
                ignorance.DebugDisplay = true;
                ignorance.Channels = new IgnoranceTransport.IgnoranceChannelTypes[]
                {
                    IgnoranceTransport.IgnoranceChannelTypes.Reliable,
                    IgnoranceTransport.IgnoranceChannelTypes.Unreliable
                };
                ignorance.MaxAllowedPacketSize = MaxMessageSize;
                ignorance.clientMaxNativeWaitTime = 1;
                ignorance.PacketBufferCapacity = 32 * 1024 * 1024;
                transports.Add(ignorance);
            }

            // SimpleWeb (Websocket)
            if (!IsHost || Config.Options.TransportLayer == "SimpleWeb")
            {
                Mirror.SimpleWeb.SimpleWebTransport simpleWebTransport = mirrorRoot.AddComponent<Mirror.SimpleWeb.SimpleWebTransport>();
                simpleWebTransport.port = (ushort)uri.Port;
                simpleWebTransport.maxMessageSize = MaxMessageSize;
                simpleWebTransport.receiveTimeout = Timeout;
                simpleWebTransport.sendTimeout = Timeout;
                transports.Add(simpleWebTransport);
            }

            // Kcp [SHOULD BE LAST TRANSPORT BEFORE MULTIPLEX]
            if (!IsHost || Config.Options.TransportLayer == "kcp")
            {
                kcp2k.KcpTransport kcp = mirrorRoot.AddComponent<kcp2k.KcpTransport>();
                kcp.debugLog = true;
#if DEBUG
                kcp.statisticsGUI = true;
#endif
                kcp.ReceiveWindowSize = (uint)MaxMessageSize;
                kcp.SendWindowSize = (uint)MaxMessageSize;
                kcp.Port = (ushort)uri.Port;
                kcp.Timeout = Timeout;
                transports.Add(kcp);
            }

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
            if (Config.Options.SimulateLatency)
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
