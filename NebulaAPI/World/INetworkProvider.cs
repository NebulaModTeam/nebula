// unset

using System;

namespace NebulaAPI
{
    public interface INetworkProvider : IDisposable
    {
        /// <summary>
        /// Send packet to Host (If ran on Client) or all Clients (If ran on Host)
        /// </summary>
        void SendPacket<T>(T packet) where T : class, new();
        /// <summary>
        /// Send packet to all Clients within current star system
        /// </summary>
        void SendPacketToLocalStar<T>(T packet) where T : class, new();
        /// <summary>
        /// Send packet to all Clients within current planet
        /// </summary>
        void SendPacketToLocalPlanet<T>(T packet) where T : class, new();
        
        /// <summary>
        /// Send packet to all Clients on a planet
        /// </summary>
        void SendPacketToPlanet<T>(T packet, int planetId) where T : class, new();
        /// <summary>
        /// Send packet to all Clients within star system
        /// </summary>
        void SendPacketToStar<T>(T packet, int starId) where T : class, new();
        
        void SendPacketToStarExclude<T>(T packet, int starId, INebulaConnection exclude) where T : class, new();
    }
}