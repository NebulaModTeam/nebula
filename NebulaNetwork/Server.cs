#region

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using HarmonyLib;
using NebulaAPI;
using NebulaAPI.DataStructures;
using NebulaAPI.GameState;
using NebulaAPI.Networking;
using NebulaAPI.Packets;
using NebulaModel;
using NebulaModel.DataStructures;
using NebulaModel.Logger;
using NebulaModel.Networking;
using NebulaModel.Networking.Serialization;
using NebulaModel.Packets.GameHistory;
using NebulaModel.Packets.Players;
using NebulaModel.Packets.Session;
using NebulaModel.Utils;
using NebulaNetwork.Messaging;
using NebulaNetwork.Ngrok;
using NebulaWorld;
using NebulaWorld.Player;
using NebulaWorld.SocialIntegration;
using Open.Nat;
using UnityEngine;
using WebSocketSharp;
using WebSocketSharp.Server;
using AuthenticationSchemes = WebSocketSharp.Net.AuthenticationSchemes;
using NetworkCredential = WebSocketSharp.Net.NetworkCredential;

#endregion

namespace NebulaNetwork;

public class Server : IServer
{
    [Obsolete] public IPlayerManager PlayerManager { get; } = new PlayerManager();

    public INetPacketProcessor PacketProcessor { get; set; } = new NebulaNetPacketProcessor();

    private const float GAME_RESEARCH_UPDATE_INTERVAL = 2;
    private const float STATISTICS_UPDATE_INTERVAL = 1;
    private const float LAUNCH_UPDATE_INTERVAL = 4;
    private const float DYSONSPHERE_UPDATE_INTERVAL = 2;
    private const float WARNING_UPDATE_INTERVAL = 1;
    private readonly bool loadSaveFile;

    private float dysonLaunchUpateTimer = 1;
    private float dysonSphereUpdateTimer;

    private float gameResearchHashUpdateTimer;
    private NgrokManager ngrokManager;
    private float productionStatisticsUpdateTimer;


    private WebSocketServer socket;
    private float warningUpdateTimer;

    private readonly ConcurrentDictionary<INebulaConnection, INebulaPlayer> playerConnections = new();
    private ConcurrentQueue<ushort> PlayerIdPool = new();
    private int highestPlayerID;

    public IReadOnlyDictionary<INebulaConnection, INebulaPlayer> PlayerConnections => playerConnections;
    public IReadOnlyCollection<INebulaPlayer> Players => playerConnections.Values.ToList();

    public Server(ushort port, bool loadSaveFile = false)
    {
        Port = port;
        this.loadSaveFile = loadSaveFile;
    }

    public ushort Port { get; set; }

    public string NgrokAddress => ngrokManager.NgrokAddress;
    public bool NgrokActive => ngrokManager.IsNgrokActive();
    public bool NgrokEnabled => ngrokManager.NgrokEnabled;
    public string NgrokLastErrorCode => ngrokManager.NgrokLastErrorCode;
    public event EventHandler<INebulaConnection> Connected;
    public event EventHandler<INebulaConnection> Disconnected;

    // Placeholder until we implement Connected and Disconnected event on the socket level.
    internal void OnSocketConnection(INebulaConnection conn)
    {
        // Generate new data for the player
        var playerId = GetNextPlayerId();

        // this is truncated to ushort.MaxValue
        var birthPlanet = GameMain.galaxy.PlanetById(GameMain.galaxy.birthPlanetId);
        var playerData = new PlayerData(playerId, -1,
            position: new Double3(birthPlanet.uPosition.x, birthPlanet.uPosition.y, birthPlanet.uPosition.z));

        conn.ConnectionStatus = EConnectionStatus.Pending;

        INebulaPlayer newPlayer = new NebulaPlayer(conn, playerData);
        if (!playerConnections.TryAdd(conn, newPlayer))
            throw new InvalidOperationException($"Connection {conn.Id} already exists!");

        // return newPlayer;
        Connected?.Invoke(this, conn);
    }

    private ushort GetNextPlayerId()
    {
        ushort nextId;
        if (!PlayerIdPool.TryDequeue(out nextId))
            nextId = (ushort)Interlocked.Increment(ref highestPlayerID);
        return nextId;
    }

