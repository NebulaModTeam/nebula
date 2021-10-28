using NebulaAPI;
using NebulaModel.Logger;
using NebulaModel.Networking.Serialization;
using System;
using Valve.Sockets;

namespace NebulaModel
{
    public abstract class NetworkProvider : IDisposable, INetworkProvider
    {
        public NetPacketProcessor PacketProcessor { get; protected set; }

        public IPlayerManager PlayerManager { get; }

        protected static NetworkingSockets sockets = null;
        protected static NetworkingUtils utils = null;
        protected static NetworkProvider provider = null;

        protected NetworkProvider(IPlayerManager playerManager)
        {
            provider = this;
            PacketProcessor = new NetPacketProcessor();
            PlayerManager = playerManager;
            if (sockets == null)
            {
                InitializeValveSockets();
            }
        }

        ~NetworkProvider()
        {
            if(provider == this)
                provider = null;
        }

        private void InitializeValveSockets()
        {
            Library.Initialize();

            sockets = new NetworkingSockets();
            utils = new NetworkingUtils();

            sockets.SetManualPollMode(true);

            utils.SetDebugCallback(DebugType.Everything, (DebugType type, string message) =>
            {
                Log.Info(message);
            });

            utils.SetStatusCallback((ref StatusInfo info) =>
            {
                provider.OnEvent(ref info);
            });

            // Set high speeds
            Configuration configSendRateMax = new Configuration();
            configSendRateMax.data.Int32 = 0x10000000;
            configSendRateMax.dataType = ConfigurationDataType.Int32;
            configSendRateMax.value = ConfigurationValue.SendRateMax;

            Configuration configSendRateMin = new Configuration();
            configSendRateMin.data.Int32 = 0x10000000;
            configSendRateMin.dataType = ConfigurationDataType.Int32;
            configSendRateMin.value = ConfigurationValue.SendRateMin;

            Configuration configSendBuffer = new Configuration();
            configSendBuffer.data.Int32 = 0x100000;
            configSendBuffer.dataType = ConfigurationDataType.Int32;
            configSendBuffer.value = ConfigurationValue.SendBufferSize;

            utils.SetConfigurationValue(configSendRateMax, ConfigurationScope.Global, new IntPtr());
            utils.SetConfigurationValue(configSendRateMin, ConfigurationScope.Global, new IntPtr());
            utils.SetConfigurationValue(configSendBuffer, ConfigurationScope.Global, new IntPtr());
        }

        protected abstract void OnEvent(ref StatusInfo info);

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
