#region

using NebulaAPI.Packets;
using NebulaModel.Logger;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Logistics;

#endregion

/*
 * When a client opens a stations UI we sync the complete state of settings and storage.
 * After that he will receive live updates while the UI is opened.
 */
namespace NebulaNetwork.PacketProcessors.Logistics;

[RegisterPacketProcessor]
public class StationUIInitialSyncRequestProcessor : PacketProcessor<StationUIInitialSyncRequest>
{
    protected override void ProcessPacket(StationUIInitialSyncRequest packet, NebulaConnection conn)
    {
        if (IsClient)
        {
            return;
        }

        var stationPool = GameMain.data.galaxy?.PlanetById(packet.PlanetId)?.factory?.transport?.stationPool;

        var stationComponent = stationPool?[packet.StationId];

        if (stationComponent == null)
        {
            Log.Warn(
                $"StationUIInitialSyncRequestProcessor: Unable to find requested station on planet {packet.PlanetId} with id {packet.StationId} and gid {packet.StationGId}");
            return;
        }

        var storage = stationComponent.storage;

        var itemId = new int[storage.Length];
        var itemCountMax = new int[storage.Length];
        var itemCount = new int[storage.Length];
        var itemInc = new int[storage.Length];
        var localLogic = new int[storage.Length];
        var remoteLogic = new int[storage.Length];
        var remoteOrder = new int[storage.Length];

        for (var i = 0; i < stationComponent.storage.Length; i++)
        {
            itemId[i] = storage[i].itemId;
            itemCountMax[i] = storage[i].max;
            itemCount[i] = storage[i].count;
            itemInc[i] = storage[i].inc;
            localLogic[i] = (int)storage[i].localLogic;
            remoteLogic[i] = (int)storage[i].remoteLogic;
            remoteOrder[i] = storage[i].remoteOrder;
        }

        conn.SendPacket(new StationUIInitialSync(
            packet.PlanetId,
            packet.StationId,
            packet.StationGId,
            stationComponent.tripRangeDrones,
            stationComponent.tripRangeShips,
            stationComponent.deliveryDrones,
            stationComponent.deliveryShips,
            stationComponent.warpEnableDist,
            stationComponent.warperNecessary,
            stationComponent.includeOrbitCollector,
            stationComponent.energy,
            stationComponent.energyPerTick,
            stationComponent.pilerCount,
            itemId,
            itemCountMax,
            itemCount,
            itemInc,
            localLogic,
            remoteLogic,
            remoteOrder
        ));
    }
}
