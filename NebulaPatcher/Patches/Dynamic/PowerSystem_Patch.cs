using HarmonyLib;
using NebulaModel.Logger;
using NebulaModel.Packets.PowerSystem;
using NebulaWorld;
using NebulaWorld.Factory;
using PowerNetworkStructures;
using System.Collections.Generic;
using UnityEngine;

namespace NebulaPatcher.Patches.Dynamic
{
    [HarmonyPatch(typeof(PowerSystem))]
    internal class PowerSystem_Patch
    {
        private const float REQUEST_INTERVAL = 1;
        private static float timePassed;

        [HarmonyPrefix]
        [HarmonyPatch(nameof(PowerSystem.GameTick))]
        public static bool PowerSystem_GameTick_Prefix(PowerSystem __instance, long time, ref bool isActive, bool isMultithreadMode)
        {
            //Enable signType update on remote planet every 64 tick
            if ((time & 63) == 0 && Multiplayer.IsActive && Multiplayer.Session.LocalPlayer.IsHost)
            {
                isActive |= true;
            }

            if (Multiplayer.IsActive && Multiplayer.Session.LocalPlayer.IsClient)
            {
                // if player is not in a solar system he has no factories loaded, thus nothing to sync here
                if(GameMain.localStar == null)
                {
                    return false;
                }

                UpdateAnimations(__instance, time, isActive, isMultithreadMode);

                timePassed += Time.deltaTime;

                if(timePassed >= REQUEST_INTERVAL)
                {
                    timePassed = 0;

                    List<int> pIDs = new List<int>();
                    for(int i = 0; i < GameMain.localStar.planetCount; i++)
                    {
                        if(GameMain.localStar.planets[i]?.factory != null)
                        {
                            pIDs.Add(GameMain.localStar.planets[i].id);
                        }
                    }

                    Multiplayer.Session.Network.SendPacket(new PowerSystemUpdateRequest(pIDs.ToArray()));
                }

                return false;
            }
            return true;
        }

