using NebulaModel.DataStructures;

namespace NebulaModel.Packets.Session
{
    public class HandshakeResponse
    {
        public int AlgoVersion { get; set; }
        public int GalaxySeed { get; set; }
        public int StarCount { get; set; }
        public float ResourceMultiplier { get; set; }
        public PlayerData LocalPlayerData { get; set; }
        public byte[] CompressedGS2Settings { get; set; }

        public HandshakeResponse() { }

        public HandshakeResponse(int algoVersion, int galaxySeed, int starCount, float resourceMultiplier, PlayerData localPlayerData, byte[] compressedGS2Settings = null)
        {
            AlgoVersion = algoVersion;
            GalaxySeed = galaxySeed;
            StarCount = starCount;
            ResourceMultiplier = resourceMultiplier;
            LocalPlayerData = localPlayerData;
            CompressedGS2Settings = compressedGS2Settings;
            if (CompressedGS2Settings == null)
            {
                CompressedGS2Settings = new byte[1];
                CompressedGS2Settings[0] = 0;
            }
        }
    }
}
