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
