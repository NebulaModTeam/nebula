namespace NebulaModel.Packets.Factory.Laboratory
{
    public class LaboratoryUpdateStoragePacket
    {
        public int Value { get; set; }
        public int Index { get; set; }
        public int LabIndex { get; set; }
        public LaboratoryUpdateStoragePacket() { }
        public LaboratoryUpdateStoragePacket(int value, int index, int labIndex)
        {
            this.Value = value;
            this.Index = index;
            this.LabIndex = labIndex;
        }
    }
}
