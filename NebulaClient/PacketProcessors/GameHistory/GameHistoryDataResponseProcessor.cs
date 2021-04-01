using NebulaModel.Attributes;
using NebulaModel.Logger;
using NebulaModel.Networking;
using NebulaModel.Packets.GameHistory;
using NebulaModel.Packets.Processors;

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
            using (BinaryUtils.Reader reader = new BinaryUtils.Reader(packet.HistoryBinaryData))
            {
                GameMain.data.history.Import(reader.BinaryReader);
            }
        }
    }
}
