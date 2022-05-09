using NebulaModel.DataStructures;

namespace NebulaModel.Packets.Session
{
    public class PlayerJoining
    {
        public PlayerData PlayerData { get; set; }
        public ushort NumPlayers { get; set; }

        public PlayerJoining() { }
        public PlayerJoining(PlayerData playerData, ushort numPlayers)
        {
            PlayerData = playerData;
            NumPlayers = numPlayers;
        }
    }
}
