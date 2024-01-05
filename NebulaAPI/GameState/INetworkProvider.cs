#region

using System;
using NebulaAPI.Networking;

#endregion

namespace NebulaAPI.GameState;

public interface INetworkProvider : IDisposable
{
    [Obsolete("Deprecated API, use IServer.GetPlayer instead.")]
    IPlayerManager PlayerManager { get; }

    [Obsolete("Dev note: we need to move this out of the public API, but it'll need a lot of packet processor updates.")]
    INetPacketProcessor PacketProcessor { get; }

    /// <summary>
    ///     Send packet to Host (If ran on Client) or all Clients (If ran on Host)
    /// </summary>
    void SendPacket<T>(T packet) where T : class, new();

    /// <summary>
    /// Send a packet to all players that match a predicate
    /// </summary>
    /// <param name="packet"></param>
    /// <param name="condition"></param>
    /// <typeparam name="T"></typeparam>
    public void SendIfCondition<T>(T packet, Predicate<INebulaPlayer> condition) where T : class, new();

    /// <summary>
    ///     Broadcast packet to all Players within current star system
    /// </summary>
    void SendPacketToLocalStar<T>(T packet) where T : class, new();

    /// <summary>
    ///     Broadcast packet to all Players within current planet
    /// </summary>
    void SendPacketToLocalPlanet<T>(T packet) where T : class, new();

    /// <summary>
    ///     Send packet to all Clients on a planet
    /// </summary>
    void SendPacketToPlanet<T>(T packet, int planetId) where T : class, new();

    /// <summary>
    ///     Send packet to all Clients within star system
    /// </summary>
    void SendPacketToStar<T>(T packet, int starId) where T : class, new();

    /// <summary>
    ///     Send packet to all Clients except the excluded client
    /// </summary>
    void SendPacketExclude<T>(T packet, INebulaConnection exclude) where T : class, new();

    /// <summary>
    ///     Send packet to all Clients within star system except the excluded client
    /// </summary>
    void SendPacketToStarExclude<T>(T packet, int starId, INebulaConnection exclude) where T : class, new();
}
