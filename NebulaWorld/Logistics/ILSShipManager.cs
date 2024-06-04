#region

using System;
using NebulaModel.DataStructures;
using NebulaModel.Logger;
using NebulaModel.Packets.Logistics;
using UnityEngine;

#endregion

namespace NebulaWorld.Logistics;

public class ILSShipManager
{
    public readonly ToggleSwitch PatchLockILS = new();

    // the following 4 are needed to prevent a packet flood when the filter on a belt connected to a PLS/ILS is set.
    public int ItemSlotLastSelectedIndex = 0;
    public int ItemSlotLastSlotId = 0;
    public int ItemSlotStationGId = 0;
    public int ItemSlotStationId = 0;

    /*
     * When the host notifies the client that a ship started its travel client needs to check if he got both ILS in his gStationPool
     * if not we create a fake entry (which gets updated to the full one when client arrives that planet) and also request the stations dock position
     */
    public static void IdleShipGetToWork(ILSIdleShipBackToWork packet)
    {
        var planetA = GameMain.galaxy.PlanetById(packet.PlanetA);
        var planetB = GameMain.galaxy.PlanetById(packet.PlanetB);

        if (planetA == null || planetB == null || packet.ThisGId < 1)
        {
            return;
        }
        var stationPool = GameMain.data.galacticTransport.stationPool;
        if (stationPool.Length <= packet.ThisGId)
        {
            CreateFakeStationComponent(packet.ThisGId, packet.PlanetA, packet.StationMaxShipCount);
        }
        else if (stationPool[packet.ThisGId] == null)
        {
            CreateFakeStationComponent(packet.ThisGId, packet.PlanetA, packet.StationMaxShipCount);
        }
        if (stationPool.Length <= packet.OtherGId)
        {
            CreateFakeStationComponent(packet.OtherGId, packet.PlanetB, packet.StationMaxShipCount);
        }
        else if (stationPool[packet.OtherGId] == null)
        {
            CreateFakeStationComponent(packet.OtherGId, packet.PlanetB, packet.StationMaxShipCount);
        }

        var stationComponent = stationPool[packet.ThisGId];
        if (stationComponent == null)
        {
            return; // This shouldn't happen, but guard just in case
        }
        if (stationComponent.idleShipCount <= 0 || stationComponent.workShipCount >= stationComponent.workShipDatas.Length)
        {
            return; // Ship count is outside the range
        }

        stationComponent.workShipDatas[stationComponent.workShipCount].stage = -2;
        stationComponent.workShipDatas[stationComponent.workShipCount].planetA = packet.PlanetA;
        stationComponent.workShipDatas[stationComponent.workShipCount].planetB = packet.PlanetB;
        stationComponent.workShipDatas[stationComponent.workShipCount].otherGId = packet.OtherGId;
        stationComponent.workShipDatas[stationComponent.workShipCount].direction = 1;
        stationComponent.workShipDatas[stationComponent.workShipCount].t = 0f;
        stationComponent.workShipDatas[stationComponent.workShipCount].itemId = packet.ItemId;
        stationComponent.workShipDatas[stationComponent.workShipCount].itemCount = packet.ItemCount;
        stationComponent.workShipDatas[stationComponent.workShipCount].inc = packet.Inc;
        stationComponent.workShipDatas[stationComponent.workShipCount].shipIndex = packet.ShipIndex;
        stationComponent.workShipDatas[stationComponent.workShipCount].warperCnt = packet.ShipWarperCount;
        stationComponent.warperCount = packet.StationWarperCount;

        stationComponent.workShipCount++;
        stationComponent.idleShipCount--;
        stationComponent.IdleShipGetToWork(packet.ShipIndex);

        var shipSailSpeed = GameMain.history.logisticShipSailSpeedModified;
        var shipWarpSpeed = GameMain.history.logisticShipWarpDrive
            ? GameMain.history.logisticShipWarpSpeedModified
            : shipSailSpeed;
        var astroPoses = GameMain.galaxy.astrosData;

        var canWarp = shipWarpSpeed > shipSailSpeed + 1f;
        var trip = (astroPoses[packet.PlanetB].uPos - astroPoses[packet.PlanetA].uPos).magnitude +
                   astroPoses[packet.PlanetB].uRadius + astroPoses[packet.PlanetA].uRadius;
        stationComponent.energy -= stationComponent.CalcTripEnergyCost(trip, shipSailSpeed, canWarp);
    }

    /*
     * this is also triggered by server and called once a ship lands back to the dock station
     */
    public static void WorkShipBackToIdle(ILSWorkShipBackToIdle packet)
    {
        if (packet.GId < 1)
        {
            return;
        }

        var stationPool = GameMain.data.galacticTransport.stationPool;
        if (stationPool.Length <= packet.GId)
        {
            CreateFakeStationComponent(packet.GId, packet.PlanetA, packet.StationMaxShipCount);
        }
        else if (stationPool[packet.GId] == null)
        {
            CreateFakeStationComponent(packet.GId, packet.PlanetA, packet.StationMaxShipCount);
        }

        var stationComponent = stationPool[packet.GId];
        if (stationComponent == null)
        {
            return; // This shouldn't happen, but guard just in case
        }
        if (stationComponent.workShipCount <= 0 || stationComponent.workShipDatas.Length <= packet.WorkShipIndex)
        {
            return; // Ship count is outside the range
        }

        Array.Copy(stationComponent.workShipDatas, packet.WorkShipIndex + 1, stationComponent.workShipDatas,
            packet.WorkShipIndex, stationComponent.workShipDatas.Length - packet.WorkShipIndex - 1);
        stationComponent.workShipCount--;
        stationComponent.idleShipCount++;
        stationComponent.WorkShipBackToIdle(packet.ShipIndex);
        Array.Clear(stationComponent.workShipDatas, stationComponent.workShipCount,
            stationComponent.workShipDatas.Length - stationComponent.workShipCount);
    }

