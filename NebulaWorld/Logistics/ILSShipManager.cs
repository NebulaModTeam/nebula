using NebulaModel.DataStructures;
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
            PlanetData planetA = GameMain.galaxy.PlanetById(packet.PlanetA);
            PlanetData planetB = GameMain.galaxy.PlanetById(packet.PlanetB);

            if (planetA != null && planetB != null)
            {
                if (GameMain.data.galacticTransport.stationCapacity <= packet.ThisGId)
                {
                    CreateFakeStationComponent(packet.ThisGId, packet.PlanetA, packet.StationMaxShipCount);
                }
                else if (GameMain.data.galacticTransport.stationPool[packet.ThisGId] == null)
                {
                    CreateFakeStationComponent(packet.ThisGId, packet.PlanetA, packet.StationMaxShipCount);
                }
                else if (GameMain.data.galacticTransport.stationPool[packet.ThisGId].shipDockPos == Vector3.zero)
                {
                    RequestgStationDockPos(packet.ThisGId);
                }
                if (GameMain.data.galacticTransport.stationCapacity <= packet.OtherGId)
                {
                    CreateFakeStationComponent(packet.OtherGId, packet.PlanetB, packet.StationMaxShipCount);
                }
                else if (GameMain.data.galacticTransport.stationPool[packet.OtherGId] == null)
                {
                    CreateFakeStationComponent(packet.OtherGId, packet.PlanetB, packet.StationMaxShipCount);
                }
                else if (GameMain.data.galacticTransport.stationPool[packet.OtherGId].shipDockPos == Vector3.zero)
                {
                    RequestgStationDockPos(packet.OtherGId);
                }

                StationComponent stationComponent = GameMain.data.galacticTransport.stationPool[packet.ThisGId];

                stationComponent.workShipDatas[stationComponent.workShipCount].stage = -2;
                stationComponent.workShipDatas[stationComponent.workShipCount].planetA = packet.PlanetA;
                stationComponent.workShipDatas[stationComponent.workShipCount].planetB = packet.PlanetB;
                stationComponent.workShipDatas[stationComponent.workShipCount].otherGId = packet.OtherGId;
                stationComponent.workShipDatas[stationComponent.workShipCount].direction = 1;
                stationComponent.workShipDatas[stationComponent.workShipCount].t = 0f;
                stationComponent.workShipDatas[stationComponent.workShipCount].itemId = packet.ItemId;
                stationComponent.workShipDatas[stationComponent.workShipCount].itemCount = packet.ItemCount;
                stationComponent.workShipDatas[stationComponent.workShipCount].inc = packet.Inc;
                stationComponent.workShipDatas[stationComponent.workShipCount].gene = packet.Gene;
                stationComponent.workShipDatas[stationComponent.workShipCount].shipIndex = packet.ShipIndex;
                stationComponent.workShipDatas[stationComponent.workShipCount].warperCnt = packet.ShipWarperCount;
                stationComponent.warperCount = packet.StationWarperCount;

                stationComponent.workShipCount++;
                stationComponent.idleShipCount--;
                stationComponent.IdleShipGetToWork(packet.ShipIndex);

                float shipSailSpeed = GameMain.history.logisticShipSailSpeedModified;
                float shipWarpSpeed = (GameMain.history.logisticShipWarpDrive ? GameMain.history.logisticShipWarpSpeedModified : shipSailSpeed);
                AstroPose[] astroPoses = GameMain.galaxy.astroPoses;

                bool canWarp = shipWarpSpeed > shipSailSpeed + 1f;
                double trip = (astroPoses[packet.PlanetB].uPos - astroPoses[packet.PlanetA].uPos).magnitude + astroPoses[packet.PlanetB].uRadius + astroPoses[packet.PlanetA].uRadius;
                stationComponent.energy -= stationComponent.CalcTripEnergyCost(trip, shipSailSpeed, canWarp);
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

            if (GameMain.data.galacticTransport.stationCapacity <= packet.ThisGId)
            {
                CreateFakeStationComponent(packet.ThisGId, packet.PlanetA, packet.StationMaxShipCount);
            }
            else if (GameMain.data.galacticTransport.stationPool[packet.ThisGId] == null)
            {
                CreateFakeStationComponent(packet.ThisGId, packet.PlanetA, packet.StationMaxShipCount);
            }
            else if (GameMain.data.galacticTransport.stationPool[packet.ThisGId].shipDockPos == Vector3.zero)
            {
                RequestgStationDockPos(packet.ThisGId);
            }

            StationComponent stationComponent = GameMain.data.galacticTransport.stationPool[packet.ThisGId];

            Array.Copy(stationComponent.workShipDatas, packet.WorkShipIndex + 1, stationComponent.workShipDatas, packet.WorkShipIndex, stationComponent.workShipDatas.Length - packet.WorkShipIndex - 1);
            stationComponent.workShipCount--;
            stationComponent.idleShipCount++;
            stationComponent.WorkShipBackToIdle(packet.ShipIndex);
            Array.Clear(stationComponent.workShipDatas, stationComponent.workShipCount, stationComponent.workShipDatas.Length - stationComponent.workShipCount);
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
            stationComponent.workShipOrders = new RemoteLogisticOrder[maxShipCount];
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
         * As StationComponent.InternalTickRemote() needs to have the dock position to correctly compute ship movement we request it here from server.
         */
        private void RequestgStationDockPos(int GId)
        {
            Multiplayer.Session.Network.SendPacket(new ILSRequestShipDock(GId));
        }

        // This is triggered by server when InternalTickRemote() calls AddItem() or TakeItem()
        public void AddTakeItem(ILSShipAddTake packet)
        {
            if (!Multiplayer.IsActive || Multiplayer.Session.LocalPlayer.IsHost || GameMain.data.galacticTransport.stationPool.Length <= packet.StationGID)
            {
                return;
            }

            StationComponent stationComponent = GameMain.data.galacticTransport.stationPool[packet.StationGID];
            if (stationComponent != null && stationComponent.gid == packet.StationGID && stationComponent.storage != null)
            {
                if (packet.AddItem)
                {
                    stationComponent.AddItem(packet.ItemId, packet.ItemCount, packet.Inc);
                }
                else
                {
                    int itemId = packet.ItemId;
                    int itemCount = packet.ItemCount;
                    int Inc;
                    stationComponent.TakeItem(ref itemId, ref itemCount, out Inc);
                    // we need to update the ShipData here too, luckily our transpiler sends the workShipDatas index in the inc field
                    stationComponent.workShipDatas[packet.Inc].itemCount = itemCount;
                    stationComponent.workShipDatas[packet.Inc].inc = Inc;
                }
            }
        }

        // is triggered by server when InternalTickRemote() updates the StationComponent.storage array
        public void UpdateStorage(ILSUpdateStorage packet)
        {
            if (!Multiplayer.IsActive || Multiplayer.Session.LocalPlayer.IsHost || GameMain.data.galacticTransport.stationPool.Length <= packet.GId)
            {
                return;
            }

            StationComponent stationComponent = GameMain.data.galacticTransport.stationPool[packet.GId];
            if(stationComponent != null && stationComponent.gid == packet.GId && stationComponent.storage != null)
            {
                StationStore[] obj = stationComponent.storage;
                lock (obj)
                {
                    stationComponent.storage[packet.Index].count = packet.Count;
                    stationComponent.storage[packet.Index].inc = packet.Inc;
                }
            }
        }

        public void UpdateSlotData(ILSUpdateSlotData packet)
        {
            PlanetData pData = null;
            StationComponent stationComponent = null;

            if (packet.StationGId == 0) // PLS
            {
                pData = GameMain.galaxy.PlanetById(packet.PlanetId);
                stationComponent = pData?.factory?.transport?.stationPool[packet.StationId];
            }
            else // ILS
            {
                if (packet.StationGId < GameMain.data.galacticTransport.stationPool.Length)
                {
                    stationComponent = GameMain.data.galacticTransport.stationPool[packet.StationGId];
                }
            }

            if (stationComponent?.slots != null)
            {
                stationComponent.slots[packet.Index].storageIdx = packet.StorageIdx;
            }
        }
    }
}
