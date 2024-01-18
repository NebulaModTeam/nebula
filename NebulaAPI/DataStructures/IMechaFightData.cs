using NebulaAPI.Interfaces;

namespace NebulaAPI.DataStructures
{
    public interface IMechaFightData
    {
        bool AutoReplenishFuel { get; set; }
        bool AutoReplenishAmmo { get; set; }
        bool AutoReplenishHangar { get; set; }
        int Hp { get; set; }
        long EnergyShieldEnergy { get; set; }
        int AmmoItemId { get; set; }
        int AmmoInc { get; set; }
        int AmmoBulletCount { get; set; }
        int AmmoSelectSlot { get; set; }
        int AmmoMuzzleFire { get; set; }
        int AmmoRoundFire { get; set; }
        int AmmoMuzzleIndex { get; set; }
        bool LaserActive { get; set; }
        bool LaserRecharging { get; set; }
        long LaserEnergy { get; set; }
        int LaserFire { get; set; }
        int BombFire { get; set; }
        StorageComponent AmmoStorage { get; set; }
        StorageComponent BombStorage { get; set; }
        EnemyHatredTarget AmmoHatredTarget { get; set; }
        EnemyHatredTarget LaserHatredTarget { get; set; }
        StorageComponent FighterStorage { get; set; }
        CombatModuleComponent GroundCombatModule { get; set; }
        CombatModuleComponent SpaceCombatModule { get; set; }
        public void Serialize(INetDataWriter writer);
        public void Deserialize(INetDataReader reader);
        public void UpdateMech(Player destination);
    }
}
