using NebulaModel.DataStructures;
using UnityEngine;

namespace NebulaModel.Packets
{
    public class Movement
    {
        public ushort PlayerId { get; set; }
        public NebulaTransform Transform { get; set; }
        public NebulaTransform ModelTransform { get; set; }

        public Movement() { }

        public Movement(Transform transform, Transform modelTransform)
        {
            PlayerId = 0;
            Transform = new NebulaTransform(transform);
            ModelTransform = new NebulaTransform(modelTransform);
        }
    }
}
