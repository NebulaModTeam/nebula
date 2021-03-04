using LiteNetLib;

namespace NebulaModel.GameLogic
{
    public static class LocalPlayer
    {
        public static bool IsMasterClient { get; set; }
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
