#region

using NebulaAPI.Packets;
using NebulaModel.Logger;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Factory.Belt;

#endregion

namespace NebulaNetwork.PacketProcessors.Factory.Belt;

[RegisterPacketProcessor]
internal class BeltUpdatePickupItemsProcessor : PacketProcessor<BeltUpdatePickupItemsPacket>
{
    protected override void ProcessPacket(BeltUpdatePickupItemsPacket packet, NebulaConnection conn)
    {
        var traffic = GameMain.galaxy.PlanetById(packet.PlanetId)?.factory?.cargoTraffic;
        if (traffic == null)
        {
            return;
        }
        //Iterate though belt updates and remove target items
        foreach (var t in packet.BeltUpdates)
        {
            if (t.BeltId >= traffic.beltPool.Length)
            {
                return;
            }
            var beltComponent = traffic.beltPool[t.BeltId];
            var cargoPath = traffic.GetCargoPath(beltComponent.segPathId);
            var itemId = t.ItemId;
            //Check if belt exists
            if (cargoPath == null)
            {
                continue;
            }
            // Search downstream for target item
            for (var k = beltComponent.segIndex + beltComponent.segPivotOffset;
                 k <= beltComponent.segIndex + beltComponent.segLength - 1;
                 k++)
            {
                if (cargoPath.TryPickItem(k - 4 - 1, 12, itemId, out _, out _) != 0)
                {
                    return;
                }
            }
            // Search upstream for target item
            for (var k = beltComponent.segIndex + beltComponent.segPivotOffset - 1; k >= beltComponent.segIndex; k--)
            {
                if (cargoPath.TryPickItem(k - 4 - 1, 12, itemId, out _, out _) != 0)
                {
                    return;
                }
            }
            Log.Warn(
                $"BeltUpdatePickupItem: Cannot pick item{itemId} on belt{t.BeltId}, planet{packet.PlanetId}");
        }
    }
}
