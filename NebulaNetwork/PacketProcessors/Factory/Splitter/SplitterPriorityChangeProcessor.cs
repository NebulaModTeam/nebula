#region

using NebulaAPI.Packets;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Factory.Splitter;

#endregion

namespace NebulaNetwork.PacketProcessors.Factory.Splitter;

[RegisterPacketProcessor]
internal class SplitterPriorityChangeProcessor : PacketProcessor<SplitterPriorityChangePacket>
{
    protected override void ProcessPacket(SplitterPriorityChangePacket packet, NebulaConnection conn)
    {
        var pool = GameMain.galaxy.PlanetById(packet.PlanetId)?.factory?.cargoTraffic.splitterPool;
        if (pool != null && packet.SplitterIndex != -1 && packet.SplitterIndex < pool.Length &&
            pool[packet.SplitterIndex].id != -1)
        {
            pool[packet.SplitterIndex].SetPriority(packet.Slot, packet.IsPriority, packet.Filter);
        }
    }
}
