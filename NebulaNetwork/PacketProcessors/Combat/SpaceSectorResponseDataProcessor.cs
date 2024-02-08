#region

using NebulaAPI.Packets;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Combat;


#endregion

namespace NebulaNetwork.PacketProcessors.Combat;

[RegisterPacketProcessor]
internal class SpaceSectorResponseDataProcessor : PacketProcessor<SpaceSectorResponseDataPacket>
{
    protected override void ProcessPacket(SpaceSectorResponseDataPacket packet, NebulaConnection conn)
    {
        if (IsHost)
        {
            return;
        }

        using (var reader = new BinaryUtils.Reader(packet.SpaceSectorResponseData))
        {
            NebulaWorld.Combat.CombatManager.SerializeOverwrite = true;
            GameMain.data.spaceSector.Import(reader.BinaryReader);
            NebulaWorld.Combat.CombatManager.SerializeOverwrite = false;
        }
        GameMain.mainPlayer.mecha.CheckCombatModuleDataIsValidPatch();
    }
}
