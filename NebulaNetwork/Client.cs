#region

using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using HarmonyLib;
using NebulaAPI;
using NebulaAPI.GameState;
using NebulaAPI.Networking;
using NebulaAPI.Packets;
using NebulaModel;
using NebulaModel.Logger;
using NebulaModel.Networking;
using NebulaModel.Networking.Serialization;
using NebulaModel.Packets.GameStates;
using NebulaModel.Packets.Players;
using NebulaModel.Packets.Routers;
using NebulaModel.Packets.Session;
using NebulaModel.Utils;
using NebulaWorld;
using NebulaWorld.GameStates;
using UnityEngine;
using UnityEngine.UI;
using WebSocketSharp;

#endregion

namespace NebulaNetwork;

public class Client : IClient
{
    public INetPacketProcessor PacketProcessor { get; set; } = new NebulaNetPacketProcessor();

    private const float FRAGEMENT_UPDATE_INTERVAL = 0.1f;
    private const float GAME_STATE_UPDATE_INTERVAL = 1f;
    private const float MECHA_SYNCHONIZATION_INTERVAL = 30f;

    private readonly AccessTools.FieldRef<WebSocket, MemoryStream> fragmentsBufferRef =
        AccessTools.FieldRefAccess<WebSocket, MemoryStream>("_fragmentsBuffer");

    private readonly string serverPassword;
    private readonly string serverProtocol;

    private WebSocket clientSocket;

    private float fragmentUpdateTimer;
    private float gameStateUpdateTimer;
    private float mechaSynchonizationTimer;
    private NebulaConnection serverConnection;
    private bool websocketAuthenticationFailure;

    public Client(string url, int port, string protocol, string password = "")
        : this(new IPEndPoint(Dns.GetHostEntry(url).AddressList[0], port), protocol, password)
    {
    }

    public Client(IPEndPoint endpoint, string protocol = "", string password = "")
    {
        ServerEndpoint = endpoint;
        serverPassword = password;
        if (protocol != "")
        {
            serverProtocol = protocol;
        }
    }

    public IPEndPoint ServerEndpoint { get; set; }

    public void Start()
    {
        foreach (var assembly in AssembliesUtils.GetNebulaAssemblies())
        {
            PacketUtils.RegisterAllPacketNestedTypesInAssembly(assembly, PacketProcessor as NebulaNetPacketProcessor);
        }

        PacketUtils.RegisterAllPacketProcessorsInCallingAssembly(PacketProcessor as NebulaNetPacketProcessor, false);

        foreach (var assembly in NebulaModAPI.TargetAssemblies)
        {
            PacketUtils.RegisterAllPacketNestedTypesInAssembly(assembly, PacketProcessor as NebulaNetPacketProcessor);
            PacketUtils.RegisterAllPacketProcessorsInAssembly(assembly, PacketProcessor as NebulaNetPacketProcessor, false);
        }
#if DEBUG
        PacketProcessor.SimulateLatency = true;
#endif

        clientSocket = new WebSocket($"{serverProtocol}://{ServerEndpoint}/socket");
        clientSocket.Log.Level = LogLevel.Debug;
        clientSocket.Log.Output = Log.SocketOutput;
        clientSocket.OnOpen += ClientSocket_OnOpen;
        clientSocket.OnClose += ClientSocket_OnClose;
        clientSocket.OnMessage += ClientSocket_OnMessage;

        var currentLogOutput = clientSocket.Log.Output;
        clientSocket.Log.Output = (logData, arg2) =>
        {
            currentLogOutput(logData, arg2);

            // This method of detecting an authentication failure is super finicky, however there is no other way to do this in the websocket package we are currently using
            if (logData.Level == LogLevel.Fatal && logData.Message == "Requires the authentication.")
            {
                websocketAuthenticationFailure = true;
            }
        };

        if (!string.IsNullOrWhiteSpace(serverPassword))
        {
            clientSocket.SetCredentials("nebula-player", serverPassword, true);
        }

        websocketAuthenticationFailure = false;

        clientSocket.Connect();

        ((LocalPlayer)Multiplayer.Session.LocalPlayer).IsHost = false;

        if (Config.Options.RememberLastIP)
        {
            // We've successfully connected, set connection as last ip, cutting out "ws://"(but not others, like wss) and "/socket"
            Config.Options.LastIP = serverProtocol == "ws" ? ServerEndpoint.ToString() : $"{serverProtocol}://{ServerEndpoint.ToString()}";
            Config.SaveOptions();
        }

        if (Config.Options.RememberLastClientPassword && !string.IsNullOrWhiteSpace(serverPassword))
        {
            Config.Options.LastClientPassword = serverPassword;
            Config.SaveOptions();
        }

        try
        {
            NebulaModAPI.OnMultiplayerSessionChange(true);
            NebulaModAPI.OnMultiplayerGameStarted?.Invoke();
        }
        catch (Exception e)
        {
            Log.Error("NebulaModAPI.OnMultiplayerGameStarted error:\n" + e);
        }
    }

