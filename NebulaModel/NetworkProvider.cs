using NebulaAPI;
using NebulaModel.Networking;
using NebulaModel.Networking.Serialization;
using System;

namespace NebulaModel
{
    public abstract class NetworkProvider : IDisposable, INetworkProvider
    {
        public NetPacketProcessor PacketProcessor { get; protected set; }

        public readonly IPlayerManager PlayerManager;

        protected NetworkProvider(IPlayerManager playerManager)
        {
            PacketProcessor = new NetPacketProcessor();
            PlayerManager = playerManager;
        }

        public abstract void Start();

        public abstract void Stop();

        public abstract void Dispose();

        public abstract void SendPacket<T>(T packet) where T : class, new();

        public abstract void SendPacketToLocalStar<T>(T packet) where T : class, new();

        public abstract void SendPacketToLocalPlanet<T>(T packet) where T : class, new();

        public abstract void SendPacketToPlanet<T>(T packet, int planetId) where T : class, new();

        public abstract void SendPacketToStar<T>(T packet, int starId) where T : class, new();

        public abstract void SendPacketToStarExclude<T>(T packet, int starId, INebulaConnection exclude)
            where T : class, new();

        public abstract void Update();
    }
}
