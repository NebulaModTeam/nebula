using NebulaModel.Attributes;
using NebulaModel.Networking;
using NebulaModel.Packets.Factory.Laboratory;
using NebulaModel.Packets.Processors;

namespace NebulaClient.PacketProcessors.Factory.Labratory
{
    [RegisterPacketProcessor]
    class LaboratoryUpdateCubesProcessor : IPacketProcessor<LaboratoryUpdateCubesPacket>
    {
        public void ProcessPacket(LaboratoryUpdateCubesPacket packet, NebulaConnection conn)
        {
            LabComponent[] pool = GameMain.localPlanet?.factory?.factorySystem.labPool;
            if (pool != null && packet.LabIndex != -1 && packet.LabIndex < pool.Length && pool[packet.LabIndex].id != -1)
            {
                pool[packet.LabIndex].matrixServed[packet.Index] = packet.Value;
            }
        }
    }
}