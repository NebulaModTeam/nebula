using NebulaModel.Logger;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace NebulaWorld.Logistics
{
    public class CourierManager
    {
        public int CourierCount { get; private set; }
        public CourierData[] CourierDatas { get; private set; }

        private Vector3[] EntityPositions;
        private Dictionary<int, Vector3> PlayerPostions;
        private int tmp_iter;
        private float courierSpeed;

        public CourierManager()
        {
            CourierCount = 0;
            CourierDatas = new CourierData[10];
            EntityPositions = new Vector3[10];
            PlayerPostions = new Dictionary<int, Vector3>();
            tmp_iter = 0;
        }

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
                CourierData[] sourceArray = CourierDatas;
                CourierDatas = new CourierData[CourierDatas.Length * 2];
                Array.Copy(sourceArray, CourierDatas, CourierCount);
                Vector3[] sourceArray2 = EntityPositions;
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
            using (Multiplayer.Session.World.GetRemotePlayersModels(out Dictionary<ushort, RemotePlayerModel> remotePlayersModels))
            {
                foreach (RemotePlayerModel model in remotePlayersModels.Values)
                {
                    // Check only players on the same planet
                    if (model.Movement.GetLastPosition().LocalPlanetId != GameMain.mainPlayer.planetId)
                    {
                        continue;
                    }
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
                for (int j = 0; j < CourierCount; j++)
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
                        Vector3 EntityPos = EntityPositions[j];
                        ref Vector3 courierPtr = ref CourierDatas[j].end;
                        Vector3 playerPos = PlayerPostions[CourierDatas[j].endId];
                        Vector3 vector1 = playerPos - courierPtr;
                        Vector3 vector2 = playerPos - EntityPos;
                        float courierToPlayerDist = vector1.magnitude;
                        float entityToPlayerDist = vector2.magnitude;
                        float courierHeight = courierPtr.magnitude;
                        float playerHeight = playerPos.magnitude;
                        if (courierToPlayerDist < 1.4f)
                        {
                            float entityHeight = EntityPos.magnitude;
                            double cosValue = (double)(EntityPos.x * playerPos.x + EntityPos.y * playerPos.y + EntityPos.z * playerPos.z) / (entityHeight * playerHeight);
                            if (cosValue < -1.0)
                            {
                                cosValue = -1.0;
                            }
                            else if (cosValue > 1.0)
                            {
                                cosValue = 1.0;
                            }
                            // courier reach player
                            CourierDatas[j].begin = EntityPos;
                            CourierDatas[j].maxt = (float)(Math.Acos(cosValue) * ((entityHeight + playerHeight) * 0.5));
                            CourierDatas[j].maxt = (float)Math.Sqrt((double)(CourierDatas[j].maxt * CourierDatas[j].maxt) + (entityHeight - playerHeight) * (entityHeight - playerHeight));
                            CourierDatas[j].t = CourierDatas[j].maxt;
                        }
                        else
                        {
                            CourierDatas[j].begin = courierPtr;
                            float progress = courierSpeed * 0.016666668f / courierToPlayerDist;
                            if (progress > 1f)
                            {
                                progress = 1f;
                            }
                            Vector3 vector3 = new(vector1.x * progress, vector1.y * progress, vector1.z * progress);
                            float totalTime = courierToPlayerDist / courierSpeed;
                            if (totalTime < 0.03333333f)
                            {
                                totalTime = 0.03333333f;
                            }
                            float deltaHeight = (playerHeight - courierHeight) / totalTime * 0.016666668f;
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
                        // Courier reach remote player, swtich item count to display color change
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
                int i = 0;
                for (int j = 0; j < CourierCount; j++)
                {
                    if (CourierDatas[j].maxt > 0)
                    {
                        CourierDatas[i] = CourierDatas[j];
                        EntityPositions[i] = EntityPositions[j];
                        i++;
                    }
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
}
