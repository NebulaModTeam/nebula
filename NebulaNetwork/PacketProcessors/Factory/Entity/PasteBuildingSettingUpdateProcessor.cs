using NebulaModel.Attributes;
using Mirror;
using NebulaModel.Packets;
using NebulaModel.Packets.Factory;
using NebulaWorld.Factory;

namespace NebulaNetwork.PacketProcessors.Factory.Entity
{
    // Processes pasting settings (e.g. item to make in an assembler) onto buildings events
    [RegisterPacketProcessor]
    class PasteBuildingSettingUpdateProcessor : PacketProcessor<PasteBuildingSettingUpdate>
    {
        public override void ProcessPacket(PasteBuildingSettingUpdate packet, NetworkConnection conn)
        {
            if (GameMain.galaxy.PlanetById(packet.PlanetId)?.factory != null)
            {
                BuildingParameters backup = BuildingParameters.clipboard;
                BuildingParameters.clipboard = packet.GetBuildingSettings();
                using (FactoryManager.IsIncomingRequest.On())
                {
                    GameMain.galaxy.PlanetById(packet.PlanetId).factory.PasteBuildingSetting(packet.ObjectId);
                }
                BuildingParameters.clipboard = backup;
            }
        }
    }
}