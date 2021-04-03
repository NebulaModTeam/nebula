using NebulaModel.Packets.Logistics;
using HarmonyLib;
using UnityEngine;
using System;

namespace NebulaWorld.Logistics
{
    class StationUIManager
    {
        public static void UpdateUI(StationUI packet)
        {
            if (packet.isStorageUI)
            {
                UpdateStorageUI(packet);
            }
            else
            {
                UpdateSettingsUI(packet);
            }
        }

        private static void UpdateSettingsUI(StationUI packet)
        {
            UIStationWindow stationWindow = UIRoot.instance.uiGame.stationWindow;
            if (stationWindow != null)
            {
                int _stationId = (int)AccessTools.Field(typeof(UIStationWindow), "_stationId")?.GetValue(stationWindow);
                if (packet.stationId == _stationId)
                {
                    // client has the same station window opened at this time, so we can just use the vanilla function
                    LocalPlayer.PatchLocks["UIStationWindow"] = true;
                    if (packet.settingIndex == 0)
                    {
                        stationWindow.OnMaxChargePowerSliderValueChange(packet.settingValue);
                    }
                    if(packet.settingIndex == 1)
                    {
                        stationWindow.OnMaxTripDroneSliderValueChange(packet.settingValue);
                    }
                    if (packet.settingIndex == 2)
                    {
                        stationWindow.OnMaxTripVesselSliderValueChange(packet.settingValue);
                    }
                    if (packet.settingIndex == 3)
                    {
                        stationWindow.OnMinDeliverDroneValueChange(packet.settingValue);
                    }
                    if (packet.settingIndex == 4)
                    {
                        stationWindow.OnMinDeliverVesselValueChange(packet.settingValue);
                    }
                    if (packet.settingIndex == 5)
                    {
                        stationWindow.OnWarperDistanceValueChange(packet.settingValue);
                    }
                    if (packet.settingIndex == 6)
                    {
                        stationWindow.OnWarperNecessaryClick(0);
                    }
                    if (packet.settingIndex == 7)
                    {
                        Type[] args = new Type[1];
                        object[] values = new object[1];
                        args[0] = typeof(int);
                        values[0] = 0;
                        AccessTools.Method(typeof(UIStationWindow), "OnIncludeOrbitCollectorClick", args).Invoke(stationWindow, values);
                    }
                    if(packet.settingIndex >= 8 && packet.settingIndex <= 10)
                    {
                        PlanetData pData = GameMain.galaxy.PlanetById(packet.planetId);
                        if(pData?.factory != null && pData?.factory?.transport != null)
                        {
                            StationComponent[] stationPool = pData.factory.transport.stationPool;
                            if(stationPool.Length > packet.stationId)
                            {
                                if(packet.settingIndex == 8)
                                {
                                    stationPool[packet.stationId].idleDroneCount = (int)packet.settingValue;
                                }
                                if (packet.settingIndex == 9)
                                {
                                    stationPool[packet.stationId].idleShipCount = (int)packet.settingValue;
                                }
                                if (packet.settingIndex == 10)
                                {
                                    stationPool[packet.stationId].warperCount = (int)packet.settingValue;
                                }
                            }
                        }
                    }
                    if(packet.settingIndex == 11)
                    {
                        PlanetData pData = GameMain.galaxy.PlanetById(packet.planetId);
                        if(pData?.factory != null && pData?.factory?.transport != null)
                        {
                            StationComponent[] stationPool = pData.factory.transport.stationPool;
                            if(stationPool.Length > packet.stationId && stationPool[packet.stationId].storage != null)
                            {
                                for(int i = 0; i < stationPool[packet.stationId].storage.Length; i++)
                                {
                                    if(stationPool[packet.stationId].storage[i].itemId == packet.itemId)
                                    {
                                        stationPool[packet.stationId].storage[i].count = (int)packet.settingValue;
                                        break;
                                    }
                                }
                            }
                        }
                    }
                    LocalPlayer.PatchLocks["UIStationWindow"] = false;
                }
                else
                {
                    PlanetData pData = GameMain.galaxy.PlanetById(packet.planetId);
                    if (packet.settingIndex == 0)
                    {
                        // station window is not opened at the moment, so search for the StationComponent and its factory to adjust the power consumption
                        // if we have no factory for it loaded that usually means we are a client and have not visited the corresponding planet yet
                        // which means we get the correct data once we travel there, so we dont need to sync in that case
                        if (pData?.factory != null && pData?.factory?.powerSystem != null && pData?.factory?.transport != null)
                        {
                            PowerConsumerComponent[] consumerPool = pData.factory.powerSystem.consumerPool;
                            StationComponent[] stationPool = pData.factory.transport.stationPool;
                            if (consumerPool.Length > stationPool[packet.stationId].pcId)
                            {
                                consumerPool[stationPool[packet.stationId].pcId].workEnergyPerTick = (long)(50000.0 * (double)packet.settingValue + 0.5);
                            }
                        }
                    }
                    if(packet.settingIndex == 1)
                    {
                        if(pData?.factory != null && pData?.factory?.transport != null)
                        {
                            StationComponent[] stationPool = pData.factory.transport.stationPool;
                            if(stationPool.Length > packet.stationId)
                            {
                                stationPool[packet.stationId].tripRangeDrones = Math.Cos((double)packet.settingValue / 180.0 * 3.141592653589793);
                            }
                        }
                    }
                    if (packet.settingIndex == 2)
                    {
                        if (pData?.factory != null && pData?.factory?.transport != null)
                        {
                            StationComponent[] stationPool = pData.factory.transport.stationPool;
                            if (stationPool.Length > packet.stationId)
                            {
                                double value = packet.settingValue;
                                if(value > 40.5)
                                {
                                    value = 10000.0;
                                }
                                else if(value > 20.5)
                                {
                                    value = value * 2f - 20f;
                                }
                                stationPool[packet.stationId].tripRangeShips = 2400000.0 * value;
                            }
                        }
                    }
                    if(packet.settingIndex == 3)
                    {
                        if(pData?.factory != null && pData?.factory?.transport != null)
                        {
                            StationComponent[] stationPool = pData.factory.transport.stationPool;
                            if(stationPool.Length > packet.stationId)
                            {
                                int value = (int)(packet.settingValue * 10f + 0.5f);
                                if(value < 1)
                                {
                                    value = 1;
                                }
                                stationPool[packet.stationId].deliveryDrones = value;
                            }
                        }
                    }
                    if (packet.settingIndex == 4)
                    {
                        if (pData?.factory != null && pData?.factory?.transport != null)
                        {
                            StationComponent[] stationPool = pData.factory.transport.stationPool;
                            if (stationPool.Length > packet.stationId)
                            {
                                int value = (int)(packet.settingValue * 10f + 0.5f);
                                if (value < 1)
                                {
                                    value = 1;
                                }
                                stationPool[packet.stationId].deliveryShips = value;
                            }
                        }
                    }
                    if (packet.settingIndex == 5)
                    {
                        if (pData?.factory != null && pData?.factory?.transport != null)
                        {
                            StationComponent[] stationPool = pData.factory.transport.stationPool;
                            if (stationPool.Length > packet.stationId)
                            {
                                double value = packet.settingValue;
                                if(value < 1.5)
                                {
                                    value = 0.2;
                                }
                                else if(value < 7.5)
                                {
                                    value = value * 0.5 - 0.5;
                                }
                                else if(value < 16.5)
                                {
                                    value -= 4f;
                                }
                                else if(value < 20.5)
                                {
                                    value = value * 2f - 20f;
                                }
                                else
                                {
                                    value = 60;
                                }
                                stationPool[packet.stationId].warpEnableDist = 40000.0 * value;
                            }
                        }
                    }
                    if(packet.settingIndex == 6)
                    {
                        if(pData?.factory != null && pData?.factory?.transport != null)
                        {
                            StationComponent[] stationPool = pData.factory.transport.stationPool;
                            if(stationPool.Length > packet.stationId)
                            {
                                stationPool[packet.stationId].warperNecessary = !stationPool[packet.stationId].warperNecessary;
                            }
                        }
                    }
                    if (packet.settingIndex == 7)
                    {
                        if (pData?.factory != null && pData?.factory?.transport != null)
                        {
                            StationComponent[] stationPool = pData.factory.transport.stationPool;
                            if (stationPool.Length > packet.stationId)
                            {
                                stationPool[packet.stationId].includeOrbitCollector = !stationPool[packet.stationId].includeOrbitCollector;
                            }
                        }
                    }
                    if (packet.settingIndex >= 8 && packet.settingIndex <= 10)
                    {
                        if (pData?.factory != null && pData?.factory?.transport != null)
                        {
                            StationComponent[] stationPool = pData.factory.transport.stationPool;
                            if (stationPool.Length > packet.stationId)
                            {
                                if (packet.settingIndex == 8)
                                {
                                    stationPool[packet.stationId].idleDroneCount = (int)packet.settingValue;
                                }
                                if (packet.settingIndex == 9)
                                {
                                    stationPool[packet.stationId].idleShipCount = (int)packet.settingValue;
                                }
                                if (packet.settingIndex == 10)
                                {
                                    stationPool[packet.stationId].warperCount = (int)packet.settingValue;
                                }
                            }
                        }
                    }
                    if(packet.settingIndex == 11)
                    {
                        if (pData?.factory != null && pData?.factory?.transport != null)
                        {
                            StationComponent[] stationPool = pData.factory.transport.stationPool;
                            if (stationPool.Length > packet.stationId && stationPool[packet.stationId].storage != null)
                            {
                                for (int i = 0; i < stationPool[packet.stationId].storage.Length; i++)
                                {
                                    if (stationPool[packet.stationId].storage[i].itemId == packet.itemId)
                                    {
                                        stationPool[packet.stationId].storage[i].count = (int)packet.settingValue;
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        private static void UpdateStorageUI(StationUI packet)
        {
            PlanetData pData = GameMain.galaxy.PlanetById(packet.planetId);
            // if we did not find a corresponding station we exit (should only happen for clients that have not received any transporting or did not visit that planet)
            // NOTE: call PlanetTransport::NewStationComponent() for clients when we add one, else PlanetTransport::GetStationComponent() will not be able to find it
            // NOTE: PlanetTransport::NewStationComponent() will also call GalacticTransport::AddStationComponent()

            if (pData == null)
            {
                // this should never happen
                return;
            }
            if (pData.factory == null && !LocalPlayer.IsMasterClient)
            {
                // in this case client will receive the settings once he arrives at the planet and receives the PlanetFactory
            }
            else
            {
                LocalPlayer.PatchLocks["PlanetTransport"] = true;
                pData.factory.transport.SetStationStorage(packet.stationId, packet.storageIdx, packet.itemId, packet.itemCountMax, packet.localLogic, packet.remoteLogic, null);
                LocalPlayer.PatchLocks["PlanetTransport"] = false;
            }
        }
    }
}
