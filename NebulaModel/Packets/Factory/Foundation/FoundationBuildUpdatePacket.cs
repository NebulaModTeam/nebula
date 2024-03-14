#region

using NebulaAPI.DataStructures;
using UnityEngine;

#endregion

namespace NebulaModel.Packets.Factory.Foundation;

public class FoundationBuildUpdatePacket
{
    public FoundationBuildUpdatePacket() { }

    public FoundationBuildUpdatePacket(Vector3 center, float radius, int reformSize, bool veinBuried, float fade0)
    {
        Radius = radius;
        ReformSize = reformSize;
        VeinBuried = veinBuried;
        Fade0 = fade0;
        //Assume FlattenTerrainReform are all called in BuildTool_Reform
        var btr = GameMain.mainPlayer.controller.actionBuild.reformTool;
        ReformType = btr?.brushType ?? -1;
        ReformColor = btr?.brushColor ?? -1;
        PlanetId = GameMain.mainPlayer.planetId;
        GroundTestPos = new Float3(btr?.castGroundPos ?? Vector3.zero);
        if (center != btr?.reformCenterPoint) //Pit (circle)
        {
            ReformIndices = btr?.extraCircleIndices;
            ExtraCenter = center.ToFloat3();
            CirclePointCount = btr.circlePointCount;
            IsCircle = true;
        }
        else //Normal reform
        {
            ReformIndices = btr?.cursorIndices;
            ExtraCenter = new Float3(Vector3.zero);
            CirclePointCount = 0;
            IsCircle = false;
        }
    }

    public float Radius { get; set; }
    public int ReformSize { get; set; }
    public bool VeinBuried { get; set; }
    public float Fade0 { get; set; }
    public int ReformType { get; set; }
    public int ReformColor { get; set; }
    public int PlanetId { get; set; }
    public int[] ReformIndices { get; set; }
    public Float3 GroundTestPos { get; set; }
    public Float3 ExtraCenter { get; set; }
    public int CirclePointCount { get; set; }
    public bool IsCircle { get; set; }
}
