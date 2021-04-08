namespace NebulaModel.Packets.Factory.PowerExchanger
{
    public class PowerExchangerChangeModePacket
    {
        public int PowerExchangerIndex { get; set; }
        public int Mode { get; set; }

        public PowerExchangerChangeModePacket() { }

        public PowerExchangerChangeModePacket(int powerExchangerIndex, int mode)
        {
            PowerExchangerIndex = powerExchangerIndex;
            Mode = mode;
        }
    }
}
