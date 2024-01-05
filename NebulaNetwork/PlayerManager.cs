#region

using System;
using System.Collections.Generic;
using System.Linq;
using NebulaAPI.DataStructures;
using NebulaAPI.Extensions;
using NebulaAPI.GameState;
using NebulaAPI.Networking;
using NebulaModel;
using NebulaModel.Networking;
using NebulaWorld;

#endregion

namespace NebulaNetwork;

[Obsolete()]
public class PlayerManager : IPlayerManager
{
    [Obsolete("Use Server.Players or Server.PlayerConnections")]
    public Locker GetPendingPlayers(out IReadOnlyDictionary<INebulaConnection, INebulaPlayer> pendingPlayers)
    {
        pendingPlayers = Multiplayer.Session.Server.PlayerConnections.GetPending();

        return new Locker(new object());
    }

    [Obsolete("Use Server.Players or Server.PlayerConnections")]
    public Locker GetSyncingPlayers(out IReadOnlyDictionary<INebulaConnection, INebulaPlayer> syncingPlayers)
    {
        syncingPlayers = Multiplayer.Session.Server.PlayerConnections.GetSyncing();
        return new Locker(new object());
    }

    [Obsolete("Use Server.Players or Server.PlayerConnections")]
    public Locker GetConnectedPlayers(out IReadOnlyDictionary<INebulaConnection, INebulaPlayer> connectedPlayers)
    {
        connectedPlayers = Multiplayer.Session.Server.PlayerConnections.GetConnected();
        return new Locker(new object());
    }

    [Obsolete("Use SaveManager.PlayerSaves")]
    public Locker GetSavedPlayerData(out IReadOnlyDictionary<string, IPlayerData> savedPlayerData)
    {
        savedPlayerData = SaveManager.PlayerSaves;
        return new Locker(new object());
    }

    [Obsolete("Use Server.Players or Server.PlayerConnections")]
    public IPlayerData[] GetAllPlayerDataIncludingHost()
    {
        var connectedPlayers = Multiplayer.Session.Server.PlayerConnections.Values.ToArray();
        var i = 0;
        IPlayerData[] result;
        if (Multiplayer.IsDedicated)
        {
            // If host is dedicated server, don't include it
            result = new IPlayerData[connectedPlayers.Length];
        }
        else
        {
            result = new IPlayerData[1 + connectedPlayers.Length];
            result[i++] = Multiplayer.Session.LocalPlayer.Data;
        }

        foreach (var player in connectedPlayers)
        {
            result[i++] = player.Data;
        }

        return result;
    }

    [Obsolete]
    public INebulaPlayer GetPlayer(INebulaConnection conn)
    {
        // This shouldn't be nullable, if we have a NebulaConnection we definitely have a NebulaPlayer so First() > FirstOrDefault.
        return Multiplayer.Session.Server.Players.GetByConnectionHandle(conn);
    }

    [Obsolete]
    public INebulaPlayer GetPlayerById(ushort playerId)
    {
        // Let First() throw on nulls, this should only be safe for existing players.
        return Multiplayer.Session.Server.PlayerConnections.First(kvp => kvp.Key.Id == playerId).Value;
    }

    [Obsolete]
    public INebulaPlayer GetConnectedPlayerByUsername(string username)
    {
        // Let First() throw on nulls, this should only be safe for existing players.
        return Multiplayer.Session.Server.PlayerConnections
            .First(kvp => kvp.Value.Data.Username == username).Value;
    }

    [Obsolete]
    public INebulaPlayer GetSyncingPlayer(INebulaConnection conn)
    {
        return Multiplayer.Session.Server.Players.GetSyncing().GetByConnectionHandle(conn);
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
        using (GetConnectedPlayers(out var connectedPlayers))
        {
            foreach (var player in connectedPlayers.Select(kvp => kvp.Value)
                         .Where(player => !player.Connection.Equals(exclude)))
            {
                player.SendPacket(packet);
            }
        }
    }

    public void SendPacketToOtherPlayers<T>(T packet, INebulaPlayer sender) where T : class, new()
    {
        using (GetConnectedPlayers(out var connectedPlayers))
        {
            foreach (var player in connectedPlayers.Select(kvp => kvp.Value).Where(player => player != sender))
            {
                player.SendPacket(packet);
            }
        }
    }
}
