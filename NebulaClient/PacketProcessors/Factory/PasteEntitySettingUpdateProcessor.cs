using NebulaModel.Attributes;
using NebulaModel.Networking;
using NebulaModel.Packets.Factory;
using NebulaModel.Packets.Processors;
using NebulaWorld.Factory;

namespace NebulaClient.PacketProcessors.Factory
{
    [RegisterPacketProcessor]
    class PasteEntitySettingUpdateProcessor : IPacketProcessor<PasteEntitySettingUpdate>
    {
        public void ProcessPacket(PasteEntitySettingUpdate packet, NebulaConnection conn)
        {
            EntitySettingDesc backup = EntitySettingDesc.clipboard;
            EntitySettingDesc.clipboard = packet.GetEntitySettings();
            FactoryManager.EventFromServer = true;
            GameMain.localPlanet.factory.PasteEntitySetting(packet.EntityId);
            FactoryManager.EventFromServer = false;
            EntitySettingDesc.clipboard = backup;
        }
    }
}