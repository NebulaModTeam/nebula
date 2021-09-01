using NebulaAPI;
using NebulaModel.DataStructures;
using NebulaModel.Logger;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.GameStates;
using NebulaModel.Utils;
using UnityEngine;

namespace NebulaNetwork.PacketProcessors.GameStates
{
    [RegisterPacketProcessor]
    public class GameStateUpdateProcessor : PacketProcessor<GameStateUpdate>
    {
        public override void ProcessPacket(GameStateUpdate packet, NebulaConnection conn)
        {
            GameState state = packet.State;

            // We offset the tick received to account for the time it took to receive the packet
            long timeOffset = TimeUtils.CurrentUnixTimestampMilliseconds() - packet.State.timestamp;
            long tickOffsetSinceSent = (long)System.Math.Round(timeOffset / (GameMain.tickDeltaTime * 1000));
            state.gameTick += tickOffsetSinceSent;

            // We allow for a small drift of 5 ticks since the tick offset using the ping is only an approximation
            if (GameMain.gameTick > 0 && Mathf.Abs(state.gameTick - GameMain.gameTick) > 5)
            {
                Log.Info($"Game Tick got updated since it was desynced, was {GameMain.gameTick}, received {state.gameTick}");
                GameMain.gameTick = state.gameTick;
            }
        }
    }
}
