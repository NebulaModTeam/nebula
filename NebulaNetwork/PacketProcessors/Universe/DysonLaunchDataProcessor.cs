using NebulaAPI;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Universe;
using NebulaWorld;

namespace NebulaNetwork.PacketProcessors.Universe
{
    [RegisterPacketProcessor]
    internal class DysonLaunchDataProcessor : PacketProcessor<DysonLaunchDataPacket>
    {
        public override void ProcessPacket(DysonLaunchDataPacket packet, NebulaConnection conn)
        {
            if (IsHost)
            {
                return;
            }
            Multiplayer.Session.Launch.ImportPacket(packet);
        }
    }
}
