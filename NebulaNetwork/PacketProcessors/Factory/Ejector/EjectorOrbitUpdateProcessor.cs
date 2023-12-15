#region

using NebulaAPI.Packets;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Factory.Ejector;

#endregion

namespace NebulaNetwork.PacketProcessors.Factory.Ejector;

[RegisterPacketProcessor]
internal class EjectorOrbitUpdateProcessor : PacketProcessor<EjectorOrbitUpdatePacket>
{
    protected override void ProcessPacket(EjectorOrbitUpdatePacket packet, NebulaConnection conn)
    {
        var pool = GameMain.galaxy.PlanetById(packet.PlanetId)?.factory?.factorySystem?.ejectorPool;
        if (pool != null && packet.EjectorIndex != -1 && packet.EjectorIndex < pool.Length &&
            pool[packet.EjectorIndex].id != -1)
        {
            pool[packet.EjectorIndex].SetOrbit(packet.NewOrbitIndex);
        }
    }
}
