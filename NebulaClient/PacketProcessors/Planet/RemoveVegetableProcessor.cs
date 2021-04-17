using NebulaModel.Attributes;
using NebulaModel.Networking;
using NebulaModel.Packets.Planet;
using NebulaModel.Packets.Processors;
using NebulaWorld.Factory;

namespace NebulaClient.PacketProcessors.Planet
{
    [RegisterPacketProcessor]
    class RemoveVegetableProcessor : IPacketProcessor<RemoveVegetablePacket>
    {
        public void ProcessPacket(RemoveVegetablePacket packet, NebulaConnection conn)
        {
            if (GameMain.data.factories[packet.FactorytId] != null)
            {
                FactoryManager.EventFromServer = true;
                GameMain.data.factories[packet.FactorytId].RemoveVegeWithComponents(packet.VegeId);
                FactoryManager.EventFromServer = false;
            }
        }
    }
}
