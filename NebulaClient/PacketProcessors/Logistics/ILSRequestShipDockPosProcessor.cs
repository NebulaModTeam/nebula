using NebulaModel.Attributes;
using NebulaModel.Networking;
using NebulaModel.Packets.Logistics;
using NebulaModel.Packets.Processors;
using HarmonyLib;
using UnityEngine;

namespace NebulaClient.PacketProcessors.Logistics
{
    [RegisterPacketProcessor]
    class ILSRequestShipDockPosProcessor: IPacketProcessor<ILSRequestShipDockPos>
    {
        public void ProcessPacket(ILSRequestShipDockPos packet, NebulaConnection conn)
        {
            while(GameMain.data.galacticTransport.stationCapacity <= packet.stationGId)
            {
                object[] args = new object[1];
                args[0] = GameMain.data.galacticTransport.stationCapacity * 2;
                AccessTools.Method(typeof(GalacticTransport), "SetStationCapacity").Invoke(GameMain.data.galacticTransport, args);
            }
            if(GameMain.data.galacticTransport.stationPool[packet.stationGId] == null)
            {
                GameMain.data.galacticTransport.stationPool[packet.stationGId] = new StationComponent();
                GameMain.data.galacticTransport.stationPool[packet.stationGId].gid = packet.stationGId;
            }
            GameMain.data.galacticTransport.stationPool[packet.stationGId].shipDockPos.x = packet.shipDockPos.x;
            GameMain.data.galacticTransport.stationPool[packet.stationGId].shipDockPos.y = packet.shipDockPos.y;
            GameMain.data.galacticTransport.stationPool[packet.stationGId].shipDockPos.z = packet.shipDockPos.z;
            // TODO: add other important information
        }
    }
}
