#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using NebulaAPI.DataStructures;
using NebulaModel.Packets.Players;
using UnityEngine;
using Random = System.Random;

#endregion

namespace NebulaWorld.Player;

public class DroneManager : IDisposable
{
    private static int[] DronePriorities = new int[255];
    private static readonly Random rnd = new();
    private static Dictionary<ushort, List<int>> PlayerDroneBuildingPlans = [];
    private static Dictionary<int, long> PendingBuildRequests = [];
    private static Dictionary<ushort, Vector3> CachedPositions = [];

    public DroneManager()
    {
        DronePriorities = new int[255];
        PlayerDroneBuildingPlans = [];
        PendingBuildRequests = [];
        CachedPositions = [];
    }

    public void Dispose()
    {
        DronePriorities = null;
        PlayerDroneBuildingPlans = null;
        PendingBuildRequests = null;
        CachedPositions = null;
        GC.SuppressFinalize(this);
    }

    public static void AddPlayerDronePlan(ushort playerId, int entityId)
    {
        if (!PlayerDroneBuildingPlans.TryGetValue(playerId, out var value))
        {
            value = [];
            PlayerDroneBuildingPlans.Add(playerId, value);
        }

        if (!value.Contains(entityId))
        {
            value.Add(entityId);
        }
    }

    public static void RemovePlayerDronePlan(ushort playerId, int entityId)
    {
        if (PlayerDroneBuildingPlans.TryGetValue(playerId, out var value))
        {
            value.Remove(entityId);
        }
    }
    public static void RemovePlayerDronePlan(int entityId)
    {
        foreach (KeyValuePair<ushort, List<int>> kvp in PlayerDroneBuildingPlans)
        {
            if (kvp.Value.Contains(entityId))
            {
                RemovePlayerDronePlan(kvp.Key, entityId);
                return;
            }
        }
    }
    public static void RemovePlayerDronePlans(ushort playerId)
    {
        if (PlayerDroneBuildingPlans.ContainsKey(playerId))
        {
            PlayerDroneBuildingPlans.Remove(playerId);
        }
    }

    public static int[] GetPlayerDronePlans(ushort playerId)
    {
        return PlayerDroneBuildingPlans.TryGetValue(playerId, out var plan) ? plan.ToArray() : null;
    }

    // intended to be called on the host only
    public static void RemoveOrphanDronePlans(List<ushort> allPlayerIds)
    {
        foreach (KeyValuePair<ushort, List<int>> kv in PlayerDroneBuildingPlans)
        {
            if (!allPlayerIds.Contains(kv.Key))
            {
                PlanetFactory factory = GameMain.galaxy.PlanetById(GameMain.mainPlayer.planetId).factory;
                var DronePlans = GetPlayerDronePlans(kv.Key);
                if (DronePlans.Length > 0)
                {
                    var player = Multiplayer.Session.Network.PlayerManager.GetPlayerById(kv.Key);
                    Multiplayer.Session.Network.SendPacketToPlanet(new RemoveDroneOrdersPacket(DronePlans), player.Data.LocalPlanetId);

                    for (int i = 1; i < factory.constructionSystem.drones.cursor; i++)
                    {
                        ref DroneComponent drone = ref factory.constructionSystem.drones.buffer[i];
                        if (DronePlans.Contains(drone.targetObjectId))
                        {
                            // recycle drones from other player, removing them visually
                            // RecycleDrone_Postfix takes care of removing drones from mecha that do not belong to us
                            RemoveBuildRequest(drone.targetObjectId);
                            GameMain.mainPlayer.mecha.constructionModule.RecycleDrone(factory, ref drone);
                        }
                    }
                }

                PlayerDroneBuildingPlans.Remove(kv.Key);
            }
        }
    }

    public static void AddBuildRequest(int entityId)
    {
        if (!PendingBuildRequests.ContainsKey(entityId))
        {
            PendingBuildRequests.Add(entityId, GameMain.gameTick);
        }
    }

    public static bool IsPendingBuildRequest(int entityId)
    {
        bool isPresent = PendingBuildRequests.ContainsKey(entityId);
        if (isPresent && Multiplayer.Session.LocalPlayer.IsClient)
        {
            // clients can run in a situation where they have requested sending out drones but never received an answer, potentially leading to a deadlock
            // thus we need to free up requests after a specific amount of time if there was no response yet.
            if (GameMain.gameTick - PendingBuildRequests[entityId] > 800)
            {
                Multiplayer.Session.Network.SendPacket(new RemoveDroneOrdersPacket(new int[] { entityId }));
                RemoveBuildRequest(entityId);
                GameMain.galaxy.PlanetById(GameMain.mainPlayer.planetId)?.factory?.constructionSystem.constructServing.Remove(entityId);
                isPresent = false;
            }
        }
        return isPresent;
    }

