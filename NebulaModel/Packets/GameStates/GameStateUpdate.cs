using NebulaAPI;
using NebulaModel.DataStructures;

namespace NebulaModel.Packets.GameStates
{
    [HidePacketInDebugLogs]
    public class GameStateUpdate
    {
        public GameState State { get; set; }
    }
}
