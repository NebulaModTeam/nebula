using HarmonyLib;
using NebulaModel.Logger;
using NebulaModel.Packets.Logistics;
using NebulaPatcher.Patches.Transpilers;
using NebulaWorld;
using System;
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
                //UpdateShipRendering(__instance, timeGene, dt, shipSailSpeed, shipWarpSpeed, shipCarries, gStationPool, astroPoses, relativePos, relativeRot, starmap, consumeRegister);
                //Debug.Log(GameMain.galaxy.PlanetById(__instance.planetId).displayName + ": " + __instance.workShipCount + "/" + __instance.idleShipCount);
                StationComponent_Transpiler.ILSUpdateShipPos(__instance, timeGene, dt, shipSailSpeed, shipWarpSpeed, shipCarries, gStationPool, astroPoses, relativePos, relativeRot, starmap, consumeRegister);
                __instance.ShipRenderersOnTick(astroPoses, relativePos, relativeRot);
               //Debug.Log(GameMain.data.galacticTransport.shipRenderer.shipCount + " | " + GameMain.data.galacticTransport.stationCursor + " | " + GameMain.data.galacticTransport.shipRenderer.transport.stationCursor);
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

        private static void UpdateShipRendering(StationComponent __instance, int timeGene, double dt, float shipSailSpeed, float shipWarpSpeed, int shipCarries, StationComponent[] gStationPool, AstroPose[] astroPoses, VectorLF3 relativePos, Quaternion relativeRot, bool starmap, int[] consumeRegister)
        {
            float shipSailSpeedComputed = Mathf.Sqrt(shipSailSpeed / 600f); // num24
            float shipSailSpeedComputedLog = shipSailSpeedComputed;
            if(shipSailSpeedComputedLog > 1f)
            {
                shipSailSpeedComputedLog = Mathf.Log(shipSailSpeedComputedLog) + 1f; // num25
            }
            float shipSailTimeDelta = shipSailSpeedComputed * 0.006f + 1E-05f;
            AstroPose astroPose = astroPoses[__instance.planetId];

            for(int i = 0; i < __instance.workShipCount; i++)
            {
                ShipData shipData = __instance.workShipDatas[i];
                if(shipData.otherGId <= 0)
                {
                    shipData.direction = -1;
                    if (shipData.stage > 0)
                    {
                        shipData.stage = 0;
                    }
                }
                if(shipData.stage < -1)
                {
                    if (shipData.direction > 0)
                    {
                        shipData.t += 0.03335f;
                        if (shipData.t > 1f)
                        {
                            shipData.t = 0f;
                            shipData.stage = -1;
                        }
                    }
                    else
                    {
                        shipData.t -= 0.03335f;
                        if (shipData.t < 0f)
                        {
                            shipData.t = 0f;
                            continue;
                        }
                        shipData.uPos = astroPose.uPos + Maths.QRotateLF(astroPose.uRot, __instance.shipDiskPos[shipData.shipIndex]);
                        shipData.uVel.x = 0f;
                        shipData.uVel.y = 0f;
                        shipData.uVel.z = 0f;
                        shipData.uSpeed = 0f;
                        shipData.uRot = astroPose.uRot * __instance.shipDiskRot[shipData.shipIndex];
                        shipData.uAngularVel.x = 0f;
                        shipData.uAngularVel.y = 0f;
                        shipData.uAngularVel.z = 0f;
                        shipData.uAngularSpeed = 0f;
                        shipData.pPosTemp = Vector3.zero;
                        shipData.pRotTemp = Quaternion.identity;
                        __instance.shipRenderers[shipData.shipIndex].anim.z = 0f;
                        Debug.Log("WOHA");
                    }
                }
                if(shipData.stage == -1)
                {
                    if(shipData.direction > 0)
                    {
                        shipData.t += shipSailTimeDelta;
                        if(shipData.t > 1f)
                        {
                            shipData.t = 1f;
                            shipData.stage = 0;
                        }
                        __instance.shipRenderers[shipData.shipIndex].anim.z = shipData.t;
                        float shipTime = (3f - shipData.t - shipData.t) * shipData.t * shipData.t;
                        shipData.uPos = astroPose.uPos + Maths.QRotateLF(astroPose.uRot, __instance.shipDiskPos[shipData.shipIndex] + __instance.shipDiskPos[shipData.shipIndex].normalized * (25f * shipTime));
                        shipData.uRot = astroPose.uRot * __instance.shipDiskRot[shipData.shipIndex];
                    }
                    else
                    {
                        shipData.t -= shipSailTimeDelta * 0.6666667f;
                        if (shipData.t < 0f)
                        {
                            shipData.t = 1f; // this is vanilla but is it right?
                            shipData.stage = -2;
                        }
                        __instance.shipRenderers[shipData.shipIndex].anim.z = shipData.t;
                        float shipTime = (3f - shipData.t - shipData.t) * shipData.t * shipData.t;

                        VectorLF3 lhs = astroPose.uPos + Maths.QRotateLF(astroPose.uRot, __instance.shipDiskPos[shipData.shipIndex]);
                        VectorLF3 lhs2 = astroPose.uPos + Maths.QRotateLF(astroPose.uRot, shipData.pPosTemp);
                        shipData.uPos = lhs * (double)(1f - shipTime) + lhs2 * (double)shipTime;
                        shipData.uRot = astroPose.uRot * Quaternion.Slerp(__instance.shipDiskRot[shipData.shipIndex], shipData.pRotTemp, shipTime * 2f - 1f);
                    }
                    shipData.uVel.x = 0f;
                    shipData.uVel.y = 0f;
                    shipData.uVel.z = 0f;
                    shipData.uSpeed = 0f;
                    shipData.uAngularVel.x = 0f;
                    shipData.uAngularVel.y = 0f;
                    shipData.uAngularVel.z = 0f;
                    shipData.uAngularSpeed = 0f;
                    Debug.Log("WEYY");
                }
                if(shipData.stage == 0)
                {
                    AstroPose astroPose2 = astroPoses[shipData.planetB];
                    VectorLF3 lhs3;
                    if (shipData.direction > 0)
                    {
                        lhs3 = astroPose2.uPos + Maths.QRotateLF(astroPose2.uRot, gStationPool[shipData.otherGId].shipDockPos + gStationPool[shipData.otherGId].shipDockPos.normalized * 25f);
                    }
                    else
                    {
                        lhs3 = astroPose.uPos + Maths.QRotateLF(astroPose.uRot, __instance.shipDiskPos[shipData.shipIndex] + __instance.shipDiskPos[shipData.shipIndex].normalized * 25f);
                    }
                    VectorLF3 vectorLF = lhs3 - shipData.uPos;
                    double d = vectorLF.x * vectorLF.x + vectorLF.y * vectorLF.y + vectorLF.z * vectorLF.z;
                    double distShipDock = Math.Sqrt(d); //num31
                    VectorLF3 vectorLF2 = (shipData.direction <= 0) ? (astroPose2.uPos - shipData.uPos) : (astroPose.uPos - shipData.uPos);
                    double distShipDock2 = vectorLF2.x * vectorLF2.x + vectorLF2.y * vectorLF2.y + vectorLF2.z * vectorLF2.z; // num32

                    bool warpFasterThanSail = shipWarpSpeed > shipSailSpeed + 1f; // flag
                    bool shipIsNearPlanet = distShipDock2 <= (double)(astroPose.uRadius * astroPose.uRadius) * 2.25; // flag8
                    bool shipIsLanding = false; // flag9

                    float helperFloat1 = 0f; // num33
                    float helperFloat2 = 0f; // num40

                    if(distShipDock < 6.0)
                    {
                        shipData.t = 1f;
                        shipData.stage = shipData.direction;
                        shipIsLanding = true;
                    }
                    if (warpFasterThanSail)
                    {
                        double magnitude = (astroPose.uPos - astroPose2.uPos).magnitude;
                        double realShipWarpSpeed = ((double)shipWarpSpeed >= magnitude * 2.0) ? magnitude * 2.0 : ((double)shipWarpSpeed); // num35

                        if(shipData.warpState <= 0f)
                        {
                            shipData.warpState = 0f;
                            bool warperFree = (bool)AccessTools.Field(typeof(StationComponent), "warperFree").GetValue(__instance);
                            if(distShipDock2 > 25000000.0 && distShipDock > __instance.warpEnableDist * 0.5 && shipData.uSpeed >= shipSailSpeed && (shipData.warperCnt > 0 || warperFree))
                            {
                                // WEY HERE WARPING STARTS
                                shipData.warperCnt--;
                                shipData.warpState += (float)dt;
                            }
                        }
                        else
                        {
                            helperFloat1 = (float)(realShipWarpSpeed * ((Math.Pow(1001.0, (double)shipData.warpState) - 1.0) / 1000.0));
                            double helperDouble1 = (double)helperFloat1 * 0.0449 + 5000.0 + (double)shipSailSpeed * 0.25;
                            double distMinusHelperDouble1 = distShipDock - helperDouble1; // num38

                            if(distMinusHelperDouble1 < 0.0)
                            {
                                distMinusHelperDouble1 = 0.0;
                            }
                            if(distShipDock < helperDouble1)
                            {
                                shipData.warpState -= (float)(dt * 4.0);
                            }
                            else
                            {
                                shipData.warpState += (float)dt;
                            }

                            if (shipData.warpState < 0f)
                            {
                                shipData.warpState = 0f;
                            }
                            else if (shipData.warpState > 1f)
                            {
                                shipData.warpState = 1f;
                            }

                            if (shipData.warpState > 0f)
                            {
                                helperFloat1 = (float)(realShipWarpSpeed * ((Math.Pow(1001.0, (double)shipData.warpState) - 1.0) / 1000.0));
                                if ((double)helperFloat1 * dt > distMinusHelperDouble1)
                                {
                                    helperFloat1 = (float)(distMinusHelperDouble1 / dt * 1.01);
                                }
                            }
                        }
                    }
                    double timeETA = distShipDock / ((double)shipData.uSpeed + 0.1) * 0.382 * (double)shipSailSpeedComputedLog; // num39
                    if(shipData.warpState > 0f)
                    {
                        helperFloat2 = (shipData.uSpeed = shipSailSpeed + helperFloat1);
                        if (helperFloat2 > shipSailSpeed)
                        {
                            helperFloat2 = shipSailSpeed;
                        }
                    }
                    else
                    {
                        float speedMulTime = (float)((double)shipData.uSpeed * timeETA) + 6f; // num41
                        if(speedMulTime > shipSailSpeed)
                        {
                            speedMulTime = shipSailSpeed;
                        }

                        float idkFloat1 = (float)dt * ((!shipIsNearPlanet) ? shipSailSpeed * 0.12f * shipSailSpeedComputedLog : shipSailSpeed * 0.03f * shipSailSpeedComputedLog); // num42
                        if(shipData.uSpeed < speedMulTime - idkFloat1)
                        {
                            shipData.uSpeed += idkFloat1;
                        }
                        else if(shipData.uSpeed > speedMulTime + shipSailSpeed * 0.4f * shipSailSpeedComputed)
                        {
                            shipData.uSpeed -= shipSailSpeed * 0.4f * shipSailSpeedComputed;
                        }
                        else
                        {
                            shipData.uSpeed = speedMulTime;
                        }
                        helperFloat2 = shipData.uSpeed;
                    }
                    int IntHelper1 = -1;
                    // 541
                }
            }
        }
    }
}
