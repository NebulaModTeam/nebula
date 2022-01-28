using NebulaModel.Logger;
using NebulaModel.Packets.Logistics;
using System;
using UnityEngine;

namespace NebulaWorld.Logistics
{
    public class StationUIManager : IDisposable
    {
        public int UpdateCooldown; // cooldown is reserved for future use
        public bool UIRequestedShipDronWarpChange; // when receiving a ship, drone or warp change only take/add items from the one issuing the request
        public StationUI SliderBarPacket; // store the change of slider bar temporary, only send it when mouse button is released.
        public int StorageMaxChangeId; // index of the storage that its slider value changed by the user. -1: None, -2: Syncing

        public StationUIManager()
        {
        }

        public void Dispose()
        {
        }

        public void DecreaseCooldown()
        {
            // cooldown is reserved for future use
            if (UpdateCooldown > 0)
            {
                UpdateCooldown--;
            }
        }

        public void UpdateUI(ref StationUI packet)
        {
            StationComponent stationComponent = GetStation(packet);
            if (stationComponent == null)
            {
                return;
            }
            if (packet.IsStorageUI)
            {
                UpdateStorageUI(stationComponent, packet);
            }
            else
            {
                UpdateSettingsUI(stationComponent, ref packet);
            }

            // If station window is opened and veiwing the updating station, refresh the window.
            UIStationWindow stationWindow = UIRoot.instance.uiGame.stationWindow;
            if (stationWindow != null && stationWindow.active)
            {
                if (stationWindow.factory?.planetId == packet.PlanetId && stationWindow.stationId == packet.StationId)
                {
                    stationWindow.OnStationIdChange();                    
                }
            }
        }

