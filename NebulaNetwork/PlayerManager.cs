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
    private readonly ThreadSafe threadSafe = new();
    private int highestPlayerID;

    public Locker GetPendingPlayers(out Dictionary<INebulaConnection, INebulaPlayer> pendingPlayers)
    {
        return threadSafe.pendingPlayers.GetLocked(out pendingPlayers);
    }

    public Locker GetSyncingPlayers(out Dictionary<INebulaConnection, INebulaPlayer> syncingPlayers)
    {
        return threadSafe.syncingPlayers.GetLocked(out syncingPlayers);
    }

    public Locker GetConnectedPlayers(out Dictionary<INebulaConnection, INebulaPlayer> connectedPlayers)
    {
        return threadSafe.connectedPlayers.GetLocked(out connectedPlayers);
    }

    public Locker GetSavedPlayerData(out Dictionary<string, IPlayerData> savedPlayerData)
    {
        return threadSafe.savedPlayerData.GetLocked(out savedPlayerData);
    }

    public IPlayerData[] GetAllPlayerDataIncludingHost()
    {
        using (GetConnectedPlayers(out var connectedPlayers))
        {
            var i = 0;
            IPlayerData[] result;
            if (Multiplayer.IsDedicated)
            {
                // If host is dedicated server, don't include it
                result = new IPlayerData[connectedPlayers.Count];
            }
            else
            {
                result = new IPlayerData[1 + connectedPlayers.Count];
                result[i++] = Multiplayer.Session.LocalPlayer.Data;
            }
            foreach (var kvp in connectedPlayers)
            {
                result[i++] = kvp.Value.Data;
            }

            return result;
        }
    }

    public INebulaPlayer GetPlayer(INebulaConnection conn)
    {
        using (GetConnectedPlayers(out var connectedPlayers))
        {
            if (connectedPlayers.TryGetValue(conn, out var player))
            {
                return player;
            }
        }

        return null;
    }

    public INebulaPlayer GetPlayerById(ushort playerId)
    {
        INebulaPlayer player;
        using (GetConnectedPlayers(out var connectedPlayers))
        {
            if ((player = connectedPlayers.Values.FirstOrDefault(plr => plr.Id == playerId)) != null)
            {
                return player;
            }
        }
        using (GetSyncingPlayers(out var syncingPlayers))
        {
            if ((player = syncingPlayers.Values.FirstOrDefault(plr => plr.Id == playerId)) != null)
            {
                return player;
            }
        }
        using (GetPendingPlayers(out var pendingPlayers))
        {
            if ((player = pendingPlayers.Values.FirstOrDefault(plr => plr.Id == playerId)) != null)
            {
                return player;
            }
        }
        return null;
    }

    public INebulaPlayer GetConnectedPlayerByUsername(string username)
    {
        using (GetConnectedPlayers(out var connectedPlayers))
        {
            return connectedPlayers.Values
                .FirstOrDefault(plr =>
                    plr.Data != null &&
                    string.Equals(plr.Data.Username, username, StringComparison.InvariantCultureIgnoreCase));
        }
    }

    public INebulaPlayer GetSyncingPlayer(INebulaConnection conn)
    {
        using (GetSyncingPlayers(out var syncingPlayers))
        {
            if (syncingPlayers.TryGetValue(conn, out var player))
            {
                return player;
            }
        }

        return null;
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
            foreach (var player in connectedPlayers.Select(kvp => kvp.Value).Where(player =>
                         player.Data.LocalPlanetId == planetId && !player.Connection.Equals(sender)))
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

    [SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "TBD")]
    private sealed class ThreadSafe
    {
        internal readonly Queue<ushort> availablePlayerIds = new();
        internal readonly Dictionary<INebulaConnection, INebulaPlayer> connectedPlayers = [];
        internal readonly Dictionary<INebulaConnection, INebulaPlayer> pendingPlayers = [];
        internal readonly Dictionary<string, IPlayerData> savedPlayerData = [];
        internal readonly Dictionary<INebulaConnection, INebulaPlayer> syncingPlayers = [];
    }
}
