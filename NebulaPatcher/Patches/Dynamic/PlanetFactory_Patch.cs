﻿using HarmonyLib;
using NebulaAPI;
using NebulaModel.Logger;
using NebulaModel.Packets.Factory;
using NebulaModel.Packets.Planet;
using NebulaWorld;
using UnityEngine;

namespace NebulaPatcher.Patches.Dynamic
{
    [HarmonyPatch(typeof(PlanetFactory))]
    internal class PlanetFactory_patch
    {
        [HarmonyPostfix]
        [HarmonyPatch(nameof(PlanetFactory.AddPrebuildData))]
        public static void AddPrebuildData_Postfix(PlanetFactory __instance, PrebuildData prebuild, ref int __result)
        {
            if (!Multiplayer.IsActive)
            {
                return;
            }

            // If the host game called the method, we need to compute the PrebuildId ourself
            if (Multiplayer.Session.LocalPlayer.IsHost)
            {
                Multiplayer.Session.Factories.SetPrebuildRequest(__instance.planetId, __result, Multiplayer.Session.LocalPlayer.Id);
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(PlanetFactory.BuildFinally))]
        public static bool BuildFinally_Prefix(PlanetFactory __instance, Player player, int prebuildId)
        {
            if (!Multiplayer.IsActive)
            {
                return true;
            }

            if (Multiplayer.Session.LocalPlayer.IsHost)
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

            if (Multiplayer.Session.LocalPlayer.IsHost || !Multiplayer.Session.Factories.IsIncomingRequest.Value)
            {
                int author = Multiplayer.Session.Factories.PacketAuthor == NebulaModAPI.AUTHOR_NONE ? Multiplayer.Session.LocalPlayer.Id : Multiplayer.Session.Factories.PacketAuthor;
                int entityId = Multiplayer.Session.LocalPlayer.IsHost ? NebulaWorld.Factory.FactoryManager.GetNextEntityId(__instance) : -1;
                Multiplayer.Session.Network.SendPacket(new BuildEntityRequest(__instance.planetId, prebuildId, author, entityId));
            }

            if (!Multiplayer.Session.LocalPlayer.IsHost && !Multiplayer.Session.Factories.IsIncomingRequest.Value && !Multiplayer.Session.Drones.IsPendingBuildRequest(-prebuildId))
            {
                Multiplayer.Session.Drones.AddBuildRequestSent(-prebuildId);
            }

            return Multiplayer.Session.LocalPlayer.IsHost || Multiplayer.Session.Factories.IsIncomingRequest.Value;
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(PlanetFactory.UpgradeFinally))]
        public static bool UpgradeFinally_Prefix(PlanetFactory __instance, Player player, int objId, ItemProto replace_item_proto)
        {
            if (!Multiplayer.IsActive)
            {
                return true;
            }

            if (objId == 0 || replace_item_proto == null)
            {
                return false;
            }

            if (Multiplayer.Session.LocalPlayer.IsHost || !Multiplayer.Session.Factories.IsIncomingRequest.Value)
            {
                Multiplayer.Session.Network.SendPacket(new UpgradeEntityRequest(__instance.planetId, objId, replace_item_proto.ID, Multiplayer.Session.Factories.PacketAuthor == -1 ? Multiplayer.Session.LocalPlayer.Id : Multiplayer.Session.Factories.PacketAuthor));
            }

            return Multiplayer.Session.LocalPlayer.IsHost || Multiplayer.Session.Factories.IsIncomingRequest.Value;
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(PlanetFactory.GameTick))]
        public static bool InternalUpdate_Prefix()
        {
            if (Multiplayer.IsActive)
            {
                Multiplayer.Session.Storage.IsHumanInput = false;
            }
            return true;
        }

        [HarmonyPostfix]
        [HarmonyPatch(nameof(PlanetFactory.GameTick))]
        public static void InternalUpdate_Postfix()
        {
            if (Multiplayer.IsActive)
            {
                Multiplayer.Session.Storage.IsHumanInput = true;
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(PlanetFactory.PasteBuildingSetting))]
        public static void PasteBuildingSetting_Prefix(PlanetFactory __instance, int objectId)
        {
            if (Multiplayer.IsActive && !Multiplayer.Session.Factories.IsIncomingRequest.Value)
            {
                Multiplayer.Session.Network.SendPacketToLocalStar(new PasteBuildingSettingUpdate(objectId, BuildingParameters.clipboard, GameMain.localPlanet?.id ?? -1));
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(PlanetFactory.FlattenTerrainReform))]
        public static void FlattenTerrainReform_Prefix(PlanetFactory __instance, Vector3 center, float radius, int reformSize, bool veinBuried, float fade0)
        {
            if (Multiplayer.IsActive && !Multiplayer.Session.Factories.IsIncomingRequest.Value)
            {
                Multiplayer.Session.Network.SendPacketToLocalStar(new FoundationBuildUpdatePacket(radius, reformSize, veinBuried, fade0));
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(nameof(PlanetFactory.RemoveVegeWithComponents))]
        public static void RemoveVegeWithComponents_Postfix(PlanetFactory __instance, int id)
        {
            if (Multiplayer.IsActive && !Multiplayer.Session.Planets.IsIncomingRequest)
            {
                Multiplayer.Session.Network.SendPacketToLocalStar(new VegeMinedPacket(GameMain.localPlanet?.id ?? -1, id, 0, false));
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(nameof(PlanetFactory.RemoveVeinWithComponents))]
        public static void RemoveVeinWithComponents_Postfix(PlanetFactory __instance, int id)
        {
            if (Multiplayer.IsActive && !Multiplayer.Session.Planets.IsIncomingRequest)
            {
                if (Multiplayer.Session.LocalPlayer.IsHost)
                {
                    Multiplayer.Session.Network.SendPacketToStar(new VegeMinedPacket(__instance.planetId, id, 0, true), __instance.planet.star.id);
                }
                else
                {
                    Multiplayer.Session.Network.SendPacketToLocalStar(new VegeMinedPacket(__instance.planetId, id, 0, true));
                }
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(nameof(PlanetFactory.EnableEntityWarning))]
        public static void EnableEntityWarning_Postfix(PlanetFactory __instance, int entityId)
        {
            if (Multiplayer.IsActive && entityId > 0 && __instance.entityPool[entityId].id == entityId)
            {
                if (Multiplayer.Session.LocalPlayer.IsClient)
                {
                    //Becasue WarningSystem.NewWarningData is blocked on client, we give it a dummy warningId
                    __instance.entityPool[entityId].warningId = 1;
                }
                Multiplayer.Session.Network.SendPacketToLocalStar(new EntityWarningSwitchPacket(__instance.planetId, entityId, true));
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(nameof(PlanetFactory.DisableEntityWarning))]
        public static void DisableEntityWarning_Postfix(PlanetFactory __instance, int entityId)
        {
            if (Multiplayer.IsActive && entityId > 0 && __instance.entityPool[entityId].id == entityId)
            {
                Multiplayer.Session.Network.SendPacketToLocalStar(new EntityWarningSwitchPacket(__instance.planetId, entityId, false));
            }
        }
    }
}
