using NebulaModel.DataStructures;

namespace NebulaModel.Packets.Session
{
    public class HandshakeResponse
    {
        public int AlgoVersion { get; set; }
        public int GalaxySeed { get; set; }
        public int StarCount { get; set; }
        public float ResourceMultiplier { get; set; }
        public bool IsNewPlayer { get; set; }
        public PlayerData LocalPlayerData { get; set; }

        public HandshakeResponse() { }

        public HandshakeResponse(int algoVersion, int galaxySeed, int starCount, float resourceMultiplier, bool isNewPlayer, PlayerData localPlayerData)
        {
            AlgoVersion = algoVersion;
            GalaxySeed = galaxySeed;
            StarCount = starCount;
            ResourceMultiplier = resourceMultiplier;
            IsNewPlayer = isNewPlayer;
            LocalPlayerData = localPlayerData;
        }
    }
}
