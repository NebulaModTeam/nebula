using NebulaAPI;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.GameHistory;

namespace NebulaNetwork.PacketProcessors.GameHistory
{
    [RegisterPacketProcessor]
    internal class GameHistoryResearchUpdateProcessor : PacketProcessor<GameHistoryResearchUpdatePacket>
    {
        public override void ProcessPacket(GameHistoryResearchUpdatePacket packet, NebulaConnection conn)
        {
            GameHistoryData data = GameMain.data.history;
            if (packet.TechId != data.currentTech)
            {                
                NebulaModel.Logger.Log.Warn($"CurrentTech mismatch! Server:{packet.TechId} Local:{data.currentTech}");
                //Replace currentTech to match with server
                data.currentTech = packet.TechId;
                data.techQueue[0] = packet.TechId;
            }
            TechState state = data.techStates[data.currentTech];
            state.hashUploaded = packet.HashUploaded;
            state.hashNeeded = packet.HashNeeded;
            data.techStates[data.currentTech] = state;
        }
    }
}
