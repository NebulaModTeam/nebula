using NebulaModel.Attributes;
using NebulaModel.Networking;
using NebulaModel.Packets.Logistics;
using NebulaModel.Packets.Processors;
using UnityEngine;

namespace NebulaClient.PacketProcessors.Logistics
{
    [RegisterPacketProcessor]
    class ILSShipDataUpdateProcessor : IPacketProcessor<ILSShipDataUpdate>
    {
        public void ProcessPacket(ILSShipDataUpdate packet, NebulaConnection conn)
        {
            GalacticTransport gTransport = GameMain.data.galacticTransport;
            if(packet.stationGId < gTransport.stationCursor)
            {
                for(int i = 0; i < packet.shipIndex.Length; i++)
                {
                    gTransport.stationPool[packet.stationGId].workShipDatas[i].shipIndex = packet.shipIndex[i];
                    gTransport.stationPool[packet.stationGId].workShipDatas[i].otherGId = packet.otherGId[i];
                    gTransport.stationPool[packet.stationGId].workShipDatas[i].direction = packet.direction[i];
                    gTransport.stationPool[packet.stationGId].workShipDatas[i].itemId = packet.itemId[i];
                }
                Debug.Log("UPDATED");
            }
        }
    }
}
