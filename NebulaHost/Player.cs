using LiteNetLib;
using NebulaModel.DataStructures;
using NebulaModel.Networking;
using NebulaModel.Packets.Players;

namespace NebulaHost
{
    public class Player
    {
        public NebulaConnection connection { get; set; }
        public ushort Id { get; }
        public Float3 Position { get; set; }
        public Float3 Rotation { get; set; }
        public Float3 BodyRotation { get; set; }
        public Float3 PlayerColor { get; set; }

        public Player(NebulaConnection connection, ushort playerId)
        {
            this.connection = connection;
            Id = playerId;
        }

        public void SendPacket<T>(T packet, DeliveryMethod deliveryMethod = DeliveryMethod.ReliableOrdered) where T : class, new()
        {
            connection.SendPacket(packet, deliveryMethod);
        }

        public void SetPosition(PlayerMovement packet)
        {
            Position = packet.Position;
            Rotation = packet.Rotation;
            BodyRotation = packet.BodyRotation;
        }
    }
}
