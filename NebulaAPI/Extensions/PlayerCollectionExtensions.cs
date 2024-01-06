using System.Collections.Generic;
using System.Linq;
using NebulaAPI.GameState;
using NebulaAPI.Networking;

namespace NebulaAPI.Extensions;

public static class PlayerCollectionExtensions
{
    public static IReadOnlyDictionary<INebulaConnection, INebulaPlayer> GetByConnectionStatus(
        this IReadOnlyDictionary<INebulaConnection, INebulaPlayer> playerConnections, EConnectionStatus status)
    {
        return playerConnections
            .Where(kvp => kvp.Key.ConnectionStatus == status)
            .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
    }

    public static IReadOnlyCollection<INebulaPlayer> GetByConnectionStatus(
        this IReadOnlyCollection<INebulaPlayer> players, EConnectionStatus status)
    {
        return players
            .Where(value => value.Connection.ConnectionStatus == status)
            .ToList();
    }

    public static INebulaPlayer GetByPlayerId(
        this IReadOnlyCollection<INebulaPlayer> players, ushort id)
    {
        return players.FirstOrDefault(p => p.Id == id);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="players"></param>
    /// <param name="conn"></param>
    /// <returns>
    /// The player corresponding to the connection handle.
    /// Can return null, if the player disconnects before the call.
    /// </returns>
    public static INebulaPlayer GetByConnectionHandle(
        this IReadOnlyCollection<INebulaPlayer> players, INebulaConnection conn)
    {
        return players.FirstOrDefault(p => p.Connection.Equals(conn));
    }

    public static INebulaPlayer GetByUsername(
        this IReadOnlyCollection<INebulaPlayer> players, string username)
    {
        return players.FirstOrDefault(p => p.Data.Username == username);
    }

    public static IReadOnlyDictionary<INebulaConnection, INebulaPlayer> GetPending(
        this IReadOnlyDictionary<INebulaConnection, INebulaPlayer> playerConnections)
    {
        return playerConnections.GetByConnectionStatus(EConnectionStatus.Pending);
    }

    public static IReadOnlyDictionary<INebulaConnection, INebulaPlayer> GetSyncing(
        this IReadOnlyDictionary<INebulaConnection, INebulaPlayer> playerConnections)
    {
        return playerConnections.GetByConnectionStatus(EConnectionStatus.Syncing);
    }

    public static IReadOnlyDictionary<INebulaConnection, INebulaPlayer> GetConnected(
        this IReadOnlyDictionary<INebulaConnection, INebulaPlayer> playerConnections)
    {
        return playerConnections.GetByConnectionStatus(EConnectionStatus.Connected);
    }


    public static IReadOnlyCollection<INebulaPlayer> GetPending(
        this IReadOnlyCollection<INebulaPlayer> players)
    {
        return players.GetByConnectionStatus(EConnectionStatus.Pending);
    }

    public static IReadOnlyCollection<INebulaPlayer> GetSyncing(
        this IReadOnlyCollection<INebulaPlayer> players)
    {
        return players.GetByConnectionStatus(EConnectionStatus.Syncing);
    }

    public static IReadOnlyCollection<INebulaPlayer> GetConnected(
        this IReadOnlyCollection<INebulaPlayer> players)
    {
        return players.GetByConnectionStatus(EConnectionStatus.Connected);
    }

    /// <summary>
    /// Returns a collection of all player data, including host.
    /// </summary>
    /// <returns></returns>
    public static IEnumerable<IPlayerData> GetAllPlayerData(this IReadOnlyCollection<INebulaPlayer> players)
    {
        var saves = players
            .Select(p => p.Data).ToList();
        // If the host is a player, append their data to the list
        if (!NebulaModAPI.MultiplayerSession.IsDedicated)
            saves.Add(NebulaModAPI.MultiplayerSession.LocalPlayer.Data);

        return saves;
    }
}
