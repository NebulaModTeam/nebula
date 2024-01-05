#region

using System;
using System.Collections.Generic;
using System.Linq;
using NebulaAPI.DataStructures;
using NebulaModel.DataStructures;
using NebulaModel.Packets.Players;
using UnityEngine;
using Random = UnityEngine.Random;

#endregion

namespace NebulaWorld.Player;

public class DroneManager : IDisposable
{
    private static Dictionary<ushort, List<int>> PlayerDroneBuildingPlans = [];
    private static Dictionary<int, long> PendingBuildRequests = [];
    private static Dictionary<ushort, PlayerPosition> CachedPositions = [];
    private static long lastCheckedTick = 0;

    public DroneManager()
    {
        PlayerDroneBuildingPlans = [];
        PendingBuildRequests = [];
        CachedPositions = [];
        lastCheckedTick = 0;
    }

    public void Dispose()
    {
        PlayerDroneBuildingPlans = null;
        PendingBuildRequests = null;
        CachedPositions = null;
        lastCheckedTick = 0;
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
        foreach (var kvp in PlayerDroneBuildingPlans.Where(kvp => kvp.Value.Contains(entityId)))
        {
            RemovePlayerDronePlan(kvp.Key, entityId);
            return;
        }
    }

    public static void RemovePlayerDronePlans(ushort playerId)
    {
        PlayerDroneBuildingPlans.Remove(playerId);
    }

    public static int[] GetPlayerDronePlans(ushort playerId)
    {
        return PlayerDroneBuildingPlans.TryGetValue(playerId, out var plan) ? [.. plan] : null;
    }
    public static int GetPlayerDronePlansCount(ushort playerId)
    {
        return PlayerDroneBuildingPlans.TryGetValue(playerId, out var plan) ? plan.Count : 0;
    }

