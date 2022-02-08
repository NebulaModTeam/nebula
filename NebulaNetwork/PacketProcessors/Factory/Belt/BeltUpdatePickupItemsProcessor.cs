using NebulaAPI;
using NebulaModel.Logger;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Belt;

namespace NebulaNetwork.PacketProcessors.Factory.Belt
{
    [RegisterPacketProcessor]
    internal class BeltUpdatePickupItemsProcessor : PacketProcessor<BeltUpdatePickupItemsPacket>
    {
        public override void ProcessPacket(BeltUpdatePickupItemsPacket packet, NebulaConnection conn)
        {
            CargoTraffic traffic = GameMain.galaxy.PlanetById(packet.PlanetId)?.factory?.cargoTraffic;
            if (traffic != null)
            {
                //Iterate though belt updates and remove target items
                for (int i = 0; i < packet.BeltUpdates.Length; i++)
                {
                    if (packet.BeltUpdates[i].BeltId >= traffic.beltPool.Length)
                    {
                        return;
                    }
                    BeltComponent beltComponent = traffic.beltPool[packet.BeltUpdates[i].BeltId];
                    CargoPath cargoPath = traffic.GetCargoPath(beltComponent.segPathId);
                    int ItemId = packet.BeltUpdates[i].ItemId;
                    //Check if belt exists
                    if (cargoPath != null)
                    {
                        // Search downstream for target item
                        for (int k = beltComponent.segIndex + beltComponent.segPivotOffset; k <= (beltComponent.segIndex + beltComponent.segLength - 1); k++)
                        {
                            if (cargoPath.TryPickItem(k - 4 - 1, 12, ItemId, out _, out _) != 0)
                            {
                                return;
                            }
                        }
                        // Search upstream for target item
                        for (int k = beltComponent.segIndex + beltComponent.segPivotOffset - 1; k >= beltComponent.segIndex; k--)
                        {
                            if (cargoPath.TryPickItem(k - 4 - 1, 12, ItemId, out _, out _) != 0)
                            {
                                return;
                            }
                        }
                        Log.Warn($"BeltUpdatePickupItem: Cannot pick item{ItemId} on belt{packet.BeltUpdates[i].BeltId}, planet{packet.PlanetId}");
                    }
                }
            }
        }
    }
}
