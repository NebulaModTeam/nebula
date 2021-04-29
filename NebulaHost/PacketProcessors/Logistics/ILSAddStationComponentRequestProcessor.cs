using NebulaModel.Attributes;
using NebulaModel.Networking;
using NebulaModel.Packets.Logistics;
using NebulaModel.Packets.Processors;
using NebulaModel.DataStructures;

/*
 * if clients build an ILS they request the GId of it, so we need to look it up in the hosts gStationPool
 */
namespace NebulaHost.PacketProcessors.Logistics
{
    [RegisterPacketProcessor]
    public class ILSAddStationComponentRequestProcessor: IPacketProcessor<ILSAddStationComponentRequest>
    {
        public void ProcessPacket(ILSAddStationComponentRequest packet, NebulaConnection conn)
        {
            StationComponent[] gStationPool = GameMain.data.galacticTransport.stationPool;
            foreach(StationComponent stationComponent in gStationPool)
            {
                if(stationComponent != null && stationComponent.planetId == packet.planetId)
                {
                    if(stationComponent.shipDockPos == DataStructureExtensions.ToVector3(packet.shipDockPos))
                    {
                        conn.SendPacket(new ILSAddStationComponentResponse(stationComponent.gid, packet.planetId, stationComponent.shipDockPos));
                        return;
                    }
                }
            }
        }
    }
}
