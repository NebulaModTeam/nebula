#region

using NebulaModel.DataStructures;

#endregion

namespace NebulaModel.Packets.Session;

public class HandshakeResponse
{
    public HandshakeResponse() { }

    public HandshakeResponse(in GameDesc gameDesc, bool isNewPlayer, PlayerData localPlayerData, byte[] modsSettings,
        int settingsCount, bool syncSoil, ushort numPlayers, string discordPartyId)
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

    public int GalaxyAlgo { get; }
    public int GalaxySeed { get; }
    public int StarCount { get; }
    public float ResourceMultiplier { get; }
    public bool IsSandboxMode { get; }
    public int[] SavedThemeIds { get; }
    public bool IsNewPlayer { get; }
    public PlayerData LocalPlayerData { get; }
    public byte[] ModsSettings { get; }
    public int ModsSettingsCount { get; }
    public bool SyncSoil { get; }
    public ushort NumPlayers { get; }
    public string DiscordPartyId { get; }
}
