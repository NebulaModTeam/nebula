using HarmonyLib;
using NebulaModel.Logger;
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
        [HarmonyPatch("IdleShipGetToWork")]
        public static bool IdleShipGetToWork_Prefix(StationComponent __instance, int index)
        {
            if(SimulatedWorld.Initialized && LocalPlayer.IsMasterClient)
            {
                Log.Info($"starting ship {index} and array length is {__instance.workShipDatas.Length}");
                PlanetData planetA = GameMain.galaxy.PlanetById(__instance.workShipDatas[index].planetA);
                PlanetData planetB = GameMain.galaxy.PlanetById(__instance.workShipDatas[index].planetA);
                if (planetA != null && planetB != null)
                {
                    Log.Info($"ship goes from {planetA.displayName} to {planetB.displayName}");
                }
                Log.Info($"it transfers {__instance.workShipDatas[index].itemCount} of item {__instance.workShipDatas[index].itemId}");
            }
            return true;
        }
    }
}
