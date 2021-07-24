using NebulaModel.Attributes;
using NebulaModel.Networking;
using NebulaModel.Packets.Players;
using NebulaModel.Packets;
using NebulaWorld;

namespace NebulaNetwork.PacketProcessors.Players
{
    [RegisterPacketProcessor]
    class NewDroneOrderProcessor : PacketProcessor<NewDroneOrderPacket>
    {
        public override void ProcessPacket(NewDroneOrderPacket packet, NebulaConnection conn)
        {
            SimulatedWorld.UpdateRemotePlayerDrone(packet);
        }
    }
}
