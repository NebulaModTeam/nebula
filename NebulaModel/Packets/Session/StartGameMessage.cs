using NebulaModel.DataStructures;

namespace NebulaModel.Packets.Session
{
    public class StartGameMessage
    {
        public bool IsAllowedToStart { get; set; }
        public PlayerData LocalPlayerData { get; set; }
        public StartGameMessage() { }
        public StartGameMessage(bool isAllowedToStart, PlayerData localPlayerData)
        {
            IsAllowedToStart = isAllowedToStart;
            LocalPlayerData = localPlayerData;
        }
    }
}
