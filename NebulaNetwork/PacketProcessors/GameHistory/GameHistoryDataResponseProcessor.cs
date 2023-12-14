#region

using NebulaAPI;
using NebulaModel.Logger;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.GameHistory;

#endregion

namespace NebulaNetwork.PacketProcessors.GameHistory;

[RegisterPacketProcessor]
internal class GameHistoryDataResponseProcessor : PacketProcessor<GameHistoryDataResponse>
{
    public override void ProcessPacket(GameHistoryDataResponse packet, NebulaConnection conn)
    {
        if (IsHost)
        {
            return;
        }

        //Reset all current values
        GameMain.data.history.Init(GameMain.data);

        Log.Info("Parsing History data from the server.");
        using (var reader = new BinaryUtils.Reader(packet.HistoryBinaryData))
        {
            GameMain.data.history.Import(reader.BinaryReader);
        }
        GameMain.sandboxToolsEnabled = packet.SandboxToolsEnabled;
    }
}
