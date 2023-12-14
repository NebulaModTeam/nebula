namespace NebulaAPI.GameState;

public interface IMultiplayerSession
{
    INetworkProvider Network { get; }
    ILocalPlayer LocalPlayer { get; }
    IFactoryManager Factories { get; }

    bool IsGameLoaded { get; }
}