    // Placeholder until we implement Connected and Disconnected event on the socket level.
    internal void OnSocketDisconnection(INebulaConnection conn)
    {
        Multiplayer.Session.NumPlayers -= 1;
        DiscordManager.UpdateRichPresence();

        playerConnections.TryRemove(conn, out var player);

        // @TODO: Why can this happen in the first place?
        // Figure out why it was possible before the move and fix that issue at the root.
        if (player is null)
        {
            Log.Warn("Player is null - Disconnect logic NOT CALLED!");

            if (!Config.Options.SyncSoil)
            {
                return;
            }

            // now we need to recalculate the current sand amount :C
            GameMain.mainPlayer.sandCount = Multiplayer.Session.LocalPlayer.Data.Mecha.SandCount;
            // using (GetConnectedPlayers(out var connectedPlayers))
            {
                var connectedPlayers = playerConnections
                    .Where(kvp => kvp.Key.ConnectionStatus == EConnectionStatus.Connected);
                foreach (var entry in connectedPlayers)
                {
                    GameMain.mainPlayer.sandCount += entry.Value.Data.Mecha.SandCount;
                }
            }

            UIRoot.instance.uiGame.OnSandCountChanged(GameMain.mainPlayer.sandCount,
                GameMain.mainPlayer.sandCount - Multiplayer.Session.LocalPlayer.Data.Mecha.SandCount);
            SendPacket(new PlayerSandCount(GameMain.mainPlayer.sandCount));

            return;
        }

        // player is valid
        SendPacketExclude(new PlayerDisconnected(player.Id, Multiplayer.Session.NumPlayers), conn);
        // For sync completed player who triggered OnPlayerJoinedGame() before
        if (conn.ConnectionStatus == EConnectionStatus.Connected)
        {
            SimulatedWorld.OnPlayerLeftGame(player);
        }

        PlayerIdPool.Enqueue(player.Id);

        Multiplayer.Session.PowerTowers.OnClientDisconnect();
        Multiplayer.Session.Statistics.UnRegisterPlayer(player.Id);
        Multiplayer.Session.DysonSpheres.UnRegisterPlayer(conn);

        //Notify players about queued building plans for drones
        var DronePlans = DroneManager.GetPlayerDronePlans(player.Id);
        if (DronePlans is { Length: > 0 } && player.Data.LocalPlanetId > 0)
        {
            Multiplayer.Session.Network.SendPacketToPlanet(new RemoveDroneOrdersPacket(DronePlans),
                player.Data.LocalPlanetId);
            //Remove it also from host queue, if host is on the same planet
            if (GameMain.mainPlayer.planetId == player.Data.LocalPlanetId)
            {
                //todo:replace
                //foreach (var t in DronePlans)
                //{
                //    GameMain.mainPlayer.mecha.droneLogic.serving.Remove(t);
                //}
            }
        }

        // Note: using Keys or Values directly creates a readonly snapshot at the moment of call, as opposed to enumerating the dict.
        var syncCount = playerConnections.Keys.Count(key => key.ConnectionStatus == EConnectionStatus.Syncing);
        if (conn.ConnectionStatus is not EConnectionStatus.Syncing || syncCount != 0)
        {
            return;
        }

        SendPacket(new SyncComplete());
        Multiplayer.Session.World.OnAllPlayersSyncCompleted();
        Disconnected?.Invoke(this, conn);
    }

