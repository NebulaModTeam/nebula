using NebulaModel.DataStructures;
using NebulaModel.Logger;
using NebulaModel.Packets.Logistics;
using System;
using UnityEngine;

namespace NebulaWorld.Logistics
{
    public class ILSShipManager
    {
        public readonly ToggleSwitch PatchLockILS = new ToggleSwitch();

        // the following 4 are needed to prevent a packet flood when the filter on a belt connected to a PLS/ILS is set.
        public int ItemSlotLastSelectedIndex = 0;
        public int ItemSlotLastSlotId = 0;
        public int ItemSlotStationId = 0;
        public int ItemSlotStationGId = 0;

        public ILSShipManager()
        {
        }

        public void Dispose()
        {
        }

        /*
         * When the host notifies the client that a ship started its travel client needs to check if he got both ILS in his gStationPool
         * if not we create a fake entry (which gets updated to the full one when client arrives that planet) and also request the stations dock position
         */
        public void IdleShipGetToWork(ILSShipData packet)
        {
            PlanetData planetA = GameMain.galaxy.PlanetById(packet.planetA);
            PlanetData planetB = GameMain.galaxy.PlanetById(packet.planetB);

            if (planetA != null && planetB != null)
            {
                if (GameMain.data.galacticTransport.stationCapacity <= packet.planetAStationGID)
                {
                    CreateFakeStationComponent(packet.planetAStationGID, packet.planetA, packet.planetAStationMaxShipCount);
                }
                else if (GameMain.data.galacticTransport.stationPool[packet.planetAStationGID] == null)
                {
                    CreateFakeStationComponent(packet.planetAStationGID, packet.planetA, packet.planetAStationMaxShipCount);
                }
                else if (GameMain.data.galacticTransport.stationPool[packet.planetAStationGID].shipDockPos == Vector3.zero)
                {
                    RequestgStationDockPos(packet.planetAStationGID);
                }
                if (GameMain.data.galacticTransport.stationCapacity <= packet.planetBStationGID)
                {
                    CreateFakeStationComponent(packet.planetBStationGID, packet.planetB, packet.planetBStationMaxShipCount);
                }
                else if (GameMain.data.galacticTransport.stationPool[packet.planetBStationGID] == null)
                {
                    CreateFakeStationComponent(packet.planetBStationGID, packet.planetB, packet.planetBStationMaxShipCount);
                }
                else if (GameMain.data.galacticTransport.stationPool[packet.planetBStationGID].shipDockPos == Vector3.zero)
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
                if (stationComponent.idleShipCount < 0)
                {
                    stationComponent.idleShipCount = 0;
                }
                if (stationComponent.workShipCount > packet.planetAStationMaxShipCount)
                {
                    stationComponent.workShipCount = packet.planetAStationMaxShipCount;
                }
                stationComponent.IdleShipGetToWork(packet.origShipIndex);

                StationComponent otherStationComponent = GameMain.data.galacticTransport.stationPool[packet.planetBStationGID];
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
        }

        /*
         * this is also triggered by server and called once a ship lands back to the dock station
         */
        public void WorkShipBackToIdle(ILSShipData packet)
        {
            if (!Multiplayer.IsActive || Multiplayer.Session.LocalPlayer.IsHost)
            {
                return;
            }

            if (GameMain.data.galacticTransport.stationCapacity <= packet.planetAStationGID)
            {
                CreateFakeStationComponent(packet.planetAStationGID, packet.planetA, packet.planetAStationMaxShipCount);
            }
            else if (GameMain.data.galacticTransport.stationPool[packet.planetAStationGID] == null)
            {
                CreateFakeStationComponent(packet.planetAStationGID, packet.planetA, packet.planetBStationMaxShipCount);
            }
            else if (GameMain.data.galacticTransport.stationPool[packet.planetAStationGID].shipDockPos == Vector3.zero)
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
            if (stationComponent.workShipCount > packet.planetAStationMaxShipCount)
            {
                stationComponent.workShipCount = packet.planetAStationMaxShipCount;
            }
            stationComponent.WorkShipBackToIdle(packet.origShipIndex);
        }

        /*
         * Create an entry in the gStationPool with minimal info for ships to travel and render correctly.
         * The information is needed in StationComponent.InternalTickRemote(), but we use a reverse patched version of that
         * which is stripped down to the ship movement and rendering part.
         */
        public void CreateFakeStationComponent(int GId, int planetId, int maxShipCount, bool computeDisk = true)
        {
            // it may be needed to make additional room for the new ILS
            while (GameMain.data.galacticTransport.stationCapacity <= GId)
            {
                GameMain.data.galacticTransport.SetStationCapacity(GameMain.data.galacticTransport.stationCapacity * 2);
            }


            GameMain.data.galacticTransport.stationPool[GId] = new StationComponent();
            StationComponent stationComponent = GameMain.data.galacticTransport.stationPool[GId];
            stationComponent.isStellar = true;
            stationComponent.gid = GId;
            stationComponent.planetId = planetId;
            stationComponent.workShipDatas = new ShipData[maxShipCount];
            stationComponent.shipRenderers = new ShipRenderingData[maxShipCount];
            stationComponent.shipUIRenderers = new ShipUIRenderingData[maxShipCount];
            stationComponent.workShipCount = 0;
            stationComponent.idleShipCount = 0;
            stationComponent.shipDockPos = Vector3.zero; //gets updated later by server packet
            stationComponent.shipDockRot = Quaternion.identity; // gets updated later by server packet
            if (computeDisk)
            {
                stationComponent.shipDiskPos = new Vector3[maxShipCount];
                stationComponent.shipDiskRot = new Quaternion[maxShipCount];

                for (int i = 0; i < maxShipCount; i++)
                {
                    stationComponent.shipDiskRot[i] = Quaternion.Euler(0f, 360f / maxShipCount * i, 0f);
                    stationComponent.shipDiskPos[i] = stationComponent.shipDiskRot[i] * new Vector3(0f, 0f, 11.5f);
                }
                for (int j = 0; j < maxShipCount; j++)
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
        private void RequestgStationDockPos(int GId)
        {
            Multiplayer.Session.Network.SendPacket(new ILSRequestShipDock(GId));
        }

        /*
         * Update the items that are currently in transfer by ships
         */
        public void UpdateRemoteOrder(ILSRemoteOrderData packet)
        {
            if (!Multiplayer.IsActive || Multiplayer.Session.LocalPlayer.IsHost)
            {
                return;
            }
            foreach (StationComponent stationComponent in GameMain.data.galacticTransport.stationPool)
            {
                if (stationComponent != null && stationComponent.gid == packet.stationGID)
                {
                    if (stationComponent.storage == null || packet.storageIndex >= stationComponent.storage.Length)
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
        public void AddTakeItem(ILSShipItems packet)
        {
            if (!Multiplayer.IsActive || Multiplayer.Session.LocalPlayer.IsHost || GameMain.data.galacticTransport.stationPool.Length <= packet.stationGID)
            {
                return;
            }

            StationComponent stationComponent = GameMain.data.galacticTransport.stationPool[packet.stationGID];
            if (stationComponent != null && stationComponent.gid == packet.stationGID && stationComponent.storage != null)
            {
                if (packet.addItem)
                {
                    stationComponent.AddItem(packet.itemId, packet.itemCount);
                    for (int i = 0; i < stationComponent.storage.Length; i++)
                    {
                        if (stationComponent.storage[i].itemId == packet.itemId)
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
        private void RefreshValuesUI(StationComponent stationComponent, int storageIndex)
        {
            UIStationWindow stationWindow = UIRoot.instance.uiGame.stationWindow;
            if (stationWindow != null && stationWindow._stationId == stationComponent.id)
            {
                UIStationStorage[] stationStorageUI = stationWindow.storageUIs;
                if (stationStorageUI != null && stationStorageUI.Length > storageIndex)
                {
                    stationStorageUI[storageIndex].RefreshValues();
                }
            }
        }

        public void UpdateSlotData(ILSUpdateSlotData packet)
        {
            Log.Info($"Updating slot data for planet {packet.PlanetId}, station {packet.StationId} gid {packet.StationGId}. Index {packet.Index}, storageIdx {packet.StorageIdx}");

            // Clients only care about what happens on their planet, hosts always need to apply this.
            // Using PlanetFactory to prevent getting the "fakes" that are creates on clients.
            if (Multiplayer.Session.LocalPlayer.IsHost || (!Multiplayer.Session.LocalPlayer.IsHost && packet.PlanetId == GameMain.localPlanet?.id))
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
