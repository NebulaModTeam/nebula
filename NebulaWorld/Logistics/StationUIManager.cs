using NebulaModel.Packets.Logistics;
using HarmonyLib;
using System;
using System.Collections.Generic;
using NebulaModel.Networking;
using UnityEngine;
using UnityEngine.EventSystems;

namespace NebulaWorld.Logistics
{
    public static class StationUIManager
    {
        private static Dictionary<int, List<NebulaConnection>> StationUISubscribers;
        public static int UpdateCooldown; // cooldown is used to slow down updates on storage slider
        public static BaseEventData lastMouseEvent;
        public static bool lastMouseEventWasDown;
        public static GameObject lastSelectedGameObj;
        public static int UIIsSyncedStage; // 0 == not synced, 1 == request sent, 2 == synced | this is only used client side
        public static int UIStationId;
        public static bool UIRequestedShipDronWarpChange; // when receiving a ship, drone or warp change only take/add items from the one issuing the request

        public static void Initialize()
        {
            StationUISubscribers = new Dictionary<int, List<NebulaConnection>>();
            UpdateCooldown = 0;
            UIIsSyncedStage = 0;
            UIStationId = 0;
            UIRequestedShipDronWarpChange = false;
        }
        // When a client opens a station's UI he requests a subscription for live updates, so add him to the list
        public static void AddSubscriber(int stationGId, NebulaConnection player)
        {
            List<NebulaConnection> players;
            if(!StationUISubscribers.TryGetValue(stationGId, out players))
            {
                players = new List<NebulaConnection>();
                StationUISubscribers.Add(stationGId, players);
            }
            for (int i = 0; i < StationUISubscribers[stationGId].Count; i++)
            {
                if(StationUISubscribers[stationGId][i] == player)
                {
                    return;
                }
            }
            StationUISubscribers[stationGId].Add(player);
        }
        public static void RemoveSubscriber(int stationGId, NebulaConnection player)
        {
            List<NebulaConnection> players;
            if(StationUISubscribers.TryGetValue(stationGId, out players))
            {
                StationUISubscribers[stationGId].Remove(player);
                if(StationUISubscribers[stationGId].Count == 0)
                {
                    StationUISubscribers.Remove(stationGId);
                }
            }
        }
        public static List<NebulaConnection> GetSubscribers(int stationGId)
        {
            List<NebulaConnection> players;
            if(!StationUISubscribers.TryGetValue(stationGId, out players))
            {
                return new List<NebulaConnection>();
            }
            return players;
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
            if((UpdateCooldown == 0 || !packet.isStorageUI) && LocalPlayer.IsMasterClient)
            {
                UpdateCooldown = 10;
                if (packet.isStorageUI)
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
                if (packet.isStorageUI)
                {
                    UpdateStorageUI(packet);
                }
                else
                {
                    UpdateSettingsUI(packet);
                }
            }
        }
        /*
         * if the local player does not have the corresponding station window opened we still need to update some (or all for host) settings
         * so do that here
         */
        private static void UpdateSettingsUIBackground(StationUI packet, PlanetData pData, int stationGId)
        {
            StationComponent[] stationPool = GameMain.data.galacticTransport.stationPool;
            int stationId = stationGId;
            // if we have the planet factory loaded take the local transport array, if not take the global galactic array
            if (pData?.factory != null && pData?.factory?.transport != null)
            {
                stationPool = pData.factory.transport.stationPool;
                for (int i = 0; i < stationPool.Length; i++)
                {
                    if (stationPool[i] != null && stationPool[i].gid == stationGId)
                    {
                        stationId = stationPool[i].id;
                        break;
                    }
                }
            }
            // update drones, ships, warpers and energy consumption for everyone
            if ((packet.settingIndex >= StationUI.UIsettings.setDroneCount && packet.settingIndex <= StationUI.UIsettings.setWarperCount) || packet.settingIndex == StationUI.UIsettings.MaxChargePower)
            {
                if (stationPool.Length > stationId)
                {
                    if (packet.settingIndex == (int)StationUI.UIsettings.MaxChargePower && pData.factory?.powerSystem != null)
                    {
                        PowerConsumerComponent[] consumerPool = pData.factory.powerSystem.consumerPool;
                        if (consumerPool.Length > stationPool[stationId].pcId)
                        {
                            consumerPool[stationPool[stationId].pcId].workEnergyPerTick = (long)(50000.0 * (double)packet.settingValue + 0.5);
                        }
                    }
                    if (packet.settingIndex == StationUI.UIsettings.setDroneCount)
                    {
                        stationPool[stationId].idleDroneCount = (int)packet.settingValue;
                    }
                    if (packet.settingIndex == StationUI.UIsettings.setShipCount)
                    {
                        stationPool[stationId].idleShipCount = (int)packet.settingValue;
                    }
                    if (packet.settingIndex == StationUI.UIsettings.setWarperCount)
                    {
                        stationPool[stationId].warperCount = (int)packet.settingValue;
                    }
                }
            }
            // only host should update everything
            if (!LocalPlayer.IsMasterClient)
            {
                // this is changed in pr #269 anyways to reflect this logic (update for everyone, not just host. after all why should'nt we)
                //return;
            }
            if(packet.settingIndex == StationUI.UIsettings.MaxTripDrones)
            {
                if (stationPool.Length > stationId)
                {
                    stationPool[stationId].tripRangeDrones = Math.Cos((double)packet.settingValue / 180.0 * 3.141592653589793);
                }
            }
            if(packet.settingIndex == StationUI.UIsettings.MaxTripVessel)
            {
                if (stationPool.Length > stationId)
                {
                    double value = packet.settingValue;
                    if (value > 40.5)
                    {
                        value = 10000.0;
                    }
                    else if (value > 20.5)
                    {
                        value = value * 2f - 20f;
                    }
                    stationPool[stationId].tripRangeShips = 2400000.0 * value;
                }
            }
            if(packet.settingIndex == StationUI.UIsettings.MinDeliverDrone)
            {
                if (stationPool.Length > stationId)
                {
                    int value = (int)(packet.settingValue * 10f + 0.5f);
                    if (value < 1)
                    {
                        value = 1;
                    }
                    stationPool[stationId].deliveryDrones = value;
                }
            }
            if(packet.settingIndex == StationUI.UIsettings.MinDeliverVessel)
            {
                if (stationPool.Length > stationId)
                {
                    int value = (int)(packet.settingValue * 10f + 0.5f);
                    if (value < 1)
                    {
                        value = 1;
                    }
                    stationPool[stationId].deliveryShips = value;
                }
            }
            if(packet.settingIndex == StationUI.UIsettings.WarpDistance)
            {
                if (stationPool.Length > stationId)
                {
                    double value = packet.settingValue;
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
                    stationPool[stationId].warpEnableDist = 40000.0 * value;
                }
            }
            if(packet.settingIndex == StationUI.UIsettings.warperNeeded)
            {
                if (stationPool.Length > stationId)
                {
                    stationPool[stationId].warperNecessary = !stationPool[stationId].warperNecessary;
                }
            }
            if(packet.settingIndex == StationUI.UIsettings.includeCollectors)
            {
                if (stationPool.Length > stationId)
                {
                    stationPool[stationId].includeOrbitCollector = !stationPool[stationId].includeOrbitCollector;
                }
            }
            if (packet.settingIndex == StationUI.UIsettings.addOrRemoveItemFromStorageResp)
            {
                if (stationPool[stationId].storage != null)
                {
                    stationPool[stationId].storage[packet.storageIdx].count = (int)packet.settingValue;
                }
            }
        }
        /*
         * update settings and item, drone, ship and warper count
         * first determine if the local player has the station window opened and hadle that accordingly.
         */
        private static void UpdateSettingsUI(StationUI packet)
        {
            UIStationWindow stationWindow = UIRoot.instance.uiGame.stationWindow;
            if (stationWindow != null)
            {
                int _stationId = (int)AccessTools.Field(typeof(UIStationWindow), "_stationId")?.GetValue(stationWindow);
                PlanetData pData = GameMain.galaxy.PlanetById(packet.planetId);
                if(pData?.factory == null || pData?.factory?.transport == null)
                {
                    if(GameMain.data.galacticTransport.stationPool.Length > packet.stationGId && GameMain.data.galacticTransport.stationPool[packet.stationGId] != null)
                    {
                        // client never was on this planet before or has it unloaded, but has a fake structure created, so update it
                        UpdateSettingsUIBackground(packet, pData, packet.stationGId);
                    }
                    return;
                }
                for (int i = 0; i < pData.factory.transport.stationPool.Length; i++)
                {
                    if(pData.factory.transport.stationPool[i] != null)
                    {
                        int id = ((packet.isStellar == true) ? pData.factory.transport.stationPool[i].gid : pData.factory.transport.stationPool[i].id);
                        if (id == packet.stationGId)
                        {
                            if (pData.factory.transport.stationPool[i].id != _stationId)
                            {
                                // receiving side has the UI closed or another stations UI opened. still update drones, ships, warpers and power consumption for clients and update all for host
                                UpdateSettingsUIBackground(packet, pData, pData.factory.transport.stationPool[i].gid);
                                return;
                            }
                            else
                            {
                                break;
                            }
                        }
                    }
                }

                // this locks the patches so we can call vanilla functions without triggering our patches to avoid endless loops
                using (ILSShipManager.PatchLockILS.On())
                {
                    if (packet.settingIndex == StationUI.UIsettings.MaxChargePower)
                    {
                        stationWindow.OnMaxChargePowerSliderValueChange(packet.settingValue);
                    }
                    if (packet.settingIndex == StationUI.UIsettings.MaxTripDrones)
                    {
                        stationWindow.OnMaxTripDroneSliderValueChange(packet.settingValue);
                    }
                    if (packet.settingIndex == StationUI.UIsettings.MaxTripVessel)
                    {
                        stationWindow.OnMaxTripVesselSliderValueChange(packet.settingValue);
                    }
                    if (packet.settingIndex == StationUI.UIsettings.MinDeliverDrone)
                    {
                        stationWindow.OnMinDeliverDroneValueChange(packet.settingValue);
                    }
                    if (packet.settingIndex == StationUI.UIsettings.MinDeliverVessel)
                    {
                        stationWindow.OnMinDeliverVesselValueChange(packet.settingValue);
                    }
                    if (packet.settingIndex == StationUI.UIsettings.WarpDistance)
                    {
                        stationWindow.OnWarperDistanceValueChange(packet.settingValue);
                    }
                    if (packet.settingIndex == StationUI.UIsettings.warperNeeded)
                    {
                        stationWindow.OnWarperNecessaryClick(0);
                    }
                    if (packet.settingIndex == StationUI.UIsettings.includeCollectors)
                    {
                        Type[] args = new Type[1];
                        object[] values = new object[1];
                        args[0] = typeof(int);
                        values[0] = 0;
                        AccessTools.Method(typeof(UIStationWindow), "OnIncludeOrbitCollectorClick", args).Invoke(stationWindow, values);
                    }
                    if (packet.settingIndex >= StationUI.UIsettings.setDroneCount && packet.settingIndex <= StationUI.UIsettings.setWarperCount)
                    {
                        StationComponent[] stationPool = pData.factory.transport.stationPool;
                        if (packet.settingIndex == StationUI.UIsettings.setDroneCount)
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
                            stationPool[_stationId].idleDroneCount = (int)packet.settingValue;
                        }
                        if (packet.settingIndex == StationUI.UIsettings.setShipCount)
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
                            stationPool[_stationId].idleShipCount = (int)packet.settingValue;
                        }
                        if (packet.settingIndex == StationUI.UIsettings.setWarperCount)
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
                            stationPool[_stationId].warperCount = (int)packet.settingValue;
                        }
                    }
                    /*
                     * the idea is that clients request that they want to apply a change and do so once the server responded with an okay.
                     * the calls to OnItemIconMouseDown() and OnItemIconMouseUp() are blocked for clients and called only from here.
                     */
                    if (packet.settingIndex == StationUI.UIsettings.addOrRemoveItemFromStorageReq)
                    {
                        StationComponent[] stationPool = pData.factory.transport.stationPool;
                        if (stationPool[_stationId].storage != null)
                        {
                            if (packet.shouldMimick)
                            {
                                BaseEventData mouseEvent = lastMouseEvent;
                                UIStationStorage[] storageUIs = (UIStationStorage[])AccessTools.Field(typeof(UIStationWindow), "storageUIs").GetValue(stationWindow);

                                if (lastMouseEvent != null)
                                {
                                    // TODO: change this such that only server sends the response, else clients with a desynced state could change servers storage to a faulty value
                                    // issue #249
                                    if (lastMouseEventWasDown)
                                    {
                                        storageUIs[packet.storageIdx].OnItemIconMouseDown(mouseEvent);
                                        StationUI packet2 = new StationUI(packet.stationGId, packet.planetId, packet.storageIdx, StationUI.UIsettings.addOrRemoveItemFromStorageResp, packet.itemId, stationPool[_stationId].storage[packet.storageIdx].count, packet.isStellar);
                                        LocalPlayer.SendPacket(packet2);
                                    }
                                    else
                                    {
                                        storageUIs[packet.storageIdx].OnItemIconMouseUp(mouseEvent);
                                        StationUI packet2 = new StationUI(packet.stationGId, packet.planetId, packet.storageIdx, StationUI.UIsettings.addOrRemoveItemFromStorageResp, packet.itemId, stationPool[_stationId].storage[packet.storageIdx].count, packet.isStellar);
                                        LocalPlayer.SendPacket(packet2);
                                    }
                                    lastMouseEvent = null;
                                }
                            }
                        }
                    }
                    if (packet.settingIndex == StationUI.UIsettings.addOrRemoveItemFromStorageResp)
                    {
                        StationComponent[] stationPool = pData.factory.transport.stationPool;
                        if (stationPool[_stationId].storage != null)
                        {
                            stationPool[_stationId].storage[packet.storageIdx].count = (int)packet.settingValue;
                        }
                    }
                }
            }
        }

        private static void UpdateStorageUI(StationUI packet)
        {
            PlanetData pData = GameMain.galaxy.PlanetById(packet.planetId);
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
                /*
                 * we need to find the stations id in the PlanetTransport structure to call SetStationStorage
                 */
                int id = -1;
                if(pData.factory?.transport == null)
                {
                    return;
                }
                if((!packet.isStellar && packet.stationGId >= pData.factory.transport.stationPool.Length) || pData.factory.transport == null)
                {
                    return;
                }
                else if (packet.isStellar)
                {
                    foreach(StationComponent stationComponent in pData.factory.transport.stationPool)
                    {
                        if(stationComponent != null && stationComponent.gid == packet.stationGId)
                        {
                            id = stationComponent.id;
                        }
                    }
                }
                else
                {
                    // if its a PLS server sends the id and not GId
                    id = pData.factory.transport.stationPool[packet.stationGId].id;
                }

                if(id == -1)
                {
                    return;
                }

                using (ILSShipManager.PatchLockILS.On())
                {
                    pData.factory.transport.SetStationStorage(id, packet.storageIdx, packet.itemId, packet.itemCountMax, packet.localLogic, packet.remoteLogic, (packet.shouldMimick == true) ? GameMain.mainPlayer : null);
                }
            }
        }
    }
}
