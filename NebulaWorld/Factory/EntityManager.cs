using NebulaModel.Packets.Factory;
using System;
using System.Reflection;
using UnityEngine;

namespace NebulaWorld.Factory
{
    public class EntityManager
    {
        public static void BuildEntity(Vector3 pos, Quaternion rot, ItemProto proto, short protoId, PlanetData pData)
        {
            if(pData.factory == null)
            {
                if (LocalPlayer.IsMasterClient)
                {
                    pData.factory = GameMain.data.GetOrCreateFactory(pData);
                }
                else
                {
                    // we have not received the data from the server yet, but it will be requested once we arrive there anyways
                    return;
                }
            }
            if (pData.physics == null)
            {
                pData.physics = new PlanetPhysics(pData);
                pData.physics.Init();
            }
            // make room for entity if needed
            if (proto != null && pData.type != EPlanetType.Gas)
            {
                int sandGathered = pData.factory.FlattenTerrain(pos, rot, new Bounds(proto.prefabDesc.buildCollider.pos, proto.prefabDesc.buildCollider.ext * 2f), 6f, 1f, false, false);
                // dont give sand to player as he did not build it (or should i?)
            }
            // place the entity
            int ret = pData.factory.AddEntityDataWithComponents(new EntityData
            {
                protoId = protoId,
                pos = pos,
                rot = rot
            }, 0);

            GameMain.mainPlayer.controller.actionBuild.NotifyBuilt(0, ret);
            GameMain.history.MarkItemBuilt((int)protoId);
            GameMain.gameScenario.NotifyOnBuild(pData.id, protoId, ret);
        }
        public static int[] MinerGetUsefullVeins(ref int[] tmp_ids, ref int veinCount, Vector3 entityPos, Quaternion entityRot, EMinerType minerType, PlanetData pData)
        {
            Pose pose;
            pose.position = entityPos;
            pose.rotation = entityRot;

            Vector3 center = pose.position + pose.forward * -1.2f;
            Vector3 rhs = -pose.forward;
            Vector3 up = pose.up;

            NearColliderLogic collider = pData.physics.nearColliderLogic;
            if(collider == null)
            {
                int[] rip = new int[1];
                rip[0] = 0;
                return rip;
            }
            int veinsInAreaNonAlloc = collider.GetVeinsInAreaNonAlloc(center, 12f, tmp_ids);
            VeinData[] veinPool = pData.factory.veinPool;

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
        public static PrebuildData createPrebuildData(ItemProto proto, int protoId, Vector3 pos, Quaternion rot, PlanetData pData)
        {
            PrebuildData prebuild = default(PrebuildData);
            prebuild.protoId = (short)protoId;
            prebuild.modelIndex = (short)proto.prefabDesc.modelIndex;
            prebuild.pos = prebuild.pos2 = pos;
            prebuild.rot = prebuild.rot2 = rot;

            if (proto.prefabDesc.minerType == EMinerType.Vein)
            {
                // get veins that the miner could connect to
                int veinCount = 0;
                int[] tmp_ids = new int[1024];

                int[] veinIDs = MinerGetUsefullVeins(ref tmp_ids, ref veinCount, pos, rot, proto.prefabDesc.minerType, pData);
                prebuild.InitRefArray(veinCount);
                Array.Copy(veinIDs, prebuild.refArr, veinCount);
            }
            return prebuild;
        }
        public static void PlaceEntityPrebuild(EntityPlaced packet)
        {
            ItemProto proto = LDB.items.Select((int)packet.protoId);

            Vector3 pos = new Vector3(packet.pos.x, packet.pos.y, packet.pos.z);
            Quaternion rot = new Quaternion(packet.rot.x, packet.rot.y, packet.rot.z, packet.rot.w);

            PlanetData pData = GameMain.galaxy.PlanetById(packet.planetId);

            if (proto == null || pData == null)
            {
                return;
            }
            if(pData.factory == null)
            {
                if (LocalPlayer.IsMasterClient)
                {
                    pData.factory = GameMain.data.GetOrCreateFactory(pData);
                }
                else
                {
                    // request factory on arrival
                    return;
                }
            }

            PrebuildData prebuild = createPrebuildData(proto, packet.protoId, pos, rot, pData);

            LocalPlayer.prebuildReceivedList.Add(prebuild, packet.planetId);
            // the following should work as we only spawn a prebuild when the player is on the same planet.
            int id = pData.factory.AddPrebuildDataWithComponents(prebuild);

            pData.factory.prebuildPool[id].id = 0; // this effectively prevents drones from interacting with it
        }
        public static void RemovePrebuildModel(int modelIndex, int id, bool setBuffer = true)
        {
            ObjectRenderer renderer = GameMain.gpuiManager.GetPrebuildRenderer(modelIndex);
            if(renderer == null)
            {
                return;
            }
            renderer.instPool[id].objId = 0U;
            renderer.instPool[id].posx = 0f;
            renderer.instPool[id].posy = 0f;
            renderer.instPool[id].posz = 0f;
            renderer.instPool[id].rotx = 0f;
            renderer.instPool[id].roty = 0f;
            renderer.instPool[id].rotz = 0f;
            renderer.instPool[id].rotw = 0f;
            renderer.instRecycle[renderer.instRecycleCursor++] = id;
            ComputeBuffer instBuffer = (ComputeBuffer)typeof(ObjectRenderer).GetField("instBuffer", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(renderer);
            if(instBuffer == null)
            {
                return;
            }
            if (renderer.instRecycleCursor == renderer.instCursor - 1)
            {
                Array.Clear(renderer.instPool, 0, renderer.instCapacity);
                Array.Clear(renderer.instRecycle, 0, renderer.instRecycle.Length);
                renderer.instCursor = 1;
                renderer.instRecycleCursor = 0;
                instBuffer.SetData(renderer.instPool);
            }
            else if (renderer.instRecycleCursor < renderer.instCursor - 1)
            {
                if (setBuffer)
                {
                    instBuffer.SetData(renderer.instPool, id, id, 1);
                }
            }
            else
            {
                Assert.CannotBeReached();
            }
            renderer.SyncInstBuffer();
        }
        public static void RemovePrebuildWithComponents(PrebuildData prebuild, PlanetData pData)
        {
            RemovePrebuildModel(prebuild.modelIndex, prebuild.modelId, true);

            PlanetFactory factory = pData.factory;
            if(factory == null)
            {
                if (LocalPlayer.IsMasterClient)
                {
                    factory = GameMain.data.GetOrCreateFactory(pData);
                }
                else
                {
                    // we cant process here as we did not load the factory yet.
                    // but in this case the factory will be requested from the server on arrival anyways
                    return;
                }
            }

            factory.prebuildPool[prebuild.id].SetNull();
            factory.ClearObjectConn(-prebuild.id);
            Array.Clear(factory.prebuildConnPool, prebuild.id * 16, 16);

            if (factory.planet.physics != null)
            {
                factory.planet.physics.RemoveLinkedColliderData(prebuild.colliderId);
                factory.planet.physics.NotifyObjectRemove(EObjectType.Prebuild, prebuild.id);
            }
            if(factory.planet.audio != null)
            {
                factory.planet.audio.NotifyObjectRemove(EObjectType.Prebuild, prebuild.id);
            }
        }
        public static void PlaceEntity(EntityPlaced packet)
        {
            ItemProto proto = LDB.items.Select((int)packet.protoId);
            Vector3 pos = new Vector3(packet.pos.x, packet.pos.y, packet.pos.z);
            Quaternion rot = new Quaternion(packet.rot.x, packet.rot.y, packet.rot.z, packet.rot.w);

            PlanetData pData = GameMain.galaxy.PlanetById(packet.planetId);

            // remove prebuild from internal list
            if (LocalPlayer.prebuildReceivedList.ContainsValue(packet.planetId))
            {
                foreach (PrebuildData preData in LocalPlayer.prebuildReceivedList.Keys)
                {
                    if (preData.pos == pos && preData.rot == rot)
                    {
                        RemovePrebuildWithComponents(preData, pData);
                        LocalPlayer.prebuildReceivedList.Remove(preData);
                        break;
                    }
                }
            }

            // the following should care for factory loading.
            BuildEntity(pos, rot, proto, packet.protoId, pData);
            // if the factory is still null it means we are a client and have not loaded the factory yet
            // thats why we exit here. factory will be synced on arrival
            Debug.Log("1");
            if (pData.factory == null)
            {
                return;
            }

            if (proto != null)
            {
                PrefabDesc prefab = proto.prefabDesc;
                int pcID = -1; // used to connect power consumers to power generators

                if (prefab.isPowerGen)
                {
                    // NOTE: not sure if entityId needs to be unique or whatsoever, just testing things here
                    int entityId;
                    if (pData.factory.powerSystem.genCursor > 0)
                    {
                        entityId = pData.factory.powerSystem.genPool[pData.factory.powerSystem.genCursor - 1].entityId;
                    }
                    else
                    {
                        entityId = 0;
                    }
                    int powerId = pData.factory.powerSystem.NewGeneratorComponent(entityId, prefab);
                    pData.factory.powerSystem.genPool[powerId].productId = prefab.powerProductId;
                }
                if (prefab.isPowerConsumer) // this is actually essential to connect the consumer with the generators
                {
                    int entityId;
                    if (pData.factory.powerSystem.consumerCursor > 0)
                    {
                        entityId = pData.factory.powerSystem.consumerPool[pData.factory.powerSystem.consumerCursor - 1].entityId;
                    }
                    else
                    {
                        entityId = 0;
                    }
                    pcID = pData.factory.powerSystem.NewConsumerComponent(entityId, prefab.workEnergyPerTick, prefab.idleEnergyPerTick);
                }
                if (prefab.minerType != EMinerType.None && prefab.minerPeriod > 0)
                {
                    // get veins that the miner could connect to
                    int veinCount = 0;
                    int[] tmp_ids = new int[1024];

                    int[] veinIDs = MinerGetUsefullVeins(ref tmp_ids, ref veinCount, pos, rot, prefab.minerType, pData);
                    // veinIDs should now contain the id's the miner can connect to
                    // now we need to add it to the miner pool and tell it the veins to connect to
                    // entityId should be the last one in the array as we just added it
                    int entityId = pData.factory.entityPool[pData.factory.entityCursor - 1].id;
                    int minerId = pData.factory.factorySystem.NewMinerComponent(entityId, prefab.minerType, prefab.minerPeriod);
                    if (minerId != 0)
                    {
                        MinerComponent[] minerPool = pData.factory.factorySystem.minerPool;
                        minerPool[minerId].InitVeinArray(veinCount);
                        if (veinCount > 0)
                        {
                            Array.Copy(veinIDs, minerPool[minerId].veins, veinCount);
                        }
                        for (int i = 0; i < minerPool[minerId].veinCount; i++)
                        {
                            pData.factory.RefreshVeinMiningDisplay(minerPool[minerId].veins[i], entityId, 0);
                        }
                        minerPool[minerId].ArrageVeinArray();
                        if (pcID != 0)
                        {
                            // this is hugely important to get power to the building!!!
                            minerPool[minerId].pcId = pcID;
                        }
                        minerPool[minerId].GetMinimumVeinAmount(pData.factory, pData.factory.veinPool);
                        // TODO: do some stuff with entitySignPool, is it important?
                    }
                }
            }
        }
    }
}
