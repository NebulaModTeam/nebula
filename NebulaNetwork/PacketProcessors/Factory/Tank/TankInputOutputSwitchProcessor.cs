using NebulaModel.Attributes;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Factory.Tank;

namespace NebulaNetwork.PacketProcessors.Factory.Tank
{
    [RegisterPacketProcessor]
    class TankInputOutputSwitchProcessor : PacketProcessor<TankInputOutputSwitchPacket>
    {
        public override void ProcessPacket(TankInputOutputSwitchPacket packet, NebulaConnection conn)
        {
            TankComponent[] pool = GameMain.galaxy.PlanetById(packet.PlanetId)?.factory?.factoryStorage?.tankPool;
            if (pool != null && packet.TankIndex != -1 && packet.TankIndex < pool.Length && pool[packet.TankIndex].id != -1)
            {
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
    }
}