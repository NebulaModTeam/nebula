namespace NebulaModel.Packets.Session
{
    public class LobbyResponse
    {
        public int GalaxyAlgo { get; set; }
        public int GalaxySeed { get; set; }
        public int StarCount { get; set; }
        public float ResourceMultiplier { get; set; }
        public int[] SavedThemeIds { get; set; }
        public byte[] ModsSettings { get; set; }
        public int ModsSettingsCount { get; set; }
        public ushort NumPlayers { get; set; }

        public LobbyResponse() { }
        public LobbyResponse(int galaxyAlgo, int galaxySeed, int starCount, float resourceMultiplier, int[] savedThemeIds, byte[] modsSettings, int settingsCount, ushort numPlayers)
        {
            GalaxyAlgo = galaxyAlgo;
            GalaxySeed = galaxySeed;
            StarCount = starCount;
            ResourceMultiplier = resourceMultiplier;
            SavedThemeIds = savedThemeIds;
            ModsSettings = modsSettings;
            ModsSettingsCount = settingsCount;
            NumPlayers = numPlayers;
        }
    }
}
