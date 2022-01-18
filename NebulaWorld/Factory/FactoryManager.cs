using BepInEx;
using NebulaAPI;
using NebulaModel.DataStructures;
using NebulaModel.Logger;
using NebulaModel.Packets.Factory.Inserter;
using System;
using System.Collections.Generic;
using System.Timers;
using UnityEngine;

namespace NebulaWorld.Factory
{
    public class FactoryManager : IFactoryManager
    {
        private sealed class ThreadSafe
        {
            internal readonly Dictionary<PrebuildOwnerKey, ushort> PrebuildRequests = new Dictionary<PrebuildOwnerKey, ushort>();
            internal readonly Dictionary<int, Timer> PlanetTimers = new Dictionary<int, Timer>();
        }
        private readonly ThreadSafe threadSafe = new ThreadSafe();

        private Locker GetPrebuildRequests(out Dictionary<PrebuildOwnerKey, ushort> prebuildRequests)
        {
            return threadSafe.PrebuildRequests.GetLocked(out prebuildRequests);
        }

        private Locker GetPlanetTimers(out Dictionary<int, Timer> planetTimers)
        {
            return threadSafe.PlanetTimers.GetLocked(out planetTimers);
        }

        private readonly ToggleSwitch isIncomingRequest = new ToggleSwitch();
        public IToggle IsIncomingRequest => isIncomingRequest;

        public PlanetFactory EventFactory { get; set; }
        public int PacketAuthor { get; set; }
        public int TargetPlanet { get; set; }

        public FactoryManager()
        {
            PacketAuthor = NebulaModAPI.AUTHOR_NONE;
            TargetPlanet = NebulaModAPI.PLANET_NONE;
        }

        public void Dispose()
        {
        }

        public void AddPlanetTimer(int planetId)
        {
            using (GetPlanetTimers(out Dictionary<int, Timer> planetTimers))
            {
                // We don't want to load or unload the planet we are currently on
                if (planetId == GameMain.localPlanet?.id)
                {
                    return;
                }

                // .NET Framework 4.7.2 does not have TryAdd so we must make sure the dictionary does not already contain the planet
                if (!planetTimers.ContainsKey(planetId))
                {
                    // We haven't loaded this planet, let's load it
                    LoadPlanetData(planetId);

                    // Create a new 10 second timer for this planet
                    planetTimers.Add(planetId, new Timer(TimeSpan.FromSeconds(10).TotalMilliseconds));

                    Timer planetTimer = planetTimers[planetId];
                    planetTimer.Elapsed += (sender, e) => PlanetTimer_Elapsed(planetId);
                    planetTimer.AutoReset = false;
                    planetTimer.Start();
                }
                // If a timer for the planet already exists, reset it.
                else
                {
                    planetTimers[planetId].Stop();
                    planetTimers[planetId].Start();
                }
            }
        }

        private void PlanetTimer_Elapsed(int planetId)
        {
            RemovePlanetTimer(planetId);

            // Timer has finished without another event resetting it so we can unload the planet's data
            // We must use ThreadingHelper in order to ensure this runs on the main thread, otherwise this will trigger a crash
            ThreadingHelper.Instance.StartSyncInvoke(() => UnloadPlanetData(planetId));
        }

        private bool RemovePlanetTimer(int planetId)
        {
            using (GetPlanetTimers(out Dictionary<int, Timer> planetTimers))
            {
                planetTimers[planetId].Stop();
                planetTimers[planetId].Dispose();
                planetTimers[planetId] = null;
                return planetTimers.Remove(planetId);
            }
        }

