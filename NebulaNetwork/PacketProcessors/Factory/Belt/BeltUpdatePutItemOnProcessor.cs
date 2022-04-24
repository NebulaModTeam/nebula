using NebulaAPI;
using NebulaModel.Logger;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Factory.Belt;
using NebulaWorld;

namespace NebulaNetwork.PacketProcessors.Factory.Belt
{
    [RegisterPacketProcessor]
    internal class BeltUpdatePutItemOnProcessor : PacketProcessor<BeltUpdatePutItemOnPacket>
    {
        public override void ProcessPacket(BeltUpdatePutItemOnPacket packet, NebulaConnection conn)
        {
            using (Multiplayer.Session.Factories.IsIncomingRequest.On())
            {
                CargoTraffic cargoTraffic = GameMain.galaxy.PlanetById(packet.PlanetId)?.factory?.cargoTraffic;
                if (cargoTraffic == null)
                {
                    return;
                }
                if(packet.ItemCount == 1)
                {
                    if (!cargoTraffic.PutItemOnBelt(packet.BeltId, packet.ItemId, packet.ItemInc))
                    {
                        Log.Warn($"BeltUpdatePutItemOn: Cannot put item{packet.ItemId} on belt{packet.BeltId}, planet{packet.PlanetId}");
                    }
                }
                else
                {
                    bool ret = false;
                    if (cargoTraffic.beltPool[packet.BeltId].id != 0 && cargoTraffic.beltPool[packet.BeltId].id == packet.BeltId)
                    {
                        int index = cargoTraffic.beltPool[packet.BeltId].segIndex + cargoTraffic.beltPool[packet.BeltId].segPivotOffset;
                        ret = cargoTraffic.GetCargoPath(cargoTraffic.beltPool[packet.BeltId].segPathId).TryInsertItem(index, packet.ItemId, packet.ItemCount, packet.ItemInc);
                    }
                    if (!ret)
                    {
                        Log.Warn($"BeltUpdatePutItemOn: Cannot put item{packet.ItemId} on belt{packet.BeltId}, planet{packet.PlanetId}");
                    }
                }
            }
        }
    }
}