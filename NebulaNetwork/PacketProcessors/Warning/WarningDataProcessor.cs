using NebulaAPI;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Warning;
using NebulaWorld;

namespace NebulaNetwork.PacketProcessors.Warning
{
    [RegisterPacketProcessor]
    internal class WarningDataProcessor : PacketProcessor<WarningDataPacket>
    {
        public override void ProcessPacket(WarningDataPacket packet, NebulaConnection conn)
        {
            Multiplayer.Session.Warning.TickData = packet.Tick;
            using (BinaryUtils.Reader reader = new BinaryUtils.Reader(packet.BinaryData))
            {
                Multiplayer.Session.Warning.ImportBinaryData(reader.BinaryReader, packet.ActiveWarningCount);
            }
        }
    }
}
