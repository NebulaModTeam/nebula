namespace NebulaAPI;

public interface IPlayerTechBonuses : INetSerializable
{
    double coreEnergyCap { get; }
    double corePowerGen { get; }
    double reactorPowerGen { get; }
    double walkPower { get; }
    double jumpEnergy { get; }
    double thrustPowerPerAcc { get; }
    double warpKeepingPowerPerSpeed { get; }
    double warpStartPowerPerSpeed { get; }
    double miningPower { get; }
    double replicatePower { get; }
    double researchPower { get; }
    double droneEjectEnergy { get; }
    double droneEnergyPerMeter { get; }
    int coreLevel { get; }
    int thrusterLevel { get; }
    float miningSpeed { get; }
    float replicateSpeed { get; }
    float walkSpeed { get; }
    float jumpSpeed { get; }
    float maxSailSpeed { get; }
    float maxWarpSpeed { get; }
    float buildArea { get; }
    int droneCount { get; }
    float droneSpeed { get; }
    int droneMovement { get; }
    int inventorySize { get; }
    bool deliveryPackageUnlocked { get; }
    int deliveryPackageColCount { get; set; }
    int deliveryPackageStackSizeMultiplier { get; }
}
