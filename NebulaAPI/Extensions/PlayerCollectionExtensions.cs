﻿using System.Collections.Generic;
using System.Linq;
using NebulaAPI.GameState;
using NebulaAPI.Networking;

namespace NebulaAPI.Extensions;

public static class PlayerCollectionExtensions
{
    // public static IReadOnlyCollection<IPlayerData> GetAllPlayerDataIncludingHost(
    //     this IReadOnlyCollection<INebulaPlayer> players, ILocalPlayer host)
    // {
    //     var all = players
    //         .Select(p => p.Data)
    //         .ToList();
    //     all.Add(host.Data);
    //
    //     return all;
    // }

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
        return players.First(p => p.Id == id);
    }

    public static INebulaPlayer GetByConnectionHandle(
        this IReadOnlyCollection<INebulaPlayer> players, INebulaConnection conn)
    {
        return players.First(p => p.Connection.Equals(conn));
    }

    public static INebulaPlayer GetByUsername(
        this IReadOnlyCollection<INebulaPlayer> players, string username)
    {
        return players.First(p => p.Data.Username == username);
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
}