#region

using NebulaAPI.DataStructures;
using NebulaAPI.Interfaces;

#endregion

namespace NebulaModel.DataStructures;

public class PlayerTechBonuses : IPlayerTechBonuses
{
    public PlayerTechBonuses() { }

    public PlayerTechBonuses(Mecha source)
    {
        coreEnergyCap = source.coreEnergyCap;
        corePowerGen = source.corePowerGen;
        reactorPowerGen = source.reactorPowerGen;
        walkPower = source.walkPower;
        jumpEnergy = source.jumpEnergy;
        thrustPowerPerAcc = source.thrustPowerPerAcc;
        warpKeepingPowerPerSpeed = source.warpKeepingPowerPerSpeed;
        warpStartPowerPerSpeed = source.warpStartPowerPerSpeed;
        miningPower = source.miningPower;
        replicatePower = source.replicatePower;
        researchPower = source.researchPower;
        droneEjectEnergy = source.droneEjectEnergy;
        droneEnergyPerMeter = source.droneEnergyPerMeter;
        coreLevel = source.coreLevel;
        thrusterLevel = source.thrusterLevel;
        miningSpeed = source.miningSpeed;
        replicateSpeed = source.replicateSpeed;
        walkSpeed = source.walkSpeed;
        jumpSpeed = source.jumpSpeed;
        maxSailSpeed = source.maxSailSpeed;
        maxWarpSpeed = source.maxWarpSpeed;
        buildArea = source.buildArea;
        inventorySize = source.player.package.size;
        deliveryPackageUnlocked = source.player.deliveryPackage.unlocked;
        deliveryPackageColCount = source.player.deliveryPackage.colCount;
        deliveryPackageStackSizeMultiplier = source.player.deliveryPackage.stackSizeMultiplier;
        droneCount = source.constructionModule.droneCount;
        instantBuildEnergy = source.instantBuildEnergy;
        hpMaxUpgrade = source.hpMaxUpgrade;
        energyShieldUnlocked = source.energyShieldUnlocked;
        energyShieldRadius = source.energyShieldRadius;
        energyShieldCapacity = source.energyShieldCapacity;
        laserEnergyCapacity = source.laserEnergyCapacity;
        laserLocalAttackRange = source.laserLocalAttackRange;
        laserSpaceAttackRange = source.laserSpaceAttackRange;
        laserLocalEnergyCost = source.laserLocalEnergyCost;
        laserSpaceEnergyCost = source.laserSpaceEnergyCost;
        laserLocalDamage = source.laserLocalDamage;
        laserSpaceDamage = source.laserSpaceDamage;
        groundFleetCount = source.groundCombatModule.fleetCount;
        spaceFleetCount = source.spaceCombatModule.fleetCount;
    }

    public double coreEnergyCap { get; set; }
    public double corePowerGen { get; set; }
    public double reactorPowerGen { get; set; }
    public double walkPower { get; set; }
    public double jumpEnergy { get; set; }
    public double thrustPowerPerAcc { get; set; }
    public double warpKeepingPowerPerSpeed { get; set; }
    public double warpStartPowerPerSpeed { get; set; }
    public double miningPower { get; set; }
    public double replicatePower { get; set; }
    public double researchPower { get; set; }
    public double droneEjectEnergy { get; set; }
    public double droneEnergyPerMeter { get; set; }
    public int coreLevel { get; set; }
    public int thrusterLevel { get; set; }
    public float miningSpeed { get; set; }
    public float replicateSpeed { get; set; }
    public float walkSpeed { get; set; }
    public float jumpSpeed { get; set; }
    public float maxSailSpeed { get; set; }
    public float maxWarpSpeed { get; set; }
    public float buildArea { get; set; }
    public int droneCount { get; set; }
    public int inventorySize { get; set; }
    public bool deliveryPackageUnlocked { get; set; }
    public int deliveryPackageColCount { get; set; }
    public int deliveryPackageStackSizeMultiplier { get; set; }
    public double instantBuildEnergy { get; set; }
    public int hpMaxUpgrade { get; set; }
    public bool energyShieldUnlocked { get; set; }
    public float energyShieldRadius { get; set; }
    public long energyShieldCapacity { get; set; }
    public long laserEnergyCapacity { get; set; }
    public float laserLocalAttackRange { get; set; }
    public float laserSpaceAttackRange { get; set; }
    public int laserLocalEnergyCost { get; set; }
    public int laserSpaceEnergyCost { get; set; }
    public int laserLocalDamage { get; set; }
    public int laserSpaceDamage { get; set; }
    public int groundFleetCount { get; set; }
    public int spaceFleetCount { get; set; }

