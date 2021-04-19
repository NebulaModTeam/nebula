using HarmonyLib;
using NebulaWorld;
using NebulaModel.Packets.Logistics;
using NebulaWorld.Logistics;
using UnityEngine;
using System.Collections.Generic;

namespace NebulaPatcher.Patches.Dynamic
{
    [HarmonyPatch(typeof(GalacticTransport))]
    class GalacticTransport_Patch
    {
        [HarmonyPostfix]
        [HarmonyPatch("SetForNewGame")]
        public static void SetForNewGame_Postfix()
        {
            if(SimulatedWorld.Initialized && !LocalPlayer.IsMasterClient)
            {
                LocalPlayer.SendPacket(new ILSRequestgStationPoolSync());
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch("AddStationComponent")]
        public static bool AddStationComponent_Prefix(GalacticTransport __instance, int planetId, StationComponent station)
        {
            if (!SimulatedWorld.Initialized || LocalPlayer.IsMasterClient || LocalPlayer.PatchLocks["GalacticTransport"])
            {
                return true;
            }
            if (!ILSShipManager.AddStationComponentQueue.ContainsKey(planetId))
            {
                ILSShipManager.AddStationComponentQueue.Add(planetId, new List<StationComponent>());
            }
            ILSShipManager.AddStationComponentQueue[planetId].Add(station);
            LocalPlayer.SendPacket(new ILSAddStationComponentRequest(planetId, station.shipDockPos));
            /*
            // if we are a client and have a fake station already saved, update it with the full data given now
            // this should happen when this client arrives at a planet for the first time whichs interplanetar logistic
            // he was already syncing
            while (__instance.stationRecycleCursor > 0 && __instance.stationPool[__instance.stationRecycle[__instance.stationRecycleCursor - 1]] != null)
            {
                Debug.Log("increase because " + (__instance.stationRecycleCursor - 1) + " is ptr to " + __instance.stationRecycle[__instance.stationRecycleCursor - 1] + " which is " + GameMain.galaxy.PlanetById(__instance.stationPool[__instance.stationRecycle[__instance.stationRecycleCursor - 1]].planetId).displayName);
                __instance.stationRecycleCursor++;
                if(__instance.stationRecycleCursor >= __instance.stationPool.Length)
                {
                    __instance.stationRecycleCursor = 0;
                }
            }
            int debug = 0;
            if (__instance.stationRecycleCursor > 0)
            {
                Debug.Log((__instance.stationPool[__instance.stationRecycle[__instance.stationRecycleCursor - 1]] == null) ? "ITS NULL" : "ITS NOT NULL");
                Debug.Log("recycle " + __instance.stationRecycleCursor);
                debug = __instance.stationRecycle[__instance.stationRecycleCursor-1];
            }
            else
            {
                Debug.Log("no recycle");
                debug = __instance.stationCursor;
            }
            Debug.Log("adding station id " + station.id + " on " + GameMain.galaxy.PlanetById(planetId).displayName + " to gid " + debug);
            */
            /*
            for (int i = 0; i < GameMain.data.galacticTransport.stationPool.Length; i++)
            {
                if(GameMain.data.galacticTransport.stationPool[i]?.id == station.id && GameMain.data.galacticTransport.stationPool[i]?.planetId == planetId)
                {
                    Debug.Log("replacing station " + station.id + " on " + GameMain.galaxy.PlanetById(planetId).displayName + " at gid " + station.gid + " with provided one from server");
                    GameMain.data.galacticTransport.stationPool[i] = null;
                    GameMain.data.galacticTransport.stationRecycle[GameMain.data.galacticTransport.stationRecycleCursor++] = i;
                    return true;
                }
            }
            // if we are a client and have no fake station then just add the new one normally
            // this should happen when a player places an ILS on a FactoryData known to this client
            // or when this client arrives at a planet for the first time which contains ILS
            */
            return true;
        }

        [HarmonyPostfix]
        [HarmonyPatch("AddStationComponent")]
        public static void AddStationComponent_Postfix(GalacticTransport __instance)
        {
            if(__instance.stationRecycleCursor > 0)
            {
                Debug.Log("CURSOR NOW AT: " + __instance.stationCursor + " recycle: " + __instance.stationRecycleCursor + "(" + __instance.stationRecycle[__instance.stationRecycleCursor - 1] + ")");
            }
            else
            {
                Debug.Log("CURSOR NOW AT: " + __instance.stationCursor + " recycle: " + __instance.stationRecycleCursor);
            }
            for(int i = 0; i < __instance.stationPool.Length; i++)
            {
                if(__instance.stationPool[i] != null)
                {
                    Debug.Log(i + ": " + __instance.stationPool[i].gid + " " + __instance.stationPool[i].id + " " + __instance.stationPool[i].planetId);
                }
                else
                {
                    Debug.Log(i + ": null");
                }
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch("RemoveStationComponent")]
        public static void RemoveStationComponent_Postfix(GalacticTransport __instance, int gid)
        {
            if (__instance.stationRecycleCursor > 0)
            {
                Debug.Log("REM: CURSOR NOW AT: " + __instance.stationCursor + " recycle: " + __instance.stationRecycleCursor + "(" + __instance.stationRecycle[__instance.stationRecycleCursor - 1] + ")");
            }
            else
            {
                Debug.Log("REM: CURSOR NOW AT: " + __instance.stationCursor + " recycle: " + __instance.stationRecycleCursor);
            }
        }

        /*
         * We probably dont need to patch this as EntityManager.cs will at some point keep track of removed entities.
         * And as such will be able to call this for clients if needed
        [HarmonyPrefix]
        [HarmonyPatch("RemoveStationComponent")]
        public static bool RemoveStationComponent_Prefix(GalacticTransport __instance, int gid)
        {
            if (!SimulatedWorld.Initialized || LocalPlayer.IsMasterClient)
            {
                return true;
            }
            
            return false;
        }
        */
    }
}
