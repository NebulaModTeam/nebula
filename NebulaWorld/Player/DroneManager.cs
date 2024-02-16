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
    private static Dictionary<ushort, PlayerPosition> CachedPositions = [];
    private static long lastCheckedTick = 0;

    public DroneManager()
    {
        CachedPositions = [];
        lastCheckedTick = 0;
    }

    public void Dispose()
    {
        CachedPositions = null;
        lastCheckedTick = 0;
        GC.SuppressFinalize(this);
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

    public static Vector3 GetPlayerPosition(ushort playerId)
    {
        return CachedPositions.TryGetValue(playerId, out var value) ? value.Position : GameMain.mainPlayer.position;
    }
}
