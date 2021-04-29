using HarmonyLib;
using NebulaModel.Logger;
using NebulaModel.Packets.Logistics;
using NebulaWorld;
using NebulaWorld.Logistics;
using UnityEngine;

namespace NebulaPatcher.Patches.Dynamic
{
    [HarmonyPatch(typeof(StationComponent))]
    class StationComponent_Patch
    {
        [HarmonyPrefix]
        [HarmonyPatch("InternalTickRemote")]
        public static bool InternalTickRemote_Prefix(StationComponent __instance, int timeGene, double dt, float shipSailSpeed, float shipWarpSpeed, int shipCarries, StationComponent[] gStationPool, AstroPose[] astroPoses, VectorLF3 relativePos, Quaternion relativeRot, bool starmap, int[] consumeRegister)
        {
            if(SimulatedWorld.Initialized && !LocalPlayer.IsMasterClient)
            {
                // skip vanilla code entirely and use our modified version instead (which focuses on ship movement)
                // call our InternalTickRemote() for every StationComponent in game. Normally this would be done by each PlanetFactory, but as a client
                // we dont have every PlanetFactory at hand.
                // so iterate over the GameMain.data.galacticTransport.stationPool array which should also include the fake entries for factories we have not loaded yet.
                // but do this at another place to not trigger it more often than needed (GameData::GameTick())
                return false;
            }
            return true;
        }

        [HarmonyPrefix]
        [HarmonyPatch("RematchRemotePairs")]
        public static bool RematchRemotePairs_Prefix(StationComponent __instance, StationComponent[] gStationPool, int gStationCursor, int keyStationGId, int shipCarries)
        {
            if (SimulatedWorld.Initialized && !LocalPlayer.IsMasterClient)
            {
                // skip vanilla code entirely for clients as we do this event based triggered by the server
                return false;
            }
            return true;
        }

        // this one is to catch changes to workShipData to update rendering for clients
        [HarmonyPostfix]
        [HarmonyPatch("RematchRemotePairs")]
        public static void RematchRemotePairs_Postfix(StationComponent __instance)
        {
            if(SimulatedWorld.Initialized && LocalPlayer.IsMasterClient && __instance.isStellar)
            {
                int[] shipIndex = new int[__instance.workShipDatas.Length];
                int[] otherGId = new int[__instance.workShipDatas.Length];
                int[] direction = new int[__instance.workShipDatas.Length];
                int[] itemId = new int[__instance.workShipDatas.Length];
                for(int i = 0; i < __instance.workShipDatas.Length; i++)
                {
                    shipIndex[i] = __instance.workShipDatas[i].shipIndex;
                    otherGId[i] = __instance.workShipDatas[i].otherGId;
                    direction[i] = __instance.workShipDatas[i].direction;
                    itemId[i] = __instance.workShipDatas[i].itemId;
                }
                LocalPlayer.SendPacket(new ILSShipDataUpdate(__instance.gid, shipIndex, otherGId, direction, itemId));
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch("IdleShipGetToWork")]
        public static void IdleShipGetToWork_Postfix(StationComponent __instance, int index)
        {
            if(SimulatedWorld.Initialized && LocalPlayer.IsMasterClient)
            {
                ILSShipData packet = new ILSShipData(true, __instance.workShipDatas[__instance.workShipCount-1].planetA, __instance.workShipDatas[__instance.workShipCount - 1].planetB, __instance.workShipDatas[__instance.workShipCount - 1].itemId, __instance.workShipDatas[__instance.workShipCount - 1].itemCount, __instance.gid, __instance.workShipDatas[__instance.workShipCount - 1].otherGId, index, __instance.workShipDatas[__instance.workShipCount - 1].warperCnt, __instance.warperCount);
                LocalPlayer.SendPacket(packet);
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch("WorkShipBackToIdle")]
        public static void WorkShipBackToIdle_Postfix(StationComponent __instance, int index)
        {
            if(SimulatedWorld.Initialized && LocalPlayer.IsMasterClient)
            {
                ILSShipData packet = new ILSShipData(false, __instance.gid, index);
                LocalPlayer.SendPacket(packet);
            }
        }

        // as we unload PlanetFactory objects when leaving the star system we need to prevent the call on ILS entries in the gStationPool array
        [HarmonyPrefix]
        [HarmonyPatch("Free")]
        public static bool Free_Prefix(StationComponent __instance)
        {
            if(!SimulatedWorld.Initialized || !ILSShipManager.PatchLockILS)
            {
                return true;
            }
            if (__instance.isStellar)
            {
                return false;
            }
            return true;
        }
    }
}
