using System.Security.Policy;

namespace NebulaModel.Packets.Combat;

public class CombatStatDamagePacket
{
    public CombatStatDamagePacket() { }

    public CombatStatDamagePacket(int damage, int slice, in SkillTarget target, in SkillTarget caster)
    {
        Damage = damage;
        Slice = slice;
        TargetType = (short)target.type;
        TargetId = target.id;
        TargetAstroId = target.astroId;
        CasterType = (short)caster.type;
        CasterId = caster.id;
        CasterAstroId = caster.astroId;
    }

    public int Damage { get; set; }
    public int Slice { get; set; }
    public short TargetType { get; set; }
    public int TargetId { get; set; }
    public int TargetAstroId { get; set; }
    public short CasterType { get; set; }
    public int CasterId { get; set; }
    public int CasterAstroId { get; set; }
}
