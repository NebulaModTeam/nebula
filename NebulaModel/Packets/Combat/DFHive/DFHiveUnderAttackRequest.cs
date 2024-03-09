using NebulaAPI.DataStructures;

namespace NebulaModel.Packets.Combat.DFHive;

public class DFHiveUnderAttackRequest
{
    public DFHiveUnderAttackRequest() { }

    public DFHiveUnderAttackRequest(int hiveAstroId, ref VectorLF3 centerUPos, float radius)
    {
        HiveAstroId = hiveAstroId;
        CenterUPos = new Float3((float)centerUPos.x, (float)centerUPos.y, (float)centerUPos.z);
        Radius = radius;
    }

    public int HiveAstroId { get; set; }
    public Float3 CenterUPos { get; set; }
    public float Radius { get; set; }
}
