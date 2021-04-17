using NebulaModel.Packets.Logistics;
using NebulaModel.Logger;
using HarmonyLib;
using UnityEngine;

namespace NebulaWorld.Logistics
{
    public static class ILSShipManager
    {
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
                stationComponent.workShipDatas[packet.origShipIndex].stage = -2;
                stationComponent.workShipDatas[packet.origShipIndex].planetA = packet.planetA;
                stationComponent.workShipDatas[packet.origShipIndex].planetB = packet.planetB;
                stationComponent.workShipDatas[packet.origShipIndex].otherGId = packet.planetBStationGID;
                stationComponent.workShipDatas[packet.origShipIndex].direction = 1;
                stationComponent.workShipDatas[packet.origShipIndex].t = 0f;
                stationComponent.workShipDatas[packet.origShipIndex].itemId = packet.itemId;
                stationComponent.workShipDatas[packet.origShipIndex].itemCount = packet.itemCount;
                stationComponent.workShipDatas[packet.origShipIndex].gene = 0; // WHAT IS THIS?
                stationComponent.workShipDatas[packet.origShipIndex].shipIndex = packet.origShipIndex;
                stationComponent.workShipDatas[packet.origShipIndex].warperCnt = packet.warperCnt;

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
                Log.Info($"Received ship message (departing): {planetA.displayName} -> {planetB.displayName} transporting {packet.itemCount} of {packet.itemId} and index is {packet.origShipIndex}");
                //Log.Info($"Array Length is: {GameMain.data.galacticTransport.stationPool.Length} and there is also: {GameMain.data.galacticTransport.stationCapacity}");
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
            stationComponent.workShipDatas[packet.origShipIndex] = new ShipData();
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
            Log.Info($"Received ship message (landing): transporting {packet.itemCount} of {packet.itemId} and shipindex is {packet.origShipIndex} planet: {GameMain.galaxy.PlanetById(stationComponent.planetId).displayName}");
            //Log.Info($"Array Length is: {GameMain.data.galacticTransport.stationPool.Length} and there is also: {GameMain.data.galacticTransport.stationCapacity}");
        }

        public static void CreateFakeStationComponent(int GId, int planetId, bool computeDisk = true)
        {
            Debug.Log("Creating fake StationComponent with GId: " + GId + " on " + GameMain.galaxy.PlanetById(planetId).displayName);
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
            Debug.Log("cursor: " + GameMain.data.galacticTransport.stationCursor);
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
                    PlanetData pData = GameMain.galaxy.PlanetById(stationComponent.planetId);
                    if(pData?.factory?.transport != null)
                    {
                        foreach(StationComponent stationComponentPlanet in pData.factory.transport.stationPool)
                        {
                            if(stationComponentPlanet != null && stationComponentPlanet.gid == stationComponent.gid)
                            {
                                stationComponentPlanet.storage[packet.storageIndex].remoteOrder = packet.remoteOrder;
                                break;
                            }
                        }
                    }
                    break;
                }
            }
        }

        public static void AddTakeItem(ILSShipItems packet)
        {
            if(!SimulatedWorld.Initialized || LocalPlayer.IsMasterClient)
            {
                return;
            }

            foreach(StationComponent stationComponent in GameMain.data.galacticTransport.stationPool)
            {
                if(stationComponent != null && stationComponent.gid == packet.stationGID)
                {
                    PlanetData pData = GameMain.galaxy.PlanetById(stationComponent.planetId);
                    if(pData?.factory?.transport != null)
                    {
                        foreach(StationComponent stationComponentPlanet in pData.factory.transport.stationPool)
                        {
                            if(stationComponentPlanet != null && stationComponentPlanet.gid == stationComponent.gid)
                            {
                                if (packet.AddItem)
                                {
                                    //Log.Info($"Calling AddItem() with item {packet.itemId} and amount {packet.itemCount}");
                                    stationComponentPlanet.AddItem(packet.itemId, packet.itemCount);
                                }
                                else
                                {
                                    //Log.Info($"Calling TakeItem() with item {packet.itemId} and amount {packet.itemCount}");
                                    int itemId = packet.itemId;
                                    int itemCount = packet.itemCount;
                                    stationComponentPlanet.TakeItem(ref itemId, ref itemCount);
                                }
                                break;
                            }
                        }
                    }
                    break;
                }
            }
        }
    }
}
