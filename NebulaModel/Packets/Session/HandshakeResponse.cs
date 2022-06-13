using NebulaModel.DataStructures;

namespace NebulaModel.Packets.Session
{
    public class HandshakeResponse
    {
        public int GalaxyAlgo { get; set; }
        public int GalaxySeed { get; set; }
        public int StarCount { get; set; }
        public float ResourceMultiplier { get; set; }
        public bool IsSandboxMode { get; set; }
        public int[] SavedThemeIds { get; set; }
        public bool IsNewPlayer { get; set; }
        public PlayerData LocalPlayerData { get; set; }
        public byte[] ModsSettings { get; set; }
        public int ModsSettingsCount { get; set; }
        public bool SyncSoil { get; set; }
        public ushort NumPlayers { get; set; }
        public string DiscordPartyId { get; set; }

        public HandshakeResponse() { }

        public HandshakeResponse(in GameDesc gameDesc, bool isNewPlayer, PlayerData localPlayerData, byte[] modsSettings, int settingsCount, bool syncSoil, ushort numPlayers, string discordPartyId)
        {
            GalaxyAlgo = gameDesc.galaxyAlgo;
            GalaxySeed = gameDesc.galaxySeed;
            StarCount = gameDesc.starCount;
            ResourceMultiplier = gameDesc.resourceMultiplier;
            IsSandboxMode = gameDesc.isSandboxMode;
            SavedThemeIds = gameDesc.savedThemeIds;
            IsNewPlayer = isNewPlayer;
            LocalPlayerData = localPlayerData;
            ModsSettings = modsSettings;
            ModsSettingsCount = settingsCount;
            SyncSoil = syncSoil;
            NumPlayers = numPlayers;
            DiscordPartyId = discordPartyId;
        }
    }
}
