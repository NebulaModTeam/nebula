using HarmonyLib;
using NebulaModel.Logger;
using NebulaModel.Packets.Factory;
using NebulaModel.Packets.Planet;
using NebulaWorld;
using NebulaWorld.Factory;
using UnityEngine;
using NebulaWorld.Planet;
using NebulaWorld.Player;

namespace NebulaPatcher.Patches.Dynamic
{
    [HarmonyPatch(typeof(PlanetFactory))]
    class BuildFinally_patch
    {
        [HarmonyPostfix]
        [HarmonyPatch("AddPrebuildData")]
        public static void AddPrebuildData_Postfix(PlanetFactory __instance, PrebuildData prebuild, ref int __result)
        {
            if (!SimulatedWorld.Initialized)
                return;

            // If the host game called the method, we need to compute the PrebuildId ourself
            if (LocalPlayer.IsMasterClient && !FactoryManager.EventFromClient)
            {
               
                FactoryManager.SetPrebuildRequest(__instance.planetId, __result, LocalPlayer.PlayerId);
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch("BuildFinally")]
        public static bool BuildFinally_Prefix(PlanetFactory __instance, Player player, int prebuildId)
        {
            if (!SimulatedWorld.Initialized)
                return true;

            if (LocalPlayer.IsMasterClient)
            {
                if (!FactoryManager.ContainsPrebuildRequest(__instance.planetId, prebuildId))
                {
                    // This prevents duplicating the entity when multiple players trigger the BuildFinally for the same entity at the same time.
                    // If it occurs in any other circumstances, it means that we have some desynchronization between clients and host prebuilds buffers.
                    Log.Warn($"BuildFinally was called without having a corresponding PrebuildRequest for the prebuild {prebuildId} on the planet {__instance.planetId}");
                    return false;
                }

                // Remove the prebuild request from the list since we will now convert it to a real building
                FactoryManager.RemovePrebuildRequest(__instance.planetId, prebuildId);
            }

            if (LocalPlayer.IsMasterClient || !FactoryManager.EventFromServer)
            {
                LocalPlayer.SendPacket(new BuildEntityRequest(__instance.planetId, prebuildId, FactoryManager.PacketAuthor == -1 ? LocalPlayer.PlayerId : FactoryManager.PacketAuthor));
            }

            if (!LocalPlayer.IsMasterClient && !FactoryManager.EventFromServer && !DroneManager.IsPendingBuildRequest(-prebuildId))
            {
                DroneManager.AddBuildRequestSent(-prebuildId);
            }

            return LocalPlayer.IsMasterClient || FactoryManager.EventFromServer;
        }


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

            if (LocalPlayer.IsMasterClient || !FactoryManager.EventFromServer)
            {
                LocalPlayer.SendPacket(new DestructEntityRequest(__instance.planetId, objId, FactoryManager.PacketAuthor == -1 ? LocalPlayer.PlayerId : FactoryManager.PacketAuthor));
            }

            return LocalPlayer.IsMasterClient || FactoryManager.EventFromServer;
        }

        [HarmonyPrefix]
        [HarmonyPatch("UpgradeFinally")]
        public static bool UpgradeFinally_Prefix(PlanetFactory __instance, Player player, int objId, ItemProto replace_item_proto)
        {
            if (!SimulatedWorld.Initialized)
                return true;

            if (LocalPlayer.IsMasterClient || !FactoryManager.EventFromServer)
            {
                LocalPlayer.SendPacket(new UpgradeEntityRequest(__instance.planetId, objId, replace_item_proto.ID));
            }

            return LocalPlayer.IsMasterClient || FactoryManager.EventFromServer;
        }

        [HarmonyPrefix]
        [HarmonyPatch("GameTick")]
        public static bool InternalUpdate_Prefix()
        {
            StorageManager.IsHumanInput = false;
            return true;
        }

        [HarmonyPostfix]
        [HarmonyPatch("GameTick")]
        public static void InternalUpdate_Postfix()
        {
            StorageManager.IsHumanInput = true;
        }

        [HarmonyPrefix]
        [HarmonyPatch("PasteEntitySetting")]
        public static void PasteEntitySetting_Prefix(PlanetFactory __instance, int entityId)
        {
            if (SimulatedWorld.Initialized && !FactoryManager.EventFromServer && !FactoryManager.EventFromClient)
            {
                LocalPlayer.SendPacketToLocalStar(new PasteEntitySettingUpdate(entityId, EntitySettingDesc.clipboard, GameMain.localPlanet?.factoryIndex ?? -1));
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch("FlattenTerrainReform")]
        public static void FlattenTerrainReform_Prefix(PlanetFactory __instance, Vector3 center, float radius, int reformSize, bool veinBuried, float fade0)
        {
            if (SimulatedWorld.Initialized && !FactoryManager.EventFromClient && !FactoryManager.EventFromServer)
            {
                LocalPlayer.SendPacketToLocalStar(new FoundationBuildUpdatePacket(radius, reformSize, veinBuried, fade0));
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch("RemoveVegeWithComponents")]
        public static void RemoveVegeWithComponents_Prefix(PlanetFactory __instance, int id)
        {
            if (SimulatedWorld.Initialized && !PlanetManager.EventFromClient && !PlanetManager.EventFromServer)
            {
                LocalPlayer.SendPacketToLocalStar(new RemoveVegetablePacket(GameMain.localPlanet?.factoryIndex ?? -1, id));
            }
        }
    }
}
