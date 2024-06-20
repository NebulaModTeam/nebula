using NebulaAPI.DataStructures;
using UnityEngine;

namespace NebulaModel.Packets.Factory.Foundation;

public class VeinPosUpdatePacket
{
    public VeinPosUpdatePacket() { }

    public VeinPosUpdatePacket(int planetId, int veinId, Vector3 pos)
    {
        PlanetId = planetId;
        VeinId = veinId;
        Pos = new Float3(pos);
    }

    public int PlanetId { get; set; }
    public int VeinId { get; set; }
    public Float3 Pos { get; set; }
}
