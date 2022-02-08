using NebulaAPI;
using NebulaModel.DataStructures;

namespace NebulaModel.Packets.GameStates
{
    [HidePacketInDebugLogs]
    public class GameStateUpdate
    {
        public long SentTime { get; set; }
        public long GameTick { get; set; }
        public float UnitsPerSecond { get; set; }

        public GameStateUpdate() { }
        public GameStateUpdate(long sentTime, long gameTick, float unitsPerSecond)
        {
            SentTime = sentTime;
            GameTick = gameTick;
            UnitsPerSecond = unitsPerSecond;
        }
    }
}
