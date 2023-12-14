#region

using NebulaAPI.DataStructures;
using UnityEngine;

#endregion

namespace NebulaModel.Packets.Factory.Foundation;

public class FoundationBuildUpdatePacket
{
    public FoundationBuildUpdatePacket() { }

    public FoundationBuildUpdatePacket(float radius, int reformSize, bool veinBuried, float fade0)
    {
        Radius = radius;
        ReformSize = reformSize;
        VeinBuried = veinBuried;
        Fade0 = fade0;
        var btr = GameMain.mainPlayer.controller.actionBuild.reformTool;
        ReformType = btr?.brushType ?? -1;
        ReformColor = btr?.brushColor ?? -1;
        PlanetId = GameMain.mainPlayer.planetId;
        ReformIndices = btr?.cursorIndices;
        GroundTestPos = new Float3(btr?.castGroundPos ?? Vector3.zero);
    }

    public float Radius { get; }
    public int ReformSize { get; }
    public bool VeinBuried { get; }
    public float Fade0 { get; }
    public int ReformType { get; }
    public int ReformColor { get; }
    public int PlanetId { get; }
    public int[] ReformIndices { get; }
    public Float3 GroundTestPos { get; }
}
