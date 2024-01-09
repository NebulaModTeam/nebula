#region

using System;
using NebulaAPI.Packets;

#endregion

namespace NebulaAPI.GameState;

public interface INetworkProvider : IDisposable
{
    IPlayerManager PlayerManager { get; set; }

    /// <summary>
    ///     Send packet to Host (If ran on Client) or all Clients (If ran on Host)
    /// </summary>
    void SendPacket<T>(T packet) where T : class, new();

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

    /// <summary>
    ///     Send packet to Client directly (If possible) or indirectly by relaying via host (If Client is not directly reachable)
    /// </summary>
    void SendPacketToClient<T>(T packet, string clientUsername) where T : class, new ();

    void Update();
}
