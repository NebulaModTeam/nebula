namespace NebulaModel.Packets.Factory.Laboratory
{
    public class LaboratoryUpdateCubesPacket
    {
        public int Value { get; set; }
        public int Index { get; set; }
        public int LabIndex { get; set; }
        public int PlanetId { get; set; }
        public LaboratoryUpdateCubesPacket() { }
        public LaboratoryUpdateCubesPacket(int value, int index, int labIndex, int planetId)
        {
            Value = value;
            Index = index;
            LabIndex = labIndex;
            PlanetId = planetId;
        }
    }
}
