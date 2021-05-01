using NebulaModel.Attributes;
using NebulaModel.Networking;
using NebulaModel.Packets.Logistics;
using NebulaModel.Packets.Processors;

/*
 * This packet updates the ships direction and is used when StationComponent.RematchRemotePairs() is called
 * This is used when a station is added or removed or a supply/demand chain is changed
 */
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
                    /*
                     * fix for #261
                     * RematchRemotePairs() goes through all GId's on the server, but client may not have a fake/true entry for each one of them
                     * for example if someone places an ILS but never uses it, then other players (except host) would not have an entry for them
                     * in their gStationPool. So skip those
                     */
                    if (gTransport.stationPool[packet.stationGId] != null)
                    {
                        gTransport.stationPool[packet.stationGId].workShipDatas[i].shipIndex = packet.shipIndex[i];
                        gTransport.stationPool[packet.stationGId].workShipDatas[i].otherGId = packet.otherGId[i];
                        gTransport.stationPool[packet.stationGId].workShipDatas[i].direction = packet.direction[i];
                        gTransport.stationPool[packet.stationGId].workShipDatas[i].itemId = packet.itemId[i];
                    }
                }
            }
        }
    }
}
