#region

using System;
using NebulaAPI.Interfaces;
using UnityEngine;

#endregion

namespace NebulaAPI.GameState;

/// <summary>
///     Represents data about factory
/// </summary>
public interface IFactoryManager : IDisposable
{
    /// <summary>
    ///     Did we receive a packet?
    /// </summary>
    IToggle IsIncomingRequest { get; }

    int PacketAuthor { get; set; }

    int TargetPlanet { get; set; }

    PlanetFactory EventFactory { get; set; }

    /// <summary>
    ///     Request to load planet
    /// </summary>
    void AddPlanetTimer(int planetId);

    void LoadPlanetData(int planetId);

    void UnloadPlanetData(int planetId);

    void InitializePrebuildRequests();

    void SetPrebuildRequest(int planetId, int prebuildId, ushort playerId);

    bool RemovePrebuildRequest(int planetId, int prebuildId);

    bool ContainsPrebuildRequest(int planetId, int prebuildId);

    int GetNextPrebuildId(int planetId);

    int GetNextPrebuildId(PlanetFactory factory);
}
