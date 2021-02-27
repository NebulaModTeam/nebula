using LiteNetLib.Utils;
using UnityEngine;

namespace NebulaModel.DataStructures
{
    public struct Float4 : INetSerializable
    {
        public float x;
        public float y;
        public float z;
        public float w;

        public Float4(float x, float y, float z, float w)
        {
            this.x = x;
            this.y = y;
            this.z = z;
            this.w = w;
        }

        public Float4(Quaternion value)
        {
            x = value.x;
            y = value.y;
            z = value.z;
            w = value.w;
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(x);
            writer.Put(y);
            writer.Put(z);
            writer.Put(w);
        }

        public void Deserialize(NetDataReader reader)
        {
            x = reader.GetFloat();
            y = reader.GetFloat();
            z = reader.GetFloat();
            w = reader.GetFloat();
        }


        public override string ToString()
        {
            return $"x: {x}, y: {y}, z: {z}, w: {w}";
        }
    }
}
