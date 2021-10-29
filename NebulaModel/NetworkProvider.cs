using NebulaAPI;
using NebulaModel.Logger;
using NebulaModel.Networking.Serialization;
using System;
using System.Threading;
using Valve.Sockets;

namespace NebulaModel
{
    public abstract class NetworkProvider : IDisposable, INetworkProvider
    {
        public NetPacketProcessor PacketProcessor { get; protected set; }

        public IPlayerManager PlayerManager { get; }

        protected static NetworkingSockets Sockets { get; private set; }
        
        protected static NetworkingUtils Utils { get; private set; }
        
        protected static NetworkProvider Provider { get; set; }

        protected static Thread Worker { get; private set; }

        protected static bool ShouldPoll { get; set; } = false;

        static NetworkProvider()
        {
            Library.Initialize();

            Sockets = new NetworkingSockets();
            Utils = new NetworkingUtils();

            Sockets.SetManualPollMode(true);

            Utils.SetDebugCallback(DebugType.Everything, (DebugType type, string message) =>
            {
                Log.Info(message);
            });

            // We have to store a static instance to the current NetworkProvider as this callback comes from native code and 
            // therefore has to be a flat cdecl call, it cannot capture anything or have any context
            Utils.SetStatusCallback((ref StatusInfo info) =>
            {
                Provider?.OnEvent(ref info);
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

            Utils.SetConfigurationValue(configSendRateMax, ConfigurationScope.Global, new IntPtr());
            Utils.SetConfigurationValue(configSendRateMin, ConfigurationScope.Global, new IntPtr());
            Utils.SetConfigurationValue(configSendBuffer, ConfigurationScope.Global, new IntPtr());

            Worker = new Thread(Poll) { Name = "Nebula Networking Worker" };
            Worker.Start();
        }

        private static void Poll()
        {
            while(true)
            {
                if(ShouldPoll)
                {
                    lock (Sockets)
                    {
                        Sockets.Poll(0);
                    }
                }
                Thread.Sleep(ShouldPoll ? 1 : 100);
            }
        }

        protected NetworkProvider(IPlayerManager playerManager)
        {
            Provider = this;
            PacketProcessor = new NetPacketProcessor();
            PlayerManager = playerManager;
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
