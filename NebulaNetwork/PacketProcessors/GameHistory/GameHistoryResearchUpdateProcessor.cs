using NebulaModel.Attributes;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.GameHistory;

namespace NebulaNetwork.PacketProcessors.GameHistory
{
    [RegisterPacketProcessor]
    class GameHistoryResearchUpdateProcessor : PacketProcessor<GameHistoryResearchUpdatePacket>
    {
        public override void ProcessPacket(GameHistoryResearchUpdatePacket packet, NebulaConnection conn)
        {
            GameHistoryData data = GameMain.data.history;
            if (packet.TechId != data.currentTech)
            {
                //Wait for the authoritative packet to enqueue new tech first
                return;
            }
            TechState state = data.techStates[data.currentTech];
            state.hashUploaded = packet.HashUploaded;
            state.hashNeeded = packet.HashNeeded;
            data.techStates[data.currentTech] = state;
        }
    }
}
