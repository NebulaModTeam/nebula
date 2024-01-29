using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using NebulaAPI.GameState;
using NebulaAPI.Networking;

namespace NebulaAPI.DataStructures;

public class ConcurrentPlayerCollection
{
    /// <summary>
    /// Get a key value pair collection of pending players.
    /// </summary>
    /// <returns></returns>
    public IReadOnlyDictionary<INebulaConnection, INebulaPlayer> Pending => playerCollections[EConnectionStatus.Pending];


    /// <summary>
    /// Get a key value pair collection of syncing players.
    /// </summary>
    /// <returns></returns>
    public IReadOnlyDictionary<INebulaConnection, INebulaPlayer> Syncing => playerCollections[EConnectionStatus.Syncing];


    /// <summary>
    /// Get a key value pair collection of connected players.
    /// </summary>
    /// <returns></returns>
    public IReadOnlyDictionary<INebulaConnection, INebulaPlayer> Connected => playerCollections[EConnectionStatus.Connected];

    /// <summary>
    /// Attempts to add a player to the collection.
    /// </summary>
    /// <param name="conn"></param>
    /// <param name="newPlayer"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException">True if successful, false if the player already exists.</exception>
    public bool TryAdd(INebulaConnection conn, INebulaPlayer newPlayer)
    {
        if (conn.ConnectionStatus == EConnectionStatus.Undefined)
            throw new InvalidOperationException("Could not add a player of undefined connection status.");
        return playerCollections[conn.ConnectionStatus].TryAdd(conn, newPlayer);
    }

    /// <summary>
    /// Attempts to remove a player from the collection.
    /// </summary>
    /// <param name="conn"></param>
    /// <param name="removedPlayer"></param>
    /// <returns>True if the player was removed.</returns>
    public bool TryRemove(INebulaConnection conn, out INebulaPlayer removedPlayer)
    {
        return playerCollections[conn.ConnectionStatus].TryRemove(conn, out removedPlayer);
    }

    /// <summary>
    /// Upgrades a player's connection status.
    /// </summary>
    /// <param name="player"></param>
    /// <param name="newStatus"></param>
    /// <returns>
    /// True on success or if the player's status is already upgraded.
    /// False if the player doesn't exist.
    /// </returns>
    public bool TryUpgrade(INebulaPlayer player, EConnectionStatus newStatus)
    {
        if (!playerCollections[player.Connection.ConnectionStatus].TryRemove(player.Connection, out _))
            return false;
        if (!playerCollections[newStatus].TryAdd(player.Connection, player))
            return true;
        player.Connection.ConnectionStatus = newStatus;
        return true;
    }

    /// <summary>
    /// Retrieves a player from the players list by their connection handle.
    /// </summary>
    /// <param name="conn"></param>
    /// <param name="connectionStatus">The connection status cache to query</param>
    /// <returns>
    /// Player or null if the player has disconnected.
    /// </returns>
    public INebulaPlayer Get(INebulaConnection conn, EConnectionStatus connectionStatus = EConnectionStatus.Connected)
    {
        playerCollections[connectionStatus].TryGetValue(conn, out var player);
        return player;
    }

    /// <summary>
    /// Retrieves a player from the connected players list by their username.
    /// </summary>
    /// <param name="username"></param>
    /// <param name="connectionStatus">The connection status cache to query</param>
    /// <returns>
    /// Player or null if the player has disconnected.
    /// </returns>
    public INebulaPlayer Get(string username, EConnectionStatus connectionStatus = EConnectionStatus.Connected)
    {
        var connectedPlayers = playerCollections[connectionStatus];
        foreach (var kvp in connectedPlayers)
        {
            if (kvp.Value.Data.Username == username)
                return kvp.Value;
        }

        return null;
    }

    /// <summary>
    /// Retrieves a player from the connected players list by their player id.
    /// </summary>
    /// <param name="playerId"></param>
    /// <param name="connectionStatus">The connection status cache to query</param>
    /// <returns>
    /// Player or null if the player has disconnected.
    /// </returns>
    public INebulaPlayer Get(ushort playerId, EConnectionStatus connectionStatus = EConnectionStatus.Connected)
    {
        var connectedPlayers = playerCollections[connectionStatus];
        foreach (var kvp in connectedPlayers)
        {
            if (kvp.Value.Id == playerId)
                return kvp.Value;
        }

        return null;
    }

    /// <summary>
    /// Returns a collection of all player data, including host.
    /// </summary>
    /// <returns></returns>
    public IEnumerable<IPlayerData> GetAllPlayerData()
    {
        var saves = playerCollections[EConnectionStatus.Connected]
            .Select(p => p.Value.Data).ToList();
        // If the host is a player, append their data to the list
        if (!NebulaModAPI.MultiplayerSession.IsDedicated)
            saves.Add(NebulaModAPI.MultiplayerSession.LocalPlayer.Data);

        return saves;
    }

    private class ReducedConcurrentDictionary<TKey, TValue> : ConcurrentDictionary<TKey, TValue>
    {
#pragma warning disable CA1822
        // These are disabled as they create a new snapshot copy of the existing values.
        public new ICollection<TKey> Keys => throw new InvalidOperationException("Accessing keys directly is not allowed.");
        public new ICollection<TValue> Values => throw new InvalidOperationException("Accessing keys directly is not allowed.");
#pragma warning restore CA1822
    }

    private readonly Dictionary<EConnectionStatus, ReducedConcurrentDictionary<INebulaConnection, INebulaPlayer>> playerCollections = new()
    {
        { EConnectionStatus.Pending, new ReducedConcurrentDictionary<INebulaConnection, INebulaPlayer>() },
        { EConnectionStatus.Syncing, new ReducedConcurrentDictionary<INebulaConnection, INebulaPlayer>() },
        { EConnectionStatus.Connected, new ReducedConcurrentDictionary<INebulaConnection, INebulaPlayer>() },
    };
}
