using NebulaModel.Attributes;
using NebulaModel.Networking;
using NebulaModel.Packets.Factory.RayReceiver;
using NebulaModel.Packets.Processors;

namespace NebulaClient.PacketProcessors.Factory.RayReceiver
{
    [RegisterPacketProcessor]
    class RayReceiverChangeLensProcessor : IPacketProcessor<RayReceiverChangeLensPacket>
    {
        public void ProcessPacket(RayReceiverChangeLensPacket packet, NebulaConnection conn)
        {
            PowerGeneratorComponent[] pool = GameMain.galaxy.PlanetById(packet.PlanetId)?.factory?.powerSystem?.genPool;
            if (pool != null && packet.GeneratorId != -1 && packet.GeneratorId < pool.Length && pool[packet.GeneratorId].id != -1)
            {
                pool[packet.GeneratorId].catalystPoint = packet.LensCount;
            }
        }
    }
}
