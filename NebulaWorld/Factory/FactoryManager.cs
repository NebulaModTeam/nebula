using BepInEx;
using HarmonyLib;
using NebulaModel.DataStructures;
using NebulaModel.Logger;
using NebulaModel.Packets.Factory.Inserter;
using System;
using System.Collections.Generic;
using System.Timers;
using UnityEngine;

namespace NebulaWorld.Factory
{
    public class FactoryManager
    {
        sealed class ThreadSafe
        {
            internal readonly Dictionary<PrebuildOwnerKey, ushort> PrebuildRequests = new Dictionary<PrebuildOwnerKey, ushort>();
            internal readonly Dictionary<int, Timer> PlanetTimers = new Dictionary<int, Timer>();
        }
        private static readonly ThreadSafe threadSafe = new ThreadSafe();

        static Locker GetPrebuildRequests(out Dictionary<PrebuildOwnerKey, ushort> prebuildRequests) =>
            threadSafe.PrebuildRequests.GetLocked(out prebuildRequests);

        static Locker GetPlanetTimers(out Dictionary<int, Timer> planetTimers) =>
            threadSafe.PlanetTimers.GetLocked(out planetTimers);

        public static readonly ToggleSwitch EventFromServer = new ToggleSwitch();
        public static readonly ToggleSwitch EventFromClient = new ToggleSwitch();
        public static PlanetFactory EventFactory { get; set; }
        public static readonly ToggleSwitch IgnoreBasicBuildConditionChecks = new ToggleSwitch();
        public static readonly ToggleSwitch DoNotAddItemsFromBuildingOnDestruct = new ToggleSwitch();

        public static int PacketAuthor { get; set; }
        public static int TargetPlanet { get; set; }
        public const int PLANET_NONE = -2;

        public static void Initialize()
        {
            PacketAuthor = -1;
            TargetPlanet = PLANET_NONE;
        }

        public static void AddPlanetTimer(int planetId)
        {
            using (GetPlanetTimers(out var planetTimers))
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

                    var planetTimer = planetTimers[planetId];
                    planetTimer.Elapsed += (sender, e) => PlanetTimer_Elapsed(planetId);
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

        private static void PlanetTimer_Elapsed(int planetId)
        {
            // Timer has finished without another event resetting it so we can unload the planet's data
            // We must use ThreadingHelper in order to ensure this runs on the main thread, otherwise this will trigger a crash
            ThreadingHelper.Instance.StartSyncInvoke(() => UnloadPlanetData(planetId));
            RemovePlanetTimer(planetId);
        }

        private static bool RemovePlanetTimer(int planetId)
        {
            using (GetPlanetTimers(out var planetTimers))
            {
                planetTimers[planetId].Stop();
                planetTimers[planetId] = null;
                return planetTimers.Remove(planetId);
            }
        }

        public static void LoadPlanetData(int planetId)
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

            if (AccessTools.Field(typeof(CargoTraffic), "beltRenderingBatch").GetValue(planet.factory.cargoTraffic) == null ||
                AccessTools.Field(typeof(CargoTraffic), "pathRenderingBatch").GetValue(planet.factory.cargoTraffic) == null)
            {
                planet.factory.cargoTraffic.CreateRenderingBatches();
            }
        }

        public static void UnloadPlanetData(int planetId)
        {
            // We don't want to unload the planet that we are currently on
            if (planetId == GameMain.localPlanet?.id)
            {
                return;
            }

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
        }

        public static void InitializePrebuildRequests()
        {
            //Load existing prebuilds to the dictionary so it will be ready to build
            if (LocalPlayer.IsMasterClient)
            {
                using (GetPrebuildRequests(out var prebuildRequests))
                {
                    foreach (PlanetFactory factory in GameMain.data.factories)
                    {
                        if (factory != null)
                        {
                            for (int i = 0; i < factory.prebuildCursor; i++)
                            {
                                if (factory.prebuildPool[i].id != 0)
                                {
                                    prebuildRequests[new PrebuildOwnerKey(factory.planetId, factory.prebuildPool[i].id)] = LocalPlayer.PlayerId;
                                }
                            }
                        }
                    }
                }
            }
        }

        public static void SetPrebuildRequest(int planetId, int prebuildId, ushort playerId)
        {
            using (GetPrebuildRequests(out var prebuildRequests))
                prebuildRequests[new PrebuildOwnerKey(planetId, prebuildId)] = playerId;
        }

        public static bool RemovePrebuildRequest(int planetId, int prebuildId)
        {
            using (GetPrebuildRequests(out var prebuildRequests))
                return prebuildRequests.Remove(new PrebuildOwnerKey(planetId, prebuildId));
        }

        public static bool ContainsPrebuildRequest(int planetId, int prebuildId)
        {
            using (GetPrebuildRequests(out var prebuildRequests))
                return prebuildRequests.ContainsKey(new PrebuildOwnerKey(planetId, prebuildId));
        }

        public static int GetNextPrebuildId(int planetId)
        {
            PlanetData planet = GameMain.galaxy.PlanetById(planetId);
            if (planet == null)
            {
                Log.Error($"Planet with id: {planetId} could not be found!!");
                return -1;
            }

            return GetNextPrebuildId(planet.factory);
        }

        public static void OnNewSetInserterPickTarget(int objId, int otherObjId, int inserterId, int offset, Vector3 pointPos)
        {
            if (SimulatedWorld.Initialized && LocalPlayer.PlayerId == PacketAuthor)
            {
                LocalPlayer.SendPacketToLocalStar(new NewSetInserterPickTargetPacket(objId, otherObjId, inserterId, offset, pointPos, GameMain.localPlanet?.id ?? -1));
            }
        }

        public static void OnNewSetInserterInsertTarget(int objId, int otherObjId, int inserterId, int offset, Vector3 pointPos)
        {
            if (SimulatedWorld.Initialized && LocalPlayer.PlayerId == PacketAuthor)
            {
                LocalPlayer.SendPacketToLocalStar(new NewSetInserterInsertTargetPacket(objId, otherObjId, inserterId, offset, pointPos, GameMain.localPlanet?.id ?? -1));
            }
        }

        static readonly AccessTools.FieldRef<object, int> GetPrebuildRecycleCursor =
            AccessTools.FieldRefAccess<int>(typeof(PlanetFactory), "prebuildRecycleCursor");

        static readonly AccessTools.FieldRef<object, int[]> GetPrebuildRecycle =
            AccessTools.FieldRefAccess<int[]>(typeof(PlanetFactory), "prebuildRecycle");

        public static int GetNextPrebuildId(PlanetFactory factory)
        {
            if (factory == null)
            {
                return -1;
            }


            int prebuildRecycleCursor = GetPrebuildRecycleCursor(factory);
            int[] prebuildRecycle = GetPrebuildRecycle(factory);
            return prebuildRecycleCursor <= 0 ? factory.prebuildCursor + 1 : prebuildRecycle[prebuildRecycleCursor - 1];
        }
    }

    struct PrebuildOwnerKey : System.IEquatable<PrebuildOwnerKey>
    {
        public readonly int PlanetId;
        public readonly int PrebuildId;

        public PrebuildOwnerKey(int planetId, int prebuildId)
        {
            this.PlanetId = planetId;
            this.PrebuildId = prebuildId;
        }

        public override int GetHashCode()
        {
            return PlanetId ^ PrebuildId;
        }

        public bool Equals(PrebuildOwnerKey other)
        {
            return other.PlanetId == this.PlanetId && other.PrebuildId == this.PrebuildId;
        }
    }
}
