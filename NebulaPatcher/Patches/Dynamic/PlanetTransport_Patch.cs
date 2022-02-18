﻿using HarmonyLib;
using NebulaModel.Logger;
using NebulaModel.Packets.Logistics;
using NebulaWorld;

namespace NebulaPatcher.Patches.Dynamic
{
    [HarmonyPatch(typeof(PlanetTransport))]
    internal class PlanetTransport_Patch
    {
        private static int RemovingStationGId = 0;

        [HarmonyPrefix]
        [HarmonyPatch(nameof(PlanetTransport.SetStationStorage))]
        public static bool SetStationStorage_Postfix(PlanetTransport __instance, int stationId, int storageIdx, int itemId, int itemCountMax, ELogisticStorage localLogic, ELogisticStorage remoteLogic, Player player)
        {
            if (Multiplayer.IsActive && !Multiplayer.Session.Ships.PatchLockILS)
            {
                StationComponent stationComponent = __instance.stationPool[stationId];

                if (stationComponent != null)
                {
                    StorageUI packet = new StorageUI(__instance.planet.id, stationComponent.id, stationComponent.gid, storageIdx, itemId, itemCountMax, localLogic, remoteLogic);
                    Multiplayer.Session.Network.SendPacket(packet);
                }

                if (Multiplayer.Session.LocalPlayer.IsHost)
                {
                    return true;
                }

                return false;
            }
            return true;
        }
        // for the PLS slot to sync properly the StationComponent of the PLS needs to have planetId set to the correct value.
        // as the game does not do that for some reason, we need to do it here
        [HarmonyPostfix]
        [HarmonyPatch(nameof(PlanetTransport.NewStationComponent))]
        public static void NewStationComponent_AddPlanetId_Postfix(PlanetTransport __instance, StationComponent __result, int _entityId, int _pcId, PrefabDesc _desc)
        {
            if (!Multiplayer.IsActive)
            {
                return;
            }

            if (!__result.isStellar && __result.planetId == 0)
            {
                __result.planetId = __instance.planet.id;
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(nameof(PlanetTransport.NewStationComponent))]
        public static void NewStationComponent_BroadcastNewILS_Postfix(PlanetTransport __instance, StationComponent __result, int _entityId, int _pcId, PrefabDesc _desc)
        {
            if (!Multiplayer.IsActive || !Multiplayer.Session.LocalPlayer.IsHost)
            {
                return;
            }

            // We don't need to do this for PLS
            if (__result.gid == 0)
            {
                return;
            }

            // After host has added the StationComponent it has planetId, id and gId, now we can inform all clients about this station
            // so they can add it to their GalacticTransport as they don't do that. Note that we're doing this in
            // PlanetTransport.NewStationComponent and not GalacticTransport.AddStationComponent because stationId will be set at this point.
            Log.Info($"Sending packet about new station component to all clients for planet {__result.planetId}, id {__result.id} with gId of {__result.gid}");
            Multiplayer.Session.Network.SendPacket(new ILSAddStationComponent(__result.planetId, __result.id, __result.gid, _desc.stationMaxShipCount));
        }


        [HarmonyPostfix]
        [HarmonyPatch(nameof(PlanetTransport.Import))]
        public static void Import_Postfix(PlanetTransport __instance)
        {
            if (!Multiplayer.IsActive)
            {
                return;
            }
            foreach (StationComponent stationComponent in __instance.stationPool)
            {
                if (stationComponent != null && stationComponent.planetId == 0 && !stationComponent.isStellar)
                {
                    stationComponent.planetId = __instance.planet.id;
                }
            }
        }
        /*
         * As clients need to access the StationComponent in gStationPool when RematchRemotePairs() is called (and this also gets called by RemoveStationComponent())
         * we need to prevent the call for client here to avoid a NRE and instead call it triggered by host after RematchRemotePairs() got called.
         * basically in a Postfix of RemoveStationComponent()
         */
        [HarmonyPrefix]
        [HarmonyPatch(nameof(PlanetTransport.RemoveStationComponent))]
        public static bool RemoveStationComponent_Prefix(PlanetTransport __instance, int id)
        {
            RemovingStationGId = __instance.stationPool[id].gid; // cache this as we need it in the postfix but its gone there already.
            return !Multiplayer.IsActive || Multiplayer.Session.LocalPlayer.IsHost || Multiplayer.Session.Ships.PatchLockILS;
        }

        /*
         * Host has called RematchRemotePairs() now and thus has send the ILSShipDataUpdate packet, so we can savely tell clients to remove the station component now.
         */
        [HarmonyPostfix]
        [HarmonyPatch(nameof(PlanetTransport.RemoveStationComponent))]
        public static void RemoveStationComponent_Postfix(PlanetTransport __instance, int id)
        {
            if (!Multiplayer.IsActive || !Multiplayer.Session.LocalPlayer.IsHost)
            {
                return;
            }
            Multiplayer.Session.Network.SendPacket(new ILSRemoveStationComponent(id, __instance.planet.id, RemovingStationGId));
        }
    }
}
