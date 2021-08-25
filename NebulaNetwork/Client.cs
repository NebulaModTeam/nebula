using HarmonyLib;
using NebulaModel;
using NebulaModel.DataStructures;
using NebulaModel.Logger;
using NebulaModel.Networking;
using NebulaModel.Packets.Players;
using NebulaModel.Packets.Routers;
using NebulaModel.Packets.Session;
using NebulaModel.Utils;
using NebulaWorld;
using System.Net;
using System.Net.Sockets;
using UnityEngine;
using WebSocketSharp;

namespace NebulaNetwork
{
    public class Client : NetworkProvider
    {
        private const int MECHA_SYNCHONIZATION_INTERVAL = 5;

        private readonly IPEndPoint serverEndpoint;
        private WebSocket clientSocket;
        private NebulaConnection serverConnection;

        private float mechaSynchonizationTimer = 0f;
        private float pingTimer = 0f;

        public Client(string url, int port)
            : this(new IPEndPoint(Dns.GetHostEntry(url).AddressList[0], port))
        {
        }

        public Client(IPEndPoint endpoint) : base(null)
        {
            serverEndpoint = endpoint;

        }

        public override void Start()
        {
            PacketUtils.RegisterAllPacketNestedTypes(PacketProcessor);
            PacketUtils.RegisterAllPacketProcessorsInCallingAssembly(PacketProcessor, false);
#if DEBUG
            PacketProcessor.SimulateLatency = true;
#endif

            clientSocket = new WebSocket($"ws://{serverEndpoint}/socket");
            clientSocket.OnOpen += ClientSocket_OnOpen;
            clientSocket.OnClose += ClientSocket_OnClose;
            clientSocket.OnMessage += ClientSocket_OnMessage;

            clientSocket.Connect();

            Multiplayer.Session.LocalPlayer.IsHost = false;

            if (Config.Options.RememberLastIP)
            {
                // We've successfully connected, set connection as last ip, cutting out "ws://" and "/socket"
                Config.Options.LastIP = serverEndpoint.ToString();
                Config.SaveOptions();
            }
        }

        public override void Stop()
        {
            clientSocket?.Close((ushort)DisconnectionReason.ClientRequestedDisconnect, "Player left the game");
        }

        public override void Dispose()
        {
            Stop();
            Multiplayer.Session.World.HidePingIndicator();
        }

        public override void SendPacket<T>(T packet)
        {
            serverConnection?.SendPacket(packet);
        }

        public override void SendPacketToLocalStar<T>(T packet)
        {
            serverConnection?.SendPacket(new StarBroadcastPacket(PacketProcessor.Write(packet), GameMain.data.localStar?.id ?? -1));
        }

        public override void SendPacketToLocalPlanet<T>(T packet)
        {
            serverConnection?.SendPacket(new PlanetBroadcastPacket(PacketProcessor.Write(packet), GameMain.mainPlayer.planetId));
        }

        public override void SendPacketToPlanet<T>(T packet, int planetId)
        {
            // Only possible from host
            throw new System.NotImplementedException();
        }

        public override void SendPacketToStar<T>(T packet, int starId)
        {
            // Only possible from host
            throw new System.NotImplementedException();
        }

        public override void SendPacketToStarExclude<T>(T packet, int starId, NebulaConnection exclude)
        {
            // Only possible from host
            throw new System.NotImplementedException();
        }

        public override void Update()
        {
            PacketProcessor.ProcessPacketQueue();

            if (Multiplayer.Session.IsGameLoaded)
            {
                mechaSynchonizationTimer += Time.deltaTime;
                if (mechaSynchonizationTimer > MECHA_SYNCHONIZATION_INTERVAL)
                {
                    SendPacket(new PlayerMechaData(GameMain.mainPlayer));
                    mechaSynchonizationTimer = 0f;
                }

                pingTimer += Time.deltaTime;
                if (pingTimer >= 1f)
                {
                    SendPacket(new PingPacket());
                    pingTimer = 0f;
                }
            }
        }

        private void ClientSocket_OnMessage(object sender, MessageEventArgs e)
        {
            if (!Multiplayer.IsLeavingGame)
            {
                PacketProcessor.EnqueuePacketForProcessing(e.RawData, new NebulaConnection(clientSocket, serverEndpoint, PacketProcessor));
            }
        }

        private void ClientSocket_OnOpen(object sender, System.EventArgs e)
        {
            DisableNagleAlgorithm(clientSocket);

            Log.Info($"Server connection established");
            serverConnection = new NebulaConnection(clientSocket, serverEndpoint, PacketProcessor);

            //TODO: Maybe some challenge-response authentication mechanism?

            SendPacket(new HandshakeRequest(
                CryptoUtils.GetPublicKey(CryptoUtils.GetOrCreateUserCert()),
                !string.IsNullOrWhiteSpace(Config.Options.Nickname) ? Config.Options.Nickname : GameMain.data.account.userName,
                new Float3(Config.Options.MechaColorR / 255, Config.Options.MechaColorG / 255, Config.Options.MechaColorB / 255)));
        }

        private void ClientSocket_OnClose(object sender, CloseEventArgs e)
        {
            serverConnection = null;

            UnityDispatchQueue.RunOnMainThread(() =>
            {
                // If the client is Quitting by himself, we don't have to inform him of his disconnection.
                if (e.Code == (ushort)DisconnectionReason.ClientRequestedDisconnect)
                    return;

                if (e.Code == (ushort)DisconnectionReason.ModVersionMismatch)
                {
                    string[] versions = e.Reason.Split(';');
                    InGamePopup.ShowWarning(
                        "Mod Version Mismatch",
                        $"Your Nebula Multiplayer Mod is not the same as the Host version.\nYou:{versions[0]} - Remote:{versions[1]}",
                        "OK",
                        Multiplayer.LeaveGame);
                    return;
                }

                if (e.Code == (ushort)DisconnectionReason.GameVersionMismatch)
                {
                    string[] versions = e.Reason.Split(';');
                    InGamePopup.ShowWarning(
                        "Game Version Mismatch",
                        $"Your version of the game is not the same as the one used by the Host.\nYou:{versions[0]} - Remote:{versions[1]}",
                        "OK",
                        Multiplayer.LeaveGame);
                    return;
                }

                if (Multiplayer.Session.IsGameLoaded)
                {
                    InGamePopup.ShowWarning(
                        "Connection Lost",
                        $"You have been disconnected from the server.\n{e.Reason}",
                        "Quit",
                        Multiplayer.LeaveGame);
                }
                else
                {
                    InGamePopup.ShowWarning(
                        "Server Unavailable",
                        $"Could not reach the server, please try again later.",
                        "OK",
                        Multiplayer.LeaveGame);
                }
            });
        }

        

        static void DisableNagleAlgorithm(WebSocket socket)
        {
            var tcpClient = AccessTools.FieldRefAccess<WebSocket, TcpClient>("_tcpClient")(socket);
            tcpClient.NoDelay = true;
        }
    }
}
