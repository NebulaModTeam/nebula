using NebulaModel.Attributes;
using NebulaModel.DataStructures;

namespace NebulaModel.Packets.GameStates
{
    [HidePacketInDebugLogs]
    public class GameStateUpdate
    {
        public GameState State { get; set; }
    }
}
