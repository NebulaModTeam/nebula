using NebulaModel.Logger;
using NebulaModel.Networking;
using NebulaModel.Packets.Logistics;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace NebulaWorld.Logistics
{
    public class StationUIManager : IDisposable
    {
        public int UpdateCooldown; // cooldown is reserved for future use
        public BaseEventData LastMouseEvent;
        public bool LastMouseEventWasDown;
        public int UIIsSyncedStage; // 0 == not synced, 1 == request sent, 2 == synced | this is only used client side
        public bool UIRequestedShipDronWarpChange; // when receiving a ship, drone or warp change only take/add items from the one issuing the request

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

        public void UpdateUI(StationUI packet)
        {
            if (packet.IsStorageUI)
            {
                UpdateStorageUI(packet);
            }
            else
            {
                UpdateSettingsUI(packet);
            }

            // If station window is opened and veiwing the updating station, refresh the window.
            UIStationWindow stationWindow = UIRoot.instance.uiGame.stationWindow;
            if (stationWindow != null && stationWindow.active)
            {
                if (stationWindow.factory?.planetId == packet.PlanetId && stationWindow.stationId == packet.StationId)
                {
                    Log.Info($"Refresh value {packet.StationId}");
                    stationWindow.OnStationIdChange();
                }
            }
        }

        /**
         * Updates to a given station that should happen in the background.
         */
        private void UpdateSettingsUIBackground(StationUI packet, PlanetData planet, StationComponent stationComponent)
        {
            StationComponent[] gStationPool = GameMain.data.galacticTransport.stationPool;

            // update drones, ships, warpers and energy consumption for everyone
            if ((packet.SettingIndex >= StationUI.EUISettings.SetDroneCount && packet.SettingIndex <= StationUI.EUISettings.SetWarperCount) || packet.SettingIndex == StationUI.EUISettings.MaxChargePower)
            {
                if (packet.SettingIndex == (int)StationUI.EUISettings.MaxChargePower && planet.factory?.powerSystem != null)
                {
                    PowerConsumerComponent[] consumerPool = planet.factory.powerSystem.consumerPool;
                    if (consumerPool.Length > stationComponent.pcId)
                    {
                        consumerPool[stationComponent.pcId].workEnergyPerTick = (long)(50000.0 * packet.SettingValue + 0.5);
                    }
                }

                if (packet.SettingIndex == StationUI.EUISettings.SetDroneCount)
                {
                    stationComponent.idleDroneCount = (int)packet.SettingValue;
                }

                if (packet.SettingIndex == StationUI.EUISettings.SetShipCount)
                {
                    stationComponent.idleShipCount = (int)packet.SettingValue;
                }

                if (packet.SettingIndex == StationUI.EUISettings.SetWarperCount)
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
                }
            }

            if (packet.SettingIndex == StationUI.EUISettings.MaxTripDrones)
            {
                stationComponent.tripRangeDrones = Math.Cos(packet.SettingValue / 180.0 * 3.141592653589793);
            }

            if (packet.SettingIndex == StationUI.EUISettings.MaxTripVessel)
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
            }

            if (packet.SettingIndex == StationUI.EUISettings.MinDeliverDrone)
            {
                int value = (int)(packet.SettingValue * 10f + 0.5f);
                if (value < 1)
                {
                    value = 1;
                }

                stationComponent.deliveryDrones = value;
            }

            if (packet.SettingIndex == StationUI.EUISettings.MinDeliverVessel)
            {
                int value = (int)(packet.SettingValue * 10f + 0.5f);
                if (value < 1)
                {
                    value = 1;
                }

                stationComponent.deliveryShips = value;
            }

            if (packet.SettingIndex == StationUI.EUISettings.WarpDistance)
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
            }

            if (packet.SettingIndex == StationUI.EUISettings.WarperNeeded)
            {
                stationComponent.warperNecessary = !stationComponent.warperNecessary;
            }

            if (packet.SettingIndex == StationUI.EUISettings.IncludeCollectors)
            {
                stationComponent.includeOrbitCollector = !stationComponent.includeOrbitCollector;
            }

            if (packet.SettingIndex == StationUI.EUISettings.AddOrRemoveItemFromStorageResponse)
            {
                if (stationComponent.storage != null)
                {
                    stationComponent.storage[packet.StorageIdx].count = (int)packet.SettingValue;
                }
            }
        }

        /*
         * Update station settings and drone, ship and warper counts.
         * 
         * First determine if the local player has the station window opened and handle that accordingly.
         */
        private void UpdateSettingsUI(StationUI packet)
        {
            StationComponent stationComponent = null;
            PlanetData planet = GameMain.galaxy?.PlanetById(packet.PlanetId);

            // If we can't find planet or the factory for said planet, we can just skip this
            if (planet?.factory?.transport == null)
            {
                return;
            }

            StationComponent[] gStationPool = GameMain.data.galacticTransport.stationPool;
            StationComponent[] stationPool = planet?.factory?.transport?.stationPool;

            // Figure out if we're dealing with a PLS or a ILS station
            stationComponent = packet.StationGId > 0 ? gStationPool[packet.StationGId] : stationPool?[packet.StationId];

            if (stationComponent == null)
            {
                Log.Warn($"UpdateStorageUI: Unable to find requested station on planet {packet.PlanetId} with id {packet.StationId} and gid of {packet.StationGId}");
                return;
            }

            // Do all updates in the background.
            UpdateSettingsUIBackground(packet, planet, stationComponent);
        }

        private void UpdateStorageUI(StationUI packet)
        {
            StationComponent stationComponent = null;
            PlanetData planet = GameMain.galaxy?.PlanetById(packet.PlanetId);

            // If we can't find planet or the factory for said planet, we can just skip this
            if (planet?.factory?.transport == null)
            {
                return;
            }

            StationComponent[] gStationPool = GameMain.data.galacticTransport.stationPool;
            StationComponent[] stationPool = planet?.factory?.transport?.stationPool;

            stationComponent = packet.StationGId > 0 ? gStationPool[packet.StationGId] : stationPool?[packet.StationId];

            if (stationComponent == null)
            {
                Log.Error($"UpdateStorageUI: Unable to find requested station on planet {packet.PlanetId} with id {packet.StationId} and gid of {packet.StationGId}");
                return;
            }

            using (Multiplayer.Session.Ships.PatchLockILS.On())
            {
                Log.Info($"Refresh storage {stationComponent.id}");
                planet.factory.transport.SetStationStorage(stationComponent.id, packet.StorageIdx, packet.ItemId, packet.ItemCountMax, packet.LocalLogic, packet.RemoteLogic, (packet.ShouldMimic == true) ? GameMain.mainPlayer : null);
            }
        }
    }
}
