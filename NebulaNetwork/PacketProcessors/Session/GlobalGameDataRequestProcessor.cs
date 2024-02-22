#region

using NebulaAPI.Packets;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.GameStates;
using NebulaModel.Packets.Session;

#endregion

namespace NebulaNetwork.PacketProcessors.Session;

[RegisterPacketProcessor]
internal class GlobalGameDataRequestProcessor : PacketProcessor<GlobalGameDataRequest>
{
    protected override void ProcessPacket(GlobalGameDataRequest packet, NebulaConnection conn)
    {
        if (IsClient)
        {
            return;
        }

        //Export GameHistoryData, SpaceSector, TrashSystem, MilestoneSystem
        //PlanetFactory, Dysonsphere, GalacticTransport will be handle else where
        var responsePacket = new GlobalGameDataResponse();
        var totalSize = 0;

        using (var writer = new BinaryUtils.Writer())
        {
            GameMain.history.Export(writer.BinaryWriter);
            responsePacket.SandboxToolsEnabled = GameMain.sandboxToolsEnabled;
            responsePacket.HistoryBinaryData = writer.CloseAndGetBytes();
            totalSize += responsePacket.HistoryBinaryData.Length;
        }

        using (var writer = new BinaryUtils.Writer())
        {
            // Initial syncing from vanilla, to be refined later in future.
            NebulaWorld.Combat.CombatManager.SerializeOverwrite = true;
            GameMain.data.spaceSector.BeginSave();
            GameMain.data.spaceSector.Export(writer.BinaryWriter);
            GameMain.data.spaceSector.EndSave();
            NebulaWorld.Combat.CombatManager.SerializeOverwrite = false;

            responsePacket.SpaceSectorBinaryData = writer.CloseAndGetBytes();
            totalSize += responsePacket.SpaceSectorBinaryData.Length;
        }

        using (var writer = new BinaryUtils.Writer())
        {
            GameMain.data.milestoneSystem.Export(writer.BinaryWriter);
            responsePacket.MilestoneSystemBinaryData = writer.CloseAndGetBytes();
            totalSize += responsePacket.MilestoneSystemBinaryData.Length;
        }

        using (var writer = new BinaryUtils.Writer())
        {
            GameMain.data.trashSystem.Export(writer.BinaryWriter);
            responsePacket.TrashSystemBinaryData = writer.CloseAndGetBytes();
            totalSize += responsePacket.TrashSystemBinaryData.Length;
        }

        conn.SendPacket(new FragmentInfo(totalSize));
        conn.SendPacket(responsePacket);
    }
}
