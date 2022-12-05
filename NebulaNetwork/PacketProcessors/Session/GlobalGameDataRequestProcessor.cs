using NebulaAPI;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.GameHistory;
using NebulaModel.Packets.Session;
using NebulaModel.Packets.Statistics;
using NebulaModel.Packets.Trash;

namespace NebulaNetwork.PacketProcessors.Session
{
    [RegisterPacketProcessor]
    class GlobalGameDataRequestProcessor : PacketProcessor<GlobalGameDataRequest>
    {
        public override void ProcessPacket(GlobalGameDataRequest packet, NebulaConnection conn)
        {
            if (IsClient)
            {
                return;
            }

            //Export GameHistoryData, TrashSystem, MilestoneSystem
            //PlanetFactory, Dysonsphere, GalacticTransport will be handle else where

            using (BinaryUtils.Writer writer = new BinaryUtils.Writer())
            {
                GameMain.history.Export(writer.BinaryWriter);
                conn.SendPacket(new GameHistoryDataResponse(writer.CloseAndGetBytes(), GameMain.sandboxToolsEnabled));
            }

            using (BinaryUtils.Writer writer = new BinaryUtils.Writer())
            {
                GameMain.data.trashSystem.Export(writer.BinaryWriter);
                conn.SendPacket(new TrashSystemResponseDataPacket(writer.CloseAndGetBytes()));
            }

            using (BinaryUtils.Writer writer = new BinaryUtils.Writer())
            {
                GameMain.data.milestoneSystem.Export(writer.BinaryWriter);
                conn.SendPacket(new MilestoneDataResponse(writer.CloseAndGetBytes()));
            }
        }
    }
}
