using HarmonyLib;
using NebulaModel.DataStructures;
using NebulaModel.Logger;

namespace NebulaWorld.Factory
{
    public class FactoryManager
    {
        private static ThreadSafeDictionary<PrebuildOwnerKey, ushort> prebuildRequests;

        public static bool EventFromServer { get; set; }
        public static bool EventFromClient { get; set; }
        public static PlanetFactory EventFactory { get; set; }
        public static bool IgnoreBasicBuildConditionChecks { get; set; }
        public static bool DoNotAddItemsFromBuildingOnDestruct { get; set; }
        public static int PacketAuthor { get; set; }

        public static void Initialize()
        {
            prebuildRequests = new ThreadSafeDictionary<PrebuildOwnerKey, ushort>();
            EventFromServer = false;
            EventFromClient = false;
            IgnoreBasicBuildConditionChecks = false;
            DoNotAddItemsFromBuildingOnDestruct = false;
            PacketAuthor = -1;
        }

        public static void SetPrebuildRequest(int planetId, int prebuildId, ushort playerId)
        {
            prebuildRequests[new PrebuildOwnerKey(planetId, prebuildId)] = playerId;
        }

        public static void RemovePrebuildRequest(int planetId, int prebuildId)
        {
            prebuildRequests.Remove(new PrebuildOwnerKey(planetId, prebuildId));
        }

        public static bool ContainsPrebuildRequest(int planetId, int prebuildId)
        {
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

        public static int GetNextPrebuildId(PlanetFactory factory)
        {
            if (factory == null)
            {
                return -1;
            }

            int prebuildRecycleCursor = (int)AccessTools.Field(typeof(PlanetFactory), "prebuildRecycleCursor").GetValue(factory);
            int[] prebuildRecycle = (int[])AccessTools.Field(typeof(PlanetFactory), "prebuildRecycle").GetValue(factory);
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

        public bool Equals(PrebuildOwnerKey other)
        {
            return other.PlanetId == this.PlanetId && other.PrebuildId == this.PrebuildId;
        }
    }
}
