using NebulaModel.Logger;
using NebulaModel.Networking;
using NebulaModel.Networking.Serialization;
using NebulaModel.Packets.Players;
using NebulaModel.Packets.Routers;
using NebulaModel.Packets.Session;
using NebulaModel.Utils;
using NebulaWorld;
using System;
using System.Net;
using UnityEngine;
using WebSocketSharp;

namespace NebulaClient
{
    public class MultiplayerClientSession : MonoBehaviour, INetworkProvider
    {
        public static MultiplayerClientSession Instance { get; protected set; }

        private WebSocket clientSocket;
        private IPEndPoint serverEndpoint;
        private NebulaConnection serverConnection;
        private float mechaSynchonizationTimer = 0f;

        public NetPacketProcessor PacketProcessor { get; protected set; }
        public bool IsConnected { get; protected set; }

        private const int MECHA_SYNCHONIZATION_INTERVAL = 5;

        private string serverIp;
        private int serverPort;

        private void Awake()
        {
            Instance = this;
        }

        public void Connect(string ip, int port)
        {
            serverIp = ip;
            serverPort = port;
            serverEndpoint = new IPEndPoint(IPAddress.Parse(serverIp), port);

            clientSocket = new WebSocket($"ws://{ip}:{port}/socket");
            clientSocket.OnOpen += ClientSocket_OnOpen;
            clientSocket.OnClose += ClientSocket_OnClose;
            clientSocket.OnMessage += ClientSocket_OnMessage;

            PacketProcessor = new NetPacketProcessor();
#if DEBUG
            PacketProcessor.SimulateLatency = true;
#endif

            PacketUtils.RegisterAllPacketNestedTypes(PacketProcessor);
            PacketUtils.RegisterAllPacketProcessorsInCallingAssembly(PacketProcessor);

            clientSocket.Connect();

            SimulatedWorld.Initialize();

            LocalPlayer.IsMasterClient = false;
            LocalPlayer.SetNetworkProvider(this);
        }

        void Disconnect()
        {
            IsConnected = false;
            clientSocket.Close((ushort)DisconnectionReason.ClientRequestedDisconnect, "Player left the game");
        }

        public void DestroySession()
        {
            Disconnect();
            Destroy(gameObject);
        }

        public void SendPacket<T>(T packet) where T : class, new()
        {
            serverConnection?.SendPacket(packet);
        }
        public void SendPacketToLocalStar<T>(T packet) where T : class, new()
        {
            serverConnection?.SendPacket(new StarBroadcastPacket(PacketProcessor.Write(packet), GameMain.data.localStar?.id ?? -1));
        }
        public void SendPacketToLocalPlanet<T>(T packet) where T : class, new()
        {
            serverConnection?.SendPacket(new PlanetBroadcastPacket(PacketProcessor.Write(packet), GameMain.mainPlayer.planetId));
        }
        public void SendPacketToPlanet<T>(T packet, int planetId) where T : class, new()
        {
            //Should send packet to particular planet
            //Not needed at the moment, used only on the host side
            throw new NotImplementedException();
        }

        public void Reconnect()
        {
            SimulatedWorld.Clear();
            Disconnect();
            Connect(serverIp, serverPort);
        }

        private void ClientSocket_OnMessage(object sender, MessageEventArgs e)
        {
            PacketProcessor.EnqueuePacketForProcessing(e.RawData, new NebulaConnection(clientSocket, serverEndpoint, PacketProcessor));
        }

        private void ClientSocket_OnOpen(object sender, System.EventArgs e)
        {
            Log.Info($"Server connection established: {clientSocket.Url}");
            serverConnection = new NebulaConnection(clientSocket, serverEndpoint, PacketProcessor);
            IsConnected = true;
            //TODO: Maybe some challenge-response authentication mechanism?
            SendPacket(new HandshakeRequest(CryptoUtils.GetPublicKey(CryptoUtils.GetOrCreateUserCert()), AccountData.me.userName));
        }

        private void ClientSocket_OnClose(object sender, CloseEventArgs e)
        {
            IsConnected = false;
            serverConnection = null;

            UnityDispatchQueue.RunOnMainThread(() =>
            {
                // If the client is Quitting by himself, we don't have to inform him of his disconnection.
                if (e.Code == (ushort)DisconnectionReason.ClientRequestedDisconnect)
                    return;

                if (e.Code == (ushort)DisconnectionReason.ModVersionMismatch)
                {
                    InGamePopup.ShowWarning(
                        "Mod Version Mismatch",
                        $"Your Nebula Multiplayer Mod is not the same as the Host version.\nMake sure to use the same version.",
                        "OK",
                        OnDisconnectPopupCloseBeforeGameLoad);
                    return;
                }

                if (e.Code == (ushort)DisconnectionReason.GameVersionMismatch)
                {
                    InGamePopup.ShowWarning(
                        "Game Version Mismatch",
                        $"Your version of the game is not the same as the one used by the Host.\nMake sure to use the same version.",
                        "OK",
                        OnDisconnectPopupCloseBeforeGameLoad);
                    return;
                }

                if (SimulatedWorld.IsGameLoaded)
                {
                    InGamePopup.ShowWarning(
                        "Connection Lost",
                        $"You have been disconnect of the server.\n{e.Reason}",
                        "Quit", "Reconnect",
                        () => { LocalPlayer.LeaveGame(); },
                        () => { Reconnect(); });
                }
                else
                {
                    InGamePopup.ShowWarning(
                        "Server Unavailable",
                        $"Could not reach the server, please try again later.",
                        "OK",
                        OnDisconnectPopupCloseBeforeGameLoad);
                }
            });
        }

        private void OnDisconnectPopupCloseBeforeGameLoad()
        {
            GameObject overlayCanvasGo = GameObject.Find("Overlay Canvas");
            Transform multiplayerMenu = overlayCanvasGo?.transform?.Find("Nebula - Multiplayer Menu");
            multiplayerMenu?.gameObject?.SetActive(true);
        }

        private void Update()
        {
            PacketProcessor.ProcessPacketQueue();

            if (SimulatedWorld.IsGameLoaded)
            {
                mechaSynchonizationTimer += Time.deltaTime;
                if (mechaSynchonizationTimer > MECHA_SYNCHONIZATION_INTERVAL)
                {
                    SendPacket(new PlayerMechaData(GameMain.mainPlayer));
                    mechaSynchonizationTimer = 0f;
                }
            }
        }
    }
}
