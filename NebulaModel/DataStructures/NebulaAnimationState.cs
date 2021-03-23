using NebulaModel.Attributes;
using NebulaModel.Networking.Serialization;

namespace NebulaModel.DataStructures
{
    [RegisterNestedType]
    public struct NebulaAnimationState : INetSerializable
    {
        public float Weight { get; set; }
        public float Speed { get; set; }
        public bool Enabled { get; set; }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(Weight);
            writer.Put(Speed);
            writer.Put(Enabled);
        }

        public void Deserialize(NetDataReader reader)
        {
            Weight = reader.GetFloat();
            Speed = reader.GetFloat();
            Enabled = reader.GetBool();
        }
    }
}
