using NebulaModel.DataStructures;
using UnityEngine;

namespace NebulaModel.Packets.Players
{
    public class NewDroneOrderPacket
    {
        public int PlanetId { get; set; }
        public int DroneId { get; set; }
        public int EntityId { get; set; }
        public ushort PlayerId { get; set; }
        public int Stage { get; set; }
        public int Priority { get; set; }
        public Float3 EntityPos { get; set; }

        public NewDroneOrderPacket() { }
        public NewDroneOrderPacket(int planetId, int droneId, int entityId, ushort playerId, int stage, int priority, Vector3 entityPos)
        {
            PlanetId = planetId;
            DroneId = droneId;
            EntityId = entityId;
            PlayerId = playerId;
            Stage = stage;
            Priority = priority;
            EntityPos = entityPos.ToFloat3();
        }
    }
}
