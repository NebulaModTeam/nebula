#region

using System;
using NebulaAPI.Networking;

#endregion

namespace NebulaAPI.GameState;

public interface INetworkProvider : IDisposable
{
    [Obsolete("Dev note: we need to move this out of the public API, and add an alternative that also works for dedicated.")]
    INetPacketProcessor PacketProcessor { get; }

    /// <summary>
    ///     Send packet to Host (If ran on Client) or all Clients (If ran on Host)
    /// </summary>
    void SendToAll<T>(T packet) where T : class, new();

    /// <summary>
    /// Send a packet to all players that match a predicate
    /// </summary>
    /// <param name="packet"></param>
    /// <param name="condition"></param>
    /// <typeparam name="T"></typeparam>
    public void SendToMatching<T>(T packet, Predicate<INebulaPlayer> condition) where T : class, new();

    /// <summary>
    ///     Broadcast packet to all Players within current star system
    /// </summary>
    void SendToLocalStar<T>(T packet) where T : class, new();

    /// <summary>
    ///     Broadcast packet to all Players within current planet
    /// </summary>
    void SendToLocalPlanet<T>(T packet) where T : class, new();

    /// <summary>
    ///     Send packet to all Clients on a planet
    /// </summary>
    void SendToPlanet<T>(T packet, int planetId) where T : class, new();

    /// <summary>
    ///     Send packet to all Clients within star system
    /// </summary>
    void SendToStar<T>(T packet, int starId) where T : class, new();

    /// <summary>
    ///     Send packet to all Clients except the excluded client
    /// </summary>
    void SendToAllExcept<T>(T packet, INebulaConnection except) where T : class, new();

    /// <summary>
    ///     Send packet to all Clients within star system except the excluded client
    /// </summary>
    void SendToStarExcept<T>(T packet, int starId, INebulaConnection except) where T : class, new();
}
