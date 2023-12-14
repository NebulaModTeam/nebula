namespace NebulaModel.Packets.Session;

public class LobbyResponse
{
    public LobbyResponse() { }

    public LobbyResponse(in GameDesc gameDesc, byte[] modsSettings, int settingsCount, ushort numPlayers, string discordPartyId)
    {
        GalaxyAlgo = gameDesc.galaxyAlgo;
        GalaxySeed = gameDesc.galaxySeed;
        StarCount = gameDesc.starCount;
        ResourceMultiplier = gameDesc.resourceMultiplier;
        IsSandboxMode = gameDesc.isSandboxMode;
        SavedThemeIds = gameDesc.savedThemeIds;
        ModsSettings = modsSettings;
        ModsSettingsCount = settingsCount;
        NumPlayers = numPlayers;
        DiscordPartyId = discordPartyId;
    }

    public int GalaxyAlgo { get; }
    public int GalaxySeed { get; }
    public int StarCount { get; }
    public float ResourceMultiplier { get; }
    public bool IsSandboxMode { get; }
    public int[] SavedThemeIds { get; }
    public byte[] ModsSettings { get; }
    public int ModsSettingsCount { get; }
    public ushort NumPlayers { get; }
    public string DiscordPartyId { get; }
}
