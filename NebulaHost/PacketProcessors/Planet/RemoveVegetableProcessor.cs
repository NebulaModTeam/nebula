using NebulaModel.Attributes;
using NebulaModel.Networking;
using NebulaModel.Packets.Planet;
using NebulaModel.Packets.Processors;
using NebulaWorld.Factory;

namespace NebulaHost.PacketProcessors.Planet
{
    [RegisterPacketProcessor]
    class RemoveVegetableProcessor : IPacketProcessor<RemoveVegetablePacket>
    {
        public void ProcessPacket(RemoveVegetablePacket packet, NebulaConnection conn)
        {
            if (GameMain.data.factories[packet.FactorytId] != null)
            {
                FactoryManager.EventFromClient = true;
                GameMain.data.factories[packet.FactorytId].RemoveVegeWithComponents(packet.VegeId);
                FactoryManager.EventFromClient = false;
            }
        }
    }
}
