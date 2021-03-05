using NebulaModel.DataStructures;

namespace NebulaModel.Packets.Session
{
    public class PlayerJoining
    {
        public PlayerData PlayerData { get; set; }

        public PlayerJoining() { }
        public PlayerJoining(PlayerData playerData) { PlayerData = playerData; }
    }
}
