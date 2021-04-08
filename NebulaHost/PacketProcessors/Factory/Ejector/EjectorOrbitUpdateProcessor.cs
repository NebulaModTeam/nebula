using NebulaModel.Attributes;
using NebulaModel.Networking;
using NebulaModel.Packets.Factory.Ejector;
using NebulaModel.Packets.Processors;

namespace NebulaHost.PacketProcessors.Factory.Ejector
{
    [RegisterPacketProcessor]
    class EjectorOrbitUpdateProcessor : IPacketProcessor<EjectorOrbitUpdatePacket>
    {
        public void ProcessPacket(EjectorOrbitUpdatePacket packet, NebulaConnection conn)
        {
            EjectorComponent[] pool = GameMain.localPlanet?.factory?.factorySystem.ejectorPool;
            if (pool != null && packet.EjectorIndex != -1 && packet.EjectorIndex < pool.Length && pool[packet.EjectorIndex].id != -1)
            {
                pool[packet.EjectorIndex].SetOrbit(packet.NewOrbitIndex);
            }
        }
    }
}