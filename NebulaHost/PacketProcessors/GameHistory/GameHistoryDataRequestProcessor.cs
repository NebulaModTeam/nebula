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
            using (MemoryStream ms = new MemoryStream())
            using (MemoryStream ms2 = new MemoryStream())
            {
                using (LZ4Stream ls = new LZ4Stream(ms, CompressionMode.Compress))
                using (BufferedStream bs = new BufferedStream(ls, 8192))
                using (BinaryWriter bw = new BinaryWriter(bs))
                {
                    GameMain.history.Export(bw);
                }
                conn.SendPacket(new GameHistoryDataResponse(ms.ToArray()));
            }
        }
    }
}