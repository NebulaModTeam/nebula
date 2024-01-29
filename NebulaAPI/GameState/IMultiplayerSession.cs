namespace NebulaAPI.GameState;

public interface IMultiplayerSession
{
    INetworkProvider Network { get; set; }
    ILocalPlayer LocalPlayer { get; set; }
    IFactoryManager Factories { get; set; }
    bool IsDedicated { get; }
    bool IsServer { get; }
    bool IsClient { get; }
    bool IsGameLoaded { get; set; }
}
