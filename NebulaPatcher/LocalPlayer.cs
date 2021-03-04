using LiteNetLib;
using NebulaClient.MonoBehaviours;
using NebulaHost.MonoBehaviours;

namespace NebulaClient
{
    public class LocalPlayer
    {
        public static bool IsMasterClient { get; protected set; }

        public static void SendPacket<T>(T packet, DeliveryMethod deliveryMethod = DeliveryMethod.ReliableOrdered) where T : class, new()
        {
            if (IsMasterClient)
            {
                MultiplayerHostSession.Instance?.SendPacket(packet, deliveryMethod);
            }
            else
            {
                MultiplayerClientSession.Instance?.SendPacket(packet, deliveryMethod);
            }
        }
    }
}
