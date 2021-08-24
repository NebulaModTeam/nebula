// unset

namespace NebulaAPI
{
    /// <summary>
    /// Represents data about factory
    /// </summary>
    public interface IFactoryManager
    {
        /// <summary>
        /// Did we receive a packet?
        /// </summary>
        IToggle IsIncomingRequest { get; }

        int PacketAuthor { get; set; }

        int TargetPlanet { get; set; }

        PlanetFactory EventFactory { get; set; }

        /// <summary>
        /// Request to load planet
        /// </summary>
        void AddPlanetTimer(int planetId);
    }
}