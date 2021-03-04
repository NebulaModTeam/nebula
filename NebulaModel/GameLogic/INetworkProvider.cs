using LiteNetLib;

namespace NebulaModel.GameLogic
{
    public interface INetworkProvider
    {
        void SendPacket<T>(T packet, DeliveryMethod deliveryMethod = DeliveryMethod.ReliableOrdered) where T : class, new();
    }
}
