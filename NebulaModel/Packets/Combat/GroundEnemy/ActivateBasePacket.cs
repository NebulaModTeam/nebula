using NebulaAPI.DataStructures;
using UnityEngine;

namespace NebulaModel.Packets.Combat.GroundEnemy;

public class ActivateBasePacket
{
    public ActivateBasePacket() { }

    public ActivateBasePacket(int planetId, int baseId)
    {
        PlanetId = planetId;
        BaseId = baseId;
    }

    public int PlanetId { get; set; }
    public int BaseId { get; set; }
}
