using NebulaModel.Attributes;
using NebulaModel.Networking;
using NebulaModel.Packets.Factory.Tank;
using NebulaModel.Packets.Processors;

namespace NebulaHost.PacketProcessors.Factory.Tank
{
    [RegisterPacketProcessor]
    class TankInputOutputSwitchProcessor : IPacketProcessor<TankInputOutputSwitchPacket>
    {
        public void ProcessPacket(TankInputOutputSwitchPacket packet, NebulaConnection conn)
        {
            TankComponent[] pool = GameMain.localPlanet?.factory?.factoryStorage?.tankPool;
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