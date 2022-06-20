using NebulaModel.Logger;
using NebulaModel.Packets.Logistics;
using System;

namespace NebulaWorld.Logistics
{
    public class StationUIManager : IDisposable
    {
        public bool UIRequestedShipDronWarpChange { get; set; } // when receiving a ship, drone or warp change only take/add items from the one issuing the request
        public StationUI SliderBarPacket { get; set; } // store the change of slider bar temporary, only send it when mouse button is released.
        public int StorageMaxChangeId { get; set; } // index of the storage that its slider value changed by the user. -1: None, -2: Syncing

        public StationUIManager()
        {
        }

        public void Dispose()
        {
        }

        public void UpdateStation(ref StationUI packet)
        {
            StationComponent stationComponent = GetStation(packet.PlanetId, packet.StationId, packet.StationGId);
            if (stationComponent == null)
            {
                return;
            }
            UpdateSettingsUI(stationComponent, ref packet);
            RefreshWindow(packet.PlanetId, packet.StationId);
        }

        public void UpdateStorage(StorageUI packet)
        {
            StationComponent stationComponent = GetStation(packet.PlanetId, packet.StationId, packet.StationGId);
            if (stationComponent == null || stationComponent.storage == null)
            {
                return;
            }
            UpdateStorageUI(stationComponent, packet);
            RefreshWindow(packet.PlanetId, packet.StationId);
        }

        private static void RefreshWindow(int planetId, int stationId)
        {
            // If station window is opened and veiwing the updating station, refresh the window.
            UIStationWindow stationWindow = UIRoot.instance.uiGame.stationWindow;
            if (stationWindow != null && stationWindow.active)
            {
                if (stationWindow.factory?.planetId == planetId && stationWindow.stationId == stationId)
                {
                    stationWindow.OnStationIdChange();
                }
            }
        }

        /**
         * Updates to a given station that should happen in the background.
         */
        private static void UpdateSettingsUI(StationComponent stationComponent, ref StationUI packet)
        {
            // SetDroneCount, SetShipCount may change packet.SettingValue
            switch (packet.SettingIndex)
            {
                case StationUI.EUISettings.MaxChargePower:
                {
                    PlanetData planet = GameMain.galaxy.PlanetById(packet.PlanetId);
                    if (planet?.factory?.powerSystem?.consumerPool != null)
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
                    int maxDroneCount = stationComponent.workDroneDatas?.Length ?? 0;
                    if (maxDroneCount == 0)
                    {
                        PlanetData planet = GameMain.galaxy.PlanetById(packet.PlanetId);
                        ItemProto itemProto = LDB.items.Select(planet.factory.entityPool[stationComponent.entityId].protoId);
                        maxDroneCount = itemProto?.prefabDesc.stationMaxDroneCount ?? 10;
                    }
                    int totalCount = Math.Min((int)packet.SettingValue, maxDroneCount);
                    stationComponent.idleDroneCount = Math.Max(totalCount - stationComponent.workDroneCount, 0);
                    totalCount = stationComponent.idleDroneCount + stationComponent.workDroneCount;
                    if (totalCount < (int)packet.SettingValue && packet.ShouldRefund)
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
                    int maxShipCount = stationComponent.workShipDatas?.Length ?? 0;
                    if (maxShipCount == 0)
                    {
                        PlanetData planet = GameMain.galaxy.PlanetById(packet.PlanetId);
                        ItemProto itemProto = LDB.items.Select(planet.factory.entityPool[stationComponent.entityId].protoId);
                        maxShipCount = itemProto?.prefabDesc.stationMaxShipCount ?? 10;
                    }
                    int totalCount = Math.Min((int)packet.SettingValue, maxShipCount);
                    stationComponent.idleShipCount = Math.Max(totalCount - stationComponent.workShipCount, 0);
                    totalCount = stationComponent.idleShipCount + stationComponent.workShipCount;
                    if (totalCount < (int)packet.SettingValue && packet.ShouldRefund)
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
                case StationUI.EUISettings.PilerCount:
                {
                    stationComponent.pilerCount = (int)packet.SettingValue;
                    break;
                }
                case StationUI.EUISettings.MaxMiningSpeed:
                {                       
                    PlanetFactory factory = GameMain.galaxy.PlanetById(packet.PlanetId)?.factory;
                    if (factory != null)
                    {
                        int speed = 10000 + (int)(packet.SettingValue + 0.5f) * 1000;
                        factory.factorySystem.minerPool[stationComponent.minerId].speed = speed;
                    }
                    break;
                }
                case StationUI.EUISettings.NameInput:
                {
                    stationComponent.name = packet.SettingString;
                    break;
                }
            }
        }

        private static StationComponent GetStation(int planetId, int stationId, int stationGid)
        {
            PlanetData planet = GameMain.galaxy.PlanetById(planetId);

            // If we can't find planet or the factory for said planet, we can just skip this
            if (planet?.factory?.transport == null)
            {
                return null;
            }

            StationComponent[] gStationPool = GameMain.data.galacticTransport.stationPool;
            StationComponent[] stationPool = planet?.factory?.transport?.stationPool;

            // Figure out if we're dealing with a PLS or a ILS station
            StationComponent stationComponent = stationGid > 0 ? gStationPool[stationGid] : stationPool?[stationId];
            return stationComponent;
        }

        private void UpdateStorageUI(StationComponent stationComponent, StorageUI packet)
        {
            using (Multiplayer.Session.Ships.PatchLockILS.On())
            {
                PlanetData planet = GameMain.galaxy.PlanetById(packet.PlanetId);
                if (packet.ItemCount == -1)
                {
                    planet.factory.transport.SetStationStorage(stationComponent.id, packet.StorageIdx, packet.ItemId, packet.ItemCountMax, packet.LocalLogic, packet.RemoteLogic, (packet.ShouldRefund == true) ? GameMain.mainPlayer : null);
                    StorageMaxChangeId = -1;
                }
                else if (packet.ItemCount == -2)
                {
                    stationComponent.storage[packet.StorageIdx].keepMode = packet.KeepMode;
                }
                else
                {
                    stationComponent.storage[packet.StorageIdx].count = packet.ItemCount;
                    stationComponent.storage[packet.StorageIdx].inc = packet.ItemInc;
                }
            }
        }
    }
}
