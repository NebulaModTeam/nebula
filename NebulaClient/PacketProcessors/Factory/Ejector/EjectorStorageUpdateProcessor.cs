using NebulaModel.Networking;
using NebulaModel.Packets.Processors;
using NebulaModel.Attributes;
using NebulaModel.Packets.Factory.Ejector;

namespace NebulaClient.PacketProcessors.Factory.Ejector
{
    [RegisterPacketProcessor]
    class EjectorStorageUpdateProcessor : IPacketProcessor<EjectorStorageUpdatePacket>
    {
        public void ProcessPacket(EjectorStorageUpdatePacket packet, NebulaConnection conn)
        {
            EjectorComponent[] pool = GameMain.localPlanet?.factory?.factorySystem.ejectorPool;
            if (pool != null && packet.EjectorIndex != -1 && packet.EjectorIndex < pool.Length && pool[packet.EjectorIndex].id != -1)
            {
                pool[packet.EjectorIndex].bulletCount = packet.NewBulletAmount;
            }
        }
    }
}