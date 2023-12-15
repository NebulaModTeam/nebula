#region

using System;
using System.Collections.Generic;
using System.Linq;
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
    private static List<int> PendingBuildRequests = [];
    private static List<Vector3> CachedPositions = [];

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


    public static void BroadcastDroneOrder(int droneId, int entityId, int stage)
    {
        if (!Multiplayer.IsActive)
        {
            return;
        }

        var priority = 0;
        if (stage is 1 or 2)
        {
            priority = rnd.Next();
            DronePriorities[droneId] = priority;
        }
        else
        {
            //todo:replace
            //GameMain.mainPlayer.mecha.droneLogic.serving.Remove(entityId);
        }
        Multiplayer.Session.Network.SendPacketToLocalPlanet(new NewDroneOrderPacket(GameMain.mainPlayer.planetId, droneId,
            entityId, Multiplayer.Session.LocalPlayer.Id, stage, priority,
            GameMain.localPlanet.factory.prebuildPool[-entityId].pos));
    }

    public static void AddPlayerDronePlan(ushort playerId, int entityId)
    {
        if (!PlayerDroneBuildingPlans.TryGetValue(playerId, out var value))
        {
            value = [];
            PlayerDroneBuildingPlans.Add(playerId, value);
        }

        value.Add(entityId);
    }

    public static void RemovePlayerDronePlan(ushort playerId, int entityId)
    {
        if (PlayerDroneBuildingPlans.TryGetValue(playerId, out var value))
        {
            value.Remove(entityId);
        }
    }

    public static int[] GetPlayerDronePlans(ushort playerId)
    {
        return PlayerDroneBuildingPlans.TryGetValue(playerId, out var plan) ? plan.ToArray() : null;
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

    public static void ClearCachedPositions()
    {
        CachedPositions.Clear();

        using (Multiplayer.Session.World.GetRemotePlayersModels(out var remotePlayersModels))
        {
            foreach (var model in remotePlayersModels.Values.Where(model =>
                         model.Movement.GetLastPosition().LocalPlanetId == GameMain.mainPlayer.planetId))
            {
                //Cache players positions for this looking for target session
                CachedPositions.Add(model.Movement.GetLastPosition().LocalPlanetPosition.ToVector3());
            }
        }
    }

    public static bool IsLocalPlayerClosestTo(ref Vector3 entityPos)
    {
        if (!Multiplayer.IsActive)
        {
            return true;
        }

        var myDistance = (GameMain.mainPlayer.position - entityPos).sqrMagnitude;

        foreach (var playerPosition in CachedPositions)
        {
            //If remote player is closer, ignore the entity
            if (myDistance > (playerPosition - entityPos).sqrMagnitude)
            {
                return false;
            }
        }
        return true;
    }
}
