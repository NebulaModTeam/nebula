#region

using NebulaAPI.Packets;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Logistics;

#endregion

/*
 * This packet updates the ships direction and is used when StationComponent.RematchRemotePairs() is called
 * This is used when a station is added or removed or a supply/demand chain is changed
 */
namespace NebulaNetwork.PacketProcessors.Logistics;

[RegisterPacketProcessor]
internal class ILSRematchRemotePairsProcessor : PacketProcessor<ILSRematchRemotePairs>
{
    protected override void ProcessPacket(ILSRematchRemotePairs packet, NebulaConnection conn)
    {
        var gTransport = GameMain.data.galacticTransport;
        if (packet.GId >= gTransport.stationCursor)
        {
            return;
        }
        for (var i = 0; i < packet.ShipIndex.Length; i++)
        {
            if (gTransport.stationPool[packet.GId] == null)
            {
                continue;
            }
            gTransport.stationPool[packet.GId].workShipDatas[packet.ShipIndex[i]].otherGId = packet.OtherGId[i];
            gTransport.stationPool[packet.GId].workShipDatas[packet.ShipIndex[i]].direction = packet.Direction[i];
            gTransport.stationPool[packet.GId].workShipDatas[packet.ShipIndex[i]].itemId = packet.ItemId[i];
        }
    }
}
