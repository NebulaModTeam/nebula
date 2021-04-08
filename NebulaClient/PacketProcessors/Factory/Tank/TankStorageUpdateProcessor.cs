using NebulaModel.Packets.Factory.Tank;
using NebulaModel.Networking;
using NebulaModel.Packets.Processors;
using NebulaModel.Attributes;

namespace NebulaClient.PacketProcessors.Factory.Tank
{
    [RegisterPacketProcessor]
    class TankStorageUpdateProcessor : IPacketProcessor<TankStorageUpdatePacket>
    {
        public void ProcessPacket(TankStorageUpdatePacket packet, NebulaConnection conn)
        {
            TankComponent[] pool = GameMain.localPlanet?.factory?.factoryStorage?.tankPool;
            if (pool != null && packet.TankIndex != -1 && packet.TankIndex < pool.Length && pool[packet.TankIndex].id != -1)
            {
                pool[packet.TankIndex].fluidId = packet.FluidId;
                pool[packet.TankIndex].fluidCount = packet.FluidCount;
            }
        }
    }
}