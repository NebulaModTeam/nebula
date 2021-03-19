using NebulaModel.DataStructures;
using UnityEngine;

namespace NebulaModel.Packets.Players
{
    public class PlayerMovement
    {
        public ushort PlayerId { get; set; }
        public int LocalPlanetId { get; set; }
        public Double3 Position { get; set; }
        public Float3 Rotation { get; set; }
        public Float3 BodyRotation { get; set; }

        public PlayerMovement() { }

        public PlayerMovement(ushort playerId, int localPlanetId, Double3 position, Float3 rotation, Float3 bodyRotation)
        {
            PlayerId = playerId;
            LocalPlanetId = localPlanetId;
            Position = position;
            Rotation = rotation;
            BodyRotation = bodyRotation;
        }
    }
}
