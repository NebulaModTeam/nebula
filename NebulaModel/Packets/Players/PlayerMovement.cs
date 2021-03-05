using NebulaModel.DataStructures;
using UnityEngine;

namespace NebulaModel.Packets.Players
{
    public class PlayerMovement
    {
        public ushort PlayerId { get; set; }
        public Float3 Position { get; set; }
        public Float3 Rotation { get; set; }
        public Float3 BodyRotation { get; set; }

        public PlayerMovement() { }

        public PlayerMovement(ushort playerId, Vector3 position, Vector3 rotation, Vector3 bodyRotation)
        {
            PlayerId = playerId;
            Position = new Float3(position);
            Rotation = new Float3(rotation);
            BodyRotation = new Float3(bodyRotation);
        }
    }
}
