using NebulaModel.Attributes;
using NebulaModel.Networking;
using NebulaModel.Packets.Planet;
using NebulaModel.Packets.Processors;
using NebulaWorld.Planet;

namespace NebulaHost.PacketProcessors.Planet
{
    [RegisterPacketProcessor]
    class RemoveVegetableProcessor : IPacketProcessor<RemoveVegetablePacket>
    {
        public void ProcessPacket(RemoveVegetablePacket packet, NebulaConnection conn)
        {
            if (packet.FactorytIndex >= 0 && GameMain.data.factories[packet.FactorytIndex] != null)
            {
                using (PlanetManager.EventFromClient.On())
                {
                    GameMain.data.factories[packet.FactorytIndex].RemoveVegeWithComponents(packet.VegeId);
                }
            }
        }
    }
}
