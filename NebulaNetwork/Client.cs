using HarmonyLib;
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
using System.Reflection;
using UnityEngine;
using Valve.Sockets;

namespace NebulaNetwork
{
    public class Client : NetworkProvider
    {
        private const int MECHA_SYNCHONIZATION_INTERVAL = 5;

        private readonly IPEndPoint serverEndpoint;
        private Address serverAddress;
        private uint connection;
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

            serverAddress = new Address();
            serverAddress.SetAddress(serverEndpoint.Address.ToString(), (ushort)serverEndpoint.Port);

            connection = Sockets.Connect(ref serverAddress);


            ((LocalPlayer)Multiplayer.Session.LocalPlayer).IsHost = false;

            if (Config.Options.RememberLastIP)
            {
                // We've successfully connected, set connection as last ip, cutting out "ws://" and "/socket"
                Config.Options.LastIP = serverEndpoint.ToString();
                Config.SaveOptions();
            }

            NebulaModAPI.OnMultiplayerGameStarted?.Invoke();
        }

        protected override void OnEvent(ref StatusInfo info)
        {
            switch (info.connectionInfo.state)
            {
                case ConnectionState.None:
                    break;

                case ConnectionState.Connected:
                    OnOpen(ref info);
                    Log.Info("Client connected to server - ID: " + connection);
                    break;

                case ConnectionState.ClosedByPeer:
                case ConnectionState.ProblemDetectedLocally:
                    Sockets.CloseConnection(info.connection);
                    if (info.connection == connection)
                        OnClose(ref info);
                    break;
            }
        }

        public override void Stop()
        {
            Sockets?.CloseConnection(connection, (int)DisconnectionReason.ClientRequestedDisconnect, "Player left the game", true);

            NebulaModAPI.OnMultiplayerGameEnded?.Invoke();

            Provider = null;
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
            Sockets?.Poll(0);
            Sockets?.RunCallbacks();

            NetworkingMessage[] messages = new NetworkingMessage[100];

            var count = Sockets?.ReceiveMessagesOnConnection(connection, messages, 100);
            for (int i = 0; i < count.GetValueOrDefault(0); ++i)
            {
                OnMessage(messages[i]);
            }

            serverConnection?.Update();

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
                    ConnectionStatus status = new ConnectionStatus();
                    Sockets.GetQuickConnectionStatus(connection, ref status);

                    Multiplayer.Session.World.UpdatePingIndicator($"Ping: {status.ping}ms");
                    pingTimer = 0f;
                }
            }
        }

        private void OnMessage(NetworkingMessage message)
        {
            if (!Multiplayer.IsLeavingGame)
            {
                byte[] rawData = new byte[message.length];
                message.CopyTo(rawData);

                var data = serverConnection.Receive(rawData);
                if (data != null)
                {
                    PacketProcessor.EnqueuePacketForProcessing(data, serverConnection);
                }
            }
        }

        private void OnOpen(ref StatusInfo info)
        {
            Log.Info($"Server connection established");
            serverConnection = new NebulaConnection(Sockets, connection, serverEndpoint, PacketProcessor);

            //TODO: Maybe some challenge-response authentication mechanism?

            SendPacket(new HandshakeRequest(
                CryptoUtils.GetPublicKey(CryptoUtils.GetOrCreateUserCert()),
                !string.IsNullOrWhiteSpace(Config.Options.Nickname) ? Config.Options.Nickname : GameMain.data.account.userName,
                Config.Options.GetMechaColors()));
        }

        private void OnClose(ref StatusInfo info)
        {
            serverConnection = null;

            var endReason = info.connectionInfo.endReason;
            var endDebug = info.connectionInfo.endDebug;
            var connection = info.connection;

            UnityDispatchQueue.RunOnMainThread(() =>
            {
                // If the client is Quitting by himself, we don't have to inform him of his disconnection.
                if (endReason == (int)DisconnectionReason.ClientRequestedDisconnect)
                {
                    return;
                }

                // Opens the pause menu on disconnection to prevent NRE when leaving the game
                if (Multiplayer.Session?.IsGameLoaded ?? false)
                {
                    GameMain.instance._paused = true;
                }

                if (endReason == (int)DisconnectionReason.ModIsMissing)
                {
                    InGamePopup.ShowWarning(
                        "Mod Mismatch",
                        $"You are missing mod {endDebug}",
                        "OK".Translate(),
                        Multiplayer.LeaveGame);
                    return;
                }

                if (endReason == (int)DisconnectionReason.ModIsMissingOnServer)
                {
                    InGamePopup.ShowWarning(
                        "Mod Mismatch",
                        $"Server is missing mod {endDebug}",
                        "OK".Translate(),
                        Multiplayer.LeaveGame);
                    return;
                }

                if (endReason == (int)DisconnectionReason.ModVersionMismatch)
                {
                    string[] versions = endDebug.Split(';');
                    InGamePopup.ShowWarning(
                        "Mod Version Mismatch",
                        $"Your mod {versions[0]} version is not the same as the Host version.\nYou:{versions[1]} - Remote:{versions[2]}",
                        "OK".Translate(),
                        Multiplayer.LeaveGame);
                    return;
                }

                if (endReason == (int)DisconnectionReason.GameVersionMismatch)
                {
                    string[] versions = endDebug.Split(';');
                    InGamePopup.ShowWarning(
                        "Game Version Mismatch",
                        $"Your version of the game is not the same as the one used by the Host.\nYou:{versions[0]} - Remote:{versions[1]}",
                        "OK".Translate(),
                        Multiplayer.LeaveGame);
                    return;
                }

                if (Multiplayer.Session.IsGameLoaded)
                {
                    InGamePopup.ShowWarning(
                        "Connection Lost",
                        $"You have been disconnected from the server.\n{endDebug}",
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