    // intended to be called on the host only
    public static void RemoveOrphanDronePlans(List<ushort> allPlayerIds)
    {
        foreach (var kv in PlayerDroneBuildingPlans)
        {
            if (allPlayerIds.Contains(kv.Key) || !CachedPositions.ContainsKey(kv.Key))
            {
                continue;
            }
            var factory = GameMain.galaxy.PlanetById(CachedPositions[kv.Key].PlanetId).factory;
            var dronePlans = GetPlayerDronePlans(kv.Key);
            if (dronePlans.Length > 0)
            {
                var player = Multiplayer.Session.Network.PlayerManager.GetPlayerById(kv.Key);
                Multiplayer.Session.Network.SendPacketToPlanet(new RemoveDroneOrdersPacket(dronePlans, CachedPositions[kv.Key].PlanetId),
                    player.Data.LocalPlanetId);

                for (var i = 1; i < factory.constructionSystem.drones.cursor; i++)
                {
                    ref var drone = ref factory.constructionSystem.drones.buffer[i];
                    if (!dronePlans.Contains(drone.targetObjectId))
                    {
                        continue;
                    }
                    // recycle drones from other player, removing them visually
                    // RecycleDrone_Postfix takes care of removing drones from mecha that do not belong to us
                    RemoveBuildRequest(drone.targetObjectId);
                    GameMain.mainPlayer.mecha.constructionModule.RecycleDrone(factory, ref drone);
                }
            }

            PlayerDroneBuildingPlans.Remove(kv.Key);
            CachedPositions.Remove(kv.Key);
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
        var isPresent = PendingBuildRequests.ContainsKey(entityId);
        if (!isPresent || !Multiplayer.Session.LocalPlayer.IsClient)
        {
            return isPresent;
        }
        // clients can run in a situation where they have requested sending out drones but never received an answer, potentially leading to a deadlock
        // thus we need to free up requests after a specific amount of time if there was no response yet.
        if (GameMain.gameTick - PendingBuildRequests[entityId] <= 600)
        {
            return true;
        }
        Multiplayer.Session.Network.SendPacket(new RemoveDroneOrdersPacket(new[] { entityId }, GameMain.mainPlayer.planetId));
        RemoveBuildRequest(entityId);
        GameMain.galaxy.PlanetById(GameMain.mainPlayer.planetId)?.factory?.constructionSystem.constructServing.Remove(entityId);
        return false;
    }

    public static int CountPendingBuildRequest()
    {
        return PendingBuildRequests.Count;
    }

    public static void RemoveBuildRequest(int entityId)
    {
        PendingBuildRequests.Remove(entityId);
    }

    public static void EjectDronesOfOtherPlayer(ushort playerId, int planetId, int targetObjectId)
    {
        RefreshCachedPositions();

        var ejectPos = GetPlayerPosition(playerId);
        ejectPos = ejectPos.normalized * (ejectPos.magnitude + 2.8f);
        var factory = GameMain.galaxy.PlanetById(planetId).factory;
        var targetPos = factory.constructionSystem._obj_hpos(targetObjectId, ref ejectPos);
        var vector3 = ejectPos + ejectPos.normalized * 4.5f +
                      ((targetPos - ejectPos).normalized + Random.insideUnitSphere) * 1.5f;

        ref var ptr =
            ref GameMain.mainPlayer.mecha.constructionModule.CreateDrone(factory, ejectPos, Quaternion.LookRotation(vector3),
                Vector3.zero);
        ptr.stage = 1;
        ptr.targetObjectId = targetObjectId;
        ptr.targetPos = targetPos;
        ptr.initialVector = vector3;
        ptr.progress = 0f;
        ptr.priority = 1;
        ptr.owner = playerId *
                    -1; // to prevent the ConstructionSystem_Transpiler.UpdateDrones_Transpiler() to remove them. Must be negative, positive ones are owned by battle bases. Store playerId in here.
    }

    public static void RefreshCachedPositions()
    {
        if (GameMain.gameTick - lastCheckedTick > 10)
        {
            lastCheckedTick = GameMain.gameTick;
            //CachedPositions.Clear();

            using (Multiplayer.Session.World.GetRemotePlayersModels(out var remotePlayersModels))
            {
                // host needs it for all players since they can build on other planets too.
                foreach (var model in remotePlayersModels.Values)
                {
                    // Cache players positions
                    if (!CachedPositions.ContainsKey(model.Movement.PlayerID))
                    {
                        CachedPositions.Add(model.Movement.PlayerID, new PlayerPosition(model.Movement.GetLastPosition().LocalPlanetPosition.ToVector3(), model.Movement.localPlanetId));
                    }
                    else
                    {
                        CachedPositions[model.Movement.PlayerID].Position = model.Movement.GetLastPosition().LocalPlanetPosition.ToVector3();
                        CachedPositions[model.Movement.PlayerID].PlanetId = model.Movement.localPlanetId;
                    }
                }
            }
        }
    }

    public static ushort GetClosestPlayerTo(int planetId, ref Vector3 entityPos)
    {
        if (!Multiplayer.IsActive)
        {
            return 0;
        }

        var shortestDistance = 0.0f;
        ushort nearestPlayer = 0;

        if (GameMain.mainPlayer.planetId == planetId)
        {
            nearestPlayer = 1; // this is host
            shortestDistance = (GameMain.mainPlayer.position - entityPos).sqrMagnitude;
        }

        foreach (var playerPosition in CachedPositions.Where(playerPosition =>
                     playerPosition.Value.PlanetId == planetId))
        {
            if (nearestPlayer == 0)
            {
                nearestPlayer = playerPosition.Key;
                shortestDistance = (playerPosition.Value.Position - entityPos).sqrMagnitude;

                continue;
            }

            var dist = (playerPosition.Value.Position - entityPos).sqrMagnitude;
            if (!(shortestDistance > dist))
            {
                continue;
            }
            shortestDistance = dist;
            nearestPlayer = playerPosition.Key;
        }
        return nearestPlayer;
    }

    public static ushort GetNextClosestPlayerToAfter(int planetId, ushort afterPlayerId, ref Vector3 entityPos)
    {
        if (!Multiplayer.IsActive)
        {
            return 0;
        }

        var afterPlayerDistance = (GetPlayerPosition(afterPlayerId) - entityPos).sqrMagnitude;
        var nextShortestDistance = afterPlayerDistance;
        var nearestPlayer = afterPlayerId;

        foreach (var playerPosition in CachedPositions.Where(playerPosition =>
                     playerPosition.Value.PlanetId == planetId))
        {
            var dist = (playerPosition.Value.Position - entityPos).sqrMagnitude;

            if (nextShortestDistance == afterPlayerDistance && dist > nextShortestDistance)
            {
                nextShortestDistance = dist;
                nearestPlayer = playerPosition.Key;
            }
            else if (nextShortestDistance > dist && dist > afterPlayerDistance)
            {
                nextShortestDistance = dist;
                nearestPlayer = playerPosition.Key;
            }
        }

        return nearestPlayer;
    }

    public static Vector3 GetPlayerPosition(ushort playerId)
    {
        return CachedPositions.TryGetValue(playerId, out var value) ? value.Position : GameMain.mainPlayer.position;
    }
}
