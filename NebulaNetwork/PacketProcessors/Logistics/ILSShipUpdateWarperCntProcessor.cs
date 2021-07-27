using NebulaModel.Attributes;
using NebulaModel.Networking;
using NebulaModel.Packets.Logistics;
using NebulaModel.Packets;

/*
 * host ntifies clients when a ship uses a warper to enter warp state
 */
namespace NebulaNetwork.PacketProcessors.Logistics
{
    [RegisterPacketProcessor]
    class ILSShipUpdateWarperCntProcessor: PacketProcessor<ILSShipUpdateWarperCnt>
    {
        public override void ProcessPacket(ILSShipUpdateWarperCnt packet, NebulaConnection conn)
        {
            StationComponent[] gStationPool = GameMain.data.galacticTransport.stationPool;
            if(gStationPool.Length > packet.stationGId)
            {
                gStationPool[packet.stationGId].workShipDatas[packet.shipIndex].warperCnt = packet.warperCnt;
            }
        }
    }
}
