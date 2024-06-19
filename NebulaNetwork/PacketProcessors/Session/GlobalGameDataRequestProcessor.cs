#region

using NebulaAPI.Packets;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.GameStates;
using NebulaModel.Packets.Session;
using NebulaWorld.GameStates;

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

        using (var writer = new BinaryUtils.Writer())
        {
            GameMain.history.Export(writer.BinaryWriter);

            conn.SendPacket(new GlobalGameDataResponse(
                GlobalGameDataResponse.EDataType.History, writer.CloseAndGetBytes()));
        }

        using (var writer = new BinaryUtils.Writer())
        {
            GameMain.data.galacticTransport.Export(writer.BinaryWriter);

            conn.SendPacket(new GlobalGameDataResponse(
                GlobalGameDataResponse.EDataType.GalacticTransport, writer.CloseAndGetBytes()));
        }

        using (var writer = new BinaryUtils.Writer())
        {
            // Note: Initial syncing from vanilla. May be refined later in future
            NebulaWorld.Combat.CombatManager.SerializeOverwrite = true;
            GameMain.data.spaceSector.BeginSave();
            GameMain.data.spaceSector.Export(writer.BinaryWriter);
            GameMain.data.spaceSector.EndSave();
            NebulaWorld.Combat.CombatManager.SerializeOverwrite = false;

            conn.SendPacket(new GlobalGameDataResponse(
                GlobalGameDataResponse.EDataType.SpaceSector, writer.CloseAndGetBytes()));
        }

        using (var writer = new BinaryUtils.Writer())
        {
            GameMain.data.milestoneSystem.Export(writer.BinaryWriter);

            conn.SendPacket(new GlobalGameDataResponse(
                GlobalGameDataResponse.EDataType.MilestoneSystem, writer.CloseAndGetBytes()));
        }

        using (var writer = new BinaryUtils.Writer())
        {
            GameMain.data.trashSystem.Export(writer.BinaryWriter);

            conn.SendPacket(new GlobalGameDataResponse(
                GlobalGameDataResponse.EDataType.TrashSystem, writer.CloseAndGetBytes()));
        }

        using (var writer = new BinaryUtils.Writer())
        {
            writer.BinaryWriter.Write(GameMain.sandboxToolsEnabled);

            conn.SendPacket(new GlobalGameDataResponse(
                GlobalGameDataResponse.EDataType.Ready, writer.CloseAndGetBytes()));
        }

        conn.SendPacket(new GameStateSaveInfoPacket(GameStatesManager.LastSaveTime));
    }
}
