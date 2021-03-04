using LiteNetLib;

namespace NebulaWorld
{
    public static class LocalPlayer
    {
        public static bool IsMasterClient { get; set; }
        public static ushort PlayerId { get; set; }

        private static INetworkProvider networkProvider;

        public static void SetNetworkProvider(INetworkProvider provider)
        {
            networkProvider = provider;
        }

        public static void SendPacket<T>(T packet, DeliveryMethod deliveryMethod = DeliveryMethod.ReliableOrdered) where T : class, new()
        {
            networkProvider?.SendPacket(packet, deliveryMethod);
        }
    }
}
