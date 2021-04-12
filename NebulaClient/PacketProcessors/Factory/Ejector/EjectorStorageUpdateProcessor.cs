using NebulaModel.Attributes;
using NebulaModel.Networking;
using NebulaModel.Packets.Factory.Ejector;
using NebulaModel.Packets.Processors;

namespace NebulaClient.PacketProcessors.Factory.Ejector
{
    [RegisterPacketProcessor]
    class EjectorStorageUpdateProcessor : IPacketProcessor<EjectorStorageUpdatePacket>
    {
        public void ProcessPacket(EjectorStorageUpdatePacket packet, NebulaConnection conn)
        {
            EjectorComponent[] pool = GameMain.data.factories[packet.FactoryIndex]?.factorySystem?.ejectorPool;
            if (pool != null && packet.EjectorIndex != -1 && packet.EjectorIndex < pool.Length && pool[packet.EjectorIndex].id != -1)
            {
                pool[packet.EjectorIndex].bulletCount = packet.NewBulletAmount;
            }
        }
    }
}