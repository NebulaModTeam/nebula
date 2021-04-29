using NebulaModel.Attributes;
using NebulaModel.Networking;
using NebulaModel.Packets.Logistics;
using NebulaModel.Packets.Processors;
using NebulaWorld.Logistics;
using HarmonyLib;

/*
 * When the client opens the UI of a station (ILS/PLS/Collector) the contents gets updated and shown to
 * the player once this packet is received. He will see a loading text before that.
 * This will also subscribe to live updates syncing changes made by other players to the station while the UI is opened
 */
namespace NebulaClient.PacketProcessors.Logistics
{
    [RegisterPacketProcessor]
    public class StationUIInitialSyncProcessor: IPacketProcessor<StationUIInitialSync>
    {
        public void ProcessPacket(StationUIInitialSync packet, NebulaConnection conn)
        {
            StationComponent[] gStationPool = null;
            if(packet.planetId == 0)
            {
                gStationPool = GameMain.data.galacticTransport.stationPool;
            }
            else
            {
                PlanetData pData = GameMain.galaxy.PlanetById(packet.planetId);
                if(pData?.factory?.transport != null)
                {
                    gStationPool = pData.factory.transport.stationPool;
                }
            }
            if(gStationPool != null && gStationPool.Length > packet.stationGId && StationUIManager.UIIsSyncedStage == 1)
            {
                StationComponent stationComponent = gStationPool[packet.stationGId];
                if(stationComponent != null)
                {
                    UIStationWindow stationWindow = UIRoot.instance.uiGame.stationWindow;

                    stationComponent.tripRangeDrones = packet.tripRangeDrones;
                    stationComponent.tripRangeShips = packet.tripRangeShips;
                    stationComponent.deliveryDrones = packet.deliveryDrones;
                    stationComponent.deliveryShips = packet.deliveryShips;
                    stationComponent.warpEnableDist = packet.warpEnableDist;
                    stationComponent.warperNecessary = packet.warperNecessary;
                    stationComponent.includeOrbitCollector = packet.includeOrbitCollector;
                    stationComponent.energy = packet.energy;
                    stationComponent.energyPerTick = packet.energyPerTick;
                    for(int i = 0; i < packet.itemId.Length; i++)
                    {
                        if(stationComponent.storage == null)
                        {
                            // 3 is games default storage places for PLS
                            stationComponent.storage = new StationStore[packet.itemId.Length];
                        }
                        stationComponent.storage[i].itemId = packet.itemId[i];
                        stationComponent.storage[i].max = packet.itemCountMax[i];
                        stationComponent.storage[i].count = packet.itemCount[i];
                        stationComponent.storage[i].remoteOrder = packet.remoteOrder[i];
                        stationComponent.storage[i].localLogic = (ELogisticStorage)packet.localLogic[i];
                        stationComponent.storage[i].remoteLogic = (ELogisticStorage)packet.remoteLogic[i];
                    }
                    if(stationWindow != null && stationWindow.active)
                    {
                        conn.SendPacket(new StationSubscribeUIUpdates(true, stationComponent.gid));
                        StationUIManager.UIIsSyncedStage++;
                        stationWindow._Free();
                        stationWindow._Init(stationComponent);
                        AccessTools.Field(typeof(UIStationWindow), "_stationId").SetValue(stationWindow, stationComponent.id);
                        stationWindow._Open();
                        stationWindow._Update();
                    }
                    StationUIManager.UIStationId = stationComponent.id;
                }
            }
        }
    }
}
