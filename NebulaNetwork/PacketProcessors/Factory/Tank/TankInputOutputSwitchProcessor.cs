#region

using NebulaAPI.Packets;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Factory.Tank;

#endregion

namespace NebulaNetwork.PacketProcessors.Factory.Tank;

[RegisterPacketProcessor]
internal class TankInputOutputSwitchProcessor : PacketProcessor<TankInputOutputSwitchPacket>
{
    protected override void ProcessPacket(TankInputOutputSwitchPacket packet, NebulaConnection conn)
    {
        var pool = GameMain.galaxy.PlanetById(packet.PlanetId)?.factory?.factoryStorage?.tankPool;
        if (pool == null || packet.TankIndex == -1 || packet.TankIndex >= pool.Length || pool[packet.TankIndex].id == -1)
        {
            return;
        }
        if (packet.IsInput)
        {
            pool[packet.TankIndex].inputSwitch = packet.IsClosed;
        }
        else
        {
            pool[packet.TankIndex].outputSwitch = packet.IsClosed;
        }
    }
}