    public void Stop()
    {
        clientSocket?.Close((ushort)DisconnectionReason.ClientRequestedDisconnect, "Player left the game");

        // load settings again to dispose the temp soil setting that could have been received from server
        Config.LoadOptions();
        try
        {
            NebulaModAPI.OnMultiplayerSessionChange(false);
            NebulaModAPI.OnMultiplayerGameEnded?.Invoke();
        }
        catch (Exception e)
        {
            Log.Error("NebulaModAPI.OnMultiplayerGameEnded error:\n" + e);
        }
    }

    public void Dispose()
    {
        Stop();
        GC.SuppressFinalize(this);
    }

    public void SendPacket<T>(T packet) where T : class, new()
    {
        serverConnection?.SendPacket(packet);
    }

    public void SendToMatching<T>(T packet, Predicate<INebulaPlayer> condition) where T : class, new()
    {
        throw new NotImplementedException();
    }

    public void SendPacketExclude<T>(T packet, INebulaConnection exclude) where T : class, new()
    {
        // Only possible from host
        throw new NotImplementedException();
    }

    public void SendPacketToLocalStar<T>(T packet) where T : class, new()
    {
        serverConnection?.SendPacket(new StarBroadcastPacket(PacketProcessor.Write(packet), GameMain.data.localStar?.id ?? -1));
    }

    public void SendPacketToLocalPlanet<T>(T packet) where T : class, new()
    {
        serverConnection?.SendPacket(new PlanetBroadcastPacket(PacketProcessor.Write(packet), GameMain.data.localPlanet?.id ?? -1));
    }

    public void SendPacketToPlanet<T>(T packet, int planetId) where T : class, new()
    {
        // Only possible from host
        throw new NotImplementedException();
    }

    public void SendPacketToStar<T>(T packet, int starId) where T : class, new()
    {
        // Only possible from host
        throw new NotImplementedException();
    }

    public void SendPacketToStarExclude<T>(T packet, int starId, INebulaConnection exclude) where T : class, new()
    {
        // Only possible from host
        throw new NotImplementedException();
    }

    public void Update()
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

