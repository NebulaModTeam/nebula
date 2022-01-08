namespace NebulaModel.Packets.PowerSystem
{
    public class PowerSystemUpdateResponse
    {
        // simulate 2d long array with 1d string array separated by ;
        public long[][] EnergyCapacity { get; set; }
        public long[][] EnergyRequired { get; set; }
        public long[][] EnergyServed { get; set; }
        public long[][] EnergyAccumulated { get; set; }
        public long[][] EnergyExchanged { get; set; }
        public double[][] ConsumerRatio { get; set; }
        public long[] PowerGenRegister { get; set; }
        public long[] PowerConRegister { get; set; }
        public long[] PowerDisRegister { get; set; }
        public long[] PowerChaRegister { get; set; }
        public long[] EnergyConsumption { get; set; }
        public PowerSystemUpdateResponse() { }
        public PowerSystemUpdateResponse(long[][] energyCapacity,
                                        long[][] energyRequired,
                                        long[][] energyServed,
                                        long[][] energyAccumulated,
                                        long[][] energyExchanged,
                                        double[][] consumerRatio,
                                        long[] powerGenRegister,
                                        long[] powerConRegister,
                                        long[] powerDisRegister,
                                        long[] powerChaRegister,
                                        long[] energyConsumption)
        {
            EnergyCapacity = energyCapacity;
            EnergyRequired = energyRequired;
            EnergyServed = energyServed;
            EnergyAccumulated = energyAccumulated;
            EnergyExchanged = energyExchanged;
            ConsumerRatio = consumerRatio;
            PowerGenRegister = powerGenRegister;
            PowerConRegister = powerConRegister;
            PowerDisRegister = powerDisRegister;
            PowerChaRegister = powerChaRegister;
            EnergyConsumption = energyConsumption;
        }
    }
}