        /**
         * Updates to a given station that should happen in the background.
         */
        private void UpdateSettingsUI(StationComponent stationComponent, ref StationUI packet)
        {
            // SetDroneCount, SetShipCount may change packet.SettingValue
            switch (packet.SettingIndex)
            {
                case StationUI.EUISettings.MaxChargePower:
                {
                    PlanetData planet = GameMain.galaxy.PlanetById(packet.PlanetId);
                    if (planet.factory?.powerSystem != null)
                    {
                        PowerConsumerComponent[] consumerPool = planet.factory.powerSystem.consumerPool;
                        if (consumerPool.Length > stationComponent.pcId)
                        {
                            consumerPool[stationComponent.pcId].workEnergyPerTick = (long)(50000.0 * packet.SettingValue + 0.5);
                        }
                    }
                    break;
                }
                case StationUI.EUISettings.SetDroneCount:
                {
                    // Check if new setting is acceptable
                    int totalCount = Math.Min((int)packet.SettingValue, stationComponent.workDroneDatas.Length);
                    stationComponent.idleDroneCount = Math.Max(totalCount - stationComponent.workDroneCount, 0);
                    if (totalCount < (int)packet.SettingValue && packet.ShouldMimic)
                    {
                        // The result is less than original setting, refund extra drones to author
                        int refund = (int)packet.SettingValue - totalCount;
                        GameMain.mainPlayer.TryAddItemToPackage(5001, refund, 0, true);
                    }
                    packet.SettingValue = totalCount;
                    break;
                }
                case StationUI.EUISettings.SetShipCount:
                {
                    // Check if new setting is acceptable
                    int totalCount = Math.Min((int)packet.SettingValue, stationComponent.workShipDatas.Length);
                    stationComponent.idleShipCount = Math.Max(totalCount - stationComponent.workShipCount, 0);
                    if (totalCount < (int)packet.SettingValue && packet.ShouldMimic)
                    {
                        // The result is less than original setting, refund extra ships to author
                        int refund = (int)packet.SettingValue - totalCount;
                        GameMain.mainPlayer.TryAddItemToPackage(5002, refund, 0, true);
                    }
                    packet.SettingValue = totalCount;
                    break;
                }
                case StationUI.EUISettings.SetWarperCount:
                {
                    stationComponent.warperCount = (int)packet.SettingValue;
                    if (stationComponent.storage != null && packet.WarperShouldTakeFromStorage)
                    {
                        for (int i = 0; i < stationComponent.storage.Length; i++)
                        {
                            if (stationComponent.storage[i].itemId == 1210 && stationComponent.storage[i].count > 0)
                            {
                                stationComponent.storage[i].count--;
                                break;
                            }
                        }
                    }
                    break;
                }
                case StationUI.EUISettings.MaxTripDrones:
                {
                    stationComponent.tripRangeDrones = Math.Cos(packet.SettingValue / 180.0 * 3.141592653589793);
                    break;
                }
                case StationUI.EUISettings.MaxTripVessel:
                {
                    double value = packet.SettingValue;
                    if (value > 40.5)
                    {
                        value = 10000.0;
                    }
                    else if (value > 20.5)
                    {
                        value = value * 2f - 20f;
                    }
                    stationComponent.tripRangeShips = 2400000.0 * value;
                    break;
                }
                case StationUI.EUISettings.MinDeliverDrone:
                {
                    int value = (int)(packet.SettingValue * 10f + 0.5f);
                    stationComponent.deliveryDrones = value < 1 ? 1 : value;
                    break;
                }
                case StationUI.EUISettings.MinDeliverVessel:
                {
                    int value = (int)(packet.SettingValue * 10f + 0.5f);
                    stationComponent.deliveryShips = value < 1 ? 1 : value;
                    break;
                }
                case StationUI.EUISettings.WarpDistance:
                {
                    double value = packet.SettingValue;
                    if (value < 1.5)
                    {
                        value = 0.2;
                    }
                    else if (value < 7.5)
                    {
                        value = value * 0.5 - 0.5;
                    }
                    else if (value < 16.5)
                    {
                        value -= 4f;
                    }
                    else if (value < 20.5)
                    {
                        value = value * 2f - 20f;
                    }
                    else
                    {
                        value = 60;
                    }
                    stationComponent.warpEnableDist = 40000.0 * value;
                    break;
                }
                case StationUI.EUISettings.WarperNeeded:
                {
                    stationComponent.warperNecessary = !stationComponent.warperNecessary;
                    break;
                }
                case StationUI.EUISettings.IncludeCollectors:
                {
                    stationComponent.includeOrbitCollector = !stationComponent.includeOrbitCollector;
                    break;
                }
                case StationUI.EUISettings.AddOrRemoveItemFromStorage:
                {
                    if (stationComponent.storage != null)
                    {
                        stationComponent.storage[packet.StorageIdx].count = (int)packet.SettingValue;
                        stationComponent.storage[packet.StorageIdx].inc = (int)packet.SettingValue2;
                    }
                    break;
                }
                case StationUI.EUISettings.PilerCount:
                {
                    stationComponent.pilerCount = (int)packet.SettingValue;
                    break;
                }
                case StationUI.EUISettings.MaxMiningSpeed:
                {                       
                    PlanetFactory factory = GameMain.galaxy.PlanetById(packet.PlanetId).factory;
                    if (factory != null)
                    {
                        int speed = 10000 + (int)(packet.SettingValue + 0.5f) * 1000;
                        long workEnergyPrefab = LDB.items.Select(factory.entityPool[stationComponent.entityId].protoId).prefabDesc.workEnergyPerTick;
                        factory.factorySystem.minerPool[stationComponent.minerId].speed = speed;
                        factory.powerSystem.consumerPool[stationComponent.pcId].workEnergyPerTick = (long)(workEnergyPrefab * (speed / 10000f) * (speed / 10000f));
                    }
                    break;
                }
            }
        }

        /*
         * Update station settings and drone, ship and warper counts.
         * 
         * First determine if the local player has the station window opened and handle that accordingly.
         */
        private StationComponent GetStation(StationUI packet)
        {
            StationComponent stationComponent = null;
            PlanetData planet = GameMain.galaxy?.PlanetById(packet.PlanetId);

            // If we can't find planet or the factory for said planet, we can just skip this
            if (planet?.factory?.transport == null)
            {
                return null;
            }

            StationComponent[] gStationPool = GameMain.data.galacticTransport.stationPool;
            StationComponent[] stationPool = planet?.factory?.transport?.stationPool;

            // Figure out if we're dealing with a PLS or a ILS station
            stationComponent = packet.StationGId > 0 ? gStationPool[packet.StationGId] : stationPool?[packet.StationId];

            if (stationComponent == null)
            {
                Log.Warn($"StationUI: Unable to find requested station on planet {packet.PlanetId} with id {packet.StationId} and gid of {packet.StationGId}");
                return null;
            }
            return stationComponent;
        }

        private void UpdateStorageUI(StationComponent stationComponent, StationUI packet)
        {
            using (Multiplayer.Session.Ships.PatchLockILS.On())
            {
                PlanetData planet = GameMain.galaxy.PlanetById(packet.PlanetId);
                planet.factory.transport.SetStationStorage(stationComponent.id, packet.StorageIdx, packet.ItemId, packet.ItemCountMax, packet.LocalLogic, packet.RemoteLogic, (packet.ShouldMimic == true) ? GameMain.mainPlayer : null);
                StorageMaxChangeId = -1;
            }
        }
    }
}