    public void Start()
    {
        if (loadSaveFile)
        {
            SaveManager.LoadServerData();
        }

        foreach (var assembly in AssembliesUtils.GetNebulaAssemblies())
        {
            PacketUtils.RegisterAllPacketNestedTypesInAssembly(assembly, PacketProcessor as NebulaNetPacketProcessor);
        }

        PacketUtils.RegisterAllPacketProcessorsInCallingAssembly(PacketProcessor as NebulaNetPacketProcessor, true);

        foreach (var assembly in NebulaModAPI.TargetAssemblies)
        {
            PacketUtils.RegisterAllPacketNestedTypesInAssembly(assembly, PacketProcessor as NebulaNetPacketProcessor);
            PacketUtils.RegisterAllPacketProcessorsInAssembly(assembly, PacketProcessor as NebulaNetPacketProcessor, true);
        }
#if DEBUG
        PacketProcessor.SimulateLatency = true;
#endif

        if (Config.Options.EnableUPnpOrPmpSupport)
        {
            Task.Run(async () =>
            {
                var discoverer = new NatDiscoverer();
                try
                {
                    var device = await discoverer.DiscoverDeviceAsync();
                    await device.CreatePortMapAsync(new Mapping(Protocol.Tcp, Port, Port, "DSP nebula"));
                    Log.Info($"Successfully created UPnp or Pmp port mapping for {Port}");
                }
                catch (NatDeviceNotFoundException)
                {
                    Log.WarnInform("No UPnp or Pmp compatible/enabled NAT device found".Translate());
                }
                catch (MappingException)
                {
                    Log.WarnInform("Could not create UPnp or Pmp port mapping".Translate());
                }
            });
        }

        ngrokManager = new NgrokManager(Port);

        socket = new WebSocketServer(IPAddress.IPv6Any, Port)
        {
            Log = { Level = LogLevel.Debug, Output = Log.SocketOutput },
            AllowForwardedRequest = true // This is required to make the websocket play nice with tunneling services like ngrok
        };

        if (!string.IsNullOrWhiteSpace(Config.Options.ServerPassword))
        {
            socket.AuthenticationSchemes = AuthenticationSchemes.Basic;
            socket.UserCredentialsFinder = id =>
            {
                var name = id.Name;

                // Return user name, password, and roles.
                return name == "nebula-player"
                    ? new NetworkCredential(name, Config.Options.ServerPassword)
                    : null; // If the user credentials are not found.
            };
        }

        DisableNagleAlgorithm(socket);
        WebSocketService.PacketProcessor = PacketProcessor as NebulaNetPacketProcessor;
        WebSocketService.Server = this;
        socket.AddWebSocketService<WebSocketService>("/socket", wse => new WebSocketService());
        try
        {
            // Set wait time higher for high latency network
            socket.WaitTime = TimeSpan.FromSeconds(20);
            socket.KeepClean = Config.Options.CleanupInactiveSessions;
            socket.Start();
        }
        catch (InvalidOperationException e)
        {
            InGamePopup.ShowError("Error", "An error occurred while hosting the game: ".Translate() + e.Message,
                "Close".Translate());
            Stop();
            Multiplayer.LeaveGame();
            return;
        }

        ((LocalPlayer)Multiplayer.Session.LocalPlayer).IsHost = true;

        ((LocalPlayer)Multiplayer.Session.LocalPlayer).SetPlayerData(new PlayerData(
                GetNextPlayerId(),
                GameMain.localPlanet?.id ?? -1,
                !string.IsNullOrWhiteSpace(Config.Options.Nickname) ? Config.Options.Nickname : GameMain.data.account.userName),
            loadSaveFile);

        Task.Run(async () =>
        {
            if (ngrokManager.IsNgrokActive())
            {
                var ip = await ngrokManager.GetNgrokAddressAsync();
                DiscordManager.UpdateRichPresence(ip, updateTimestamp: true);
                if (Multiplayer.IsDedicated)
                {
                    Log.Info($">> Ngrok address: {ip}");
                }
            }
            else
            {
                DiscordManager.UpdateRichPresence(
                    $"{(Config.Options.IPConfiguration != IPUtils.IPConfiguration.IPv6 ? await IPUtils.GetWANv4Address() : string.Empty)};" +
                    $"{(Config.Options.IPConfiguration != IPUtils.IPConfiguration.IPv4 ? await IPUtils.GetWANv6Address() : string.Empty)};" +
                    $"{Port}",
                    updateTimestamp: true);
            }
        });

        try
        {
            NebulaModAPI.OnMultiplayerGameStarted?.Invoke();
        }
        catch (Exception e)
        {
            Log.Error("NebulaModAPI.OnMultiplayerGameStarted error:\n" + e);
        }
    }

