using NebulaModel.Attributes;
using NebulaModel.Networking;
using NebulaModel.Packets.Logistics;
using NebulaModel.Packets.Processors;

/*
 * host ntifies clients when a ship uses a warper to enter warp state
 */
namespace NebulaClient.PacketProcessors.Logistics
{
    [RegisterPacketProcessor]
    class ILSShipUpdateWarperCntProcessor: IPacketProcessor<ILSShipUpdateWarperCnt>
    {
        public void ProcessPacket(ILSShipUpdateWarperCnt packet, NebulaConnection conn)
        {
            StationComponent[] gStationPool = GameMain.data.galacticTransport.stationPool;
            if(gStationPool.Length > packet.stationGId)
            {
                gStationPool[packet.stationGId].workShipDatas[packet.shipIndex].warperCnt = packet.warperCnt;
            }
        }
    }
}