            gameStateUpdateTimer += Time.deltaTime;
            if (gameStateUpdateTimer >= GAME_STATE_UPDATE_INTERVAL)
            {
                if (!GameMain.isFullscreenPaused)
                {
                    SendPacket(new GameStateRequest());
                }

                gameStateUpdateTimer = 0f;
            }
        }

        fragmentUpdateTimer += Time.deltaTime;
        if (!(fragmentUpdateTimer >= FRAGEMENT_UPDATE_INTERVAL))
        {
            return;
        }

        if (GameStatesManager.FragmentSize > 0)
        {
            GameStatesManager.UpdateBufferLength(GetFragmentBufferLength());
        }

        fragmentUpdateTimer = 0f;
    }

    private void ClientSocket_OnMessage(object sender, MessageEventArgs e)
    {
        if (!Multiplayer.IsLeavingGame)
        {
            PacketProcessor.EnqueuePacketForProcessing(e.RawData, serverConnection);
        }
    }

    private void ClientSocket_OnOpen(object sender, EventArgs e)
    {
        DisableNagleAlgorithm(clientSocket);

        Log.Info("Server connection established");
        serverConnection = new NebulaConnection(clientSocket, ServerEndpoint, PacketProcessor as NebulaNetPacketProcessor);

        //TODO: Maybe some challenge-response authentication mechanism?

        SendPacket(new LobbyRequest(
            CryptoUtils.GetPublicKey(CryptoUtils.GetOrCreateUserCert()),
            !string.IsNullOrWhiteSpace(Config.Options.Nickname) ? Config.Options.Nickname : GameMain.data.account.userName));
    }

    private void ClientSocket_OnClose(object sender, CloseEventArgs e)
    {
        // Unity's TLS bug workaround, see https://github.com/sta/websocket-sharp/issues/219
        var sslProtocolHack = (System.Security.Authentication.SslProtocols)(0xC00 | 0x300 | 0xC0);
        // TlsHandshakeFailure: 1015
        if (e.Code == 1015 && clientSocket.SslConfiguration.EnabledSslProtocols != sslProtocolHack)
        {
            clientSocket.SslConfiguration.EnabledSslProtocols = sslProtocolHack;
            clientSocket.Connect();
            return;
        }

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

            switch (e.Code)
            {
                case (ushort)DisconnectionReason.ModIsMissing:
                    InGamePopup.ShowWarning(
                        "Mod Mismatch".Translate(),
                        string.Format("You are missing mod {0}".Translate(), e.Reason),
                        "OK".Translate(),
                        Multiplayer.LeaveGame);
                    return;
                case (ushort)DisconnectionReason.ModIsMissingOnServer:
                    InGamePopup.ShowWarning(
                        "Mod Mismatch".Translate(),
                        string.Format("Server is missing mod {0}".Translate(), e.Reason),
                        "OK".Translate(),
                        Multiplayer.LeaveGame);
                    return;
                case (ushort)DisconnectionReason.ModVersionMismatch:
                    {
                        var versions = e.Reason.Split(';');
                        InGamePopup.ShowWarning(
                            "Mod Version Mismatch".Translate(),
                            string.Format("Your mod {0} version is not the same as the Host version.\nYou:{1} - Remote:{2}".Translate(),
                                versions[0], versions[1], versions[2]),
                            "OK".Translate(),
                            Multiplayer.LeaveGame);
                        return;
                    }
                case (ushort)DisconnectionReason.GameVersionMismatch:
                    {
                        var versions = e.Reason.Split(';');
                        InGamePopup.ShowWarning(
                            "Game Version Mismatch".Translate(),
                            string.Format(
                                "Your version of the game is not the same as the one used by the Host.\nYou:{0} - Remote:{1}"
                                    .Translate(), versions[0], versions[1]),
                            "OK".Translate(),
                            Multiplayer.LeaveGame);
                        return;
                    }
                case (ushort)DisconnectionReason.ProtocolError when websocketAuthenticationFailure:
                    InGamePopup.AskInput(
                        "Server Requires Password".Translate(),
                        "Server is protected. Please enter the correct password:".Translate(),
                        InputField.ContentType.Password,
                        serverPassword,
                        password =>
                        {
                            Multiplayer.ShouldReturnToJoinMenu = false;
                            Multiplayer.LeaveGame();
                            Multiplayer.ShouldReturnToJoinMenu = true;
                            Multiplayer.JoinGame(new Client(ServerEndpoint, password));
                        },
                        Multiplayer.LeaveGame
                    );
                    return;
                case (ushort)DisconnectionReason.HostStillLoading:
                    InGamePopup.ShowWarning(
                        "Server Busy".Translate(),
                        "Server is not ready to join. Please try again later.".Translate(),
                        "OK".Translate(),
                        Multiplayer.LeaveGame);
                    return;
            }

            if (Multiplayer.Session != null && (Multiplayer.Session.IsGameLoaded || Multiplayer.Session.IsInLobby))
            {
                InGamePopup.ShowWarning(
                    "Connection Lost".Translate(),
                    "You have been disconnected from the server.".Translate() + "\n" + e.Reason,
                    "Quit",
                    Multiplayer.LeaveGame);
                if (!Multiplayer.Session.IsInLobby)
                {
                    return;
                }

                Multiplayer.ShouldReturnToJoinMenu = false;
                Multiplayer.Session.IsInLobby = false;
                UIRoot.instance.galaxySelect.CancelSelect();
            }
            else
            {
                Log.Warn("Disconnect code: " + e.Code + ", reason:" + e.Reason);
                InGamePopup.ShowWarning(
                    "Server Unavailable".Translate(),
                    "Can't reach the server. Please check the network status.\n(Refer Nebula wiki for troubleshooting)".Translate(),
                    "OK".Translate(),
                    Multiplayer.LeaveGame);
            }
        });
    }

    private static void DisableNagleAlgorithm(WebSocket socket)
    {
        var tcpClient = AccessTools.FieldRefAccess<WebSocket, TcpClient>("_tcpClient")(socket);
        if (tcpClient != null)
        {
            tcpClient.NoDelay = true;
        }
    }

    private int GetFragmentBufferLength()
    {
        var fragmentsBuffer = fragmentsBufferRef(clientSocket);
        return (int)(fragmentsBuffer?.Length ?? 0);
    }
}
