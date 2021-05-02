using NebulaModel.Attributes;
using NebulaModel.Networking;
using NebulaModel.Packets.Planet;
using NebulaModel.Packets.Processors;
using NebulaWorld.Planet;

namespace NebulaClient.PacketProcessors.Planet
{
    [RegisterPacketProcessor]
    class RemoveVegetableProcessor : IPacketProcessor<RemoveVegetablePacket>
    {
        public void ProcessPacket(RemoveVegetablePacket packet, NebulaConnection conn)
        {
            if (packet.FactorytIndex >= 0 && GameMain.data.factories[packet.FactorytIndex] != null && GameMain.data.factories[packet.FactorytIndex].vegePool != null)
            {
                using (PlanetManager.EventFromServer.On())
                {
                    GameMain.data.factories[packet.FactorytIndex].RemoveVegeWithComponents(packet.VegeId);
                }
            }
        }
    }
}