    /*
     * Create an entry in the gStationPool with minimal info for ships to travel and render correctly.
     * The information is needed in StationComponent.InternalTickRemote(), but we use a reverse patched version of that
     * which is stripped down to the ship movement and rendering part.
     */
    public static void CreateFakeStationComponent(int gId, int planetId, int maxShipCount, bool computeDisk = true)
    {
        // it may be needed to make additional room for the new ILS
        while (GameMain.data.galacticTransport.stationPool.Length <= gId)
        {
            GameMain.data.galacticTransport.SetStationCapacity(GameMain.data.galacticTransport.stationPool.Length * 2);
        }


        GameMain.data.galacticTransport.stationPool[gId] = new StationComponent();
        var stationComponent = GameMain.data.galacticTransport.stationPool[gId];
        stationComponent.isStellar = true;
        stationComponent.gid = gId;
        stationComponent.planetId = planetId;
        stationComponent.workShipDatas = new ShipData[maxShipCount];
        stationComponent.workShipOrders = new RemoteLogisticOrder[maxShipCount];
        stationComponent.shipRenderers = new ShipRenderingData[maxShipCount];
        stationComponent.shipUIRenderers = new ShipUIRenderingData[maxShipCount];
        stationComponent.priorityLocks = new StationPriorityLock[6]; // dummy placeholder. the real length should be stationMaxItemKinds
        stationComponent.workShipCount = 0;
        stationComponent.idleShipCount = maxShipCount; // add dummy idle ship count to use in ILSShipManager
        stationComponent.shipDockPos = Vector3.zero; //gets updated later by server packet
        stationComponent.shipDockRot = Quaternion.identity; // gets updated later by server packet
        stationComponent.storage = []; // zero-length array for mod compatibility
        if (computeDisk)
        {
            stationComponent.shipDiskPos = new Vector3[maxShipCount];
            stationComponent.shipDiskRot = new Quaternion[maxShipCount];

            for (var i = 0; i < maxShipCount; i++)
            {
                stationComponent.shipDiskRot[i] = Quaternion.Euler(0f, 360f / maxShipCount * i, 0f);
                stationComponent.shipDiskPos[i] = stationComponent.shipDiskRot[i] * new Vector3(0f, 0f, 11.5f);
            }
            for (var j = 0; j < maxShipCount; j++)
            {
                stationComponent.shipDiskRot[j] = stationComponent.shipDockRot * stationComponent.shipDiskRot[j];
                stationComponent.shipDiskPos[j] = stationComponent.shipDockPos +
                                                  stationComponent.shipDockRot * stationComponent.shipDiskPos[j];
            }

            RequestStationDockPos(gId);
        }

        GameMain.data.galacticTransport.stationCursor = Math.Max(GameMain.data.galacticTransport.stationCursor, gId + 1);
    }

    /*
     * As StationComponent.InternalTickRemote() needs to have the dock position to correctly compute ship movement we request it here from server.
     */
    private static void RequestStationDockPos(int gId)
    {
        Multiplayer.Session.Network.SendPacket(new ILSRequestShipDock(gId));
    }

    // This is triggered by server when InternalTickRemote() calls AddItem() or TakeItem()
    public static void AddTakeItem(ILSShipAddTake packet)
    {
        if (!Multiplayer.IsActive || Multiplayer.Session.LocalPlayer.IsHost ||
            GameMain.data.galacticTransport.stationPool.Length <= packet.StationGID)
        {
            return;
        }

        var stationComponent = GameMain.data.galacticTransport.stationPool[packet.StationGID];
        if (stationComponent == null || stationComponent.gid != packet.StationGID || stationComponent.storage.Length == 0)
        {
            return;
        }
        if (packet.AddItem)
        {
            stationComponent.AddItem(packet.ItemId, packet.ItemCount, packet.Inc);
        }
        else
        {
            var itemId = packet.ItemId;
            var itemCount = packet.ItemCount;
            stationComponent.TakeItem(ref itemId, ref itemCount, out _);
            // we need to update the ShipData here too, luckily our transpiler sends the workShipDatas index in the inc field
            // update: ShipDatas.itemCount only use for rendering color, so we let clients handle it
        }
    }

    // is triggered by server when InternalTickRemote() updates the StationComponent.storage array
    public static void UpdateStorage(ILSUpdateStorage packet)
    {
        if (!Multiplayer.IsActive || Multiplayer.Session.LocalPlayer.IsHost ||
            GameMain.data.galacticTransport.stationPool.Length <= packet.GId)
        {
            return;
        }

        var stationComponent = GameMain.data.galacticTransport.stationPool[packet.GId];
        if (stationComponent == null || stationComponent.gid != packet.GId || stationComponent.storage.Length == 0)
        {
            return;
        }
        var obj = stationComponent.storage;
        lock (obj)
        {
            stationComponent.storage[packet.Index].count = packet.Count;
            stationComponent.storage[packet.Index].inc = packet.Inc;
        }
    }

    public static void UpdateSlotData(ILSUpdateSlotData packet)
    {
        var factory = GameMain.galaxy.PlanetById(packet.PlanetId)?.factory;
        StationComponent stationComponent = null;

        if (factory != null) // only update station slot for loaded factories
        {
            stationComponent = factory.transport.stationPool[packet.StationId];
        }

        if (stationComponent?.slots == null)
        {
            return;
        }
        stationComponent.slots[packet.Index].storageIdx = packet.StorageIdx;
        if (stationComponent.gid != packet.StationGId)
        {
            Log.Warn($"Station gid mismatch! local:{stationComponent.gid} remote:{packet.StationGId}");
        }
    }
}
