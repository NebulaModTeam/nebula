using LiteNetLib.Utils;
using NebulaModel.Attributes;
using UnityEngine;

namespace NebulaModel.DataStructures
{
    [RegisterNestedType]
    public struct Float3 : INetSerializable
    {
        public float x;
        public float y;
        public float z;

        public Float3(float x, float y, float z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public Float3(Vector3 value)
        {
            x = value.x;
            y = value.y;
            z = value.z;
        }
        public Color ToColor()
        {
            return new Color(x, y, z);
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(x);
            writer.Put(y);
            writer.Put(z);
        }

        public void Deserialize(NetDataReader reader)
        {
            x = reader.GetFloat();
            y = reader.GetFloat();
            z = reader.GetFloat();
        }


        public override string ToString()
        {
            return $"(x: {x}, y: {y}, z: {z})";
        }
    }
}
