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
            if (GameMain.data.factories[packet.FactoryIndex] != null)
            {
                EntitySettingDesc backup = EntitySettingDesc.clipboard;
                EntitySettingDesc.clipboard = packet.GetEntitySettings();

                using (FactoryManager.EventFromServer.On())
                {
                    GameMain.data.factories[packet.FactoryIndex].PasteEntitySetting(packet.EntityId);
                }
                EntitySettingDesc.clipboard = backup;
            }
        }
    }
}