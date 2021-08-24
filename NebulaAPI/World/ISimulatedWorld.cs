// unset

namespace NebulaAPI
{
    /// <summary>
    /// Represent current world state
    /// </summary>
    public interface ISimulatedWorld
    {
        /// <summary>
        /// Is current game multiplayer
        /// </summary>
        bool Initialized { get; }
        bool IsGameLoaded { get; }
        bool IsPlayerJoining { get; }
    }
}