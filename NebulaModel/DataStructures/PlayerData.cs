using LiteNetLib.Utils;
using NebulaModel.Attributes;

namespace NebulaModel.DataStructures
{
    [RegisterNestedType]
    public class PlayerData : INetSerializable
    {
        public ushort PlayerId { get; set; }
        public Float3 Color { get; set; }
        public Float3 Position { get; set; }
        public Float3 Rotation { get; set; }
        public Float3 BodyRotation { get; set; }

        public PlayerData() { }

        public PlayerData(ushort playerId, Float3 color, Float3 position = new Float3(), Float3 rotation = new Float3(), Float3 bodyRotation = new Float3())
        {
            PlayerId = playerId;
            Color = color;
            Position = position;
            Rotation = rotation;
            BodyRotation = bodyRotation;
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(PlayerId);
            Color.Serialize(writer);
            Position.Serialize(writer);
            Rotation.Serialize(writer);
            BodyRotation.Serialize(writer);
        }

        public void Deserialize(NetDataReader reader)
        {
            PlayerId = reader.GetUShort();
            Color = reader.GetFloat3();
            Position = reader.GetFloat3();
            Rotation = reader.GetFloat3();
            BodyRotation = reader.GetFloat3();
        }
    }
}