        private static void UpdateAnimations(PowerSystem pSys, long time, bool isActive, bool isMultiplayerMode)
        {
            if(GameMain.localPlanet == null || GameMain.localPlanet.factory == null)
            {
                return;
            }

            AnimData[] entityAnimPool = GameMain.localPlanet.factory.entityAnimPool;
            float stepTime = 0.016666668f;
            float speed = 1f;

            List<long> animCache = null;
            if(!PowerSystemManager.PowerSystemAnimationCache.TryGetValue(pSys.planet.id, out animCache))
            {
                // just too much spam
                //Log.Warn($"Could not get PowerSystem animation cache, animations will be broken for planet {pSys.planet.displayName}!");
            }

            FactoryProductionStat factoryProductionStat = GameMain.statistics.production.factoryStatPool[pSys.factory.index];

            bool useIonLayer = GameMain.history.useIonLayer;
            bool useCata = time % 10L == 0L;
            int[] productRegister = factoryProductionStat == null ? new int[0] : factoryProductionStat.productRegister;
            int[] consumeRegister = factoryProductionStat == null ? new int[0] : factoryProductionStat.consumeRegister;
            Vector3 normalized = pSys.factory.planet.runtimeLocalSunDirection.normalized;

            SignData[] entitySignPool = pSys.factory.entitySignPool;

            Vector3 playerPos = Vector3.zero;
            bool triggerPlayerRecharge = GameMain.mainPlayer.mecha.coreEnergyCap - GameMain.mainPlayer.mecha.coreEnergy > 10000.0;
            if(triggerPlayerRecharge && GameMain.mainPlayer.planetId == pSys.planet.id)
            {
                playerPos = (isMultiplayerMode ? pSys.multithreadPlayerPos : GameMain.mainPlayer.position);
            }
            else
            {
                triggerPlayerRecharge = false;
            }

            foreach (PowerNetwork pNet in GameMain.localPlanet.factory.powerSystem.netPool)
            {
                if(pNet == null || animCache == null)
                {
                    if(pNet != null && animCache == null)
                    {
                        // still update signs even if there is no power source on this planet
                        UpdateSignPool(pSys, entitySignPool, pNet, ref isActive);
                        // still compute wireless charger state
                        ComputeWirelessChargerState(pSys, pNet, triggerPlayerRecharge, playerPos);
                    }
                    continue;
                }

                ComputeWirelessChargerState(pSys, pNet, triggerPlayerRecharge, playerPos);

                for (int i = 0; i < pNet.generators.Count && pNet.id - 1 < animCache.Count; i++)
                {
                    int compIndex = pNet.generators[i];
                    int eID = pSys.genPool[compIndex].entityId;

                    long num35 = animCache[pNet.id - 1];

                    bool isPoweredByFuel = !pSys.genPool[compIndex].wind && !pSys.genPool[compIndex].photovoltaic && !pSys.genPool[compIndex].gamma;
                    long generateCurrentTick = num35 > 0 ? pSys.genPool[compIndex].generateCurrentTick : 0;

                    // first compute fuel usage and energy left in facility. Also compute generateCurrentTick as its different for each facility but based on num35 which we sync in the animCache.
                    PrepareUpdateFuelComputation(pSys, ref pSys.genPool[compIndex], normalized);
                    if (isPoweredByFuel)
                    {
                        pSys.genPool[compIndex].currentStrength = (float)((num35 > 0L && pSys.genPool[compIndex].capacityCurrentTick > 0L) ? 1 : 0);
                    }
                    if (num35 > 0L && pSys.genPool[compIndex].productId == 0)
                    {
                        long energy = (long)(pNet.generaterRatio * (double)pSys.genPool[compIndex].capacityCurrentTick + 0.99999);
                        generateCurrentTick = ((num35 < energy) ? num35 : energy);
                        if (generateCurrentTick > 0L)
                        {
                            num35 -= generateCurrentTick;
                            if (isPoweredByFuel)
                            {
                                pSys.genPool[compIndex].GenEnergyByFuel(generateCurrentTick, consumeRegister);
                            }
                            pSys.genPool[compIndex].generateCurrentTick = generateCurrentTick; // wey we can compute num46 from num35!!
                        }
                    }

                    // then update animation status based on the current energy
                    if (pSys.genPool[compIndex].wind)
                    {
                        // state is always enabled on wind as it seems, but with 0 windStrength it does not move anyways
                        speed = 0.7f;
                        entityAnimPool[eID].Step2(1U, stepTime, GameMain.localPlanet.windStrength, speed);
                    }
                    else if (pSys.genPool[compIndex].gamma && factoryProductionStat != null)
                    {
                        pSys.genPool[compIndex].GameTick_Gamma(useIonLayer, useCata, pSys.factory, productRegister, consumeRegister);
                        entityAnimPool[eID].time += stepTime;

                        if(entityAnimPool[eID].time > 1f)
                        {
                            entityAnimPool[eID].time -= 1f;
                        }

                        entityAnimPool[eID].power = (float)((double)pSys.genPool[compIndex].capacityCurrentTick / (double)pSys.genPool[compIndex].genEnergyPerTick);
                        entityAnimPool[eID].state = ((pSys.genPool[compIndex].productId > 0) ? 2U : 0U) + ((pSys.genPool[compIndex].catalystPoint > 0) ? 1U : 0U);
                        entityAnimPool[eID].working_length = entityAnimPool[eID].working_length * 0.99f + ((pSys.genPool[compIndex].catalystPoint > 0) ? 0.01f : 0f);
                    }
                    else if(pSys.genPool[pNet.generators[i]].fuelMask > 1)
                    {
                        float power = (float)((double)entityAnimPool[eID].power * 0.98 + 0.02 * (double)((generateCurrentTick > 0L && pSys.genPool[compIndex].capacityCurrentTick > 30L) ? 1 : 0));
                        if(power > 0L)
                        {
                            speed = 2f;
                        }
                        if(generateCurrentTick > 0L && power < 0f)
                        {
                            power = 0f;
                        }
                        
                        entityAnimPool[eID].Step2((entityAnimPool[eID].power > 0.1f || generateCurrentTick > 0L) ? 1U : 0U, stepTime, power, speed);
                    }
                    else
                    {
                        // capacityCurrentTick is > 30 when there is still fuel energy inside the facility (check needed to turn off animations when fuel runs out)
                        float power = (float)((double)entityAnimPool[eID].power * 0.98 + 0.02 * (double)generateCurrentTick / (double)pSys.genPool[compIndex].genEnergyPerTick);
                        if (power > 0L)
                        {
                            speed = 2f;
                        }
                        if (generateCurrentTick > 0L && power < 0.2f)
                        {
                            power = 0.2f;
                        }
                        entityAnimPool[eID].Step2(((entityAnimPool[eID].power > 0.1f || (generateCurrentTick > 0L && pSys.genPool[compIndex].capacityCurrentTick > 30L))) ? 1U : 0U, stepTime, power, speed);
                    }
                }

                UpdateSignPool(pSys, entitySignPool, pNet, ref isActive);
            }
        }

