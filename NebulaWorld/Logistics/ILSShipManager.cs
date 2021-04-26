using NebulaModel.Packets.Logistics;
using HarmonyLib;
using UnityEngine;
using System.Collections.Generic;
using System;

namespace NebulaWorld.Logistics
{
    public static class ILSShipManager
    {
        public static Dictionary<int,List<StationComponent>> AddStationComponentQueue = new Dictionary<int,List<StationComponent>>();
        public static void IdleShipGetToWork(ILSShipData packet)
        {
            PlanetData planetA = GameMain.galaxy.PlanetById(packet.planetA);
            PlanetData planetB = GameMain.galaxy.PlanetById(packet.planetB);

            if(planetA != null && planetB != null)
            {
                if (GameMain.data.galacticTransport.stationCapacity <= packet.planetAStationGID)
                {
                    CreateFakeStationComponent(packet.planetAStationGID, packet.planetA);
                }
                else if (GameMain.data.galacticTransport.stationPool[packet.planetAStationGID] == null)
                {
                    CreateFakeStationComponent(packet.planetAStationGID, packet.planetA);
                }
                else if(GameMain.data.galacticTransport.stationPool[packet.planetAStationGID].shipDockPos == Vector3.zero)
                {
                    RequestgStationDockPos(packet.planetAStationGID);
                }
                if (GameMain.data.galacticTransport.stationCapacity <= packet.planetBStationGID)
                {
                    CreateFakeStationComponent(packet.planetBStationGID, packet.planetB);
                }
                else if(GameMain.data.galacticTransport.stationPool[packet.planetBStationGID] == null)
                {
                    CreateFakeStationComponent(packet.planetBStationGID, packet.planetB);
                }
                else if(GameMain.data.galacticTransport.stationPool[packet.planetBStationGID].shipDockPos == Vector3.zero)
                {
                    RequestgStationDockPos(packet.planetBStationGID);
                }

                StationComponent stationComponent = GameMain.data.galacticTransport.stationPool[packet.planetAStationGID];
                stationComponent.workShipDatas[stationComponent.workShipCount].stage = -2;
                stationComponent.workShipDatas[stationComponent.workShipCount].planetA = packet.planetA;
                stationComponent.workShipDatas[stationComponent.workShipCount].planetB = packet.planetB;
                stationComponent.workShipDatas[stationComponent.workShipCount].otherGId = packet.planetBStationGID;
                stationComponent.workShipDatas[stationComponent.workShipCount].direction = 1;
                stationComponent.workShipDatas[stationComponent.workShipCount].t = 0f;
                stationComponent.workShipDatas[stationComponent.workShipCount].itemId = packet.itemId;
                stationComponent.workShipDatas[stationComponent.workShipCount].itemCount = packet.itemCount;
                stationComponent.workShipDatas[stationComponent.workShipCount].gene = 0; // WHAT IS THIS?
                stationComponent.workShipDatas[stationComponent.workShipCount].shipIndex = packet.origShipIndex;
                stationComponent.workShipDatas[stationComponent.workShipCount].warperCnt = packet.warperCnt;
                stationComponent.warperCount = packet.stationWarperCnt;

                stationComponent.workShipCount++;
                stationComponent.idleShipCount--;
                if(stationComponent.idleShipCount < 0)
                {
                    stationComponent.idleShipCount = 0;
                }
                if(stationComponent.workShipCount > 10)
                {
                    stationComponent.workShipCount = 10;
                }
                stationComponent.IdleShipGetToWork(packet.origShipIndex);
                StationComponent otherStationComponent = GameMain.data.galacticTransport.stationPool[packet.planetBStationGID];
                if(otherStationComponent != null && otherStationComponent.storage != null)
                {
                    for(int i = 0; i < otherStationComponent.storage.Length; i++)
                    {
                        if(otherStationComponent.storage[i].itemId == packet.itemId)
                        {
                            otherStationComponent.storage[i].remoteOrder += packet.itemCount;
                            RefreshValuesUI(otherStationComponent, i);
                            break;
                        }
                    }
                }
            }
        }

