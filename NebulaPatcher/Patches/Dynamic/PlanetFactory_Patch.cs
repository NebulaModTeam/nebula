using HarmonyLib;
using NebulaModel.Logger;
using NebulaModel.Packets.Factory;
using NebulaModel.Packets.Planet;
using NebulaWorld;
using NebulaWorld.Factory;
using NebulaWorld.Planet;
using NebulaWorld.Player;
using UnityEngine;

namespace NebulaPatcher.Patches.Dynamic
{
    [HarmonyPatch(typeof(PlanetFactory))]
    class PlanetFactory_patch
    {
        [HarmonyPostfix]
        [HarmonyPatch(nameof(PlanetFactory.AddPrebuildData))]
        public static void AddPrebuildData_Postfix(PlanetFactory __instance, PrebuildData prebuild, ref int __result)
        {
            if (!Multiplayer.IsActive)
                return;

            // If the host game called the method, we need to compute the PrebuildId ourself
            if (LocalPlayer.IsMasterClient)
            {
                Multiplayer.Session.Factories.SetPrebuildRequest(__instance.planetId, __result, LocalPlayer.PlayerId);
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(PlanetFactory.BuildFinally))]
        public static bool BuildFinally_Prefix(PlanetFactory __instance, Player player, int prebuildId)
        {
            if (!Multiplayer.IsActive)
                return true;

            if (LocalPlayer.IsMasterClient)
            {
                if (!Multiplayer.Session.Factories.ContainsPrebuildRequest(__instance.planetId, prebuildId))
                {
                    // This prevents duplicating the entity when multiple players trigger the BuildFinally for the same entity at the same time.
                    // If it occurs in any other circumstances, it means that we have some desynchronization between clients and host prebuilds buffers.
                    Log.Warn($"BuildFinally was called without having a corresponding PrebuildRequest for the prebuild {prebuildId} on the planet {__instance.planetId}");
                    return false;
                }

                // Remove the prebuild request from the list since we will now convert it to a real building
                Multiplayer.Session.Factories.RemovePrebuildRequest(__instance.planetId, prebuildId);
            }

            if (LocalPlayer.IsMasterClient || !Multiplayer.Session.Factories.IsIncomingRequest)
            {
                LocalPlayer.SendPacket(new BuildEntityRequest(__instance.planetId, prebuildId, Multiplayer.Session.Factories.PacketAuthor == FactoryManager.AUTHOR_NONE ? LocalPlayer.PlayerId : Multiplayer.Session.Factories.PacketAuthor));
            }

            if (!LocalPlayer.IsMasterClient && !Multiplayer.Session.Factories.IsIncomingRequest && !Multiplayer.Session.Drones.IsPendingBuildRequest(-prebuildId))
            {
                Multiplayer.Session.Drones.AddBuildRequestSent(-prebuildId);
            }

            return LocalPlayer.IsMasterClient || Multiplayer.Session.Factories.IsIncomingRequest;
        }

        [HarmonyPrefix]
        [HarmonyPatch("UpgradeFinally")]
        public static bool UpgradeFinally_Prefix(PlanetFactory __instance, Player player, int objId, ItemProto replace_item_proto)
        {
            if (!Multiplayer.IsActive)
                return true;

            if (LocalPlayer.IsMasterClient || !Multiplayer.Session.Factories.IsIncomingRequest)
            {
                LocalPlayer.SendPacket(new UpgradeEntityRequest(__instance.planetId, objId, replace_item_proto.ID, Multiplayer.Session.Factories.PacketAuthor == -1 ? LocalPlayer.PlayerId : Multiplayer.Session.Factories.PacketAuthor));
            }

            return LocalPlayer.IsMasterClient || Multiplayer.Session.Factories.IsIncomingRequest;
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(PlanetFactory.GameTick))]
        public static bool InternalUpdate_Prefix()
        {
            Multiplayer.Session.Storage.IsHumanInput = false;
            return true;
        }

        [HarmonyPostfix]
        [HarmonyPatch(nameof(PlanetFactory.GameTick))]
        public static void InternalUpdate_Postfix()
        {
            Multiplayer.Session.Storage.IsHumanInput = true;
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(PlanetFactory.PasteBuildingSetting))]
        public static void PasteBuildingSetting_Prefix(PlanetFactory __instance, int objectId)
        {
            if (Multiplayer.IsActive && !Multiplayer.Session.Factories.IsIncomingRequest)
            {
                LocalPlayer.SendPacketToLocalStar(new PasteBuildingSettingUpdate(objectId, BuildingParameters.clipboard, GameMain.localPlanet?.id ?? -1));
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(PlanetFactory.FlattenTerrainReform))]
        public static void FlattenTerrainReform_Prefix(PlanetFactory __instance, Vector3 center, float radius, int reformSize, bool veinBuried, float fade0)
        {
            if (Multiplayer.IsActive && !Multiplayer.Session.Factories.IsIncomingRequest)
            {
                LocalPlayer.SendPacketToLocalStar(new FoundationBuildUpdatePacket(radius, reformSize, veinBuried, fade0));
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(nameof(PlanetFactory.RemoveVegeWithComponents))]
        public static void RemoveVegeWithComponents_Postfix(PlanetFactory __instance, int id)
        {
            if (Multiplayer.IsActive && !Multiplayer.Session.Planets.IsIncomingRequest)
            {
                LocalPlayer.SendPacketToLocalStar(new VegeMinedPacket(GameMain.localPlanet?.id ?? -1, id, 0, false));
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(nameof(PlanetFactory.RemoveVeinWithComponents))]
        public static void RemoveVeinWithComponents_Postfix(PlanetFactory __instance, int id)
        {
            if (Multiplayer.IsActive && !Multiplayer.Session.Planets.IsIncomingRequest)
            {
                if (LocalPlayer.IsMasterClient)
                {
                    LocalPlayer.SendPacketToStar(new VegeMinedPacket(__instance.planetId, id, 0, true), __instance.planet.star.id);
                }
                else
                {
                    LocalPlayer.SendPacketToLocalStar(new VegeMinedPacket(__instance.planetId, id, 0, true));
                }
            }
        }
    }
}
