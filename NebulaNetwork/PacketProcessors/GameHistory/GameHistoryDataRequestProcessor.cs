using NebulaModel.Attributes;
using Mirror;
using NebulaModel.Packets;
using NebulaModel.Packets.GameHistory;
using NebulaModel.Networking;

namespace NebulaNetwork.PacketProcessors.GameHistory
{
    [RegisterPacketProcessor]
    public class GameHistoryDataRequestProcessor : PacketProcessor<GameHistoryDataRequest>
    {
        public override void ProcessPacket(GameHistoryDataRequest packet, NetworkConnection conn)
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