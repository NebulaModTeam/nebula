// unset

namespace NebulaAPI
{
    public interface INebulaPlayer
    {
        bool IsMasterClient { get; }
        ushort PlayerId { get; }

        void SendPacket<T>(T packet) where T : class, new();
        void SendPacketToLocalStar<T>(T packet) where T : class, new();
        void SendPacketToLocalPlanet<T>(T packet) where T : class, new();
        void SendPacketToStar<T>(T packet, int starId) where T : class, new();
        void SendPacketToStarExclude<T>(T packet, int starId, INebulaConnection exclude) where T : class, new();
    }
}