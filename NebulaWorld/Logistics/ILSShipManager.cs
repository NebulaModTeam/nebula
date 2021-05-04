using NebulaModel.Packets.Logistics;
using HarmonyLib;
using UnityEngine;
using System;
using System.Reflection;
using NebulaModel.DataStructures;
using NebulaModel.Logger;

namespace NebulaWorld.Logistics
{
    public static class ILSShipManager
    {
        private static AccessTools.FieldRef<object, int> FR_stationId;
        private static AccessTools.FieldRef<object, UIStationStorage[]> FR_storageUIs;
        private static MethodInfo MI_RefreshValues = null;

        public static readonly ToggleSwitch PatchLockILS = new ToggleSwitch();

        // the following 4 are needed to prevent a packet flood when the filter on a belt connected to a PLS/ILS is set.
        public static int ItemSlotLastSelectedIndex = 0;
        public static int ItemSlotLastSlotId = 0;
        public static int ItemSlotStationId = 0;
        public static int ItemSlotStationGId = 0;

        public const int ILSMaxShipCount = 10;
        public static void Initialize()
        {
            FR_stationId = AccessTools.FieldRefAccess<int>(typeof(UIStationWindow), "_stationId");
            FR_storageUIs = AccessTools.FieldRefAccess<UIStationStorage[]>(typeof(UIStationWindow), "storageUIs");
            MI_RefreshValues = AccessTools.Method(typeof(UIStationStorage), "RefreshValues");
        }
        /*
         * When the host notifies the client that a ship started its travel client needs to check if he got both ILS in his gStationPool
         * if not we create a fake entry (which gets updated to the full one when client arrives that planet) and also request the stations dock position
         */
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
                if(stationComponent.workShipCount > ILSMaxShipCount)
                {
                    stationComponent.workShipCount = ILSMaxShipCount;
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

        /*
         * this is also triggered by server and called once a ship lands back to the dock station
         */
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
            if (stationComponent.workShipCount > ILSMaxShipCount)
            {
                stationComponent.workShipCount = ILSMaxShipCount;
            }
            stationComponent.WorkShipBackToIdle(packet.origShipIndex);
        }

        /*
         * Create an entry in the gStationPool with minimal info for ships to travel and render correctly.
         * The information is needed in StationComponent.InternalTickRemote(), but we use a reverse patched version of that
         * which is stripped down to the ship movement and rendering part.
         */
        public static void CreateFakeStationComponent(int GId, int planetId, bool computeDisk = true)
        {
            // it may be needed to make additional room for the new ILS
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
            stationComponent.workShipDatas = new ShipData[ILSMaxShipCount];
            stationComponent.shipRenderers = new ShipRenderingData[ILSMaxShipCount];
            stationComponent.shipUIRenderers = new ShipUIRenderingData[ILSMaxShipCount];
            stationComponent.workShipCount = 0;
            stationComponent.idleShipCount = 0;
            stationComponent.shipDockPos = Vector3.zero; //gets updated later by server packet
            stationComponent.shipDockRot = Quaternion.identity; // gets updated later by server packet
            if (computeDisk)
            {
                stationComponent.shipDiskPos = new Vector3[ILSMaxShipCount];
                stationComponent.shipDiskRot = new Quaternion[ILSMaxShipCount];

                for (int i = 0; i < ILSMaxShipCount; i++)
                {
                    stationComponent.shipDiskRot[i] = Quaternion.Euler(0f, 360f / (float)ILSMaxShipCount * (float)i, 0f);
                    stationComponent.shipDiskPos[i] = stationComponent.shipDiskRot[i] * new Vector3(0f, 0f, 11.5f);
                }
                for (int j = 0; j < ILSMaxShipCount; j++)
                {
                    stationComponent.shipDiskRot[j] = stationComponent.shipDockRot * stationComponent.shipDiskRot[j];
                    stationComponent.shipDiskPos[j] = stationComponent.shipDockPos + stationComponent.shipDockRot * stationComponent.shipDiskPos[j];
                }

                RequestgStationDockPos(GId);
            }

            GameMain.data.galacticTransport.stationCursor++;
        }

        /*
         * As StationComponent.InternalTickRemote() neds to have the dock position to correctly compute ship movement we request it here from server.
         */
        private static void RequestgStationDockPos(int GId)
        {
            LocalPlayer.SendPacket(new ILSRequestShipDock(GId));
        }

        /*
         * Update the items that are currently in transfer by ships
         */
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

        /*
         * This is triggered by server and either adds or removes items to an ILS caused by a ship transport.
         * Also update the remoteOrder value to reflect the changes
         */
        public static void AddTakeItem(ILSShipItems packet)
        {
            if(!SimulatedWorld.Initialized || LocalPlayer.IsMasterClient || GameMain.data.galacticTransport.stationPool.Length <= packet.stationGID)
            {
                return;
            }

            StationComponent stationComponent = GameMain.data.galacticTransport.stationPool[packet.stationGID];
            if (stationComponent != null && stationComponent.gid == packet.stationGID && stationComponent.storage != null)
            {
                if (packet.addItem)
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
                    // TODO: Update remote order here (issue #230)
                }
            }
        }

        /*
         * call UIStationStorage.RefreshValues() on the current opened stations UI
         */
        private static void RefreshValuesUI(StationComponent stationComponent, int storageIndex)
        {
            UIStationWindow stationWindow = UIRoot.instance.uiGame.stationWindow;
            if (stationWindow != null && FR_stationId(stationWindow) == stationComponent.id)
            {
                UIStationStorage[] stationStorageUI = FR_storageUIs(stationWindow);
                if (stationStorageUI != null && stationStorageUI.Length > storageIndex)
                {
                    MI_RefreshValues.Invoke(stationStorageUI[storageIndex], null);
                }
            }
        }

        public static void UpdateSlotData(ILSUpdateSlotData packet)
        {
            Log.Info($"Updating slot data for planet {packet.PlanetId}, station {packet.StationId} gid {packet.StationGId}. Index {packet.Index}, storageIdx {packet.StorageIdx}");
            
            // Clients only care about what happens on their planet, hosts always need to apply this.
            // Using PlanetFactory to prevent getting the "fakes" that are creates on clients.
            if (LocalPlayer.IsMasterClient || (!LocalPlayer.IsMasterClient && packet.PlanetId == GameMain.localPlanet?.id))
            {
                PlanetData pData = GameMain.galaxy.PlanetById(packet.PlanetId);
                StationComponent stationComponent = pData?.factory?.transport?.stationPool[packet.StationId];
                
                if (stationComponent?.slots != null)
                {
                    stationComponent.slots[packet.Index].storageIdx = packet.StorageIdx;
                }
            }
        }
    }
}
