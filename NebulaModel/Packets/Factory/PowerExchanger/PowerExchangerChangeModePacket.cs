namespace NebulaModel.Packets.Factory.PowerExchanger
{
    public class PowerExchangerChangeModePacket
    {
        public int PowerExchangerIndex { get; set; }
        public int Mode { get; set; }
        public int PlanetId { get; set; }

        public PowerExchangerChangeModePacket() { }

        public PowerExchangerChangeModePacket(int powerExchangerIndex, int mode, int planetId)
        {
            PowerExchangerIndex = powerExchangerIndex;
            Mode = mode;
            PlanetId = planetId;
        }
    }
}
