using NebulaModel.Attributes;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Factory.Ejector;

namespace NebulaNetwork.PacketProcessors.Factory.Ejector
{
    [RegisterPacketProcessor]
    class EjectorOrbitUpdateProcessor : PacketProcessor<EjectorOrbitUpdatePacket>
    {
        public override void ProcessPacket(EjectorOrbitUpdatePacket packet, NebulaConnection conn)
        {
            EjectorComponent[] pool = GameMain.galaxy.PlanetById(packet.PlanetId)?.factory?.factorySystem?.ejectorPool;
            if (pool != null && packet.EjectorIndex != -1 && packet.EjectorIndex < pool.Length && pool[packet.EjectorIndex].id != -1)
            {
                pool[packet.EjectorIndex].SetOrbit(packet.NewOrbitIndex);
            }
        }
    }
}