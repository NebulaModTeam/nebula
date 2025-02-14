#region

using System;
using System.Collections.Generic;
using System.Timers;
using BepInEx;
using NebulaAPI;
using NebulaAPI.DataStructures;
using NebulaAPI.GameState;
using NebulaAPI.Interfaces;
using NebulaModel.DataStructures;
using NebulaModel.Logger;
using NebulaModel.Packets.Factory.Inserter;
using UnityEngine;

#endregion

namespace NebulaWorld.Factory;

public class FactoryManager : IFactoryManager
{
    private readonly ToggleSwitch isIncomingRequest = new();
    private readonly ThreadSafe threadSafe = new();

    public IToggle IsIncomingRequest => isIncomingRequest;

    public PlanetFactory EventFactory { get; set; }
    public int PacketAuthor { get; set; } = NebulaModAPI.AUTHOR_NONE;
    public int TargetPlanet { get; set; } = NebulaModAPI.PLANET_NONE;

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }

    public void AddPlanetTimer(int planetId)
    {
        using (GetPlanetTimers(out var planetTimers))
        {
            // We don't want to load or unload the planet we are currently on
            if (planetId == GameMain.localPlanet?.id)
            {
                if (GameMain.localPlanet != null && !GameMain.localPlanet.factoryLoaded)
                {
                    // Local planet is loading, for debug purpose print a warning message
                    Log.Warn("Local PlanetFactory is still loading!");
                }
                return;
            }

            // .NET Framework 4.7.2 does not have TryAdd so we must make sure the dictionary does not already contain the planet
            if (!planetTimers.TryGetValue(planetId, out var value))
            {
                // We haven't loaded this planet, let's load it
                LoadPlanetData(planetId);

                // Create a new 10 second timer for this planet
                planetTimers.Add(planetId, new Timer(TimeSpan.FromSeconds(10).TotalMilliseconds));

                var planetTimer = planetTimers[planetId];
                planetTimer.Elapsed += (sender, e) => PlanetTimer_Elapsed(planetId);
                planetTimer.AutoReset = false;
                planetTimer.Start();
            }
            // If a timer for the planet already exists, reset it.
            else
            {
                // Reload the planet if it is unloaded by game.
                LoadPlanetData(planetId);
                value.Stop();
                value.Start();
            }
        }
    }

    public void LoadPlanetData(int planetId)
    {
        var planet = GameMain.galaxy.PlanetById(planetId);

        if (planet.physics?.colChunks == null)
        {
            planet.physics = new PlanetPhysics(planet);
            planet.physics.Init();
        }

        planet.aux ??= new PlanetAuxData(planet);

        if (planet.audio == null)
        {
            planet.audio = new PlanetAudio(planet);
            planet.audio.Init();
        }

        if (planet.factory.cargoTraffic.beltRenderingBatch == null ||
            planet.factory.cargoTraffic.pathRenderingBatch == null)
        {
            planet.factory.cargoTraffic.CreateRenderingBatches();
        }
    }

    public void UnloadPlanetData(int planetId)
    {
        // We don't want to unload the planet that we are currently on
        if (planetId == GameMain.localPlanet?.id)
        {
            return;
        }

        var tmpTargetPlanet = Multiplayer.Session.Factories.TargetPlanet;
        Multiplayer.Session.Factories.TargetPlanet = planetId;

        var planet = GameMain.galaxy.PlanetById(planetId);

        if (planet.physics != null)
        {
            planet.physics.Free();
            planet.physics = null;
        }

        if (planet.audio != null)
        {
            planet.audio.Free();
            planet.audio = null;
        }

        planet.factory.cargoTraffic.DestroyRenderingBatches();

        Multiplayer.Session.Factories.TargetPlanet = tmpTargetPlanet;
    }

    public void InitializePrebuildRequests()
    {
        //Load existing prebuilds to the dictionary so it will be ready to build
        if (!Multiplayer.Session.LocalPlayer.IsHost)
        {
            return;
        }
        using (GetPrebuildRequests(out var prebuildRequests))
        {
            foreach (var factory in GameMain.data.factories)
            {
                if (factory == null)
                {
                    continue;
                }
                for (var i = 0; i < factory.prebuildCursor; i++)
                {
                    if (factory.prebuildPool[i].id != 0)
                    {
                        prebuildRequests[new PrebuildOwnerKey(factory.planetId, factory.prebuildPool[i].id)] =
                            Multiplayer.Session.LocalPlayer.Id;
                    }
                }
            }
        }
    }

    public void SetPrebuildRequest(int planetId, int prebuildId, ushort playerId)
    {
        using (GetPrebuildRequests(out var prebuildRequests))
        {
            prebuildRequests[new PrebuildOwnerKey(planetId, prebuildId)] = playerId;
        }
    }

    public bool RemovePrebuildRequest(int planetId, int prebuildId)
    {
        using (GetPrebuildRequests(out var prebuildRequests))
        {
            return prebuildRequests.Remove(new PrebuildOwnerKey(planetId, prebuildId));
        }
    }

    public bool ContainsPrebuildRequest(int planetId, int prebuildId)
    {
        using (GetPrebuildRequests(out var prebuildRequests))
        {
            return prebuildRequests.ContainsKey(new PrebuildOwnerKey(planetId, prebuildId));
        }
    }

    public int GetNextPrebuildId(int planetId)
    {
        var planet = GameMain.galaxy.PlanetById(planetId);
        if (planet != null)
        {
            return GetNextPrebuildId(planet.factory);
        }
        Log.Error($"Planet with id: {planetId} could not be found!!");
        return -1;

    }

    public int GetNextPrebuildId(PlanetFactory factory)
    {
        if (factory == null)
        {
            return -1;
        }


        var prebuildRecycleCursor = factory.prebuildRecycleCursor;
        var prebuildRecycle = factory.prebuildRecycle;
        return prebuildRecycleCursor <= 0 ? factory.prebuildCursor + 1 : prebuildRecycle[prebuildRecycleCursor - 1];
    }

    private Locker GetPrebuildRequests(out Dictionary<PrebuildOwnerKey, ushort> prebuildRequests)
    {
        return threadSafe.PrebuildRequests.GetLocked(out prebuildRequests);
    }

    private Locker GetPlanetTimers(out Dictionary<int, Timer> planetTimers)
    {
        return threadSafe.PlanetTimers.GetLocked(out planetTimers);
    }

    private void PlanetTimer_Elapsed(int planetId)
    {
        RemovePlanetTimer(planetId);

        // Timer has finished without another event resetting it so we can unload the planet's data
        // We must use ThreadingHelper in order to ensure this runs on the main thread, otherwise this will trigger a crash
        ThreadingHelper.Instance.StartSyncInvoke(() => UnloadPlanetData(planetId));
    }

    public bool RemovePlanetTimer(int planetId)
    {
        using (GetPlanetTimers(out var planetTimers))
        {
            if (planetTimers.TryGetValue(planetId, out var planetTimer))
            {
                planetTimer.Stop();
                planetTimer.Dispose();
                planetTimers.Remove(planetId);
                return true;
            }
            return false;
        }
    }

    public static int GetNextEntityId(PlanetFactory factory)
    {
        if (factory == null)
        {
            return -1;
        }

        return factory.entityRecycleCursor <= 0 ? factory.entityCursor : factory.entityRecycle[factory.entityRecycleCursor - 1];
    }

    public static int GetObjectProtoId(PlanetFactory factory, int objId)
    {
        if (objId == 0)
        {
            return 0;
        }
        if (objId > 0)
        {
            return objId < factory.entityPool.Length ? factory.entityPool[objId].protoId : -1;
        }
        return -objId < factory.prebuildPool.Length ? factory.prebuildPool[-objId].protoId : -1;
    }

    private sealed class ThreadSafe
    {
        internal readonly Dictionary<int, Timer> PlanetTimers = new();
        internal readonly Dictionary<PrebuildOwnerKey, ushort> PrebuildRequests = new();
    }
}

internal readonly struct PrebuildOwnerKey : IEquatable<PrebuildOwnerKey>
{
    private readonly int PlanetId;
    private readonly int PrebuildId;

    public PrebuildOwnerKey(int planetId, int prebuildId)
    {
        PlanetId = planetId;
        PrebuildId = prebuildId;
    }

    public override int GetHashCode()
    {
        return PlanetId ^ PrebuildId;
    }

    public bool Equals(PrebuildOwnerKey other)
    {
        return other.PlanetId == PlanetId && other.PrebuildId == PrebuildId;
    }

    public override bool Equals(object obj)
    {
        return obj is PrebuildOwnerKey key && Equals(key);
    }
}
