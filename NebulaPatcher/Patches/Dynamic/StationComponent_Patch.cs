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
                ILSShipData packet = new ILSShipData(true, __instance.workShipDatas[index].planetA, __instance.workShipDatas[index].planetB, __instance.workShipDatas[index].itemId, __instance.workShipDatas[index].itemCount, __instance.gid, __instance.workShipDatas[index].otherGId, index);
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

        [HarmonyPrefix]
        [HarmonyPatch("ShipRenderersOnTick")]
        public static bool ShipRenderersOnTick_Prefix(StationComponent __instance, AstroPose[] astroPoses, VectorLF3 rPos, Quaternion rRot)
        {
            if(__instance.planetId != 102 || !SimulatedWorld.Initialized || LocalPlayer.IsMasterClient)
            {
                return true;
            }
            int num = 0;
            int num2 = 0;
            int num3 = __instance.workShipDatas.Length;
            for (int i = 0; i < num3; i++)
            {
                if ((__instance.idleShipIndices & 1UL << i) != 0UL)
                {
                    num++;
                }
            }
            int num4 = __instance.idleShipCount - num;
            if (num4 > 0)
            {
                for (int j = 0; j < num3; j++)
                {
                    if (!__instance.HasShipIndex(j))
                    {
                        __instance.AddIdleShip(j);
                        num4--;
                        if (num4 == 0)
                        {
                            break;
                        }
                    }
                }
            }
            else if (num4 < 0)
            {
                for (int k = num3 - 1; k >= 0; k--)
                {
                    if ((__instance.idleShipIndices & 1UL << k) != 0UL)
                    {
                        __instance.RemoveIdleShip(k);
                        num4++;
                        if (num4 == 0)
                        {
                            break;
                        }
                    }
                }
            }
            Assert.Zero(num4);
            Debug.Log("After Assert");
            for (int l = 0; l < num3; l++)
            {
                if (__instance.HasIdleShipIndex(l))
                {
                    __instance.shipRenderers[l].gid = __instance.gid;
                    __instance.shipRenderers[l].SetPose((Vector3)astroPoses[__instance.planetId].uPos + astroPoses[__instance.planetId].uRot * __instance.shipDiskPos[l], astroPoses[__instance.planetId].uRot * __instance.shipDiskRot[l], rPos, rRot, Vector3.zero, 0);
                    num++;
                    num2 = l + 1;
                    __instance.shipRenderers[l].anim = Vector3.zero;
                    __instance.shipUIRenderers[l].gid = 0;
                }
                else if (__instance.HasWorkShipIndex(l))
                {
                    __instance.shipRenderers[l].gid = __instance.gid;
                    num2 = l + 1;
                    __instance.shipUIRenderers[l].gid = __instance.gid;
                }
                else
                {
                    __instance.shipRenderers[l].gid = 0;
                    __instance.shipRenderers[l].anim = Vector3.zero;
                    __instance.shipUIRenderers[l].gid = 0;
                }
            }
            __instance.renderShipCount = num2;
            Debug.Log(__instance.gid + " " + num2);
            return false;
        }
    }
}
