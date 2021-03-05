using LiteNetLib;
using LiteNetLib.Utils;
using NebulaModel.DataStructures;
using NebulaModel.Logger;
using NebulaModel.Networking;
using NebulaModel.Packets.GameStates;
using NebulaModel.Utils;
using NebulaWorld;
using UnityEngine;

namespace NebulaHost
{
    public class MultiplayerHostSession : MonoBehaviour, INetworkProvider
    {
        public static MultiplayerHostSession Instance { get; protected set; }

        private NetManager server;

        public PlayerManager PlayerManager { get; protected set; }
        public NetPacketProcessor PacketProcessor { get; protected set; }

        float gameStateUpdateTimer = 0;

        private void Awake()
        {
            Instance = this;
        }

        public void StartServer(int port)
        {
            EventBasedNetListener listener = new EventBasedNetListener();
            listener.ConnectionRequestEvent += OnConnectionRequest;
            listener.PeerConnectedEvent += OnPeerConnected;
            listener.PeerDisconnectedEvent += OnPeerDisconnected;
            listener.NetworkReceiveEvent += OnNetworkReceive;

            server = new NetManager(listener)
            {
                AutoRecycle = true,
#if DEBUG
                SimulateLatency = true,
                SimulatePacketLoss = true,
                SimulationMinLatency = 50,
                SimulationMaxLatency = 100,
#endif
            };

            PlayerManager = new PlayerManager();
            // TODO: Load saved player info here
            PacketProcessor = new NetPacketProcessor();
            LiteNetLibUtils.RegisterAllPacketNestedTypes(PacketProcessor);
            LiteNetLibUtils.RegisterAllPacketProcessorsInCallingAssembly(PacketProcessor);

            server.Start(port);

            SimulatedWorld.Initialize();

            LocalPlayer.SetNetworkProvider(this);
            LocalPlayer.IsMasterClient = true;
            // TODO: Load local player data
            // TODO: For some reason the local player color does not work even if we set the color to the materials.
            LocalPlayer.SetPlayerData(new PlayerData(PlayerManager.GetNextAvailablePlayerId(), new Float3(Random.value, Random.value, Random.value))); // Default color: new Float3(1.0f, 0.6846404f, 0.243137181f)));
        }

        private void OnConnectionRequest(ConnectionRequest request)
        {
            // TODO: Max player count can be enforced here.
            request.AcceptIfKey("nebula");
        }

        private void StopServer()
        {
            server?.Stop();
        }

        public void DestroySession()
        {
            StopServer();
            Destroy(gameObject);
        }

        public void SendPacket<T>(T packet, DeliveryMethod deliveryMethod = DeliveryMethod.ReliableOrdered) where T : class, new()
        {
            PlayerManager.SendPacketToAllPlayers(packet, deliveryMethod);
        }

        private void OnNetworkReceive(NetPeer peer, NetPacketReader reader, DeliveryMethod deliveryMethod)
        {
            PacketProcessor.ReadAllPackets(reader, new NebulaConnection(peer, PacketProcessor));
        }

        private void OnPeerConnected(NetPeer peer)
        {
            Log.Info($"Client connected ID: {peer.Id}, {peer.EndPoint}");
            NebulaConnection conn = new NebulaConnection(peer, PacketProcessor);
            PlayerManager.PlayerConnected(conn);
        }

        private void OnPeerDisconnected(NetPeer peer, DisconnectInfo disconnectInfo)
        {
            Log.Info($"Client disconnected: {peer.EndPoint}, reason: {disconnectInfo.Reason}");
            PlayerManager.PlayerDisconnected(new NebulaConnection(peer, PacketProcessor));
        }

        private void Update()
        {
            server?.PollEvents();

            gameStateUpdateTimer += Time.deltaTime;
            if (gameStateUpdateTimer > 1)
            {
                SendPacket(new GameStateUpdate() { State = new GameState(GameMain.gameTick) });
            }
        }
    }
}
