#region

using System;
using System.Collections.Generic;
using NebulaAPI.DataStructures;
using NebulaAPI.Networking;
using NebulaAPI.Packets;

#endregion

namespace NebulaAPI.GameState;

[Obsolete()]
public interface IPlayerManager
{
    Locker GetPendingPlayers(out IReadOnlyDictionary<INebulaConnection, INebulaPlayer> pendingPlayers);

    Locker GetSyncingPlayers(out IReadOnlyDictionary<INebulaConnection, INebulaPlayer> syncingPlayers);

    Locker GetConnectedPlayers(out IReadOnlyDictionary<INebulaConnection, INebulaPlayer> connectedPlayers);

    Locker GetSavedPlayerData(out IReadOnlyDictionary<string, IPlayerData> savedPlayerData);

    IPlayerData[] GetAllPlayerDataIncludingHost();

    INebulaPlayer GetPlayer(INebulaConnection conn);

    INebulaPlayer GetSyncingPlayer(INebulaConnection conn);

    INebulaPlayer GetPlayerById(ushort playerId);

    INebulaPlayer GetConnectedPlayerByUsername(string username);

    void SendPacketToAllPlayers<T>(T packet) where T : class, new();

    void SendPacketToLocalStar<T>(T packet) where T : class, new();

    void SendPacketToLocalPlanet<T>(T packet) where T : class, new();

    void SendPacketToPlanet<T>(T packet, int planetId) where T : class, new();

    void SendPacketToStar<T>(T packet, int starId) where T : class, new();

    void SendPacketToStarExcept<T>(T packet, int starId, INebulaConnection exclude) where T : class, new();

    void SendPacketToOtherPlayers<T>(T packet, INebulaConnection exclude) where T : class, new();

    void SendPacketToOtherPlayers<T>(T packet, INebulaPlayer sender) where T : class, new();
}
