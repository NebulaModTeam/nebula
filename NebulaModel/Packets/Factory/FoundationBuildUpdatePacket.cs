using NebulaModel.DataStructures;
using UnityEngine;

namespace NebulaModel.Packets.Factory
{
    public class FoundationBuildUpdatePacket
    {
        public float Radius { get; set; }
        public int ReformSize { get; set; }
        public bool VeinBuried { get; set; }
        public float Fade0 { get; set; }
        public int ReformType { get; set; }
        public int ReformColor { get; set; }
        public int PlanetId { get; set; }
        public int[] ReformIndices { get; set; }
        public Float3 GroundTestPos { get; set; }

        public FoundationBuildUpdatePacket() { }
        public FoundationBuildUpdatePacket(float radius, int reformSize, bool veinBuried, float fade0)
        {
            Radius = radius;
            ReformSize = reformSize;
            VeinBuried = veinBuried;
            Fade0 = fade0;
            PlayerAction_Build pab = GameMain.mainPlayer.controller?.actionBuild;
            ReformType = pab?.reformType ?? -1;
            ReformColor = pab?.reformColor ?? -1;
            PlanetId = GameMain.mainPlayer.planetId;
            ReformIndices = pab?.reformIndices;
            GroundTestPos = new Float3(pab?.groundTestPos ?? Vector3.zero);
        }
    }
}
