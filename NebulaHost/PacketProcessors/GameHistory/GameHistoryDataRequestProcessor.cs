using LZ4;
using NebulaModel.Attributes;
using NebulaModel.Networking;
using NebulaModel.Packets.GameHistory;
using NebulaModel.Packets.Processors;
using System.IO;
using System.IO.Compression;

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