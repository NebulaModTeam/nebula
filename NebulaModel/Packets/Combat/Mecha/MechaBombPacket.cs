using NebulaAPI.DataStructures;
using UnityEngine;

namespace NebulaModel.Packets.Combat.Mecha;

public class MechaBombPacket
{
    public MechaBombPacket() { }

    public MechaBombPacket(ushort playerId, int nearStarId, in VectorLF3 uVelocity,
        in VectorLF3 uVel, in Vector3 uAgl, int protoId)
    {
        PlayerId = playerId;
        NearStarId = nearStarId;
        UVelocity = new Double3(uVelocity.x, uVelocity.y, uVelocity.z);
        UVel = new Double3(uVel.x, uVel.y, uVel.z);
        UAgl = new Float3(uAgl);
        ProtoId = protoId;
    }

    public ushort PlayerId { get; set; }
    public int NearStarId { get; set; }
    public Double3 UVelocity { get; set; }
    public Double3 UVel { get; set; }
    public Float3 UAgl { get; set; }
    public int ProtoId { get; set; }
}