        public static void WorkShipBackToIdle(ILSShipData packet)
        {
            if(!SimulatedWorld.Initialized || LocalPlayer.IsMasterClient)
            {
                return;
            }

            if(GameMain.data.galacticTransport.stationCapacity <= packet.planetAStationGID)
            {
                CreateFakeStationComponent(packet.planetAStationGID, packet.planetA);
            }
            else if(GameMain.data.galacticTransport.stationPool[packet.planetAStationGID] == null)
            {
                CreateFakeStationComponent(packet.planetAStationGID, packet.planetA);
            }
            else if(GameMain.data.galacticTransport.stationPool[packet.planetAStationGID].shipDockPos == Vector3.zero)
            {
                RequestgStationDockPos(packet.planetAStationGID);
            }

            StationComponent stationComponent = GameMain.data.galacticTransport.stationPool[packet.planetAStationGID];
            Array.Copy(stationComponent.workShipDatas, packet.origShipIndex + 1, stationComponent.workShipDatas, packet.origShipIndex, stationComponent.workShipDatas.Length - packet.origShipIndex - 1);
            stationComponent.workShipCount--;
            stationComponent.idleShipCount++;
            if (stationComponent.idleShipCount < 0)
            {
                stationComponent.idleShipCount = 0;
            }
            if (stationComponent.workShipCount > 10)
            {
                stationComponent.workShipCount = 10;
            }
            stationComponent.WorkShipBackToIdle(packet.origShipIndex);
        }

        public static void CreateFakeStationComponent(int GId, int planetId, bool computeDisk = true)
        {
            while(GameMain.data.galacticTransport.stationCapacity <= GId)
            {
                object[] args = new object[1];
                args[0] = GameMain.data.galacticTransport.stationCapacity * 2;
                AccessTools.Method(typeof(GalacticTransport), "SetStationCapacity").Invoke(GameMain.data.galacticTransport, args);
            }

            GameMain.data.galacticTransport.stationPool[GId] = new StationComponent();
            StationComponent stationComponent = GameMain.data.galacticTransport.stationPool[GId];
            stationComponent.isStellar = true;
            stationComponent.gid = GId;
            stationComponent.planetId = planetId;
            stationComponent.workShipDatas = new ShipData[10]; // assume ILS have 10
            stationComponent.shipRenderers = new ShipRenderingData[10];
            stationComponent.shipUIRenderers = new ShipUIRenderingData[10];
            stationComponent.workShipCount = 0;
            stationComponent.idleShipCount = 0;
            stationComponent.shipDockPos = Vector3.zero; //gets updated later by server packet
            stationComponent.shipDockRot = Quaternion.identity; // gets updated later by server packet
            if (computeDisk)
            {
                stationComponent.shipDiskPos = new Vector3[10];
                stationComponent.shipDiskRot = new Quaternion[10];

                for (int i = 0; i < 10; i++)
                {
                    stationComponent.shipDiskRot[i] = Quaternion.Euler(0f, 360f / (float)10 * (float)i, 0f);
                    stationComponent.shipDiskPos[i] = stationComponent.shipDiskRot[i] * new Vector3(0f, 0f, 11.5f);
                }
                for (int j = 0; j < 10; j++)
                {
                    stationComponent.shipDiskRot[j] = stationComponent.shipDockRot * stationComponent.shipDiskRot[j];
                    stationComponent.shipDiskPos[j] = stationComponent.shipDockPos + stationComponent.shipDockRot * stationComponent.shipDiskPos[j];
                }

                RequestgStationDockPos(GId);
            }

            GameMain.data.galacticTransport.stationCursor++;
        }

        private static void RequestgStationDockPos(int GId)
        {
            LocalPlayer.SendPacket(new ILSRequestShipDock(GId));
        }

        public static void UpdateRemoteOrder(ILSRemoteOrderData packet)
        {
            if (!SimulatedWorld.Initialized || LocalPlayer.IsMasterClient)
            {
                return;
            }
            foreach(StationComponent stationComponent in GameMain.data.galacticTransport.stationPool)
            {
                if(stationComponent != null && stationComponent.gid == packet.stationGID)
                {
                    if(stationComponent.storage == null)
                    {
                        return;
                    }
                    stationComponent.storage[packet.storageIndex].remoteOrder = packet.remoteOrder;
                    RefreshValuesUI(stationComponent, packet.storageIndex);
                    break;
                }
            }
        }

