using HarmonyLib;
using NebulaModel.Logger;
using NebulaModel.Packets.Logistics;
using NebulaWorld;
using UnityEngine;

namespace NebulaPatcher.Patches.Dynamic
{
    [HarmonyPatch(typeof(StationComponent))]
    internal class StationComponent_Patch
    {
        [HarmonyPrefix]
        [HarmonyPatch(nameof(StationComponent.InternalTickRemote))]
        public static bool InternalTickRemote_Prefix(StationComponent __instance, int timeGene, double dt, float shipSailSpeed, float shipWarpSpeed, int shipCarries, StationComponent[] gStationPool, AstroData[] astroPoses, VectorLF3 relativePos, Quaternion relativeRot, bool starmap, int[] consumeRegister)
        {
            if (Multiplayer.IsActive && !Multiplayer.Session.LocalPlayer.IsHost)
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
        [HarmonyPatch(nameof(StationComponent.RematchRemotePairs))]
        public static bool RematchRemotePairs_Prefix(StationComponent __instance, StationComponent[] gStationPool, int gStationCursor, int keyStationGId, int shipCarries)
        {
            if (Multiplayer.IsActive && !Multiplayer.Session.LocalPlayer.IsHost)
            {
                // skip vanilla code entirely for clients as we do this event based triggered by the server
                return false;
            }
            return true;
        }

        [HarmonyPostfix]
        [HarmonyPatch(nameof(StationComponent.IdleShipGetToWork))]
        public static void IdleShipGetToWork_Postfix(StationComponent __instance, int index)
        {
            if (Multiplayer.IsActive && Multiplayer.Session.LocalPlayer.IsHost)
            {
                ILSIdleShipBackToWork packet = new ILSIdleShipBackToWork(__instance.workShipDatas[__instance.workShipCount - 1], __instance.gid, __instance.workShipDatas.Length, __instance.warperCount);
                Multiplayer.Session.Network.SendPacket(packet);
            }
        }

        // as we unload PlanetFactory objects when leaving the star system we need to prevent the call on ILS entries in the gStationPool array
        [HarmonyPrefix]
        [HarmonyPatch(nameof(StationComponent.Free))]
        public static bool Free_Prefix(StationComponent __instance)
        {
            if (!Multiplayer.IsActive || !Multiplayer.Session.Ships.PatchLockILS)
            {
                return true;
            }
            if (__instance.isStellar)
            {
                return false;
            }
            return true;
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(StationComponent.Reset))]
        public static bool Reset_Prefix(StationComponent __instance)
        {
            Log.Debug($"Reset called on gid {__instance.gid}");
            return true;
        }
    }
}
