#region

using System;
using System.Collections.Generic;
using NebulaAPI.DataStructures;
using NebulaModel.DataStructures;
using NebulaModel.Packets.Players;
using UnityEngine;
using Random = UnityEngine.Random;
#pragma warning disable IDE1006 // Naming Styles

#endregion

namespace NebulaWorld.Player;

public class DroneManager : IDisposable
{
    public const float MinSqrDistance = 225.0f;

    private readonly Dictionary<ushort, PlayerPosition> cachedPositions = [];
    private Vector3[] localPlayerPos = new Vector3[2];
    private int localPlayerCount = 0;
    private long lastCheckedTick = 0;
    private readonly List<CraftData> crafts = [];
    private readonly Stack<int> craftRecyleIds = [];
    private readonly DataPool<DroneComponent> drones = new();

    public DroneManager()
    {
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }

    public void EjectMechaDroneFromOtherPlayer(PlayerEjectMechaDronePacket packet)
    {
        RefreshCachedPositions();

        var ejectPos = GetPlayerEjectPosition(packet.PlayerId);
        var factory = GameMain.galaxy.PlanetById(packet.PlanetId).factory;
        var targetPos = factory.constructionSystem._obj_hpos(packet.TargetObjectId, ref ejectPos);
        var initialVector = ejectPos + ejectPos.normalized * 4.5f +
                      ((targetPos - ejectPos).normalized + Random.insideUnitSphere) * 1.5f;
        // Use custom CreateDrone to store in separate pool
        ref var ptr = ref CreateDrone(factory, ejectPos, Quaternion.LookRotation(initialVector), Vector3.zero, packet.PlayerId);
        ptr.stage = 1;
        ptr.priority = packet.DronePriority;
        ptr.targetObjectId = packet.TargetObjectId;
        ptr.nextTarget1ObjectId = packet.Next1ObjectId;
        ptr.nextTarget2ObjectId = packet.Next2ObjectId;
        ptr.nextTarget3ObjectId = packet.Next3ObjectId;
        ptr.targetPos = targetPos;
        ptr.initialVector = initialVector;
        ptr.progress = 0f;
        ptr.owner = packet.PlanetId; // Use drone.owner field to store planetId

        if (packet.TargetObjectId > 0) // Repair
        {
            ref var entity = ref factory.entityPool[packet.TargetObjectId];
            if (entity.id != packet.TargetObjectId || entity.constructStatId == 0)
            {
                return;
            }
            factory.constructionSystem.constructStats.buffer[entity.constructStatId].repairerCount++;
        }
    }

    public void RefreshCachedPositions()
    {
        if (GameMain.gameTick != lastCheckedTick)
        {
            lastCheckedTick = GameMain.gameTick;
            localPlayerCount = 0;
            var currentLocalPlanetId = GameMain.localPlanet?.id ?? int.MinValue;
            //CachedPositions.Clear();

            using (Multiplayer.Session.World.GetRemotePlayersModels(out var remotePlayersModels))
            {
                // host needs it for all players since they can build on other planets too.
                foreach (var model in remotePlayersModels.Values)
                {
                    var playerPos = model.Movement.GetLastPosition().LocalPlanetPosition.ToVector3();
                    var ejectPos = playerPos.normalized * (playerPos.magnitude + 2.8f);
                    var localPlanetId = model.Movement.localPlanetId;

                    // Cache players positions
                    if (cachedPositions.TryGetValue(model.Movement.PlayerID, out var playerPosition))
                    {
                        playerPosition.Position = ejectPos;
                        playerPosition.PlanetId = localPlanetId;
                    }
                    else
                    {
                        cachedPositions.Add(model.Movement.PlayerID, new PlayerPosition(ejectPos, model.Movement.localPlanetId));
                    }

                    if (currentLocalPlanetId != localPlanetId) continue;
                    if (localPlayerCount >= localPlayerPos.Length)
                    {
                        var newArray = new Vector3[localPlayerPos.Length * 2];
                        Array.Copy(localPlayerPos, newArray, localPlayerPos.Length);
                        localPlayerPos = newArray;
                    }
                    localPlayerPos[localPlayerCount++] = playerPos;
                }
            }
        }
    }

