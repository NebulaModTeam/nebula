using HarmonyLib;
using NebulaModel;
using NebulaModel.Networking;
using NebulaModel.Networking.Serialization;
using NebulaModel.Utils;
using NebulaWorld;
using System.Net.Sockets;
using WebSocketSharp;
using WebSocketSharp.Server;

namespace NebulaNetwork
{
    public class Server : NetworkProvider
    {
        private WebSocketServer socket;

        public PlayerManager PlayerManager { get; protected set; }

        private readonly int port;
        private readonly bool loadSaveFile;

        public Server(int port, bool loadSaveFile = false)
        {
            this.port = port;
            this.loadSaveFile = loadSaveFile;

            PacketUtils.RegisterAllPacketNestedTypes(PacketProcessor);
            PacketUtils.RegisterAllPacketProcessorsInCallingAssembly(PacketProcessor, true);
#if DEBUG
            PacketProcessor.SimulateLatency = true;
#endif
        }

        public override void Start()
        {
            socket = new WebSocketServer(System.Net.IPAddress.IPv6Any, port);
            DisableNagleAlgorithm(socket);
            socket.AddWebSocketService("/socket", () => new WebSocketService(PlayerManager, PacketProcessor));
        }

        public override void Stop()
        {
            socket?.Stop();
        }

        public override void Dispose()
        {
            Stop();
        }

        public override void SendPacket<T>(T packet)
        {
            PlayerManager.SendPacketToAllPlayers(packet);
        }

        public override void SendPacketToLocalStar<T>(T packet)
        {
            PlayerManager.SendPacketToLocalStar(packet);
        }

        public override void SendPacketToLocalPlanet<T>(T packet)
        {
            PlayerManager.SendPacketToLocalPlanet(packet);
        }

        public override void SendPacketToPlanet<T>(T packet, int planetId)
        {
            PlayerManager.SendPacketToPlanet(packet, planetId);
        }

        public override void SendPacketToStar<T>(T packet, int starId)
        {
            PlayerManager.SendPacketToStar(packet, starId);
        }

        public override void SendPacketToStarExclude<T>(T packet, int starId, NebulaConnection exclude)
        {
            PlayerManager.SendPacketToStarExcept(packet, starId, exclude);
        }

        void DisableNagleAlgorithm(WebSocketServer socketServer)
        {
            TcpListener listener = AccessTools.FieldRefAccess<WebSocketServer, TcpListener>("_listener")(socketServer);
            listener.Server.NoDelay = true;
        }

        private class WebSocketService : WebSocketBehavior
        {
            private readonly PlayerManager playerManager;
            private readonly NetPacketProcessor packetProcessor;

            public WebSocketService(PlayerManager playerManager, NetPacketProcessor packetProcessor)
            {
                this.playerManager = playerManager;
                this.packetProcessor = packetProcessor;
            }

            protected override void OnOpen()
            {
                if (Multiplayer.Session.IsGameLoaded == false)
                {
                    // Reject any connection that occurs while the host's game is loading.
                    Context.WebSocket.Close((ushort)DisconnectionReason.HostStillLoading, "Host still loading, please try again later.");
                    return;
                }

                NebulaModel.Logger.Log.Info($"Client connected ID: {ID}");
                NebulaConnection conn = new NebulaConnection(Context.WebSocket, Context.UserEndPoint, packetProcessor);
                playerManager.PlayerConnected(conn);
            }

            protected override void OnMessage(MessageEventArgs e)
            {
                packetProcessor.EnqueuePacketForProcessing(e.RawData, new NebulaConnection(Context.WebSocket, Context.UserEndPoint, packetProcessor));
            }

            protected override void OnClose(CloseEventArgs e)
            {
                // If the reason of a client disconnect is because we are still loading the game,
                // we don't need to inform the other clients since the disconnected client never
                // joined the game in the first place.
                if (e.Code == (short)DisconnectionReason.HostStillLoading)
                    return;

                NebulaModel.Logger.Log.Info($"Client disconnected: {ID}, reason: {e.Reason}");
                UnityDispatchQueue.RunOnMainThread(() =>
                {
                    playerManager.PlayerDisconnected(new NebulaConnection(Context.WebSocket, Context.UserEndPoint, packetProcessor));
                });
            }

            protected override void OnError(ErrorEventArgs e)
            {
                // TODO: Decide what to do here - does OnClose get called too?
            }
        }
    }
}
