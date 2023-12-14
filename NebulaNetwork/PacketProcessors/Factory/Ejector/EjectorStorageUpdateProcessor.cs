#region

using NebulaAPI;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Factory.Ejector;

#endregion

namespace NebulaNetwork.PacketProcessors.Factory.Ejector;

[RegisterPacketProcessor]
internal class EjectorStorageUpdateProcessor : PacketProcessor<EjectorStorageUpdatePacket>
{
    public override void ProcessPacket(EjectorStorageUpdatePacket packet, NebulaConnection conn)
    {
        var pool = GameMain.galaxy.PlanetById(packet.PlanetId)?.factory?.factorySystem?.ejectorPool;
        if (pool != null && packet.EjectorIndex != -1 && packet.EjectorIndex < pool.Length &&
            pool[packet.EjectorIndex].id != -1)
        {
            pool[packet.EjectorIndex].bulletCount = packet.ItemCount;
            pool[packet.EjectorIndex].bulletInc = packet.ItemInc;
        }
    }
}
