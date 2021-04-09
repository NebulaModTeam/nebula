using NebulaModel.Attributes;
using NebulaModel.Networking;
using NebulaModel.Packets.Factory;
using NebulaModel.Packets.Processors;
using NebulaWorld.Factory;

namespace NebulaHost.PacketProcessors.Factory
{
    [RegisterPacketProcessor]
    class PasteEntitySettingUpdateProcessor : IPacketProcessor<PasteEntitySettingUpdate>
    {
        public void ProcessPacket(PasteEntitySettingUpdate packet, NebulaConnection conn)
        {
            EntitySettingDesc backup = EntitySettingDesc.clipboard;
            EntitySettingDesc.clipboard = packet.GetEntitySettings();
            FactoryManager.EventFromClient = true;
            GameMain.localPlanet.factory.PasteEntitySetting(packet.EntityId);
            FactoryManager.EventFromClient = false;
            EntitySettingDesc.clipboard = backup;
        }
    }
}