using NebulaModel.Networking;
using NebulaModel.Packets.Processors;
using NebulaModel.Attributes;
using NebulaModel.Packets.Factory.Splitter;

namespace NebulaHost.PacketProcessors.Factory.Splitter
{
    [RegisterPacketProcessor]
    class SplitterFilterChangeProcessor : IPacketProcessor<SplitterFilterChangePacket>
    {
        public void ProcessPacket(SplitterFilterChangePacket packet, NebulaConnection conn)
        {
            SplitterComponent[] pool = GameMain.localPlanet?.factory?.cargoTraffic?.splitterPool;
            if (pool != null && packet.SplitterIndex != -1 && packet.SplitterIndex < pool.Length && pool[packet.SplitterIndex].id != -1)
            {
                pool[packet.SplitterIndex].outFilter = packet.ItemId;
            }
        }
    }
}