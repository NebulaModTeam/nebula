namespace NebulaModel.Packets.PowerSystem
{
    public class PowerSystemUpdateResponse
    {
        // simulate 2d long array with 1d string array separated by ;
        public string[] EnergyCapacity { get; set; }
        public string[] EnergyRequired { get; set; }
        public string[] EnergyServed { get; set; }
        public string[] EnergyAccumulated { get; set; }
        public string[] EnergyExchanged { get; set; }
        public string[] ConsumerRatio { get; set; }
        public long[] PowerGenRegister { get; set; }
        public long[] PowerConRegister { get; set; }
        public long[] PowerDisRegister { get; set; }
        public long[] PowerChaRegister { get; set; }
        public long[] EnergyConsumption { get; set; }
        public PowerSystemUpdateResponse() { }
        public PowerSystemUpdateResponse(string[] energyCapacity,
                                        string[] energyRequired,
                                        string[] energyServed,
                                        string[] energyAccumulated,
                                        string[] energyExchanged,
                                        string[] consumerRatio,
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
