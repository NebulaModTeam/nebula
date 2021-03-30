using NebulaModel.Attributes;
using NebulaModel.Networking;
using NebulaModel.Packets.GameHistory;
using NebulaModel.Packets.Processors;
using System.IO;
using LZ4;
using System.IO.Compression;
using NebulaModel.Logger;

namespace NebulaClient.PacketProcessors.GameHistory
{
    [RegisterPacketProcessor]
    class GameHistoryDataResponseProcessor : IPacketProcessor<GameHistoryDataResponse>
    {
        public void ProcessPacket(GameHistoryDataResponse packet, NebulaConnection conn)
        {
            //Reset all current values
            GameMain.data.history.Init(GameMain.data);

            Log.Info($"Parsing History data from the server.");
            using (MemoryStream ms = new MemoryStream(packet.HistoryBinaryData))
            using (LZ4Stream ls = new LZ4Stream(ms, CompressionMode.Decompress))
            using (BufferedStream bs = new BufferedStream(ls, 8192))
            using (BinaryReader br = new BinaryReader(bs))
            {
                GameMain.data.history.Import(br);
            }
        }
    }
}
