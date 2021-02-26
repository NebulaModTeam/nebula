using NebulaModel.DataStructures;

namespace NebulaModel.Packets
{
    public class EntityTransformUpdate
    {
        public NebulaId Id { get; set; }
        public Float3 Position { get; set; }
        public Float3 Rotation { get; set; }
    }
}
