#region

using System;
using System.Collections.Generic;
using NebulaAPI.DataStructures;
using NebulaAPI.GameState;
using NebulaAPI.Networking;
using NebulaWorld;

#endregion

namespace NebulaNetwork;

[Obsolete()]
public class PlayerManager : IPlayerManager
{
    [Obsolete("Use Server.Players or Server.PlayerConnections")]
    public Locker GetPendingPlayers(out IReadOnlyDictionary<INebulaConnection, INebulaPlayer> pendingPlayers)
    {
        pendingPlayers = Multiplayer.Session.Server.Players.Pending;

        return new Locker(new object());
    }

    [Obsolete("Use Server.Players or Server.PlayerConnections")]
    public Locker GetSyncingPlayers(out IReadOnlyDictionary<INebulaConnection, INebulaPlayer> syncingPlayers)
    {
        syncingPlayers = Multiplayer.Session.Server.Players.Syncing;
        return new Locker(new object());
    }

    [Obsolete("Use Server.Players or Server.PlayerConnections")]
    public Locker GetConnectedPlayers(out IReadOnlyDictionary<INebulaConnection, INebulaPlayer> connectedPlayers)
    {
        connectedPlayers = Multiplayer.Session.Server.Players.Connected;
        return new Locker(new object());
    }

    [Obsolete("Use SaveManager.PlayerSaves")]
    public Locker GetSavedPlayerData(out IReadOnlyDictionary<string, IPlayerData> savedPlayerData)
    {
        savedPlayerData = SaveManager.PlayerSaves;
        return new Locker(new object());
    }

    [Obsolete]
    public INebulaPlayer GetPlayer(INebulaConnection conn)
    {
        // This shouldn't be nullable, if we have a NebulaConnection we definitely have a NebulaPlayer so First() > FirstOrDefault.
        return Multiplayer.Session.Server.Players.Get(conn);
    }

    [Obsolete]
    public INebulaPlayer GetPlayerById(ushort playerId)
    {
        // Let First() throw on nulls, this should only be safe for existing players.
        return Multiplayer.Session.Server.Players.Get(playerId);
    }

    [Obsolete]
    public INebulaPlayer GetConnectedPlayerByUsername(string username)
    {
        // Let First() throw on nulls, this should only be safe for existing players.
        return Multiplayer.Session.Server.Players.Get(username);
    }

    [Obsolete]
    public INebulaPlayer GetSyncingPlayer(INebulaConnection conn)
    {
        return Multiplayer.Session.Server.Players.Get(conn, EConnectionStatus.Syncing);
    }

    public void SendPacketToAllPlayers<T>(T packet) where T : class, new()
    {
        Multiplayer.Session.Server.SendPacket(packet);
    }

    public void SendPacketToLocalStar<T>(T packet) where T : class, new()
    {
        Multiplayer.Session.Server.SendPacketToLocalStar(packet);
    }

    public void SendPacketToLocalPlanet<T>(T packet) where T : class, new()
    {
        Multiplayer.Session.Server.SendPacketToLocalPlanet(packet);
    }

    public void SendPacketToPlanet<T>(T packet, int planetId) where T : class, new()
    {
        Multiplayer.Session.Server.SendPacketToPlanet(packet, planetId);
    }

    public void SendPacketToStar<T>(T packet, int starId) where T : class, new()
    {
        Multiplayer.Session.Server.SendPacketToStar(packet, starId);
    }

    public void SendPacketToStarExcept<T>(T packet, int starId, INebulaConnection exclude) where T : class, new()
    {
        Multiplayer.Session.Server.SendPacketToStarExclude(packet, starId, exclude);
    }

    public void SendPacketToOtherPlayers<T>(T packet, INebulaConnection exclude) where T : class, new()
    {
        Multiplayer.Session.Server.SendPacketExclude(packet, exclude);
    }

    public void SendPacketToOtherPlayers<T>(T packet, INebulaPlayer sender) where T : class, new()
    {
        Multiplayer.Session.Server.SendPacketExclude(packet, sender.Connection);
    }
}
