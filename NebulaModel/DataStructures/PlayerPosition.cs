using UnityEngine;

namespace NebulaModel.DataStructures;

public class PlayerPosition
{
    public PlayerPosition(Vector3 position, int planetId)
    {
        Position = position;
        PlanetId = planetId;
    }

    public Vector3 Position { get; set; }
    public int PlanetId { get; set; }
}
