using NebulaModel.Packets.Logistics;
using HarmonyLib;
using NebulaModel.Logger;
using System;
using System.Collections.Generic;
using NebulaModel.Networking;
using UnityEngine;
using UnityEngine.EventSystems;

namespace NebulaWorld.Logistics
{
    public class Subscribers
    {
        public int PlanetId { get; }
        public int StationId { get; }
        public int StationGId { get; }
        public List<NebulaConnection> Connections { get; set; }
        public Subscribers(int planetId, int stationId, int stationGId)
        {
            PlanetId = planetId;
            StationId = stationId;
            StationGId = stationGId;
            Connections = new List<NebulaConnection>();
        }

        public override string ToString() => $"{PlanetId}.{StationId}.{StationGId}";

        public static string GetKey(int planetId, int stationId, int statgionGId)
        {
            return $"{planetId}.{stationId}.{statgionGId}";
        }
    }
    
    public static class StationUIManager
    {
        private static Dictionary<string, Subscribers> _stationUISubscribers;
        public static int UpdateCooldown; // cooldown is used to slow down updates on storage slider
        public static BaseEventData LastMouseEvent;
        public static bool LastMouseEventWasDown;
        public static GameObject LastSelectedGameObj;
        public static int UIIsSyncedStage; // 0 == not synced, 1 == request sent, 2 == synced | this is only used client side
        public static int UIStationId;
        public static bool UIRequestedShipDronWarpChange; // when receiving a ship, drone or warp change only take/add items from the one issuing the request

        public static void Initialize()
        {
            _stationUISubscribers = new Dictionary<string, Subscribers>();
            UpdateCooldown = 0;
            UIIsSyncedStage = 0;
            UIStationId = 0;
            UIRequestedShipDronWarpChange = false;
        }
        // When a client opens a station's UI he requests a subscription for live updates, so add him to the list
        public static void AddSubscriber(int planetId, int stationId, int stationGId, NebulaConnection connection)
        {
            // Attempt to find existing subscribers to a specific station, if we couldn't find an existing one
            // we must initialize a new Subscribers for this specific station.
            if(!_stationUISubscribers.TryGetValue(Subscribers.GetKey(planetId, stationId, stationGId), out Subscribers subscribers))
            {
                _stationUISubscribers.Add(Subscribers.GetKey(planetId, stationId, stationGId), new Subscribers(planetId, stationId, stationGId));
            }

            _stationUISubscribers.TryGetValue(Subscribers.GetKey(planetId, stationId, stationGId), out subscribers);
            
            subscribers?.Connections.Add(connection);
        }
        public static void RemoveSubscriber(int planetId, int stationId, int stationGId, NebulaConnection connection)
        {
            if(_stationUISubscribers.TryGetValue(Subscribers.GetKey(planetId, stationId, stationGId), out Subscribers subscribers))
            {
                subscribers.Connections.Remove(connection);

                if (subscribers.Connections.Count == 0)
                {
                    _stationUISubscribers.Remove(subscribers.ToString());
                }
            }
        }
        
        public static List<NebulaConnection> GetSubscribers(int planetId, int stationId, int stationGId)
        {
            if(!_stationUISubscribers.TryGetValue(Subscribers.GetKey(planetId, stationId, stationGId), out Subscribers subscribers))
            {
                return new List<NebulaConnection>();
            }
            
            return subscribers.Connections;
        }

        public static void DecreaseCooldown()
        {
            // cooldown is for the storage sliders
            if(UpdateCooldown > 0)
            {
                UpdateCooldown--;
            }
        }

        public static void UpdateUI(StationUI packet)
        {
            if((UpdateCooldown == 0 || !packet.IsStorageUI) && LocalPlayer.IsMasterClient)
            {
                UpdateCooldown = 10;
                if (packet.IsStorageUI)
                {
                    UpdateStorageUI(packet);
                }
                else
                {
                    UpdateSettingsUI(packet);
                }
            }
            else if(!LocalPlayer.IsMasterClient)
            {
                if (packet.IsStorageUI)
                {
                    UpdateStorageUI(packet);
                }
                else
                {
                    UpdateSettingsUI(packet);
                }
            }
        }
        
