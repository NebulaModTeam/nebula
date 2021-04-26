using NebulaModel.Attributes;
using NebulaModel.Networking;
using NebulaModel.Packets.Logistics;
using NebulaModel.Packets.Processors;
using NebulaWorld.Logistics;
using HarmonyLib;
using UnityEngine;

namespace NebulaClient.PacketProcessors.Logistics
{
    [RegisterPacketProcessor]
    public class StationUIInitialSyncProcessor: IPacketProcessor<StationUIInitialSync>
    {
        public void ProcessPacket(StationUIInitialSync packet, NebulaConnection conn)
        {
            Debug.Log("received sync packet");
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
            Debug.Log("selected");
            if(gStationPool != null && gStationPool.Length > packet.stationGId && StationUIManager.UIIsSyncedStage == 1)
            {
                StationComponent stationComponent = gStationPool[packet.stationGId];
                if(stationComponent != null)
                {
                    Debug.Log("updating");
                    UIStationWindow stationWindow = UIRoot.instance.uiGame.stationWindow;
                    if (stationWindow != null && stationWindow.active)
                    {
                        // here it was
                    }
                    stationComponent.tripRangeDrones = packet.tripRangeDrones;
                    stationComponent.tripRangeShips = packet.tripRangeShips;
                    stationComponent.deliveryDrones = packet.deliveryDrones;
                    stationComponent.deliveryShips = packet.deliveryShips;
                    stationComponent.warpEnableDist = packet.warpEnableDist;
                    stationComponent.warperNecessary = packet.warperNecessary;
                    stationComponent.includeOrbitCollector = packet.includeOrbitCollector;
                    stationComponent.energy = packet.energy;
                    stationComponent.energyPerTick = packet.energyPerTick;
                    Debug.Log("before loop");
                    for(int i = 0; i < packet.itemId.Length; i++)
                    {
                        Debug.Log((stationComponent.storage == null) ? "null" : "not null");
                        if(stationComponent.storage == null)
                        {
                            // 3 is games default storage places for PLS
                            stationComponent.storage = new StationStore[packet.itemId.Length];
                        }
                        stationComponent.storage[i].itemId = packet.itemId[i];
                        stationComponent.storage[i].max = packet.itemCountMax[i];
                        stationComponent.storage[i].count = packet.itemCount[i];
                        Debug.Log("before remote");
                        stationComponent.storage[i].remoteOrder = packet.remoteOrder[i];
                        Debug.Log("done first");
                        switch (packet.localLogic[i])
                        {
                            case 0:
                                stationComponent.storage[i].localLogic = ELogisticStorage.None;
                                break;
                            case 1:
                                stationComponent.storage[i].localLogic = ELogisticStorage.Supply;
                                break;
                            case 2:
                                stationComponent.storage[i].localLogic = ELogisticStorage.Demand;
                                break;
                        }
                        Debug.Log("done med");
                        switch (packet.remoteLogic[i])
                        {
                            case 0:
                                stationComponent.storage[i].remoteLogic = ELogisticStorage.None;
                                break;
                            case 1:
                                stationComponent.storage[i].remoteLogic = ELogisticStorage.Supply;
                                break;
                            case 2:
                                stationComponent.storage[i].remoteLogic = ELogisticStorage.Demand;
                                break;
                        }
                        Debug.Log("done last");
                    }
                    Debug.Log("after loop");
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
                    Debug.Log("last thing");
                    StationUIManager.UIStationId = stationComponent.id;
                }
            }
        }
    }
}