    public float GetClosestRemotePlayerSqrDistance(Vector3 pos)
    {
        var result = float.MaxValue;
        for (var i = 0; i < localPlayerCount; i++)
        {
            var sqrMagnitude = (pos - localPlayerPos[i]).sqrMagnitude;
            if (sqrMagnitude < result) result = sqrMagnitude;
        }
        return result;
    }

    public Vector3 GetPlayerEjectPosition(ushort playerId)
    {
        return cachedPositions.TryGetValue(playerId, out var value) ? value.Position : GameMain.mainPlayer.position;
    }

    public void ClearAllRemoteDrones()
    {
        crafts.Clear();
        craftRecyleIds.Clear();
        drones.Reset();
    }

    public void UpdateDrones(PlanetFactory factory, ObjectRenderer[] renderers, bool sync_gpu_inst, float dt, long time)
    {
        // Mimic from ConstructionSystem.UpdateDrones        
        var constructionDroneSpeed = factory.gameData.history.constructionDroneSpeed;
        var planetId = factory.planetId;

        for (var droneId = 1; droneId < drones.cursor; droneId++)
        {
            ref var ptr = ref drones.buffer[droneId];
            if (ptr.owner != planetId) //ptr.owner is planetId in the custom pool
            {
                continue;
            }
            var craftData = crafts[ptr.craftId];
            var playerId = (ushort)craftData.owner;
            RefreshCachedPositions();
            if (!cachedPositions.TryGetValue(playerId, out var playerPosition) || playerPosition.PlanetId != planetId)
            {
                // If the owner leave the planet, recycle the drone
                RecycleDrone(factory, ref ptr);
                continue;
            }

            // Update drone stage and craft position
            var ejectPos = playerPosition.Position;
            var meachEnerey = (double)float.MaxValue; // Dummy value for remote drones
            var mecahEnergyChange = 0.0;
            var result = ptr.InternalUpdate(ref craftData, factory, ref ejectPos, constructionDroneSpeed, dt,
                ref meachEnerey, ref mecahEnergyChange, 0, 0, out _);
            crafts[ptr.craftId] = craftData;

            // Repair or find the next target
            UpdateDroneStageAndTarget(factory, ref ptr, in craftData, result, time);

            if (sync_gpu_inst)
            {
                UpdateGpuInstance(renderers, in craftData, ptr.stage);
            }
        }
    }

    private void UpdateDroneStageAndTarget(PlanetFactory factory, ref DroneComponent ptr, in CraftData craftData, int result, long time)
    {
        // DroneComponent.stage [1]:eject from player [2]:going to target [3]:on target [4]:back to player
        if (ptr.stage == 3)
        {
            if (ptr.targetObjectId > 0) // Repair entity
            {
                result = (factory.constructionSystem.Repair(ptr.targetObjectId, 1.0f, time) ? 1 : 0); // Assume energy ratio is 1.0f
                if (result == 1)
                {
                    ptr.targetObjectId = 0;
                }
            }
            else if (result == 0) // Mod: Do not wait for prebuild to finished, just advance to the next target
            {
                result = 1;
            }
        }
        if (result == 1 && ptr.stage == 4)
        {
            RecycleDrone(factory, ref ptr);
        }
        if (result != 0 && (ptr.stage == 2 || ptr.stage == 3 || ptr.stage == 4))
        {
            ptr.movement--;
            if (ptr.movement <= 0)
            {
                ptr.movement = 0;
                ptr.stage = 4;
                ptr.targetObjectId = 0;
            }
            else if (ptr.nextTarget1ObjectId != 0)
            {
                ptr.stage = 2;
                ptr.targetObjectId = ptr.nextTarget1ObjectId;
                ptr.targetPos = factory.constructionSystem._obj_hpos(ptr.nextTarget1ObjectId);
                ptr.nextTarget1ObjectId = 0;
            }
            else if (ptr.nextTarget2ObjectId != 0)
            {
                ptr.stage = 2;
                ptr.targetObjectId = ptr.nextTarget2ObjectId;
                ptr.targetPos = factory.constructionSystem._obj_hpos(ptr.nextTarget2ObjectId);
                ptr.nextTarget2ObjectId = 0;
            }
            else if (ptr.nextTarget3ObjectId != 0)
            {
                ptr.stage = 2;
                ptr.targetObjectId = ptr.nextTarget3ObjectId;
                ptr.targetPos = factory.constructionSystem._obj_hpos(ptr.nextTarget3ObjectId);
                ptr.nextTarget3ObjectId = 0;
            }
            else if (factory.constructionSystem.FindNextRepair(0, craftData.pos, out var foundPos, out var targetId))
            {
                ptr.stage = 2;
                ptr.targetObjectId = targetId;
                ptr.targetPos = foundPos;
                var buffer = factory.constructionSystem.constructStats.buffer;
                var constructStatId = factory.entityPool[targetId].constructStatId;
                buffer[constructStatId].repairerCount++;
            }
            else
            {
                ptr.stage = 4;
                ptr.targetObjectId = 0;
            }
        }
    }

