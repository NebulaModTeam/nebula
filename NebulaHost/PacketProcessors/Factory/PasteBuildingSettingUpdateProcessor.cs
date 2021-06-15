using NebulaModel.Attributes;
using NebulaModel.Networking;
using NebulaModel.Packets.Factory;
using NebulaModel.Packets.Processors;
using NebulaWorld.Factory;

namespace NebulaHost.PacketProcessors.Factory
{
    [RegisterPacketProcessor]
    class PasteBuildingSettingUpdateProcessor : IPacketProcessor<PasteBuildingSettingUpdate>
    {
        public void ProcessPacket(PasteBuildingSettingUpdate packet, NebulaConnection conn)
        {
            if (GameMain.data.factories[packet.FactoryIndex].planet.factory != null)
            {
                BuildingParameters backup = BuildingParameters.clipboard;
                BuildingParameters.clipboard = packet.GetBuildingSettings();

                using (FactoryManager.EventFromServer.On())
                {
                    GameMain.data.factories[packet.FactoryIndex].planet.factory.PasteBuildingSetting(packet.ObjectId);
                }
                BuildingParameters.clipboard = backup;
            }
        }
    }
}