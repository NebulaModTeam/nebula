﻿#region

using System;
using NebulaAPI.Packets;
using NebulaAPI.Simulation;

#endregion

namespace NebulaAPI.GameState;

// @TODO: Refactor ISimulation out of NetworkProviders and keep the latter strictly network related?
// 
public interface INetworkProvider : IDisposable, ISimulation
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
}
