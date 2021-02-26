using NebulaModel.DataStructures;
using UnityEngine;

namespace NebulaModel.Packets
{
    public class Movement
    {
        public ushort PlayerId { get; set; }
        public Float3 Position { get; set; }
        public Float3 Rotation { get; set; }

        public Movement() { }

        public Movement(ushort playerId, Float3 position, Float3 rotation)
        {
            PlayerId = playerId;
            Position = position;
            Rotation = rotation;
        }

        public Movement(ushort playerId, Transform transform)
        {
            PlayerId = playerId;
            Position = new Float3(transform.position);
            Rotation = new Float3(transform.eulerAngles);
        }

        public Movement(Float3 position, Float3 rotation)
        {
            PlayerId = 0;
            Position = position;
            Rotation = rotation;
        }

        public Movement(Transform transform)
        {
            PlayerId = 0;
            Position = new Float3(transform.position);
            Rotation = new Float3(transform.eulerAngles);
        }

        public override string ToString()
        {
            return $"PlayerId: {PlayerId}, Position: {Position}, Rotation: {Rotation}";
        }
    }
}
