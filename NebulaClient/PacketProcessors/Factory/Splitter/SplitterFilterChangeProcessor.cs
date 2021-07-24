using NebulaModel.Attributes;
using NebulaModel.Networking;
using NebulaModel.Packets.Factory.Splitter;
using NebulaModel.Packets;

namespace NebulaClient.PacketProcessors.Factory.Splitter
{
    [RegisterPacketProcessor]
    class SplitterFilterChangeProcessor : PacketProcessor<SplitterFilterChangePacket>
    {
        public override void ProcessPacket(SplitterFilterChangePacket packet, NebulaConnection conn)
        {
            SplitterComponent[] pool = GameMain.galaxy.PlanetById(packet.PlanetId)?.factory?.cargoTraffic?.splitterPool;
            if (pool != null && packet.SplitterIndex != -1 && packet.SplitterIndex < pool.Length && pool[packet.SplitterIndex].id != -1)
            {
                pool[packet.SplitterIndex].outFilter = packet.ItemId;
            }
        }
    }
}