    public void Stop()
    {
        socket?.Stop();

        ngrokManager?.StopNgrok();

        try
        {
            NebulaModAPI.OnMultiplayerGameEnded?.Invoke();
        }
        catch (Exception e)
        {
            Log.Error("NebulaModAPI.OnMultiplayerGameEnded error:\n" + e);
        }
    }

    public void Disconnect(INebulaConnection conn, DisconnectionReason reason, string reasonMessage = "")
    {
        playerConnections.TryRemove(conn, out var player);
        if (Encoding.UTF8.GetBytes(reasonMessage).Length <= 123)
        {
            ((NebulaConnection)conn).peerSocket.Close((ushort)reason, reasonMessage);
        }
        else
        {
            throw new ArgumentException("Reason string cannot take up more than 123 bytes");
        }
    }

    public void Dispose()
    {
        Stop();
        GC.SuppressFinalize(this);
    }

    public void SendPacket<T>(T packet) where T : class, new()
    {
        // PlayerManager.SendPacketToAllPlayers(packet);
    }

    public void SendPacketToLocalStar<T>(T packet) where T : class, new()
    {
        // PlayerManager.SendPacketToLocalStar(packet);
    }

    public void SendPacketToLocalPlanet<T>(T packet) where T : class, new()
    {
        // PlayerManager.SendPacketToLocalPlanet(packet);
    }

    public void SendPacketToPlanet<T>(T packet, int planetId) where T : class, new()
    {
        // PlayerManager.SendPacketToPlanet(packet, planetId);
    }

    public void SendPacketToStar<T>(T packet, int starId) where T : class, new()
    {
        // PlayerManager.SendPacketToStar(packet, starId);
    }

    public void SendPacketExclude<T>(T packet, INebulaConnection exclude) where T : class, new()
    {
        // PlayerManager.SendPacketToOtherPlayers(packet, exclude);
    }

    public void SendPacketToStarExclude<T>(T packet, int starId, INebulaConnection exclude) where T : class, new()
    {
        // PlayerManager.SendPacketToStarExcept(packet, starId, exclude);
    }

    public void Update()
    {
        PacketProcessor.ProcessPacketQueue();

        if (!Multiplayer.Session.IsGameLoaded)
        {
            return;
        }

        gameResearchHashUpdateTimer += Time.deltaTime;
        productionStatisticsUpdateTimer += Time.deltaTime;
        dysonLaunchUpateTimer += Time.deltaTime;
        dysonSphereUpdateTimer += Time.deltaTime;
        warningUpdateTimer += Time.deltaTime;

        if (gameResearchHashUpdateTimer > GAME_RESEARCH_UPDATE_INTERVAL)
        {
            gameResearchHashUpdateTimer = 0;
            if (GameMain.data.history.currentTech != 0)
            {
                var state = GameMain.data.history.techStates[GameMain.data.history.currentTech];
                SendPacket(new GameHistoryResearchUpdatePacket(GameMain.data.history.currentTech, state.hashUploaded,
                    state.hashNeeded, GameMain.statistics.techHashedFor10Frames));
            }
        }

        if (productionStatisticsUpdateTimer > STATISTICS_UPDATE_INTERVAL)
        {
            productionStatisticsUpdateTimer = 0;
            Multiplayer.Session.Statistics.SendBroadcastIfNeeded();
        }

        if (dysonLaunchUpateTimer > LAUNCH_UPDATE_INTERVAL)
        {
            dysonLaunchUpateTimer = 0;
            Multiplayer.Session.Launch.SendBroadcastIfNeeded();
        }

        if (dysonSphereUpdateTimer > DYSONSPHERE_UPDATE_INTERVAL)
        {
            dysonSphereUpdateTimer = 0;
            Multiplayer.Session.DysonSpheres.UpdateSphereStatusIfNeeded();
        }

        if (!(warningUpdateTimer > WARNING_UPDATE_INTERVAL))
        {
            return;
        }

        warningUpdateTimer = 0;
        Multiplayer.Session.Warning.SendBroadcastIfNeeded();
    }

    private static void DisableNagleAlgorithm(WebSocketServer socketServer)
    {
        var listener = AccessTools.FieldRefAccess<WebSocketServer, TcpListener>("_listener")(socketServer);
        listener.Server.NoDelay = true;
    }
}
