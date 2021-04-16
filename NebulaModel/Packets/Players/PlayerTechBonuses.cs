using NebulaModel.Networking.Serialization;

namespace NebulaModel.Packets.Players
{
    public class PlayerTechBonuses : INetSerializable
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
        float droneSpeed { get; set; }
        int droneMovement { get; set; }
        int inventorySize { get; set; }

        public PlayerTechBonuses() { }

        public PlayerTechBonuses(Mecha source)
        {
            this.coreEnergyCap = source.coreEnergyCap;
            this.corePowerGen = source.corePowerGen;
            this.reactorPowerGen = source.reactorPowerGen;
            this.walkPower = source.walkPower;
            this.jumpEnergy = source.jumpEnergy;
            this.thrustPowerPerAcc = source.thrustPowerPerAcc;
            this.warpKeepingPowerPerSpeed = source.warpKeepingPowerPerSpeed;
            this.warpStartPowerPerSpeed = source.warpStartPowerPerSpeed;
            this.miningPower = source.miningPower;
            this.replicatePower = source.replicatePower;
            this.researchPower = source.researchPower;
            this.droneEjectEnergy = source.droneEjectEnergy;
            this.droneEnergyPerMeter = source.droneEnergyPerMeter;
            this.coreLevel = source.coreLevel;
            this.thrusterLevel = source.thrusterLevel;
            this.miningSpeed = source.miningSpeed;
            this.replicateSpeed = source.replicateSpeed;
            this.walkSpeed = source.walkSpeed;
            this.jumpSpeed = source.jumpSpeed;
            this.maxSailSpeed = source.maxSailSpeed;
            this.maxWarpSpeed = source.maxWarpSpeed;
            this.buildArea = source.buildArea;
            this.droneCount = source.droneCount;
            this.droneSpeed = source.droneSpeed;
            this.droneMovement = source.droneMovement;
            this.inventorySize = source.player.package.size;
        }

        public void UpdateMech(Mecha destination)
        {
            destination.coreEnergyCap = this.coreEnergyCap;
            destination.corePowerGen = this.corePowerGen;
            destination.reactorPowerGen = this.reactorPowerGen;
            destination.walkPower = this.walkPower;
            destination.jumpEnergy = this.jumpEnergy;
            destination.thrustPowerPerAcc = this.thrustPowerPerAcc;
            destination.warpKeepingPowerPerSpeed = this.warpKeepingPowerPerSpeed;
            destination.warpStartPowerPerSpeed = this.warpStartPowerPerSpeed;
            destination.miningPower = this.miningPower;
            destination.replicatePower = this.replicatePower;
            destination.researchPower = this.researchPower;
            destination.droneEjectEnergy = this.droneEjectEnergy;
            destination.droneEnergyPerMeter = this.droneEnergyPerMeter;
            destination.coreLevel = this.coreLevel;
            destination.thrusterLevel = this.thrusterLevel;
            destination.miningSpeed = this.miningSpeed;
            destination.replicateSpeed = this.replicateSpeed;
            destination.walkSpeed = this.walkSpeed;
            destination.jumpSpeed = this.jumpSpeed;
            destination.maxSailSpeed = this.maxSailSpeed;
            destination.maxWarpSpeed = this.maxWarpSpeed;
            destination.buildArea = this.buildArea;
            destination.droneCount = this.droneCount;
            destination.droneSpeed = this.droneSpeed;
            destination.droneMovement = this.droneMovement;
            if (this.inventorySize > destination.player.package.size)
            {
                destination.player.package.SetSize(this.inventorySize);
            }
        }

        public void Serialize(NetDataWriter writer)
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
            writer.Put(droneSpeed);
            writer.Put(droneMovement);
            writer.Put(inventorySize);
        }

        public void Deserialize(NetDataReader reader)
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
            droneSpeed = reader.GetFloat();
            droneMovement = reader.GetInt();
            inventorySize = reader.GetInt();
        }
    }
}