        /**
         * Updates to a given station that should happen in the background.
         */
        private static void UpdateSettingsUIBackground(StationUI packet, PlanetData planet, StationComponent stationComponent)
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
                        consumerPool[stationComponent.pcId].workEnergyPerTick = (long)(50000.0 * (double)packet.SettingValue + 0.5);
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
                }
            }

            if (packet.SettingIndex == StationUI.EUISettings.MaxTripDrones)
            {
                stationComponent.tripRangeDrones = Math.Cos((double)packet.SettingValue / 180.0 * 3.141592653589793);
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
        private static void UpdateSettingsUI(StationUI packet)
        {
            UIStationWindow stationWindow = UIRoot.instance.uiGame.stationWindow;
            
            StationComponent stationComponent = null;
            PlanetData planet = GameMain.galaxy?.PlanetById(packet.PlanetId);
            
            // If we can't find planet or the factory for said planet, we can just skip this
            if (planet?.factory?.transport == null) return;
            
            StationComponent[] gStationPool = GameMain.data.galacticTransport.stationPool;
            StationComponent[] stationPool = planet?.factory?.transport?.stationPool;
            
            // Figure out if we're dealing with a PLS or a ILS station
            stationComponent = packet.StationGId > 0 ? gStationPool[packet.StationGId] : stationPool?[packet.StationId];
            
            if (stationComponent == null)
            {
                Log.Error($"UpdateStorageUI: Unable to find requested station on planet {packet.PlanetId} with id {packet.StationId} and gid of {packet.StationGId}");
                return;
            }

            if (stationWindow == null) return;
            
            int _stationId = (int)AccessTools.Field(typeof(UIStationWindow), "_stationId")?.GetValue(stationWindow);

            // Client has no knowledge of the planet, closed the window or
            // opened a different station, do all updates in the background.
            if(planet?.factory?.transport == null || stationComponent.id != _stationId)
            {
                UpdateSettingsUIBackground(packet, planet, stationComponent);
                return;
            }

            // this locks the patches so we can call vanilla functions without triggering our patches to avoid endless loops
            using (ILSShipManager.PatchLockILS.On())
            {
                if (packet.SettingIndex == StationUI.EUISettings.MaxChargePower)
                {
                    stationWindow.OnMaxChargePowerSliderValueChange(packet.SettingValue);
                }
                if (packet.SettingIndex == StationUI.EUISettings.MaxTripDrones)
                {
                    stationWindow.OnMaxTripDroneSliderValueChange(packet.SettingValue);
                }
                if (packet.SettingIndex == StationUI.EUISettings.MaxTripVessel)
                {
                    stationWindow.OnMaxTripVesselSliderValueChange(packet.SettingValue);
                }
                if (packet.SettingIndex == StationUI.EUISettings.MinDeliverDrone)
                {
                    stationWindow.OnMinDeliverDroneValueChange(packet.SettingValue);
                }
                if (packet.SettingIndex == StationUI.EUISettings.MinDeliverVessel)
                {
                    stationWindow.OnMinDeliverVesselValueChange(packet.SettingValue);
                }
                if (packet.SettingIndex == StationUI.EUISettings.WarpDistance)
                {
                    stationWindow.OnWarperDistanceValueChange(packet.SettingValue);
                }
                if (packet.SettingIndex == StationUI.EUISettings.WarperNeeded)
                {
                    stationWindow.OnWarperNecessaryClick(0);
                }
                if (packet.SettingIndex == StationUI.EUISettings.IncludeCollectors)
                {
                    Type[] args = new Type[1];
                    object[] values = new object[1];
                    args[0] = typeof(int);
                    values[0] = 0;
                    AccessTools.Method(typeof(UIStationWindow), "OnIncludeOrbitCollectorClick", args).Invoke(stationWindow, values);
                }
                if (packet.SettingIndex >= StationUI.EUISettings.SetDroneCount && packet.SettingIndex <= StationUI.EUISettings.SetWarperCount)
                {
                    if (packet.SettingIndex == StationUI.EUISettings.SetDroneCount)
                    {
                        Type[] args = new Type[1];
                        object[] values = new object[1];
                        args[0] = typeof(int);
                        values[0] = 0;
                        if (UIRequestedShipDronWarpChange)
                        {
                            AccessTools.Method(typeof(UIStationWindow), "OnDroneIconClick", args).Invoke(stationWindow, values);
                            UIRequestedShipDronWarpChange = false;
                        }
                        stationComponent.idleDroneCount = (int)packet.SettingValue;
                    }
                    if (packet.SettingIndex == StationUI.EUISettings.SetShipCount)
                    {
                        Type[] args = new Type[1];
                        object[] values = new object[1];
                        args[0] = typeof(int);
                        values[0] = 0;
                        if (UIRequestedShipDronWarpChange)
                        {
                            AccessTools.Method(typeof(UIStationWindow), "OnShipIconClick", args).Invoke(stationWindow, values);
                            UIRequestedShipDronWarpChange = false;
                        }
                        stationComponent.idleShipCount = (int)packet.SettingValue;
                    }
                    if (packet.SettingIndex == StationUI.EUISettings.SetWarperCount)
                    {
                        Type[] args = new Type[1];
                        object[] values = new object[1];
                        args[0] = typeof(int);
                        values[0] = 0;
                        if (UIRequestedShipDronWarpChange)
                        {
                            AccessTools.Method(typeof(UIStationWindow), "OnWarperIconClick", args).Invoke(stationWindow, values);
                            UIRequestedShipDronWarpChange = false;
                        }
                        stationComponent.warperCount = (int)packet.SettingValue;
                    }
                }
                /*
                 * the idea is that clients request that they want to apply a change and do so once the server responded with an okay.
                 * the calls to OnItemIconMouseDown() and OnItemIconMouseUp() are blocked for clients and called only from here.
                 */
                if (packet.SettingIndex == StationUI.EUISettings.AddOrRemoveItemFromStorageRequest)
                {
                    if (stationComponent.storage != null)
                    {
                        if (packet.ShouldMimic)
                        {
                            BaseEventData mouseEvent = LastMouseEvent;
                            UIStationStorage[] storageUIs = (UIStationStorage[])AccessTools.Field(typeof(UIStationWindow), "storageUIs").GetValue(stationWindow);

                            if (LastMouseEvent != null)
                            {
                                // TODO: change this such that only server sends the response, else clients with a desynced state could change servers storage to a faulty value
                                // issue #249
                                if (LastMouseEventWasDown)
                                {
                                    storageUIs[packet.StorageIdx].OnItemIconMouseDown(mouseEvent);
                                    StationUI packet2 = new StationUI(packet.PlanetId, packet.StationId, packet.StationGId, packet.StorageIdx, StationUI.EUISettings.AddOrRemoveItemFromStorageResponse, packet.ItemId, stationComponent.storage[packet.StorageIdx].count);
                                    LocalPlayer.SendPacket(packet2);
                                }
                                else
                                {
                                    storageUIs[packet.StorageIdx].OnItemIconMouseUp(mouseEvent);
                                    StationUI packet2 = new StationUI(packet.PlanetId, packet.StationId, packet.StationGId, packet.StorageIdx, StationUI.EUISettings.AddOrRemoveItemFromStorageResponse, packet.ItemId, stationComponent.storage[packet.StorageIdx].count);
                                    LocalPlayer.SendPacket(packet2);
                                }
                                LastMouseEvent = null;
                            }
                        }
                    }
                }
                if (packet.SettingIndex == StationUI.EUISettings.AddOrRemoveItemFromStorageResponse)
                {
                    if (stationComponent.storage != null)
                    {
                        stationComponent.storage[packet.StorageIdx].count = (int)packet.SettingValue;
                    }
                }
            }
        }

        private static void UpdateStorageUI(StationUI packet)
        {
            StationComponent stationComponent = null;
            PlanetData planet = GameMain.galaxy?.PlanetById(packet.PlanetId);
            
            // If we can't find planet or the factory for said planet, we can just skip this
            if (planet?.factory?.transport == null) return;
            
            StationComponent[] gStationPool = GameMain.data.galacticTransport.stationPool;
            StationComponent[] stationPool = planet?.factory?.transport?.stationPool;
            
            stationComponent = packet.StationGId > 0 ? gStationPool[packet.StationGId] : stationPool?[packet.StationId];
            
            if (stationComponent == null)
            {
                Log.Error($"UpdateStorageUI: Unable to find requested station on planet {packet.PlanetId} with id {packet.StationId} and gid of {packet.StationGId}");
                return;
            }
            
            using (ILSShipManager.PatchLockILS.On())
            {
                planet.factory.transport.SetStationStorage(stationComponent.id, packet.StorageIdx, packet.ItemId, packet.ItemCountMax, packet.LocalLogic, packet.RemoteLogic, (packet.ShouldMimic == true) ? GameMain.mainPlayer : null);
            }
        }
    }
}