    public void Serialize(INetDataWriter writer)
    {
        writer.Put(coreEnergyCap);
        writer.Put(corePowerGen);
        writer.Put(reactorPowerGen);
        writer.Put(walkPower);
        writer.Put(jumpEnergy);
        writer.Put(thrustPowerPerAcc);
        writer.Put(warpKeepingPowerPerSpeed);
        writer.Put(warpStartPowerPerSpeed);
        writer.Put(miningPower);
        writer.Put(replicatePower);
        writer.Put(researchPower);
        writer.Put(droneEjectEnergy);
        writer.Put(droneEnergyPerMeter);
        writer.Put(coreLevel);
        writer.Put(thrusterLevel);
        writer.Put(miningSpeed);
        writer.Put(replicateSpeed);
        writer.Put(walkSpeed);
        writer.Put(jumpSpeed);
        writer.Put(maxSailSpeed);
        writer.Put(maxWarpSpeed);
        writer.Put(buildArea);
        writer.Put(droneCount);
        writer.Put(inventorySize);
        writer.Put(deliveryPackageUnlocked);
        writer.Put(deliveryPackageColCount);
        writer.Put(deliveryPackageStackSizeMultiplier);
        writer.Put(instantBuildEnergy);
        writer.Put(hpMaxUpgrade);
        writer.Put(energyShieldUnlocked);
        writer.Put(energyShieldRadius);
        writer.Put(energyShieldCapacity);
        writer.Put(laserEnergyCapacity);
        writer.Put(laserLocalAttackRange);
        writer.Put(laserSpaceAttackRange);
        writer.Put(laserLocalEnergyCost);
        writer.Put(laserSpaceEnergyCost);
        writer.Put(laserLocalDamage);
        writer.Put(laserSpaceDamage);
        writer.Put(groundFleetCount);
        writer.Put(spaceFleetCount);
    }

    public void Deserialize(INetDataReader reader)
    {
        coreEnergyCap = reader.GetDouble();
        corePowerGen = reader.GetDouble();
        reactorPowerGen = reader.GetDouble();
        walkPower = reader.GetDouble();
        jumpEnergy = reader.GetDouble();
        thrustPowerPerAcc = reader.GetDouble();
        warpKeepingPowerPerSpeed = reader.GetDouble();
        warpStartPowerPerSpeed = reader.GetDouble();
        miningPower = reader.GetDouble();
        replicatePower = reader.GetDouble();
        researchPower = reader.GetDouble();
        droneEjectEnergy = reader.GetDouble();
        droneEnergyPerMeter = reader.GetDouble();
        coreLevel = reader.GetInt();
        thrusterLevel = reader.GetInt();
        miningSpeed = reader.GetFloat();
        replicateSpeed = reader.GetFloat();
        walkSpeed = reader.GetFloat();
        jumpSpeed = reader.GetFloat();
        maxSailSpeed = reader.GetFloat();
        maxWarpSpeed = reader.GetFloat();
        buildArea = reader.GetFloat();
        droneCount = reader.GetInt();
        inventorySize = reader.GetInt();
        deliveryPackageUnlocked = reader.GetBool();
        deliveryPackageColCount = reader.GetInt();
        deliveryPackageStackSizeMultiplier = reader.GetInt();
        instantBuildEnergy = reader.GetDouble();
        hpMaxUpgrade = reader.GetInt();
        energyShieldUnlocked = reader.GetBool();
        energyShieldRadius = reader.GetFloat();
        energyShieldCapacity = reader.GetLong();
        laserEnergyCapacity = reader.GetLong();
        laserLocalAttackRange = reader.GetFloat();
        laserSpaceAttackRange = reader.GetFloat();
        laserLocalEnergyCost = reader.GetInt();
        laserSpaceEnergyCost = reader.GetInt();
        laserLocalDamage = reader.GetInt();
        laserSpaceDamage = reader.GetInt();
        groundFleetCount = reader.GetInt();
        spaceFleetCount = reader.GetInt();
    }

