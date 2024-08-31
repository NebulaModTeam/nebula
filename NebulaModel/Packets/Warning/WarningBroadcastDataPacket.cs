using NebulaAPI.DataStructures;
using UnityEngine;

namespace NebulaModel.Packets.Warning;

public class WarningBroadcastDataPacket
{
    public WarningBroadcastDataPacket() { }
    public WarningBroadcastDataPacket(EBroadcastVocal vocal, int astroId, int content, in Vector3 lpos)
    {
        Vocal = (short)vocal;
        AstroId = astroId;
        Content = content;
        Lpos = new Float3(lpos);
    }

    public short Vocal { get; set; }
    public int AstroId { get; set; }
    public int Content { get; set; }
    public Float3 Lpos { get; set; }
}
