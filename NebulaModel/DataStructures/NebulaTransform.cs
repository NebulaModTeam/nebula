using LiteNetLib.Utils;
using UnityEngine;

namespace NebulaModel.DataStructures
{
    public struct NebulaTransform : INetSerializable
    {
        public Float3 Position { get; set; }
        public Float3 Rotation { get; set; }
        public Float3 Scale { get; set; }

        public NebulaTransform(Transform transform)
        {
            Position = new Float3(transform.position);
            Rotation = new Float3(transform.eulerAngles);
            Scale = new Float3(transform.localScale);
        }

        public void Serialize(NetDataWriter writer)
        {
            Position.Serialize(writer);
            Rotation.Serialize(writer);
            Scale.Serialize(writer);
        }

        public void Deserialize(NetDataReader reader)
        {
            Position = new Float3(reader.GetFloat(), reader.GetFloat(), reader.GetFloat());
            Rotation = new Float3(reader.GetFloat(), reader.GetFloat(), reader.GetFloat());
            Scale = new Float3(reader.GetFloat(), reader.GetFloat(), reader.GetFloat());
        }
    }
}
