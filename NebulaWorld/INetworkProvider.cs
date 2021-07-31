using Mirror;

namespace NebulaWorld
{
    public interface INetworkProvider
    {
        void SendPacket<T>(T packet) where T : class, new();

        void SendPacketToLocalStar<T>(T packet) where T : class, new();

        void SendPacketToLocalPlanet<T>(T packet) where T : class, new();

        void SendPacketToPlanet<T>(T packet, int planetId) where T : class, new();
        void SendPacketToStar<T>(T packet, int starId) where T : class, new();
        void SendPacketToStarExclude<T>(T packet, int starId, NetworkConnection exclude) where T : class, new();

        void DestroySession();
    }
}
