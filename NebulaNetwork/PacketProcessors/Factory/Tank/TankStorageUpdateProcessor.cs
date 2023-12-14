#region

using NebulaAPI.Packets;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Factory.Tank;

#endregion

namespace NebulaNetwork.PacketProcessors.Factory.Tank;

[RegisterPacketProcessor]
internal class TankStorageUpdateProcessor : PacketProcessor<TankStorageUpdatePacket>
{
    protected override void ProcessPacket(TankStorageUpdatePacket packet, NebulaConnection conn)
    {
        var pool = GameMain.galaxy.PlanetById(packet.PlanetId)?.factory?.factoryStorage?.tankPool;
        if (pool == null || packet.TankIndex == -1 || packet.TankIndex >= pool.Length || pool[packet.TankIndex].id == -1)
        {
            return;
        }
        pool[packet.TankIndex].fluidId = packet.FluidId;
        pool[packet.TankIndex].fluidCount = packet.FluidCount;
        pool[packet.TankIndex].fluidInc = packet.FluidInc;
    }
}
