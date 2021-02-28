using NebulaModel.DataStructures;
using UnityEngine;

namespace NebulaModel.Packets
{
    public class Movement
    {
        public ushort PlayerId { get; set; }
        public Float3 Position { get; set; }
        public Float3 Rotation { get; set; }
        public Float3 BodyRotation { get; set; }

        public Movement() { }

        public Movement(Vector3 position, Vector3 rotation, Vector3 bodyRotation)
        {
            // This will be set for packet sent from server only
            PlayerId = 0;

            Position = new Float3(position);
            Rotation = new Float3(rotation);
            BodyRotation = new Float3(bodyRotation);
        }
    }
}
