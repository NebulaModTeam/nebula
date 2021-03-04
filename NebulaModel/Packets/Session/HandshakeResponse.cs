namespace NebulaModel.Packets.Session
{
    public class HandshakeResponse
    {
        public ushort LocalPlayerId { get; set; }
        public int AlgoVersion { get; set; }
        public int GalaxySeed { get; set; }
        public int StarCount { get; set; }
        public float ResourceMultiplier { get; set; }
        public ushort[] OtherPlayerIds { get; set; }

        public HandshakeResponse() { }

        public HandshakeResponse(int algoVersion, int galaxySeed, int starCount, float resourceMultiplier, ushort localPlayerId, ushort[] otherPlayerIds)
        {
            AlgoVersion = algoVersion;
            GalaxySeed = galaxySeed;
            StarCount = starCount;
            ResourceMultiplier = resourceMultiplier;
            LocalPlayerId = localPlayerId;
            OtherPlayerIds = otherPlayerIds;
        }
    }
}
