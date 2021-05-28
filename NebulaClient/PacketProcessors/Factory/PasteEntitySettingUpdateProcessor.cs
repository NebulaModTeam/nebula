using NebulaModel.Attributes;
using NebulaModel.Networking;
using NebulaModel.Packets.Factory;
using NebulaModel.Packets.Processors;
using NebulaWorld.Factory;

namespace NebulaClient.PacketProcessors.Factory
{
    [RegisterPacketProcessor]
    class PasteEntitySettingUpdateProcessor : IPacketProcessor<PasteBuildingSettingUpdate>
    {
        public void ProcessPacket(PasteBuildingSettingUpdate packet, NebulaConnection conn)
        {
            if (GameMain.data.factories[packet.FactoryIndex] != null)
            {
                BuildingParameters backup = BuildingParameters.clipboard;
                BuildingParameters.clipboard = packet.GetBuildingSettings();
                using (FactoryManager.EventFromServer.On())
                {
                    GameMain.data.factories[packet.FactoryIndex].PasteBuildingSetting(packet.ItemId);
                }
                BuildingParameters.clipboard = backup;
            }
        }
    }
}