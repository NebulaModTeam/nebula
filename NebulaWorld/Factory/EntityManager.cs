using NebulaModel.Packets.Factory;
using System;
using UnityEngine;

namespace NebulaWorld.Factory
{
    public class EntityManager
    {
        public static void BuildEntity(Vector3 pos, Quaternion rot, ItemProto proto, short protoId)
        {
            // make room for entity if needed
            if (proto != null && GameMain.localPlanet.type != EPlanetType.Gas)
            {
                int sandGathered = GameMain.mainPlayer.factory.FlattenTerrain(pos, rot, new Bounds(proto.prefabDesc.buildCollider.pos, proto.prefabDesc.buildCollider.ext * 2f), 6f, 1f, false, false);
                // dont give sand to player as he did not build it (or should i?)
            }
            // place the entity
            int ret = GameMain.mainPlayer.factory.AddEntityDataWithComponents(new EntityData
            {
                protoId = protoId,
                pos = pos,
                rot = rot
            }, 0);

            GameMain.mainPlayer.controller.actionBuild.NotifyBuilt(0, ret);
            GameMain.history.MarkItemBuilt((int)protoId);
        }
        public static int[] MinerGetUsefullVeins(ref int[] tmp_ids, ref int veinCount, Vector3 entityPos, Quaternion entityRot, EMinerType minerType)
        {
            Pose pose;
            pose.position = entityPos;
            pose.rotation = entityRot;

            Vector3 center = pose.position + pose.forward * -1.2f;
            Vector3 rhs = -pose.forward;
            Vector3 up = pose.up;

            NearColliderLogic collider = GameMain.mainPlayer.planetData.physics.nearColliderLogic;
            int veinsInAreaNonAlloc = collider.GetVeinsInAreaNonAlloc(center, 12f, tmp_ids);
            VeinData[] veinPool = GameMain.mainPlayer.factory.veinPool;

            int[] veinIDs = new int[veinsInAreaNonAlloc];
            float veinOildClosest = 100f;
            int oilVeinId = 0;

            for (int i = 0; i < veinsInAreaNonAlloc; i++)
            {
                if (minerType == EMinerType.Vein)
                {
                    if (tmp_ids[i] != 0 && veinPool[tmp_ids[i]].id == tmp_ids[i])
                    {
                        if (veinPool[tmp_ids[i]].type != EVeinType.Oil)
                        {
                            Vector3 vpos = veinPool[tmp_ids[i]].pos;
                            Vector3 vposCenter = vpos - center;
                            float num = Vector3.Dot(up, vposCenter);
                            vposCenter -= up * num;
                            float sqrMagnitude = vposCenter.sqrMagnitude;
                            float num2 = Vector3.Dot(vposCenter.normalized, rhs);
                            if (sqrMagnitude <= 60.0625f && num2 >= 0.73 && Mathf.Abs(num) <= 2f)
                            {
                                veinIDs[veinCount++] = tmp_ids[i];
                            }
                        }
                    }
                }
                else if (minerType == EMinerType.Oil)
                {
                    if (tmp_ids[i] != 0 && veinPool[tmp_ids[i]].id == tmp_ids[i] && veinPool[tmp_ids[i]].type == EVeinType.Oil)
                    {
                        Vector3 vpos = veinPool[tmp_ids[i]].pos;
                        Vector3 vposCenter = vpos - center;
                        float num = Vector3.Dot(up, vposCenter);
                        float sqrMagnitude = (vposCenter - up * num).sqrMagnitude;
                        if (sqrMagnitude < veinOildClosest)
                        {
                            veinOildClosest = sqrMagnitude;
                            oilVeinId = tmp_ids[i];
                        }
                    }
                }
                else if (minerType == EMinerType.Water)
                {
                    // TODO
                }
            }

            if (oilVeinId != 0 && minerType == EMinerType.Oil)
            {
                veinIDs = new int[1];
                veinIDs[0] = oilVeinId;
                veinCount = 1;
            }

            return veinIDs;
        }
        public static void PlaceEntity(EntityPlaced packet)
        {
            ItemProto proto = LDB.items.Select((int)packet.protoId);
            Vector3 pos = new Vector3(packet.pos.x, packet.pos.y, packet.pos.z);
            Quaternion rot = new Quaternion(packet.rot.x, packet.rot.y, packet.rot.z, packet.rot.w);

            BuildEntity(pos, rot, proto, packet.protoId);

            if (proto != null)
            {
                PrefabDesc prefab = proto.prefabDesc;
                int pcID = -1; // used to connect power consumers to power generators

                if (prefab.isPowerGen)
                {
                    // NOTE: not sure if entityId needs to be unique or whatsoever, just testing things here
                    int entityId;
                    if (GameMain.mainPlayer.factory.powerSystem.genCursor > 0)
                    {
                        entityId = GameMain.mainPlayer.factory.powerSystem.genPool[GameMain.mainPlayer.factory.powerSystem.genCursor - 1].entityId;
                    }
                    else
                    {
                        entityId = 0;
                    }
                    int powerId = GameMain.mainPlayer.factory.powerSystem.NewGeneratorComponent(entityId, prefab);
                    GameMain.mainPlayer.factory.powerSystem.genPool[powerId].productId = prefab.powerProductId;
                }
                if (prefab.isPowerConsumer) // this is actually essential to connect the consumer with the generators
                {
                    int entityId;
                    if (GameMain.mainPlayer.factory.powerSystem.consumerCursor > 0)
                    {
                        entityId = GameMain.mainPlayer.factory.powerSystem.consumerPool[GameMain.mainPlayer.factory.powerSystem.consumerCursor - 1].entityId;
                    }
                    else
                    {
                        entityId = 0;
                    }
                    pcID = GameMain.mainPlayer.factory.powerSystem.NewConsumerComponent(entityId, prefab.workEnergyPerTick, prefab.idleEnergyPerTick);
                }
                if (prefab.minerType != EMinerType.None && prefab.minerPeriod > 0)
                {
                    // get veins that the miner could connect to
                    int veinCount = 0;
                    int[] tmp_ids = new int[1024];

                    int[] veinIDs = MinerGetUsefullVeins(ref tmp_ids, ref veinCount, pos, rot, prefab.minerType);
                    // veinIDs should now contain the id's the miner can connect to
                    // now we need to add it to the miner pool and tell it the veins to connect to
                    // entityId should be the last one in the array as we just added it
                    int entityId = GameMain.mainPlayer.factory.entityPool[GameMain.mainPlayer.factory.entityCursor - 1].id;
                    int minerId = GameMain.mainPlayer.factory.factorySystem.NewMinerComponent(entityId, prefab.minerType, prefab.minerPeriod);
                    if (minerId != 0)
                    {
                        MinerComponent[] minerPool = GameMain.mainPlayer.factory.factorySystem.minerPool;
                        minerPool[minerId].InitVeinArray(veinCount);
                        if (veinCount > 0)
                        {
                            Array.Copy(veinIDs, minerPool[minerId].veins, veinCount);
                        }
                        for (int i = 0; i < minerPool[minerId].veinCount; i++)
                        {
                            GameMain.mainPlayer.factory.RefreshVeinMiningDisplay(minerPool[minerId].veins[i], entityId, 0);
                        }
                        minerPool[minerId].ArrageVeinArray();
                        if (pcID != 0)
                        {
                            // this is hugely important to get power to the building!!!
                            minerPool[minerId].pcId = pcID;
                        }
                        minerPool[minerId].GetMinimumVeinAmount(GameMain.mainPlayer.factory, GameMain.mainPlayer.factory.veinPool);
                        // TODO: do some stuff with entitySignPool, is it important?
                    }
                }
            }
        }
    }
}