        public static void AddTakeItem(ILSShipItems packet)
        {
            if(!SimulatedWorld.Initialized || LocalPlayer.IsMasterClient || GameMain.data.galacticTransport.stationPool.Length <= packet.stationGID)
            {
                return;
            }

            StationComponent stationComponent = GameMain.data.galacticTransport.stationPool[packet.stationGID];
            if (stationComponent != null && stationComponent.gid == packet.stationGID && stationComponent.storage != null)
            {
                if (packet.AddItem)
                {
                    stationComponent.AddItem(packet.itemId, packet.itemCount);
                    for(int i = 0; i < stationComponent.storage.Length; i++)
                    {
                        if(stationComponent.storage[i].itemId == packet.itemId)
                        {
                            stationComponent.storage[i].remoteOrder -= packet.itemCount;
                            RefreshValuesUI(stationComponent, i);
                            break;
                        }
                    }
                }
                else
                {
                    int itemId = packet.itemId;
                    int itemCount = packet.itemCount;
                    stationComponent.TakeItem(ref itemId, ref itemCount);
                    /*
                    if(stationComponent.workShipDatas[packet.origShipIndex].otherGId != 0 && stationComponent.workShipDatas[packet.origShipIndex].otherGId < GameMain.data.galacticTransport.stationPool.Length)
                    {
                        StationComponent otherStationComponent = GameMain.data.galacticTransport.stationPool[stationComponent.workShipDatas[packet.origShipIndex].otherGId];
                        if (otherStationComponent != null && otherStationComponent.storage != null)
                        {
                            for (int i = 0; i < otherStationComponent.storage.Length; i++)
                            {
                                if (otherStationComponent.storage[i].itemId == packet.itemId)
                                {
                                    otherStationComponent.storage[i].remoteOrder += packet.itemCount;
                                    RefreshValuesUI(otherStationComponent, i);
                                    break;
                                }
                            }
                        }
                    }
                    for (int i = 0; i < stationComponent.storage.Length; i++)
                    {
                        if (stationComponent.storage[i].itemId == packet.itemId)
                        {
                            if (stationComponent.workShipDatas[packet.origShipIndex].direction == 0)
                            {
                                stationComponent.storage[i].remoteOrder += packet.itemCount;
                                RefreshValuesUI(stationComponent, i);
                            }
                            break;
                        }
                    }
                    */
                    // this is the second attempt, but its also not working
                    // we need to check if this is the home station of the ship or not.
                    // if not then increase remoteOrder, if it is dont change remoteOrder
                    /*
                    foreach(StationComponent tmpComp in GameMain.data.galacticTransport.stationPool)
                    {
                        if(tmpComp != null)
                        {
                            foreach(ShipData tmpData in tmpComp.workShipDatas)
                            {
                                if(tmpData.itemId == packet.itemId && tmpData.itemCount == packet.itemCount && stationComponent.gid == tmpData.otherGId)
                                {
                                    for(int i = 0; i < stationComponent.storage.Length; i++)
                                    {
                                        if(stationComponent.storage[i].itemId == packet.itemId)
                                        {
                                            stationComponent.storage[i].remoteOrder += packet.itemCount;
                                            RefreshValuesUI(stationComponent, i);
                                        }
                                    }
                                    return;
                                }
                            }
                        }
                    }
                    */
                }
                Debug.Log(GameMain.galaxy.PlanetById(stationComponent.planetId).displayName + ": dir: " + stationComponent.workShipDatas[packet.origShipIndex].direction + " carry " + stationComponent.workShipDatas[packet.origShipIndex].itemId + "(" + stationComponent.workShipDatas[packet.origShipIndex].itemCount + ")");
            }
        }

        private static void RefreshValuesUI(StationComponent stationComponent, int storageIndex)
        {
            UIStationWindow stationWindow = UIRoot.instance.uiGame.stationWindow;
            if (stationWindow != null && (int)AccessTools.Field(typeof(UIStationWindow), "_stationId")?.GetValue(stationWindow) == stationComponent.id)
            {
                UIStationStorage[] stationStorageUI = (UIStationStorage[])AccessTools.Field(typeof(UIStationWindow), "storageUIs").GetValue(stationWindow);
                if (stationStorageUI != null && stationStorageUI.Length > storageIndex)
                {
                    AccessTools.Method(typeof(UIStationStorage), "RefreshValues").Invoke(stationStorageUI[storageIndex], null);
                }
            }
        }
    }
}
