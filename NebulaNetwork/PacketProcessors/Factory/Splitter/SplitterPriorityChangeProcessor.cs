using NebulaModel.Attributes;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Factory.Splitter;
using NebulaWorld.Factory;

namespace NebulaNetwork.PacketProcessors.Factory.Splitter
{
    [RegisterPacketProcessor]
    class SplitterPriorityChangeProcessor : PacketProcessor<SplitterPriorityChangePacket>
    {
        public override void ProcessPacket(SplitterPriorityChangePacket packet, NebulaConnection conn)
        {
            SplitterComponent[] pool = GameMain.galaxy.PlanetById(packet.PlanetId)?.factory?.cargoTraffic?.splitterPool;
            if (pool != null && packet.SplitterIndex != -1 && packet.SplitterIndex < pool.Length && pool[packet.SplitterIndex].id != -1)
            {
                StorageManager.IsHumanInput = false;
                pool[packet.SplitterIndex].SetPriority(packet.Slot, packet.IsPriority, packet.Filter);
                StorageManager.IsHumanInput = true;
            }
        }
    }
}