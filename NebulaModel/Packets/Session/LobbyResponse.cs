namespace NebulaModel.Packets.Session
{
    public class LobbyResponse
    {
        public int GalaxyAlgo { get; set; }
        public int GalaxySeed { get; set; }
        public int StarCount { get; set; }
        public float ResourceMultiplier { get; set; }
        public byte[] ModsSettings { get; set; }
        public int ModsSettingsCount { get; set; }
        public LobbyResponse() { }
        public LobbyResponse(int galaxyAlgo, int galaxySeed, int starCount, float resourceMultiplier, byte[] modsSettings, int settingsCount)
        {
            GalaxyAlgo = galaxyAlgo;
            GalaxySeed = galaxySeed;
            StarCount = starCount;
            ResourceMultiplier = resourceMultiplier;
            ModsSettings = modsSettings;
            ModsSettingsCount = settingsCount;
        }
    }
}
