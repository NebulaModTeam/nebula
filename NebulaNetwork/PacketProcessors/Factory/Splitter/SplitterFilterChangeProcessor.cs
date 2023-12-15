#region

using NebulaAPI.Packets;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Factory.Splitter;

#endregion

namespace NebulaNetwork.PacketProcessors.Factory.Splitter;

[RegisterPacketProcessor]
internal class SplitterFilterChangeProcessor : PacketProcessor<SplitterFilterChangePacket>
{
    protected override void ProcessPacket(SplitterFilterChangePacket packet, NebulaConnection conn)
    {
        var pool = GameMain.galaxy.PlanetById(packet.PlanetId)?.factory?.cargoTraffic?.splitterPool;
        if (pool != null && packet.SplitterIndex != -1 && packet.SplitterIndex < pool.Length &&
            pool[packet.SplitterIndex].id != -1)
        {
            pool[packet.SplitterIndex].outFilter = packet.ItemId;
        }
    }
}
