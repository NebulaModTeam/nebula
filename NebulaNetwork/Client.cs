﻿using NebulaAPI;
using NebulaModel;
using NebulaModel.Logger;
using NebulaModel.Networking;
using NebulaModel.Packets.Players;
using NebulaModel.Packets.Routers;
using NebulaModel.Packets.Session;
using NebulaModel.Utils;
using NebulaWorld;
using System;
using System.Net;
using System.Reflection;
using UnityEngine;

namespace NebulaNetwork
{
    public class Client : NetworkProvider
    {
        private Telepathy.Client client;
        private const int MECHA_SYNCHONIZATION_INTERVAL = 5;

        private readonly IPEndPoint serverEndpoint;
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

            client = new Telepathy.Client(50 * 1024 * 1024)
            {
                OnConnected = OnConnected,
                OnData = OnMessage,
                OnDisconnected = OnDisconnected,
                ReceiveTimeout = 30000,
                SendTimeout = 30000
            };

            client.Connect(serverEndpoint.Address.ToString(), serverEndpoint.Port);

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
            //clientSocket?.Close((ushort)DisconnectionReason.ClientRequestedDisconnect, "Player left the game");
            client?.Disconnect();

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
            client.Tick(1000);

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
        private void OnConnected()
        {
            Log.Info($"Server connection established");
            serverConnection = new NebulaConnection(client, null, PacketProcessor);

            //TODO: Maybe some challenge-response authentication mechanism?

            SendPacket(new HandshakeRequest(
                CryptoUtils.GetPublicKey(CryptoUtils.GetOrCreateUserCert()),
                !string.IsNullOrWhiteSpace(Config.Options.Nickname) ? Config.Options.Nickname : GameMain.data.account.userName,
                Config.Options.GetMechaColors()));
        }

        private void OnMessage(ArraySegment<byte> obj)
        {
            if (!Multiplayer.IsLeavingGame)
            {
                PacketProcessor.ProcessPacket(obj.Array, new NebulaConnection(client, null, PacketProcessor));
            }
        }

        private void OnDisconnected()
        {
            serverConnection = null;

            UnityDispatchQueue.RunOnMainThread(() =>
            {
                //// If the client is Quitting by himself, we don't have to inform him of his disconnection.
                //if (e.Code == (ushort)DisconnectionReason.ClientRequestedDisconnect)
                //{
                //    return;
                //}

                // Opens the pause menu on disconnection to prevent NRE when leaving the game
                if (Multiplayer.Session?.IsGameLoaded ?? false)
                {
                    GameMain.instance._paused = true;
                }

                //if (e.Code == (ushort)DisconnectionReason.ModVersionMismatch)
                //{
                //    string[] versions = e.Reason.Split(';');
                //    InGamePopup.ShowWarning(
                //        "Mod Version Mismatch",
                //        $"Your Nebula Multiplayer Mod is not the same as the Host version.\nYou:{versions[0]} - Remote:{versions[1]}",
                //        "OK",
                //        Multiplayer.LeaveGame);
                //    return;
                //}

                //if (e.Code == (ushort)DisconnectionReason.GameVersionMismatch)
                //{
                //    string[] versions = e.Reason.Split(';');
                //    InGamePopup.ShowWarning(
                //        "Game Version Mismatch",
                //        $"Your version of the game is not the same as the one used by the Host.\nYou:{versions[0]} - Remote:{versions[1]}",
                //        "OK",
                //        Multiplayer.LeaveGame);
                //    return;
                //}

                if (Multiplayer.Session.IsGameLoaded)
                {
                    InGamePopup.ShowWarning(
                        "Connection Lost",
                        $"You have been disconnected from the server.\n",
                        "Quit",
                        Multiplayer.LeaveGame);
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
    }
}
