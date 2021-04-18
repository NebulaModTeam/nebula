using NebulaModel.Packets.Players;
using System;
using System.Collections.Generic;

namespace NebulaWorld.Player
{
    public static class DroneManager
    {
        public static int[] DronePriorities = new int[255];
        public static Random rnd = new Random();
        public static Dictionary<ushort, List<int>> PlayerDroneBuildingPlans = new Dictionary<ushort, List<int>>();
        public static List<int> PendingBuildRequests = new List<int>();

        public static void BroadcastDroneOrder(int droneId, int entityId, int stage)
        {
            if (!SimulatedWorld.Initialized)
            {
                return;
            }

            int priority = 0;
            if (stage == 1 || stage == 2)
            {
                priority = rnd.Next();
                DronePriorities[droneId] = priority;
            }
            else
            {
                GameMain.mainPlayer.mecha.droneLogic.serving.Remove(entityId);
            }
            LocalPlayer.SendPacketToLocalPlanet(new NewDroneOrderPacket(GameMain.mainPlayer.planetId, droneId, entityId, LocalPlayer.PlayerId, stage, priority, GameMain.localPlanet.factory.prebuildPool[-entityId].pos));
        }

        public static void AddPlayerDronePlan(ushort playerId, int entityId)
        {
            if (!PlayerDroneBuildingPlans.ContainsKey(playerId))
            {
                PlayerDroneBuildingPlans.Add(playerId, new List<int>());
            }
            PlayerDroneBuildingPlans[playerId].Add(entityId);
        }

        public static void RemovePlayerDronePlan(ushort playerId, int entityId)
        {
            if (PlayerDroneBuildingPlans.ContainsKey(playerId))
            {
                PlayerDroneBuildingPlans[playerId].Remove(entityId);
            }
        }

        public static int[] GetPlayerDronePlans(ushort playerId)
        {
            if (PlayerDroneBuildingPlans.ContainsKey(playerId))
            {
                return PlayerDroneBuildingPlans[playerId].ToArray();
            }
            return null;
        }

        public static void AddBuildRequestSent(int entityId)
        {
            PendingBuildRequests.Add(entityId);
        }

        public static bool IsPendingBuildRequest(int entityId)
        {
            return PendingBuildRequests.Contains(entityId);
        }

        public static void RemoveBuildRequest(int entityId)
        {
            PendingBuildRequests.Remove(entityId);
        }
    }
}
