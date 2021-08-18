// unset

namespace NebulaAPI
{
    public interface ISimulatedWorld
    {
        bool Initialized { get; }
        bool IsGameLoaded { get; }
        bool IsPlayerJoining { get; }
    }
}