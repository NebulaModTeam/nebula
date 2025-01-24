#region

using NebulaAPI.Packets;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Factory.Ejector;

#endregion

namespace NebulaNetwork.PacketProcessors.Factory.Ejector;

[RegisterPacketProcessor]
internal class EjectorAutoOrbitUpdateProcessor : PacketProcessor<EjectorAutoOrbitUpdatePacket>
{
    protected override void ProcessPacket(EjectorAutoOrbitUpdatePacket packet, NebulaConnection conn)
    {
        var ejectorPool = GameMain.galaxy.PlanetById(packet.PlanetId)?.factory?.factorySystem?.ejectorPool;
        if (ejectorPool == null) return;

        if (packet.EjectorIndex == -1) // Set whole planet ejector event
        {
            for (var i = 1; i < ejectorPool.Length; i++)
            {
                if (ejectorPool[i].id == i) ejectorPool[i].autoOrbit = packet.AutoOrbit;
            }
        }
        else if (packet.EjectorIndex > 0 && packet.EjectorIndex < ejectorPool.Length)
        {
            ejectorPool[packet.EjectorIndex].autoOrbit = packet.AutoOrbit;
        }
    }
}
