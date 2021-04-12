namespace NebulaModel.Packets.Factory.Laboratory
{
    public class LaboratoryUpdateStoragePacket
    {
        public int Value { get; set; }
        public int Index { get; set; }
        public int LabIndex { get; set; }
        public int FactoryIndex { get; set; }
        public LaboratoryUpdateStoragePacket() { }
        public LaboratoryUpdateStoragePacket(int value, int index, int labIndex, int factoryIndex)
        {
            Value = value;
            Index = index;
            LabIndex = labIndex;
            FactoryIndex = factoryIndex;
        }
    }
}
