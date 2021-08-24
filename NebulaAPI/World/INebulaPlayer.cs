// unset

namespace NebulaAPI
{
    /// <summary>
    /// Represents local player. Allows to send packets.
    /// </summary>
    public interface INebulaPlayer
    {
        /// <summary>
        /// Is this a Host
        /// </summary>
        bool IsMasterClient { get; }
        ushort PlayerId { get; }

        /// <summary>
        /// Send packet to Host (If ran on Client) or all Clients (If ran on Host)
        /// </summary>
        void SendPacket<T>(T packet) where T : class, new();
        /// <summary>
        /// Send packet to all Clients within current star system
        /// </summary>
        void SendPacketToLocalStar<T>(T packet) where T : class, new();
        /// <summary>
        /// Send packet to all Clients on a planet
        /// </summary>
        void SendPacketToLocalPlanet<T>(T packet) where T : class, new();
        /// <summary>
        /// Send packet to all Clients within star system
        /// </summary>
        void SendPacketToStar<T>(T packet, int starId) where T : class, new();
        void SendPacketToStarExclude<T>(T packet, int starId, INebulaConnection exclude) where T : class, new();
    }
}