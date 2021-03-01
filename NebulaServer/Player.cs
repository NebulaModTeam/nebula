using LiteNetLib;
using NebulaModel.DataStructures;
using NebulaModel.Networking;
using NebulaModel.Packets.Players;

namespace NebulaServer
{
    public class Player
    {
        public NebulaConnection connection { get; set; }
        public ushort Id => connection.Id;
        public Float3 RootPosition { get; set; }
        public Float3 RootRotation { get; set; }
        public Float3 BodyRotation { get; set; }

        public Player(NebulaConnection connection)
        {
            this.connection = connection;
        }

        public void SendPacket<T>(T packet, DeliveryMethod deliveryMethod = DeliveryMethod.ReliableOrdered) where T : class, new()
        {
            connection.SendPacket(packet, deliveryMethod);
        }

        public void UpdatePosition(Movement packet)
        {
            RootPosition = packet.Position;
            RootRotation = packet.Rotation;
            BodyRotation = packet.BodyRotation;
        }
    }
}
