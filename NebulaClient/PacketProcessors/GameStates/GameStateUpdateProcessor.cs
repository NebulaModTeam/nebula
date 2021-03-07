using NebulaModel.Attributes;
using NebulaModel.DataStructures;
using NebulaModel.Networking;
using NebulaModel.Packets.GameStates;
using NebulaModel.Packets.Processors;
using NebulaWorld;

namespace NebulaClient.PacketProcessors.GameStates
{
    [RegisterPacketProcessor]
    public class GameStateUpdateProcessor : IPacketProcessor<GameStateUpdate>
    {
        public void ProcessPacket(GameStateUpdate packet, NebulaConnection conn)
        {
            GameState state = packet.State;
            // We offset the tick received to account for the current player ping
            long tickOffsetSinceSent = (long)System.Math.Round((conn.Ping / 2.0) / (GameMain.tickDeltaTime * 1000));
            state.gameTick += tickOffsetSinceSent;

            SimulatedWorld.UpdateGameState(state);
        }
    }
}
