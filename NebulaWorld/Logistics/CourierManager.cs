#region

using System;
using System.Collections.Generic;
using System.Linq;
using NebulaModel.Logger;
using UnityEngine;

#endregion

namespace NebulaWorld.Logistics;

public class CourierManager
{
    private float courierSpeed;

    private Vector3[] EntityPositions = new Vector3[10];
    private Dictionary<int, Vector3> PlayerPostions = new();
    private int tmp_iter;

    public int CourierCount { get; set; }
    public CourierData[] CourierDatas { get; set; } = new CourierData[10];

    public void Dispose()
    {
        CourierCount = 0;
        CourierDatas = null;
        EntityPositions = null;
        PlayerPostions = null;
    }

    public void AddCourier(int playerId, in Vector3 entityPos, int itemId, int itemCount)
    {
        if (CourierCount >= CourierDatas.Length)
        {
            var sourceArray = CourierDatas;
            CourierDatas = new CourierData[CourierDatas.Length * 2];
            Array.Copy(sourceArray, CourierDatas, CourierCount);
            var sourceArray2 = EntityPositions;
            EntityPositions = new Vector3[CourierDatas.Length * 2];
            Array.Copy(sourceArray2, EntityPositions, CourierCount);
        }
        CourierDatas[CourierCount].begin = entityPos;
        CourierDatas[CourierCount].end = entityPos;
        CourierDatas[CourierCount].endId = playerId;
        CourierDatas[CourierCount].direction = 1f;
        CourierDatas[CourierCount].maxt = 1f;
        CourierDatas[CourierCount].t = 0f;
        CourierDatas[CourierCount].itemId = itemId;
        CourierDatas[CourierCount].itemCount = itemCount;
        CourierDatas[CourierCount].gene = tmp_iter++;
        EntityPositions[CourierCount] = entityPos;
        CourierCount++;
        DeterminePosition();

        courierSpeed = GameMain.history.logisticCourierSpeedModified;
    }

    private void DeterminePosition()
    {
        PlayerPostions.Clear();
        using (Multiplayer.Session.World.GetRemotePlayersModels(out var remotePlayersModels))
        {
            foreach (var model in remotePlayersModels.Values.Where(model =>
                         model.Movement.GetLastPosition().LocalPlanetId == GameMain.mainPlayer.planetId))
            {
                // Cache players positions for courier position updating
                PlayerPostions.Add(model.Movement.PlayerID, model.PlayerTransform.position);
            }
        }
    }

    public void GameTick()
    {
        if (CourierCount <= 0 || GameMain.mainPlayer.factory == null)
        {
            return;
        }

        try
        {
            DeterminePosition();

            // Calculate extra couriers position for animation
            for (var j = 0; j < CourierCount; j++)
            {
                if (CourierDatas[j].maxt <= 0f)
                {
                    continue;
                }
                if (!PlayerPostions.ContainsKey(CourierDatas[j].endId))
                {
                    // player does not exist, mark to remove later on
                    CourierDatas[j].maxt = 0;
                    continue;
                }
                if (CourierDatas[j].direction > 0f) // (CourierDatas[j].endId < 0 && CourierDatas[j].direction > 0f)
                {
                    var EntityPos = EntityPositions[j];
                    ref var courierPtr = ref CourierDatas[j].end;
                    var playerPos = PlayerPostions[CourierDatas[j].endId];
                    var vector1 = playerPos - courierPtr;
                    var vector2 = playerPos - EntityPos;
                    var courierToPlayerDist = vector1.magnitude;
                    var entityToPlayerDist = vector2.magnitude;
                    var courierHeight = courierPtr.magnitude;
                    var playerHeight = playerPos.magnitude;
                    if (courierToPlayerDist < 1.4f)
                    {
                        var entityHeight = EntityPos.magnitude;
                        var cosValue =
                            (double)(EntityPos.x * playerPos.x + EntityPos.y * playerPos.y + EntityPos.z * playerPos.z) /
                            (entityHeight * playerHeight);
                        cosValue = cosValue switch
                        {
                            < -1.0 => -1.0,
                            > 1.0 => 1.0,
                            _ => cosValue
                        };
                        // courier reach player
                        CourierDatas[j].begin = EntityPos;
                        CourierDatas[j].maxt = (float)(Math.Acos(cosValue) * ((entityHeight + playerHeight) * 0.5));
                        CourierDatas[j].maxt = (float)Math.Sqrt((double)(CourierDatas[j].maxt * CourierDatas[j].maxt) +
                                                                (entityHeight - playerHeight) * (entityHeight - playerHeight));
                        CourierDatas[j].t = CourierDatas[j].maxt;
                    }
                    else
                    {
                        CourierDatas[j].begin = courierPtr;
                        var progress = courierSpeed * 0.016666668f / courierToPlayerDist;
                        if (progress > 1f)
                        {
                            progress = 1f;
                        }
                        Vector3 vector3 = new(vector1.x * progress, vector1.y * progress, vector1.z * progress);
                        var totalTime = courierToPlayerDist / courierSpeed;
                        if (totalTime < 0.03333333f)
                        {
                            totalTime = 0.03333333f;
                        }
                        var deltaHeight = (playerHeight - courierHeight) / totalTime * 0.016666668f;
                        courierPtr += vector3;
                        courierPtr = courierPtr.normalized * (courierHeight + deltaHeight);
                        if (entityToPlayerDist > CourierDatas[j].maxt)
                        {
                            CourierDatas[j].maxt = entityToPlayerDist;
                        }
                        CourierDatas[j].t = courierToPlayerDist;
                        if (CourierDatas[j].t >= CourierDatas[j].maxt * 0.99f)
                        {
                            CourierDatas[j].t = CourierDatas[j].maxt * 0.99f;
                        }
                    }
                }
                else
                {
                    CourierDatas[j].t += 0.016666668f * courierSpeed * CourierDatas[j].direction;
                }


                if (CourierDatas[j].t >= CourierDatas[j].maxt)
                {
                    // Courier reach remote player, switch item count to display color change
                    CourierDatas[j].itemCount = CourierDatas[j].itemCount > 0 ? 0 : 10;
                    CourierDatas[j].t = CourierDatas[j].maxt;
                    CourierDatas[j].direction = -1f;
                }
                else if (CourierDatas[j].t <= 0f)
                {
                    // Courier back to home
                    CourierDatas[j].maxt = 0;
                }
            }

            // Remove marked couriers
            var i = 0;
            for (var j = 0; j < CourierCount; j++)
            {
                if (!(CourierDatas[j].maxt > 0))
                {
                    continue;
                }
                CourierDatas[i] = CourierDatas[j];
                EntityPositions[i] = EntityPositions[j];
                i++;
            }
            CourierCount = i;
        }
        catch (Exception ex)
        {
            Log.Warn("Remote couriers error! Count: " + CourierCount);
            Log.Warn(ex);
            CourierCount = 0;
        }
    }
}
