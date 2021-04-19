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
            StationComponent[] gStationPool = GameMain.data.galacticTransport.stationPool;
            if(gStationPool.Length > packet.stationGId && StationUIManager.UIIsSyncedStage == 1)
            {
                StationComponent stationComponent = gStationPool[packet.stationGId];
                if(stationComponent != null)
                {
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
                    for(int i = 0; i < packet.itemId.Length; i++)
                    {
                        stationComponent.storage[i].itemId = packet.itemId[i];
                        stationComponent.storage[i].max = packet.itemCountMax[i];
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
