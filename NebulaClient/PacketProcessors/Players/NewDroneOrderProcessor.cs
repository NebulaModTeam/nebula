using NebulaModel.Attributes;
using NebulaModel.Networking;
using NebulaModel.Packets.Players;
using NebulaModel.Packets.Processors;
using NebulaWorld;

namespace NebulaClient.PacketProcessors.Players
{
    [RegisterPacketProcessor]
    class NewDroneOrderProcessor : IPacketProcessor<NewDroneOrderPacket>
    {
        public void ProcessPacket(NewDroneOrderPacket packet, NebulaConnection conn)
        {
            SimulatedWorld.UpdateRemotePlayerDrone(packet);
        }
    }
}
