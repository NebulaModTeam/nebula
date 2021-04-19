using HarmonyLib;
using NebulaModel.DataStructures;
using NebulaModel.Logger;
using System.Collections.Generic;

namespace NebulaWorld.Factory
{
    public class FactoryManager
    {
        sealed class ThreadSafe
        {
            internal readonly Dictionary<PrebuildOwnerKey, ushort> prebuildRequests = new Dictionary<PrebuildOwnerKey, ushort>();
        }
        private static readonly ThreadSafe threadSafe = new ThreadSafe();

        static Locker GetPrebuildRequests(out Dictionary<PrebuildOwnerKey, ushort> prebuildRequests) =>
            threadSafe.prebuildRequests.GetLocked(out prebuildRequests);

        public static readonly ToggleSwitch EventFromServer = new ToggleSwitch();
        public static readonly ToggleSwitch EventFromClient = new ToggleSwitch();
        public static PlanetFactory EventFactory { get; set; }
        public static readonly ToggleSwitch IgnoreBasicBuildConditionChecks = new ToggleSwitch();
        public static readonly ToggleSwitch DoNotAddItemsFromBuildingOnDestruct = new ToggleSwitch();
        public static int PacketAuthor { get; set; }

        public static void Initialize()
        {
            PacketAuthor = -1;
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
            return prebuildRecycleCursor <= 0 ? factory.prebuildCursor : prebuildRecycle[prebuildRecycleCursor - 1];
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
