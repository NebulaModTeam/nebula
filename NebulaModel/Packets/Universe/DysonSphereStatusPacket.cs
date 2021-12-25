namespace NebulaModel.Packets.Universe
{
    public class DysonSphereStatusPacket
    {
        public int StarIndex { get; set; }
        public float GrossRadius { get; set; }
        public long EnergyReqCurrentTick { get; set; }
        public long EnergyGenCurrentTick { get; set; }

        public DysonSphereStatusPacket() {}
        public DysonSphereStatusPacket(DysonSphere dysonSphere) 
        {
            StarIndex = dysonSphere.starData.index;
            GrossRadius = dysonSphere.grossRadius;
            EnergyReqCurrentTick = dysonSphere.energyReqCurrentTick;
            EnergyGenCurrentTick = dysonSphere.energyGenCurrentTick;
        }
    }
}
