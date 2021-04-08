using NebulaModel.Networking;
using NebulaModel.Packets.Processors;
using NebulaModel.Attributes;
using NebulaModel.Packets.Factory.Laboratory;

namespace NebulaHost.PacketProcessors.Factory.Labratory
{
    [RegisterPacketProcessor]
    class LaboratoryUpdateStorageProcessor : IPacketProcessor<LaboratoryUpdateStoragePacket>
    {
        public void ProcessPacket(LaboratoryUpdateStoragePacket packet, NebulaConnection conn)
        {
            LabComponent[] pool = GameMain.localPlanet?.factory?.factorySystem.labPool;
            if (pool != null && packet.LabIndex != -1 && packet.LabIndex < pool.Length && pool[packet.LabIndex].id != -1)
            {
                pool[packet.LabIndex].served[packet.Index] = packet.Value;
            }
        }
    }
}