using NebulaModel.Attributes;
using NebulaModel.Networking;
using NebulaModel.Packets.Factory.Laboratory;
using NebulaModel.Packets;

namespace NebulaNetwork.PacketProcessors.Factory.Labratory
{
    [RegisterPacketProcessor]
    class LaboratoryUpdateCubesProcessor : PacketProcessor<LaboratoryUpdateCubesPacket>
    {
        public override void ProcessPacket(LaboratoryUpdateCubesPacket packet, NebulaConnection conn)
        {
            LabComponent[] pool = GameMain.galaxy.PlanetById(packet.PlanetId)?.factory?.factorySystem?.labPool;
            if (pool != null && packet.LabIndex != -1 && packet.LabIndex < pool.Length && pool[packet.LabIndex].id != -1)
            {
                pool[packet.LabIndex].matrixServed[packet.Index] = packet.Value;
            }
        }
    }
}