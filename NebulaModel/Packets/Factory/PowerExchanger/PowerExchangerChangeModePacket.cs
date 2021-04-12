namespace NebulaModel.Packets.Factory.PowerExchanger
{
    public class PowerExchangerChangeModePacket
    {
        public int PowerExchangerIndex { get; set; }
        public int Mode { get; set; }
        public int FactoryIndex { get; set; }

        public PowerExchangerChangeModePacket() { }

        public PowerExchangerChangeModePacket(int powerExchangerIndex, int mode, int factoryIndex)
        {
            PowerExchangerIndex = powerExchangerIndex;
            Mode = mode;
            FactoryIndex = factoryIndex;
        }
    }
}