        // use requiredEnergy to determine the animation state further down in the UpdateAnimations() method
        private static void ComputeWirelessChargerState(PowerSystem pSys, PowerNetwork pNet, bool triggerPlayerRecharge, Vector3 playerPos)
        {
            foreach(Node node in pNet.nodes)
            {
                int id = node.id;
                if(pSys.nodePool[id].id == id && pSys.nodePool[id].isCharger){
                    if (triggerPlayerRecharge)
                    {
                        float num8 = pSys.nodePool[id].powerPoint.x * 0.988f - playerPos.x;
                        float num9 = pSys.nodePool[id].powerPoint.y * 0.988f - playerPos.y;
                        float num10 = pSys.nodePool[id].powerPoint.z * 0.988f - playerPos.z;
                        if(pSys.nodePool[id].coverRadius < 15f && (double)(num8 * num8 + num9 * num9 + num10 * num10) <= 64.05)
                        {
                            if(pSys.nodePool[id].requiredEnergy == pSys.nodePool[id].idleEnergyPerTick)
                            {
                                // tower starts to work, send event to host
                            }
                            pSys.nodePool[id].requiredEnergy = pSys.nodePool[id].workEnergyPerTick;
                        }
                        else
                        {
                            if(pSys.nodePool[id].requiredEnergy == pSys.nodePool[id].workEnergyPerTick)
                            {
                                // tower stops to work, send event to host
                            }
                            pSys.nodePool[id].requiredEnergy = pSys.nodePool[id].idleEnergyPerTick;
                        }
                    }
                    else
                    {
                        if(pSys.nodePool[id].requiredEnergy == pSys.nodePool[id].workEnergyPerTick)
                        {
                            // tower stops to work, send event to host
                        }
                        pSys.nodePool[id].requiredEnergy = pSys.nodePool[id].idleEnergyPerTick;
                    }
                }
            }
        }

        private static void UpdateSignPool(PowerSystem pSys, SignData[] entitySignPool, PowerNetwork pNet, ref bool isActive)
        {
            if (isActive)
            {
                List<int> consumers = pNet.consumers;
                if (pNet.id == 0)
                {
                    for (int i = 0; i < pNet.consumers.Count; i++)
                    {
                        entitySignPool[pSys.consumerPool[consumers[i]].entityId].signType = 1U;
                    }
                }
                else if (pNet.consumerRatio < 0.10000000149011612)
                {
                    for (int i = 0; i < pNet.consumers.Count; i++)
                    {
                        entitySignPool[pSys.consumerPool[consumers[i]].entityId].signType = 2U;
                    }
                }
                else if (pNet.consumerRatio < 0.5)
                {
                    for (int i = 0; i < pNet.consumers.Count; i++)
                    {
                        entitySignPool[pSys.consumerPool[consumers[i]].entityId].signType = 3U;
                    }
                }
                else
                {
                    for (int i = 0; i < pNet.consumers.Count; i++)
                    {
                        entitySignPool[pSys.consumerPool[consumers[i]].entityId].signType = 0U;
                    }
                }
            }
        }

        // compute capacityCurrentTick
        private static void PrepareUpdateFuelComputation(PowerSystem pSys, ref PowerGeneratorComponent pComp, Vector3 normalized)
        {
            if (pComp.wind)
            {
                pComp.EnergyCap_Wind(pSys.factory.planet.windStrength);
            }
            else if (pComp.photovoltaic)
            {
                pComp.EnergyCap_PV(normalized.x, normalized.y, normalized.z, pSys.factory.planet.luminosity);
            }
            else if (pComp.gamma)
            {
                float response = (pSys.dysonSphere != null) ? pSys.dysonSphere.energyRespCoef : 0f;
                pComp.EnergyCap_Gamma(response);
            }
            else
            {
                long output = pComp.EnergyCap_Fuel();
                pSys.factory.entitySignPool[pComp.entityId].signType = ((output > 30L) ? 0U : 8U);
            }
        }
    }
}
