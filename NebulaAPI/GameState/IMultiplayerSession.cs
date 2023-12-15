namespace NebulaAPI.GameState;

public interface IMultiplayerSession
{
    INetworkProvider Network { get; set; }
    ILocalPlayer LocalPlayer { get; set; }
    IFactoryManager Factories { get; set; }

    bool IsGameLoaded { get; set; }
}