        public void LoadPlanetData(int planetId)
        {
            PlanetData planet = GameMain.galaxy.PlanetById(planetId);

            if (planet.physics == null || planet.physics.colChunks == null)
            {
                planet.physics = new PlanetPhysics(planet);
                planet.physics.Init();
            }

            if (planet.aux == null)
            {
                planet.aux = new PlanetAuxData(planet);
            }

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

            int tmpTargetPlanet = Multiplayer.Session.Factories.TargetPlanet;
            Multiplayer.Session.Factories.TargetPlanet = planetId;

            PlanetData planet = GameMain.galaxy.PlanetById(planetId);

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
            if (Multiplayer.Session.LocalPlayer.IsHost)
            {
                using (GetPrebuildRequests(out Dictionary<PrebuildOwnerKey, ushort> prebuildRequests))
                {
                    foreach (PlanetFactory factory in GameMain.data.factories)
                    {
                        if (factory != null)
                        {
                            for (int i = 0; i < factory.prebuildCursor; i++)
                            {
                                if (factory.prebuildPool[i].id != 0)
                                {
                                    prebuildRequests[new PrebuildOwnerKey(factory.planetId, factory.prebuildPool[i].id)] = Multiplayer.Session.LocalPlayer.Id;
                                }
                            }
                        }
                    }
                }
            }
        }

        public void SetPrebuildRequest(int planetId, int prebuildId, ushort playerId)
        {
            using (GetPrebuildRequests(out Dictionary<PrebuildOwnerKey, ushort> prebuildRequests))
            {
                prebuildRequests[new PrebuildOwnerKey(planetId, prebuildId)] = playerId;
            }
        }

        public bool RemovePrebuildRequest(int planetId, int prebuildId)
        {
            using (GetPrebuildRequests(out Dictionary<PrebuildOwnerKey, ushort> prebuildRequests))
            {
                return prebuildRequests.Remove(new PrebuildOwnerKey(planetId, prebuildId));
            }
        }

        public bool ContainsPrebuildRequest(int planetId, int prebuildId)
        {
            using (GetPrebuildRequests(out Dictionary<PrebuildOwnerKey, ushort> prebuildRequests))
            {
                return prebuildRequests.ContainsKey(new PrebuildOwnerKey(planetId, prebuildId));
            }
        }

        public int GetNextPrebuildId(int planetId)
        {
            PlanetData planet = GameMain.galaxy.PlanetById(planetId);
            if (planet == null)
            {
                Log.Error($"Planet with id: {planetId} could not be found!!");
                return -1;
            }

            return GetNextPrebuildId(planet.factory);
        }

        public int GetNextPrebuildId(PlanetFactory factory)
        {
            if (factory == null)
            {
                return -1;
            }


            int prebuildRecycleCursor = factory.prebuildRecycleCursor;
            int[] prebuildRecycle = factory.prebuildRecycle;
            return prebuildRecycleCursor <= 0 ? factory.prebuildCursor + 1 : prebuildRecycle[prebuildRecycleCursor - 1];
        }

        public void OnNewSetInserterPickTarget(int objId, int otherObjId, int inserterId, int offset, Vector3 pointPos)
        {
            // 1. Client receives the build request from itself 2. Host sends the build request
            if (Multiplayer.IsActive && (Multiplayer.Session.LocalPlayer.Id == PacketAuthor || (Multiplayer.Session.LocalPlayer.IsHost && PacketAuthor == NebulaModAPI.AUTHOR_NONE)))
            {
                Multiplayer.Session.Network.SendPacketToLocalStar(new NewSetInserterPickTargetPacket(objId, otherObjId, inserterId, offset, pointPos, GameMain.localPlanet?.id ?? -1));
            }
        }

        public void OnNewSetInserterInsertTarget(int objId, int otherObjId, int inserterId, int offset, Vector3 pointPos)
        {
            if (Multiplayer.IsActive && Multiplayer.Session.LocalPlayer.Id == PacketAuthor || (Multiplayer.Session.LocalPlayer.IsHost && PacketAuthor == NebulaModAPI.AUTHOR_NONE))
            {
                Multiplayer.Session.Network.SendPacketToLocalStar(new NewSetInserterInsertTargetPacket(objId, otherObjId, inserterId, offset, pointPos, GameMain.localPlanet?.id ?? -1));
            }
        }
    }

    internal readonly struct PrebuildOwnerKey : IEquatable<PrebuildOwnerKey>
    {
        public readonly int PlanetId;
        public readonly int PrebuildId;

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
    }
}
