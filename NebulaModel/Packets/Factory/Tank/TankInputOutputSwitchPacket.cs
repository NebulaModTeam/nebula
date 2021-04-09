namespace NebulaModel.Packets.Factory.Tank
{
    public class TankInputOutputSwitchPacket
    {
        public int TankIndex { get; set; }
        public bool IsInput { get; set; }
        public bool IsClosed { get; set; }
        public int FactoryIndex { get; set; }

        public TankInputOutputSwitchPacket() { }

        public TankInputOutputSwitchPacket(int tankIndex, bool isInput, bool inClosed, int factoryIndex)
        {
            TankIndex = tankIndex;
            IsInput = isInput;
            IsClosed = inClosed;
            FactoryIndex = factoryIndex;
        }
    }
}
