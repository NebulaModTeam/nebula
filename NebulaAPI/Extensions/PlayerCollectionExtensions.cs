using System.Collections.Generic;
using System.Linq;
using NebulaAPI.GameState;
using NebulaAPI.Networking;

namespace NebulaAPI.Extensions;

public static class PlayerCollectionExtensions
{
    public static IReadOnlyDictionary<INebulaConnection, INebulaPlayer> FilterByConnectionStatus(
        this IReadOnlyDictionary<INebulaConnection, INebulaPlayer> playerConnections, EConnectionStatus status)
    {
        return playerConnections
            .Where(kvp => kvp.Key.ConnectionStatus == status)
            .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
    }

    public static IReadOnlyCollection<INebulaPlayer> FilterByConnectionStatus(
        this IEnumerable<INebulaPlayer> players, EConnectionStatus status)
    {
        return players
            .Where(value => value.Connection.ConnectionStatus == status)
            .ToList();
    }

    /// <summary>
    /// Gets a player by their PlayerId.
    /// </summary>
    /// <param name="players"></param>
    /// <param name="id"></param>
    /// <returns></returns>
    public static INebulaPlayer GetPlayer(
        this IEnumerable<INebulaPlayer> players, ushort id)
    {
        return players.FirstOrDefault(p => p.Id == id);
    }

    /// <summary>
    /// Gets a player by their connection handle.
    /// </summary>
    /// <param name="players"></param>
    /// <param name="conn"></param>
    /// <returns> </returns>
    public static INebulaPlayer GetPlayer(
        this IEnumerable<INebulaPlayer> players, INebulaConnection conn)
    {
        return players.FirstOrDefault(p => p.Connection.Equals(conn));
    }

    /// <summary>
    /// Gets a player by their username.
    /// </summary>
    /// <param name="players"></param>
    /// <param name="username"></param>
    /// <returns></returns>
    public static INebulaPlayer GetPlayer(
        this IEnumerable<INebulaPlayer> players, string username)
    {
        return players.FirstOrDefault(p => p.Data.Username == username);
    }

    /// <summary>
    /// Filters to pending players, with their connection handles as keys.
    /// </summary>
    /// <param name="playerConnections"></param>
    /// <returns></returns>
    public static IReadOnlyDictionary<INebulaConnection, INebulaPlayer> Pending(
        this IReadOnlyDictionary<INebulaConnection, INebulaPlayer> playerConnections)
    {
        return playerConnections.FilterByConnectionStatus(EConnectionStatus.Pending);
    }

    /// <summary>
    /// Filters to syncing players, with their connection handles as keys.
    /// </summary>
    /// <param name="playerConnections"></param>
    /// <returns></returns>
    public static IReadOnlyDictionary<INebulaConnection, INebulaPlayer> Syncing(
        this IReadOnlyDictionary<INebulaConnection, INebulaPlayer> playerConnections)
    {
        return playerConnections.FilterByConnectionStatus(EConnectionStatus.Syncing);
    }

    /// <summary>
    /// Filters to connected players, with their connection handles as keys.
    /// </summary>
    /// <param name="playerConnections"></param>
    /// <returns></returns>
    public static IReadOnlyDictionary<INebulaConnection, INebulaPlayer> Connected(
        this IReadOnlyDictionary<INebulaConnection, INebulaPlayer> playerConnections)
    {
        return playerConnections.FilterByConnectionStatus(EConnectionStatus.Connected);
    }

    /// <summary>
    /// Filters to pending players
    /// </summary>
    /// <param name="players"></param>
    /// <returns></returns>
    public static IReadOnlyCollection<INebulaPlayer> Pending(
        this IEnumerable<INebulaPlayer> players)
    {
        return players.FilterByConnectionStatus(EConnectionStatus.Pending);
    }

    /// <summary>
    /// Filters to syncing players
    /// </summary>
    /// <param name="players"></param>
    /// <returns></returns>
    public static IReadOnlyCollection<INebulaPlayer> Syncing(
        this IEnumerable<INebulaPlayer> players)
    {
        return players.FilterByConnectionStatus(EConnectionStatus.Syncing);
    }

    /// <summary>
    /// Filters to connected players.
    /// </summary>
    /// <param name="players"></param>
    /// <returns></returns>
    public static IReadOnlyCollection<INebulaPlayer> Connected(
        this IEnumerable<INebulaPlayer> players)
    {
        return players.FilterByConnectionStatus(EConnectionStatus.Connected);
    }

    public static bool Contains(this IReadOnlyCollection<INebulaPlayer> players, INebulaConnection conn)
    {
        return players.Any(p => p.Connection.Equals(conn));
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
