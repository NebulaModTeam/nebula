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
        public byte[] ModsSettings { get; set; }
        public int ModsSettingsCount { get; set; }
        
        public HandshakeResponse() { }

        public HandshakeResponse(int algoVersion, int galaxySeed, int starCount, float resourceMultiplier, PlayerData localPlayerData, byte[] modsSettings, int settingsCount)
        {
            AlgoVersion = algoVersion;
            GalaxySeed = galaxySeed;
            StarCount = starCount;
            ResourceMultiplier = resourceMultiplier;
            IsNewPlayer = isNewPlayer;
            LocalPlayerData = localPlayerData;
            ModsSettings = modsSettings;
            ModsSettingsCount = settingsCount;
        }
    }
}
