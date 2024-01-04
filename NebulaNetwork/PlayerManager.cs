#region

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using NebulaAPI.DataStructures;
using NebulaAPI.GameState;
using NebulaAPI.Networking;
using NebulaAPI.Packets;
using NebulaModel;
using NebulaModel.DataStructures;
using NebulaModel.Logger;
using NebulaModel.Networking;
using NebulaModel.Packets.Players;
using NebulaModel.Packets.Session;
using NebulaWorld;
using NebulaWorld.Player;
using NebulaWorld.SocialIntegration;

#endregion

namespace NebulaNetwork;

[Obsolete()]
public class PlayerManager : IPlayerManager
{
    [Obsolete("Use Server.Players or Server.PlayerConnections")]
    public Locker GetPendingPlayers(out IReadOnlyDictionary<INebulaConnection, INebulaPlayer> pendingPlayers)
    {
        var server = Multiplayer.Session.Network as IServer;
        pendingPlayers = server!.PlayerConnections
                .Where(kvp => kvp.Key.ConnectionStatus == EConnectionStatus.Pending)
            as IReadOnlyDictionary<INebulaConnection, INebulaPlayer>;
        return new Locker(new object());
    }

    [Obsolete("Use Server.Players or Server.PlayerConnections")]
    public Locker GetSyncingPlayers(out IReadOnlyDictionary<INebulaConnection, INebulaPlayer> syncingPlayers)
    {
        var server = Multiplayer.Session.Network as IServer;
        syncingPlayers = server!.PlayerConnections
                .Where(kvp => kvp.Key.ConnectionStatus == EConnectionStatus.Syncing)
            as IReadOnlyDictionary<INebulaConnection, INebulaPlayer>;
        return new Locker(new object());
    }

    [Obsolete("Use Server.Players or Server.PlayerConnections")]
    public Locker GetConnectedPlayers(out IReadOnlyDictionary<INebulaConnection, INebulaPlayer> connectedPlayers)
    {
        var server = Multiplayer.Session.Network as IServer;
        connectedPlayers = server!.PlayerConnections
                .Where(kvp => kvp.Key.ConnectionStatus == EConnectionStatus.Connected)
            as IReadOnlyDictionary<INebulaConnection, INebulaPlayer>;
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
        var server = Multiplayer.Session.Network as IServer;
        var connectedPlayers = server.PlayerConnections.Values.ToArray();
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
        return Multiplayer.Session.Server.PlayerConnections.First(kvp => kvp.Key.Equals(conn)).Value;
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
        return Multiplayer.Session.Server.PlayerConnections
            .First(kvp => kvp.Key.Equals(conn)
                          // We likely don't need to do this check anymore, since we have it in NebulaConnection, but keeping it for now
                          // just to be safe.
                          && kvp.Key.ConnectionStatus == EConnectionStatus.Pending).Value;
    }

    public void SendPacketToAllPlayers<T>(T packet) where T : class, new()
    {
        using (GetConnectedPlayers(out var connectedPlayers))
        {
            foreach (var player in connectedPlayers.Select(kvp => kvp.Value))
            {
                player.SendPacket(packet);
            }
        }
    }

    public void SendPacketToLocalStar<T>(T packet) where T : class, new()
    {
        using (GetConnectedPlayers(out var connectedPlayers))
        {
            foreach (var player in connectedPlayers.Select(kvp => kvp.Value)
                         .Where(player => player.Data.LocalStarId == GameMain.data.localStar?.id))
            {
                player.SendPacket(packet);
            }
        }
    }

    public void SendPacketToLocalPlanet<T>(T packet) where T : class, new()
    {
        using (GetConnectedPlayers(out var connectedPlayers))
        {
            foreach (var player in connectedPlayers.Select(kvp => kvp.Value)
                         .Where(player => player.Data.LocalPlanetId == GameMain.data.mainPlayer.planetId))
            {
                player.SendPacket(packet);
            }
        }
    }

    public void SendPacketToPlanet<T>(T packet, int planetId) where T : class, new()
    {
        using (GetConnectedPlayers(out var connectedPlayers))
        {
            foreach (var player in connectedPlayers.Select(kvp => kvp.Value)
                         .Where(player => player.Data.LocalPlanetId == planetId))
            {
                player.SendPacket(packet);
            }
        }
    }

    public void SendPacketToStar<T>(T packet, int starId) where T : class, new()
    {
        using (GetConnectedPlayers(out var connectedPlayers))
        {
            foreach (var player in connectedPlayers.Select(kvp => kvp.Value).Where(player => player.Data.LocalStarId == starId))
            {
                player.SendPacket(packet);
            }
        }
    }

    public void SendPacketToStarExcept<T>(T packet, int starId, INebulaConnection exclude) where T : class, new()
    {
        using (GetConnectedPlayers(out var connectedPlayers))
        {
            foreach (var player in connectedPlayers.Select(kvp => kvp.Value)
                         .Where(player => player.Data.LocalStarId == starId && player != GetPlayer(exclude)))
            {
                player.SendPacket(packet);
            }
        }
    }

    public void SendRawPacketToStar(byte[] rawPacket, int starId, INebulaConnection sender)
    {
        using (GetConnectedPlayers(out var connectedPlayers))
        {
            foreach (var player in connectedPlayers.Select(kvp => kvp.Value)
                         .Where(player => player.Data.LocalStarId == starId && !player.Connection.Equals(sender)))
            {
                player.Connection.SendRawPacket(rawPacket);
            }
        }
    }

    public void SendRawPacketToPlanet(byte[] rawPacket, int planetId, INebulaConnection sender)
    {
        using (GetConnectedPlayers(out var connectedPlayers))
        {
            foreach (var player in connectedPlayers.Select(kvp => kvp.Value)
                         .Where(player => player.Data.LocalPlanetId == planetId && !player.Connection.Equals(sender)))
            {
                player.Connection.SendRawPacket(rawPacket);
            }
        }
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

    public void UpdateMechaData(IMechaData mechaData, INebulaConnection conn)
    {
        if (mechaData == null)
        {
            return;
        }

        using (GetConnectedPlayers(out var connectedPlayers))
        {
            if (!connectedPlayers.TryGetValue(conn, out var player))
            {
                return;
            }

            //Find correct player for data to update, preserve sand count if syncing is enabled
            var sandCount = player.Data.Mecha.SandCount;
            player.Data.Mecha = mechaData;
            if (Config.Options.SyncSoil)
            {
                player.Data.Mecha.SandCount = sandCount;
            }
        }
    }

    // add or take sand evenly from each connected player while soil is synced
    public void UpdateSyncedSandCount(int deltaSandCount)
    {
        using (GetConnectedPlayers(out var connectedPlayers))
        {
            foreach (var entry in connectedPlayers)
            {
                entry.Value.Data.Mecha.SandCount += deltaSandCount / (connectedPlayers.Count + 1);
                // dont be too picky here, a little bit more or less sand is ignorable i guess
                if (entry.Value.Data.Mecha.SandCount < 0)
                {
                    entry.Value.Data.Mecha.SandCount = 0;
                }
            }

            Multiplayer.Session.LocalPlayer.Data.Mecha.SandCount += deltaSandCount / (connectedPlayers.Count + 1);
        }
    }
}
