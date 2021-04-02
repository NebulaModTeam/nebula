using NebulaModel.Attributes;
using NebulaModel.Networking;
using NebulaModel.Packets.GameHistory;
using NebulaModel.Packets.Processors;

namespace NebulaHost.PacketProcessors.GameHistory
{
    [RegisterPacketProcessor]
    public class GameHistoryDataRequestProcessor : IPacketProcessor<GameHistoryDataRequest>
    {
        public void ProcessPacket(GameHistoryDataRequest packet, NebulaConnection conn)
        {
            using (BinaryUtils.Writer writer = new BinaryUtils.Writer())
            {
                GameMain.history.Export(writer.BinaryWriter);
                conn.SendPacket(new GameHistoryDataResponse(writer.CloseAndGetBytes()));
            }
        }
    }
}