    public void UpdateMech(Mecha destination)
    {
        destination.coreEnergyCap = coreEnergyCap;
        destination.corePowerGen = corePowerGen;
        destination.reactorPowerGen = reactorPowerGen;
        destination.walkPower = walkPower;
        destination.jumpEnergy = jumpEnergy;
        destination.thrustPowerPerAcc = thrustPowerPerAcc;
        destination.warpKeepingPowerPerSpeed = warpKeepingPowerPerSpeed;
        destination.warpStartPowerPerSpeed = warpStartPowerPerSpeed;
        destination.miningPower = miningPower;
        destination.replicatePower = replicatePower;
        destination.researchPower = researchPower;
        destination.droneEjectEnergy = droneEjectEnergy;
        destination.droneEnergyPerMeter = droneEnergyPerMeter;
        destination.coreLevel = coreLevel;
        destination.thrusterLevel = thrusterLevel;
        destination.miningSpeed = miningSpeed;
        destination.replicateSpeed = replicateSpeed;
        destination.walkSpeed = walkSpeed;
        destination.jumpSpeed = jumpSpeed;
        destination.maxSailSpeed = maxSailSpeed;
        destination.maxWarpSpeed = maxWarpSpeed;
        destination.buildArea = buildArea;
        destination.constructionModule.droneCount = droneCount;
        destination.constructionModule.droneIdleCount = droneCount;
        destination.constructionModule.droneAliveCount = droneCount;
        if (inventorySize > destination.player.package.size)
        {
            destination.player.package.SetSize(inventorySize);
        }
        destination.player.deliveryPackage.unlocked = deliveryPackageUnlocked;
        var deliveryPackageRowCount = (destination.player.package.size - 1) / 10 + 1;
        if (destination.player.deliveryPackage.rowCount != deliveryPackageRowCount ||
            deliveryPackageColCount != destination.player.deliveryPackage.colCount)
        {
            destination.player.deliveryPackage.rowCount = deliveryPackageRowCount;
            destination.player.deliveryPackage.colCount = deliveryPackageColCount;
            destination.player.deliveryPackage.NotifySizeChange();
        }
        destination.player.deliveryPackage.stackSizeMultiplier = deliveryPackageStackSizeMultiplier;
        destination.instantBuildEnergy = instantBuildEnergy;
        destination.hpMaxUpgrade = hpMaxUpgrade;
        destination.energyShieldUnlocked = energyShieldUnlocked;
        destination.energyShieldRadius = energyShieldRadius;
        destination.energyShieldCapacity = energyShieldCapacity;
        destination.laserEnergyCapacity = laserEnergyCapacity;
        destination.laserLocalAttackRange = laserLocalAttackRange;
        destination.laserSpaceAttackRange = laserSpaceAttackRange;
        destination.laserLocalEnergyCost = laserLocalEnergyCost;
        destination.laserSpaceEnergyCost = laserSpaceEnergyCost;
        destination.laserLocalDamage = laserLocalDamage;
        destination.laserSpaceDamage = laserSpaceDamage;
        destination.groundCombatModule.fleetCount = groundFleetCount;
        destination.spaceCombatModule.fleetCount = spaceFleetCount;
    }

    public void Import(INetDataReader reader, int revision)
    {
        coreEnergyCap = reader.GetDouble();
        corePowerGen = reader.GetDouble();
        reactorPowerGen = reader.GetDouble();
        walkPower = reader.GetDouble();
        jumpEnergy = reader.GetDouble();
        thrustPowerPerAcc = reader.GetDouble();
        warpKeepingPowerPerSpeed = reader.GetDouble();
        warpStartPowerPerSpeed = reader.GetDouble();
        miningPower = reader.GetDouble();
        replicatePower = reader.GetDouble();
        researchPower = reader.GetDouble();
        droneEjectEnergy = reader.GetDouble();
        droneEnergyPerMeter = reader.GetDouble();
        coreLevel = reader.GetInt();
        thrusterLevel = reader.GetInt();
        miningSpeed = reader.GetFloat();
        replicateSpeed = reader.GetFloat();
        walkSpeed = reader.GetFloat();
        jumpSpeed = reader.GetFloat();
        maxSailSpeed = reader.GetFloat();
        maxWarpSpeed = reader.GetFloat();
        buildArea = reader.GetFloat();
        if (revision >= 8)
        {
            droneCount = reader.GetInt();
        }
        inventorySize = reader.GetInt();
        if (revision < 7)
        {
            return;
        }
        deliveryPackageUnlocked = reader.GetBool();
        deliveryPackageColCount = reader.GetInt();
        deliveryPackageStackSizeMultiplier = reader.GetInt();
        if (revision < 8)
        {
            return;
        }
        instantBuildEnergy = reader.GetDouble();
        hpMaxUpgrade = reader.GetInt();
        energyShieldUnlocked = reader.GetBool();
        energyShieldRadius = reader.GetFloat();
        energyShieldCapacity = reader.GetLong();
        laserEnergyCapacity = reader.GetLong();
        laserLocalAttackRange = reader.GetFloat();
        laserSpaceAttackRange = reader.GetFloat();
        laserLocalEnergyCost = reader.GetInt();
        laserSpaceEnergyCost = reader.GetInt();
        laserLocalDamage = reader.GetInt();
        laserSpaceDamage = reader.GetInt();
        groundFleetCount = reader.GetInt();
        spaceFleetCount = reader.GetInt();
    }
}
