using LiteNetLib;
using NebulaModel.DataStructures;
using NebulaModel.Networking;
using NebulaModel.Packets.Players;

namespace NebulaHost
{
    public class Player
    {
        public NebulaConnection connection { get; set; }
        public ushort Id => connection.Id;
        public bool IsMasterClient { get; set; }
        public Float3 RootPosition { get; set; }
        public Float3 RootRotation { get; set; }
        public Float3 BodyRotation { get; set; }
        public Float3 PlayerColor { get; set; }

        public Player(NebulaConnection connection)
        {
            this.connection = connection;
        }

        public void SendPacket<T>(T packet, DeliveryMethod deliveryMethod = DeliveryMethod.ReliableOrdered) where T : class, new()
        {
            connection.SendPacket(packet, deliveryMethod);
        }

        public void SetPosition(PlayerMovement packet)
        {
            RootPosition = packet.Position;
            RootRotation = packet.Rotation;
            BodyRotation = packet.BodyRotation;
        }
    }
}