    public static int CountPendingBuildRequest()
    {
        return PendingBuildRequests.Count;
    }

    public static void RemoveBuildRequest(int entityId)
    {
        bool res = PendingBuildRequests.Remove(entityId);
    }

    public static void EjectDronesOfOtherPlayer(ushort playerId, int planetId, int targetObjectId)
    {
        ClearCachedPositions();

        var ejectPos = GetPlayerPosition(playerId);
        ejectPos = ejectPos.normalized * (ejectPos.magnitude + 2.8f);
        PlanetFactory factory = GameMain.galaxy.PlanetById(planetId).factory;
        Vector3 targetPos = factory.constructionSystem._obj_hpos(targetObjectId, ref ejectPos);
        Vector3 vector3 = ejectPos + ejectPos.normalized * 4.5f + ((targetPos - ejectPos).normalized + UnityEngine.Random.insideUnitSphere) * 1.5f;

        ref DroneComponent ptr = ref GameMain.mainPlayer.mecha.constructionModule.CreateDrone(factory, ejectPos, Quaternion.LookRotation(vector3), Vector3.zero);
        ptr.stage = 1;
        ptr.targetObjectId = targetObjectId;
        ptr.targetPos = targetPos;
        ptr.initialVector = vector3;
        ptr.progress = 0f;
        ptr.priority = 1;
        ptr.owner = playerId * -1; // to prevent the ConstructionSystem_Transpiler.UpdateDrones_Transpiler() to remove them. Must be negative, positive ones are owned by battle bases. Store playerId in here.
    }

    public static void ClearCachedPositions()
    {
        CachedPositions.Clear();

        using (Multiplayer.Session.World.GetRemotePlayersModels(out var remotePlayersModels))
        {
            // host needs it for all players since they can build on other planets too.
            foreach (var model in remotePlayersModels.Values)
            {
                //Cache players positions for this looking for target session
                CachedPositions.Add(model.Movement.PlayerID, model.Movement.GetLastPosition().LocalPlanetPosition.ToVector3());
            }
        }
    }

    public static ushort GetClosestPlayerTo(int planetId, ref Vector3 entityPos)
    {
        if (!Multiplayer.IsActive)
        {
            return 0;
        }

        var playerManager = Multiplayer.Session.Network.PlayerManager;

        float shortestDistance = 0.0f;
        ushort nearestPlayer = 0;

        if (GameMain.mainPlayer.planetId == planetId)
        {
            nearestPlayer = 1; // this is host
            shortestDistance = (GameMain.mainPlayer.position - entityPos).sqrMagnitude;
        }

        foreach (var playerPosition in CachedPositions)
        {
            if (playerManager.GetPlayerById(playerPosition.Key).Data.LocalPlanetId != planetId)
            {
                continue;
            }
            if (nearestPlayer == 0)
            {
                nearestPlayer = playerPosition.Key;
                shortestDistance = (playerPosition.Value - entityPos).sqrMagnitude;

                continue;
            }

            var dist = (playerPosition.Value - entityPos).sqrMagnitude;
            if (shortestDistance > dist)
            {
                shortestDistance = dist;
                nearestPlayer = playerPosition.Key;
            }
        }
        return nearestPlayer;
    }

    public static ushort GetNextClosestPlayerToAfter(int planetId, ushort afterPlayerId, ref Vector3 entityPos)
    {
        if (!Multiplayer.IsActive)
        {
            return 0;
        }

        var playerManager = Multiplayer.Session.Network.PlayerManager;

        var afterPlayerDistance = (GetPlayerPosition(afterPlayerId) - entityPos).sqrMagnitude;
        float maxShortestDistance = afterPlayerDistance;
        float nextShortestDistance = maxShortestDistance;
        ushort nearestPlayer = afterPlayerId;

        foreach (var playerPosition in CachedPositions)
        {
            if (playerManager.GetPlayerById(playerPosition.Key).Data.LocalPlanetId != planetId)
            {
                continue;
            }

            var dist = (playerPosition.Value - entityPos).sqrMagnitude;

            if (nextShortestDistance == maxShortestDistance && dist > nextShortestDistance)
            {
                nextShortestDistance = dist;
                nearestPlayer = playerPosition.Key;
            }
            else if (nextShortestDistance > dist && dist > maxShortestDistance)
            {
                nextShortestDistance = dist;
                nearestPlayer = playerPosition.Key;
            }
        }

        return nearestPlayer;
    }

    public static Vector3 GetPlayerPosition(ushort playerId)
    {
        if (CachedPositions.ContainsKey(playerId))
        {
            return CachedPositions[playerId];
        }
        return GameMain.mainPlayer.position;
    }
}
