using HarmonyLib;
using NebulaModel.Logger;
using NebulaModel.Packets.Logistics;
using NebulaWorld;
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
                // skip vanilla code entirely for clients as we do this event based triggered by the server
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

        [HarmonyPostfix]
        [HarmonyPatch("IdleShipGetToWork")]
        public static void IdleShipGetToWork_Postfix(StationComponent __instance, int index)
        {
            if(SimulatedWorld.Initialized && LocalPlayer.IsMasterClient)
            {
                ILSShipData packet = new ILSShipData(true, __instance.workShipDatas[index].planetA, __instance.workShipDatas[index].planetB, __instance.workShipDatas[index].itemId, __instance.workShipDatas[index].itemCount, __instance.workShipDatas[index].otherGId, __instance.gid, index);
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
    }
}
