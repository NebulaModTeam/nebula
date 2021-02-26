using NebulaModel.DataStructures;
using UnityEngine;

namespace NebulaModel.Packets
{
    public class PlayerSpawned
    {
        public ushort PlayerId { get; set; }
        public Float3 Position { get; set; }
        public Float3 Rotation { get; set; }

        public PlayerSpawned() { }

        public PlayerSpawned(Transform transform)
        {
            PlayerId = 0;
            Position = new Float3(transform.position);
            Rotation = new Float3(transform.eulerAngles);
        }

        public PlayerSpawned(ushort playerId, Transform transform)
        {
            PlayerId = playerId;
            Position = new Float3(transform.position);
            Rotation = new Float3(transform.eulerAngles);
        }

        public override string ToString()
        {
            return $"PlayerId: {PlayerId}, Position: {Position}, Rotation: {Rotation}";
        }
    }
}
