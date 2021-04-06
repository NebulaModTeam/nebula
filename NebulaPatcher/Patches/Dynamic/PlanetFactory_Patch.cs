using HarmonyLib;
using NebulaModel.Logger;
using NebulaModel.Packets.Factory;
using NebulaWorld;
using NebulaWorld.Factory;
using System.Collections.Generic;
using UnityEngine;

namespace NebulaPatcher.Patches.Dynamic
{
    [HarmonyPatch(typeof(PlanetFactory))]
    class BuildFinally_patch
    {
        [HarmonyPrefix]
        [HarmonyPatch("AddPrebuildDataWithComponents")]
        public static bool AddPrebuildDataWithComponents_Prefix(PlanetFactory __instance, PrebuildData prebuild)
        {
            if (!SimulatedWorld.Initialized)
                return true;

            // If the host game called the method, we need to compute the PrebuildId ourself
            if (LocalPlayer.IsMasterClient && !FactoryManager.IsIncommingRequest)
            {
                int nextPrebuildId = FactoryManager.GetNextPrebuildId(__instance);
                FactoryManager.SetPrebuildRequest(__instance.planetId, nextPrebuildId, LocalPlayer.PlayerId);
            }

            // If we are the host we need to notify all the clients to do the same in their game
            // Or if the method was called by the game on a client, we need to send a request to the host to let the host create the object first. 
            if (LocalPlayer.IsMasterClient || !FactoryManager.IsIncommingRequest)
            {
                LocalPlayer.SendPacket(new AddEntityPreviewRequest(__instance.planetId, prebuild));
            }

            // Perform the game code only if you are the host or a client which received a host request to do this action.
            return LocalPlayer.IsMasterClient || FactoryManager.IsIncommingRequest;
        }

        [HarmonyPrefix]
        [HarmonyPatch("BuildFinally")]
        public static bool BuildFinally_Prefix(PlanetFactory __instance, Player player, int prebuildId)
        {
            if (!SimulatedWorld.Initialized)
                return true;

            // If we are executing this on the host, make sure that we were having a corresponding prebuild request
            if (LocalPlayer.IsMasterClient)
            {
                if (!FactoryManager.ContainsPrebuildRequest(__instance.planetId, prebuildId))
                {
                    Log.Warn($"BuildFinally was called without having a corresponding PrebuildRequest for the prebuild {prebuildId} on the planet {__instance.planetId}");
                    return false;
                }

                // Remove the prebuild request from the list since we will now convert it to a real building
                FactoryManager.RemovePrebuildRequest(__instance.planetId, prebuildId);
            }

            // If we are the host we need to notify all the clients to do the same in their game
            // Or if the method was called by the game on a client, we need to send a request to the host to let the host decide if we can create it or not.
            if (LocalPlayer.IsMasterClient || !FactoryManager.IsIncommingRequest)
            {
                LocalPlayer.SendPacket(new BuildEntityRequest(__instance.planetId, prebuildId));
            }

            // Perform the game code only if you are the host or a client which received a host request to do this action.
            // TODO: Look at doing this in a transpiler
            return LocalPlayer.IsMasterClient || FactoryManager.IsIncommingRequest;
        }

        // TODO: Upgrade
        /*
        [HarmonyPrefix]
        [HarmonyPatch("CreateEntityDisplayComponents")]
        public static bool CreateEntityDisplayComponents_Prefix(PlanetFactory __instance, int entityId, PrefabDesc desc, short modelIndex)
        {
            __instance.entityPool[entityId].modelIndex = modelIndex != (short)0 ? modelIndex : (short)desc.modelIndex;
            __instance.entityPool[entityId].modelId = GameMain.gpuiManager.AddModel((int)__instance.entityPool[entityId].modelIndex, entityId, __instance.entityPool[entityId].pos, __instance.entityPool[entityId].rot);
            if (desc.minimapType > 0 && __instance.entityPool[entityId].mmblockId == 0)
            {
                if (__instance.entityPool[entityId].inserterId == 0)
                {
                    __instance.entityPool[entityId].mmblockId = __instance.blockContainer.AddMiniBlock(entityId, desc.minimapType, __instance.entityPool[entityId].pos, __instance.entityPool[entityId].rot, desc.selectSize);
                }
                else
                {
                    InserterComponent inserterComponent = __instance.factorySystem.inserterPool[__instance.entityPool[entityId].inserterId];
                    Assert.Positive(inserterComponent.id);
                    Vector3 pos = Vector3.Lerp(__instance.entityPool[entityId].pos, inserterComponent.pos2, 0.5f);
                    Quaternion rot = Quaternion.LookRotation(inserterComponent.pos2 - __instance.entityPool[entityId].pos, pos.normalized);
                    Vector3 scl = new Vector3(0.7f, 0.7f, (float)((double)Vector3.Distance(inserterComponent.pos2, __instance.entityPool[entityId].pos) * 0.5 + 0.200000002980232));
                    __instance.entityPool[entityId].mmblockId = __instance.blockContainer.AddMiniBlock(entityId, desc.minimapType, pos, rot, scl);
                }
            }
            if (desc.colliders != null && desc.colliders.Length > 0)
            {
                for (int index = 0; index < desc.colliders.Length; ++index)
                {
                    if (__instance.entityPool[entityId].inserterId == 0)
                    {
                        __instance.entityPool[entityId].colliderId = __instance.planet.physics.AddColliderData(desc.colliders[index].BindToObject(entityId, __instance.entityPool[entityId].colliderId, EObjectType.Entity, __instance.entityPool[entityId].pos, __instance.entityPool[entityId].rot));
                    }
                    else
                    {
                        ColliderData collider = desc.colliders[index];
                        InserterComponent inserterComponent = __instance.factorySystem.inserterPool[__instance.entityPool[entityId].inserterId];
                        Assert.Positive(inserterComponent.id);
                        Vector3 _wpos = Vector3.Lerp(__instance.entityPool[entityId].pos, inserterComponent.pos2, 0.5f);
                        Quaternion _wrot = Quaternion.LookRotation(inserterComponent.pos2 - __instance.entityPool[entityId].pos, _wpos.normalized);
                        collider.ext = new Vector3(collider.ext.x, collider.ext.y, Mathf.Max(0.1f, Vector3.Distance(inserterComponent.pos2, __instance.entityPool[entityId].pos) * 0.5f + collider.ext.z));
                        __instance.entityPool[entityId].colliderId = __instance.planet.physics.AddColliderData(collider.BindToObject(entityId, __instance.entityPool[entityId].colliderId, EObjectType.Entity, _wpos, _wrot));
                    }
                }
            }
            if (!desc.hasAudio)
                return false;

            __instance.entityPool[entityId].audioId = __instance.planet.audio.AddAudioData(entityId, EObjectType.Entity, __instance.entityPool[entityId].pos, desc);

            return false;
        }
        */

