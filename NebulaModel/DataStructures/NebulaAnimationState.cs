using NebulaAPI;

namespace NebulaModel.DataStructures
{
    [RegisterNestedType]
    public struct NebulaAnimationState : INetSerializable
    {
        public float Weight { get; set; }
        public float Speed { get; set; }
        public bool Enabled { get; set; }

        public void Serialize(INetDataWriter writer)
        {
            writer.Put(Weight);
            writer.Put(Speed);
            writer.Put(Enabled);
        }

        public void Deserialize(INetDataReader reader)
        {
            Weight = reader.GetFloat();
            Speed = reader.GetFloat();
            Enabled = reader.GetBool();
        }
    }
}
