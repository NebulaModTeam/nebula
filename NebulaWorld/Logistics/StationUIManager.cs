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
        public static int UpdateCooldown;
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
            if(UpdateCooldown > 0)
            {
                Debug.Log(UpdateCooldown);
                UpdateCooldown--;
            }
        }

        public static void UpdateUI(StationUI packet)
        {
            Debug.Log("handling packet " + packet.settingIndex);
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
            // update drones, ships and warpers for everyone
            if ((packet.settingIndex >= 8 && packet.settingIndex <= 10) || packet.settingIndex == 0)
            {
                if (stationPool.Length > stationId)
                {
                    if (packet.settingIndex == 0 && pData.factory.powerSystem != null)
                    {
                        PowerConsumerComponent[] consumerPool = pData.factory.powerSystem.consumerPool;
                        if (consumerPool.Length > stationPool[stationId].pcId)
                        {
                            consumerPool[stationPool[stationId].pcId].workEnergyPerTick = (long)(50000.0 * (double)packet.settingValue + 0.5);
                        }
                    }
                    if (packet.settingIndex == 8)
                    {
                        stationPool[stationId].idleDroneCount = (int)packet.settingValue;
                    }
                    if (packet.settingIndex == 9)
                    {
                        stationPool[stationId].idleShipCount = (int)packet.settingValue;
                        Debug.Log("ADDED SHIPS!!");
                    }
                    if (packet.settingIndex == 10)
                    {
                        stationPool[stationId].warperCount = (int)packet.settingValue;
                        Debug.Log("ADDED WARPER!!");
                    }
                }
            }
            // only host should update everything
            if (!LocalPlayer.IsMasterClient)
            {
                return;
            }
            if(packet.settingIndex == 0)
            {
                PowerConsumerComponent[] consumerPool = pData.factory.powerSystem.consumerPool;
                if (consumerPool.Length > stationPool[stationId].pcId)
                {
                    consumerPool[stationPool[stationId].pcId].workEnergyPerTick = (long)(50000.0 * (double)packet.settingValue + 0.5);
                }
            }
            if(packet.settingIndex == 1)
            {
                if (stationPool.Length > stationId)
                {
                    stationPool[stationId].tripRangeDrones = Math.Cos((double)packet.settingValue / 180.0 * 3.141592653589793);
                }
            }
            if(packet.settingIndex == 2)
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
            if(packet.settingIndex == 3)
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
            if(packet.settingIndex == 4)
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
            if(packet.settingIndex == 5)
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
            if(packet.settingIndex == 6)
            {
                if (stationPool.Length > stationId)
                {
                    stationPool[stationId].warperNecessary = !stationPool[stationId].warperNecessary;
                }
            }
            if(packet.settingIndex == 7)
            {
                if (stationPool.Length > stationId)
                {
                    stationPool[stationId].includeOrbitCollector = !stationPool[stationId].includeOrbitCollector;
                }
            }
            if(packet.settingIndex == 11)
            {
                if (stationPool.Length > stationId && stationPool[stationId].storage != null)
                {
                    for (int i = 0; i < stationPool[stationId].storage.Length; i++)
                    {
                        if (stationPool[stationId].storage[i].itemId == packet.itemId)
                        {
                            stationPool[stationId].storage[i].count = (int)packet.settingValue;
                            break;
                        }
                    }
                }
            }
        }
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
                    if(pData.factory.transport.stationPool[i] != null && pData.factory.transport.stationPool[i].gid == packet.stationGId)
                    {
                        if(pData.factory.transport.stationPool[i].id != _stationId)
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

                LocalPlayer.PatchLocks["UIStationWindow"] = true;
                LocalPlayer.PatchLocks["UIStationStorage"] = true;
                if (packet.settingIndex == 0)
                {
                    stationWindow.OnMaxChargePowerSliderValueChange(packet.settingValue);
                }
                if (packet.settingIndex == 1)
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
                if (packet.settingIndex >= 8 && packet.settingIndex <= 10)
                {
                    StationComponent[] stationPool = pData.factory.transport.stationPool;
                    if (packet.settingIndex == 8)
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
                    if (packet.settingIndex == 9)
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
                    if (packet.settingIndex == 10)
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
                if (packet.settingIndex == 11)
                {
                    StationComponent[] stationPool = pData.factory.transport.stationPool;
                    if (stationPool[_stationId].storage != null)
                    {
                        if (packet.shouldMimick)
                        {
                            BaseEventData mouseEvent = lastMouseEvent;
                            PointerEventData pointEvent = mouseEvent as PointerEventData;
                            UIStationStorage[] storageUIs = (UIStationStorage[])AccessTools.Field(typeof(UIStationWindow), "storageUIs").GetValue(stationWindow);

                            if(lastMouseEvent != null)
                            {
                                if (lastMouseEventWasDown)
                                {
                                    storageUIs[packet.storageIdx].OnItemIconMouseDown(mouseEvent);
                                    StationUI packet2 = new StationUI(packet.stationGId, packet.planetId, packet.storageIdx, 12, packet.itemId, stationPool[_stationId].storage[packet.storageIdx].count);
                                    LocalPlayer.SendPacket(packet2);
                                }
                                else
                                {
                                    storageUIs[packet.storageIdx].OnItemIconMouseUp(mouseEvent);
                                    StationUI packet2 = new StationUI(packet.stationGId, packet.planetId, packet.storageIdx, 12, packet.itemId, stationPool[_stationId].storage[packet.storageIdx].count);
                                    LocalPlayer.SendPacket(packet2);
                                }
                                lastMouseEvent = null;
                            }
                        }
                        else
                        {
                            //stationPool[_stationId].storage[packet.storageIdx].count = (int)packet.settingValue;
                        }
                    }
                }
                if(packet.settingIndex == 12)
                {
                    Debug.Log("should set it");
                    StationComponent[] stationPool = pData.factory.transport.stationPool;
                    if(stationPool[_stationId].storage != null)
                    {
                        stationPool[_stationId].storage[packet.storageIdx].count = (int)packet.settingValue;
                    }
                }
                LocalPlayer.PatchLocks["UIStationWindow"] = false;
                LocalPlayer.PatchLocks["UIStationStorage"] = false;
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
                if(pData.factory?.transport == null)
                {
                    return;
                }
                int stationId = -1;
                for(int i = 0; i < pData.factory.transport.stationPool.Length; i++)
                {
                    if(pData.factory.transport.stationPool[i] != null && pData.factory.transport.stationPool[i].gid == packet.stationGId)
                    {
                        stationId = pData.factory.transport.stationPool[i].id;
                        break;
                    }
                }
                if(stationId == -1)
                {
                    return;
                }

                LocalPlayer.PatchLocks["PlanetTransport"] = true;
                pData.factory.transport.SetStationStorage(stationId, packet.storageIdx, packet.itemId, packet.itemCountMax, packet.localLogic, packet.remoteLogic, null);
                LocalPlayer.PatchLocks["PlanetTransport"] = false;
            }
        }
    }
}