        [HarmonyPrefix]
        [HarmonyPatch("DestructFinally")]
        public static bool DestructFinally_Prefix(PlanetFactory __instance, Player player, int objId, ref int protoId)
        {
            if (!SimulatedWorld.Initialized)
                return true;

            // TODO: handle if 2 clients or if host and client trigger a destruct of the same object at the same time

            // If the object is a prebuild, remove it from the prebuild request list
            if (LocalPlayer.IsMasterClient && objId < 0)
            {
                if (!FactoryManager.ContainsPrebuildRequest(__instance.planetId, -objId))
                {
                    Log.Warn($"DestructFinally was called without having a corresponding PrebuildRequest for the prebuild {-objId} on the planet {__instance.planetId}");
                    return false;
                }

                FactoryManager.RemovePrebuildRequest(__instance.planetId, -objId);
            }


            // If we are the host we need to notify all the clients to do the same in their game
            // Or if the method was called by the game on a client, we need to send a request to the host to let the host decide if we can create it or not.
            if (LocalPlayer.IsMasterClient || !FactoryManager.IsIncommingRequest)
            {
                LocalPlayer.SendPacket(new DestructEntityRequest(__instance.planetId, objId));
            }

            // Perform the game code only if you are the host or a client which received a host request to do this action.
            return LocalPlayer.IsMasterClient || FactoryManager.IsIncommingRequest;
        }

        [HarmonyPrefix]
        [HarmonyPatch("UpgradeFinally")]
        public static bool UpgradeFinally_Prefix(PlanetFactory __instance,  Player player, int objId, ItemProto replace_item_proto)
        {
            if (!SimulatedWorld.Initialized)
                return true;

            // If we are the host we need to notify all the clients to do the same in their game
            // Or if the method was called by the game on a client, we need to send a request to the host to let the host decide if we can create it or not.
            if (LocalPlayer.IsMasterClient || !FactoryManager.IsIncommingRequest)
            {
                LocalPlayer.SendPacket(new UpgradeEntityRequest(__instance.planetId, objId, replace_item_proto.ID));
            }

            // Perform the game code only if you are the host or a client which received a host request to do this action.
            return LocalPlayer.IsMasterClient || FactoryManager.IsIncommingRequest;
        }

        /*
        [HarmonyPrefix]
        [HarmonyPatch("BuildFinally")]
        public static bool BuildFinally_Prefix(PlanetFactory __instance, Player player, int prebuildId)
        {
            if (prebuildId != 0)
            {
                PrebuildData data = __instance.prebuildPool[prebuildId];
                if (data.id == prebuildId)
                {
                    OnEntityPlaced(data.protoId, data.pos, data.rot, false);
                }
            }

            return true;
        }

        [HarmonyPrefix]
        [HarmonyPatch("AddPrebuildDataWithComponents")]
        public static bool AddPrebuildDataWithComponents_Prefix(PlanetFactory __instance, PrebuildData prebuild)
        {
            for (int i = 0; i < LocalPlayer.prebuildReceivedList.Count; i++)
            {
                foreach (PrebuildData pBuild in LocalPlayer.prebuildReceivedList.Keys)
                {
                    if (pBuild.pos == prebuild.pos && pBuild.rot == prebuild.rot)
                    {
                        return true;
                    }
                }
            }
            OnEntityPlaced(prebuild.protoId, prebuild.pos, prebuild.rot, true);
            return true;
        }

        private static void OnEntityPlaced(short protoId, Vector3 pos, Quaternion rot, bool isPrebuild)
        {
            var packet = new EntityPlaced(GameMain.localPlanet.id, protoId, pos, rot, isPrebuild);
            LocalPlayer.SendPacket(packet);
        }
        */
    }
}