    private static void UpdateGpuInstance(ObjectRenderer[] renderers, in CraftData craftData, int droneStage)
    {
        if (craftData.modelId > 0 && renderers[craftData.modelIndex] is DynamicRenderer dynamicRenderer)
        {
            var instPool = dynamicRenderer.instPool;
            var modelId = craftData.modelId;
            instPool[modelId].posx = (float)craftData.pos.x;
            instPool[modelId].posy = (float)craftData.pos.y;
            instPool[modelId].posz = (float)craftData.pos.z;
            instPool[modelId].rotx = craftData.rot.x;
            instPool[modelId].roty = craftData.rot.y;
            instPool[modelId].rotz = craftData.rot.z;
            instPool[modelId].rotw = craftData.rot.w;
            dynamicRenderer.extraPool[craftData.modelId].x = droneStage;
        }
    }

    private ref DroneComponent CreateDrone(PlanetFactory factory, Vector3 pos, Quaternion rot, Vector3 vel, ushort playerId)
    {
        int craftId;
        if (craftRecyleIds.Count > 0)
        {
            craftId = craftRecyleIds.Pop();
        }
        else
        {
            crafts.Add(default);
            craftId = crafts.Count - 1;
        }

        var craftData = default(CraftData);
        craftData.id = craftId;
        craftData.protoId = 0;
        craftData.modelIndex = 454;
        craftData.astroId = factory.planetId;
        craftData.owner = playerId; // Use craft.owner field to store playerId
        craftData.port = 0;
        craftData.prototype = ECraftProto.ConstructionDrone;
        craftData.dynamic = true;
        craftData.isSpace = false;
        craftData.pos = pos;
        craftData.rot = rot;
        craftData.vel = vel;
        var planet = factory.planet;
        if (planet.factoryLoaded || planet.factingCompletedStage >= 5)
        {
            craftData.modelId = GameMain.gpuiManager.AddModel(454, craftId, pos, rot, true);
        }
        crafts[craftId] = craftData;

        ref var drone = ref drones.Add();
        drone.craftId = craftId;

        return ref drone;
    }

    private void RecycleDrone(PlanetFactory factory, ref DroneComponent dronePtr)
    {
        if (dronePtr.targetObjectId > 0)
        {
            ref var entity = ref factory.entityPool[dronePtr.targetObjectId];
            if (entity.id == dronePtr.targetObjectId && entity.constructStatId > 0)
            {
                ref var constructStat = ref factory.constructionSystem.constructStats.buffer[entity.constructStatId];
                constructStat.repairerCount--;
                if (constructStat.repairerCount < 0) constructStat.repairerCount = 0;
            }
        }

        var craftId = dronePtr.craftId;
        var modelId = crafts[craftId].modelId;
        if (modelId != 0 && GameMain.gpuiManager.activeFactory == factory)
        {
            GameMain.gpuiManager.RemoveModel(454, modelId, true);
        }
        crafts[craftId].SetEmpty();
        craftRecyleIds.Push(craftId);
        drones.Remove(dronePtr.id);
    }
}
