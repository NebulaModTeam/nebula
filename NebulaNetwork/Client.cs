﻿using HarmonyLib;
using NebulaAPI;
using NebulaModel;
using NebulaModel.Logger;
using NebulaModel.Networking;
using NebulaModel.Packets.Players;
using NebulaModel.Packets.Routers;
using NebulaModel.Packets.Session;
using NebulaModel.Utils;
using NebulaWorld;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
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
            foreach (Assembly assembly in AssembliesUtils.GetNebulaAssemblies())
            {
                PacketUtils.RegisterAllPacketNestedTypesInAssembly(assembly, PacketProcessor);
            }
            PacketUtils.RegisterAllPacketProcessorsInCallingAssembly(PacketProcessor, false);

            foreach (Assembly assembly in NebulaModAPI.TargetAssemblies)
            {
                PacketUtils.RegisterAllPacketNestedTypesInAssembly(assembly, PacketProcessor);
                PacketUtils.RegisterAllPacketProcessorsInAssembly(assembly, PacketProcessor, false);
            }
#if DEBUG
            PacketProcessor.SimulateLatency = true;
#endif

            clientSocket = new WebSocket($"ws://{serverEndpoint}/socket");
            clientSocket.OnOpen += ClientSocket_OnOpen;
            clientSocket.OnClose += ClientSocket_OnClose;
            clientSocket.OnMessage += ClientSocket_OnMessage;

            clientSocket.Connect();

            ((LocalPlayer)Multiplayer.Session.LocalPlayer).IsHost = false;

            if (Config.Options.RememberLastIP)
            {
                // We've successfully connected, set connection as last ip, cutting out "ws://" and "/socket"
                Config.Options.LastIP = serverEndpoint.ToString();
                Config.SaveOptions();
            }

            NebulaModAPI.OnMultiplayerGameStarted?.Invoke();
        }

        public override void Stop()
        {
            clientSocket?.Close((ushort)DisconnectionReason.ClientRequestedDisconnect, "Player left the game");

            NebulaModAPI.OnMultiplayerGameEnded?.Invoke();
        }

        public override void Dispose()
        {
            Stop();
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

        public override void SendPacketToStarExclude<T>(T packet, int starId, INebulaConnection exclude)
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
            var lobbyRequest = new LobbyRequest(
                CryptoUtils.GetPublicKey(CryptoUtils.GetOrCreateUserCert()),
                !string.IsNullOrWhiteSpace(Config.Options.Nickname) ? Config.Options.Nickname : GameMain.data.account.userName,
                Config.Options.GetMechaAppearance());

            SendPacket(lobbyRequest);
        }

        private void ClientSocket_OnClose(object sender, CloseEventArgs e)
        {
            serverConnection = null;

            UnityDispatchQueue.RunOnMainThread(() =>
            {
                // If the client is Quitting by himself, we don't have to inform him of his disconnection.
                if (e.Code == (ushort)DisconnectionReason.ClientRequestedDisconnect)
                {
                    return;
                }

                // Opens the pause menu on disconnection to prevent NRE when leaving the game
                if (Multiplayer.Session?.IsGameLoaded ?? false)
                {
                    GameMain.instance._paused = true;
                }

                if (e.Code == (ushort)DisconnectionReason.ModIsMissing)
                {
                    InGamePopup.ShowWarning(
                        "Mod Mismatch",
                        $"You are missing mod {e.Reason}",
                        "OK".Translate(),
                        Multiplayer.LeaveGame);
                    return;
                }

                if (e.Code == (ushort)DisconnectionReason.ModIsMissingOnServer)
                {
                    InGamePopup.ShowWarning(
                        "Mod Mismatch",
                        $"Server is missing mod {e.Reason}",
                        "OK".Translate(),
                        Multiplayer.LeaveGame);
                    return;
                }

                if (e.Code == (ushort)DisconnectionReason.ModVersionMismatch)
                {
                    string[] versions = e.Reason.Split(';');
                    InGamePopup.ShowWarning(
                        "Mod Version Mismatch",
                        $"Your mod {versions[0]} version is not the same as the Host version.\nYou:{versions[1]} - Remote:{versions[2]}",
                        "OK".Translate(),
                        Multiplayer.LeaveGame);
                    return;
                }

                if (e.Code == (ushort)DisconnectionReason.GameVersionMismatch)
                {
                    string[] versions = e.Reason.Split(';');
                    InGamePopup.ShowWarning(
                        "Game Version Mismatch",
                        $"Your version of the game is not the same as the one used by the Host.\nYou:{versions[0]} - Remote:{versions[1]}",
                        "OK".Translate(),
                        Multiplayer.LeaveGame);
                    return;
                }

                if (Multiplayer.Session.IsGameLoaded || Multiplayer.Session.IsInLobby)
                {
                    InGamePopup.ShowWarning(
                        "Connection Lost",
                        $"You have been disconnected from the server.\n{e.Reason}",
                        "Quit",
                        Multiplayer.LeaveGame);
                    if (Multiplayer.Session.IsInLobby)
                    {
                        Multiplayer.ShouldReturnToJoinMenu = false;
                        Multiplayer.Session.IsInLobby = false;
                        UIRoot.instance.galaxySelect.CancelSelect();
                    }
                }
                else
                {
                    InGamePopup.ShowWarning(
                        "Server Unavailable",
                        $"Could not reach the server, please try again later.",
                        "OK".Translate(),
                        Multiplayer.LeaveGame);
                }
            });
        }

        private static void DisableNagleAlgorithm(WebSocket socket)
        {
            TcpClient tcpClient = AccessTools.FieldRefAccess<WebSocket, TcpClient>("_tcpClient")(socket);
            if (tcpClient != null)
            {
                tcpClient.NoDelay = true;
            }
        }
    }
}
