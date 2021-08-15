using NebulaModel.Attributes;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Factory.RayReceiver;

namespace NebulaNetwork.PacketProcessors.Factory.RayReceiver
{
    [RegisterPacketProcessor]
    class RayReceiverChangeLensProcessor : PacketProcessor<RayReceiverChangeLensPacket>
    {
        public override void ProcessPacket(RayReceiverChangeLensPacket packet, NebulaConnection conn)
        {
            PowerGeneratorComponent[] pool = GameMain.galaxy.PlanetById(packet.PlanetId)?.factory?.powerSystem?.genPool;
            if (pool != null && packet.GeneratorId != -1 && packet.GeneratorId < pool.Length && pool[packet.GeneratorId].id != -1)
            {
                pool[packet.GeneratorId].catalystPoint = packet.LensCount;
            }
        }
    }
}
