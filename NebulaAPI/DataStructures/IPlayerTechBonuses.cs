#region

using NebulaAPI.Interfaces;

#endregion

namespace NebulaAPI.DataStructures;

public interface IPlayerTechBonuses : INetSerializable
{
    double coreEnergyCap { get; set; }
    double corePowerGen { get; set; }
    double reactorPowerGen { get; set; }
    double walkPower { get; set; }
    double jumpEnergy { get; set; }
    double thrustPowerPerAcc { get; set; }
    double warpKeepingPowerPerSpeed { get; set; }
    double warpStartPowerPerSpeed { get; set; }
    double miningPower { get; set; }
    double replicatePower { get; set; }
    double researchPower { get; set; }
    double droneEjectEnergy { get; set; }
    double droneEnergyPerMeter { get; set; }
    int coreLevel { get; set; }
    int thrusterLevel { get; set; }
    float miningSpeed { get; set; }
    float replicateSpeed { get; set; }
    float walkSpeed { get; set; }
    float jumpSpeed { get; set; }
    float maxSailSpeed { get; set; }
    float maxWarpSpeed { get; set; }
    float buildArea { get; set; }
    int droneCount { get; set; }
    int inventorySize { get; set; }
    bool deliveryPackageUnlocked { get; set; }
    int deliveryPackageColCount { get; set; }
    int deliveryPackageStackSizeMultiplier { get; set; }
    double instantBuildEnergy { get; set; }
    int hpMaxUpgrade { get; set; }
    bool energyShieldUnlocked { get; set; }
    float energyShieldRadius { get; set; }
    long energyShieldCapacity { get; set; }
    long laserEnergyCapacity { get; set; }
    float laserLocalAttackRange { get; set; }
    float laserSpaceAttackRange { get; set; }
    int laserLocalEnergyCost { get; set; }
    int laserSpaceEnergyCost { get; set; }
    int laserLocalDamage { get; set; }
    int laserSpaceDamage { get; set; }
    int groundFleetCount { get; set; }
    int spaceFleetCount { get; set; }
}
