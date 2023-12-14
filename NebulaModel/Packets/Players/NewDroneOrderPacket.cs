#region

using NebulaAPI.DataStructures;
using UnityEngine;

#endregion

namespace NebulaModel.Packets.Players;

public class NewDroneOrderPacket
{
    public NewDroneOrderPacket() { }

    public NewDroneOrderPacket(int planetId, int droneId, int entityId, ushort playerId, int stage, int priority,
        Vector3 entityPos)
    {
        PlanetId = planetId;
        DroneId = droneId;
        EntityId = entityId;
        PlayerId = playerId;
        Stage = stage;
        Priority = priority;
        EntityPos = entityPos.ToFloat3();
    }

    public int PlanetId { get; }
    public int DroneId { get; }
    public int EntityId { get; }
    public ushort PlayerId { get; }
    public int Stage { get; }
    public int Priority { get; }
    public Float3 EntityPos { get; }
}
