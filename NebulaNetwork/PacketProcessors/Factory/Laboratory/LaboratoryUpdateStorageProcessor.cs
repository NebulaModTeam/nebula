#region

using NebulaAPI.Packets;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Factory.Laboratory;

#endregion

namespace NebulaNetwork.PacketProcessors.Factory.Laboratory;

[RegisterPacketProcessor]
internal class LaboratoryUpdateStorageProcessor : PacketProcessor<LaboratoryUpdateStoragePacket>
{
    protected override void ProcessPacket(LaboratoryUpdateStoragePacket packet, NebulaConnection conn)
    {
        var pool = GameMain.galaxy.PlanetById(packet.PlanetId)?.factory?.factorySystem?.labPool;
        if (pool == null || packet.LabIndex == -1 || packet.LabIndex >= pool.Length || pool[packet.LabIndex].id == -1)
        {
            return;
        }
        pool[packet.LabIndex].served[packet.Index] = packet.ItemCount;
        pool[packet.LabIndex].incServed[packet.Index] = packet.ItemInc;
    }
}
