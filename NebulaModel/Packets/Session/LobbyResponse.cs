namespace NebulaModel.Packets.Session;

public class LobbyResponse
{
    public LobbyResponse() { }

    public LobbyResponse(in GameDesc gameDesc, byte[] combatSettingsData, byte[] modsSettings, int settingsCount, ushort numPlayers, string discordPartyId)
    {
        GalaxyAlgo = gameDesc.galaxyAlgo;
        GalaxySeed = gameDesc.galaxySeed;
        StarCount = gameDesc.starCount;
        ResourceMultiplier = gameDesc.resourceMultiplier;
        IsPeaceMode = gameDesc.isPeaceMode;
        IsSandboxMode = gameDesc.isSandboxMode;
        CombatSettingsData = combatSettingsData;
        SavedThemeIds = gameDesc.savedThemeIds;
        ModsSettings = modsSettings;
        ModsSettingsCount = settingsCount;
        NumPlayers = numPlayers;
        DiscordPartyId = discordPartyId;
    }

    public int GalaxyAlgo { get; set; }
    public int GalaxySeed { get; set; }
    public int StarCount { get; set; }
    public float ResourceMultiplier { get; set; }
    public bool IsPeaceMode { get; set; }
    public bool IsSandboxMode { get; set; }
    public int[] SavedThemeIds { get; set; }
    public byte[] CombatSettingsData { get; set; }
    public byte[] ModsSettings { get; set; }
    public int ModsSettingsCount { get; set; }
    public ushort NumPlayers { get; set; }
    public string DiscordPartyId { get; set; }
}
