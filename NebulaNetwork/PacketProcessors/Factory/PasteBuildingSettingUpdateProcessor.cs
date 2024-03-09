#region

using NebulaAPI.Packets;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Factory;
using NebulaWorld;

#endregion

namespace NebulaNetwork.PacketProcessors.Factory;

// Processes pasting settings (e.g. item to make in an assembler) onto buildings events
[RegisterPacketProcessor]
internal class PasteBuildingSettingUpdateProcessor : PacketProcessor<PasteBuildingSettingUpdate>
{
    protected override void ProcessPacket(PasteBuildingSettingUpdate packet, NebulaConnection conn)
    {
        if (GameMain.galaxy.PlanetById(packet.PlanetId)?.factory == null)
        {
            return;
        }
        var backup = BuildingParameters.clipboard;
        BuildingParameters.clipboard = packet.GetBuildingSettings();
        using (Multiplayer.Session.Factories.IsIncomingRequest.On())
        {
            // skip audio and realtimetip update
            BuildingParameters.clipboard.PasteToFactoryObject(packet.ObjectId,
                GameMain.galaxy.PlanetById(packet.PlanetId).factory);
        }
        BuildingParameters.clipboard = backup;
    }
}
