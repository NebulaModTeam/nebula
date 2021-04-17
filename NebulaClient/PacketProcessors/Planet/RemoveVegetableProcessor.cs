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
            if (GameMain.data.factories[packet.FactorytIndex] != null)
            {
                PlanetManager.EventFromServer = true;
                GameMain.data.factories[packet.FactorytIndex].RemoveVegeWithComponents(packet.VegeId);
                PlanetManager.EventFromServer = false;
            }
        }
    }
}
