using NebulaModel.Attributes;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.GameHistory;

namespace NebulaNetwork.PacketProcessors.GameHistory
{
    [RegisterPacketProcessor]
    public class GameHistoryDataRequestProcessor : PacketProcessor<GameHistoryDataRequest>
    {
        public override void ProcessPacket(GameHistoryDataRequest packet, NebulaConnection conn)
        {
            if (IsClient) return;

            using (BinaryUtils.Writer writer = new BinaryUtils.Writer())
            {
                GameMain.history.Export(writer.BinaryWriter);
                conn.SendPacket(new GameHistoryDataResponse(writer.CloseAndGetBytes()));
            }
        }
    }
}