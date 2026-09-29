#region

using NebulaAPI.Packets;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Statistics;
using NebulaWorld;

#endregion

namespace NebulaNetwork.PacketProcessors.Statistics;

[RegisterPacketProcessor]
internal class StatisticsPowerDataProcessor : PacketProcessor<StatisticsPowerDataPacket>
{
    protected override void ProcessPacket(StatisticsPowerDataPacket packet, NebulaConnection conn)
    {
        using var reader = new BinaryUtils.Reader(packet.PowerBinaryData);
        Multiplayer.Session.Statistics.ImportPowerData(reader.BinaryReader);
    }
}
