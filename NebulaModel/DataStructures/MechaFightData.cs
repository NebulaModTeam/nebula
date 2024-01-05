#region
using System.IO;
using NebulaAPI.DataStructures;
using NebulaAPI.Interfaces;
#endregion

namespace NebulaModel.DataStructures
{
    internal class MechaFightData : IMechaFightData
    {
        public MechaFightData()
        {
            // This is needed for the serialization and deserialization
            AmmoStorage = new StorageComponent(3);
            BombStorage = new StorageComponent(1);
            FighterStorage = new StorageComponent(5);
            GroundCombatModule = new CombatModuleComponent();
            SpaceCombatModule = new CombatModuleComponent();

            GroundCombatModule.Reset();
            SpaceCombatModule.Reset();
            GroundCombatModule.Init(GameMain.data);
            SpaceCombatModule.Init(GameMain.data);
            GroundCombatModule.Setup(1, GameMain.data);
            SpaceCombatModule.Setup(3, GameMain.data);
        }
        public MechaFightData(Player player)
        {
            AutoReplenishFuel = player.mecha.autoReplenishFuel;
            AutoReplenishAmmo = player.mecha.autoReplenishAmmo;
            AutoReplenishHangar = player.mecha.autoReplenishHangar;
            Hp = player.mecha.hp;
            EnergyShieldEnergy = player.mecha.energyShieldEnergy;
            AmmoItemId = player.mecha.ammoItemId;
            AmmoInc = player.mecha.ammoInc;
            AmmoBulletCount = player.mecha.ammoBulletCount;
            AmmoSelectSlot = player.mecha.ammoSelectSlot;
            AmmoMuzzleFire = player.mecha.ammoMuzzleFire;
            AmmoRoundFire = player.mecha.ammoRoundFire;
            AmmoMuzzleIndex = player.mecha.ammoMuzzleIndex;
            LaserActive = player.mecha.laserActive;
            LaserRecharging = player.mecha.laserRecharging;
            LaserEnergy = player.mecha.laserEnergy;
            LaserFire = player.mecha.laserFire;
            BombFire = player.mecha.bombFire;
            AmmoStorage = player.mecha.ammoStorage;
            BombStorage = player.mecha.bombStorage;
            AmmoHatredTarget = player.mecha.ammoHatredTarget;
            LaserHatredTarget = player.mecha.laserHatredTarget;
            FighterStorage = player.mecha.fighterStorage;
            GroundCombatModule = player.mecha.groundCombatModule;
            SpaceCombatModule = player.mecha.spaceCombatModule;
        }
        public bool AutoReplenishFuel { get; set; }
        public bool AutoReplenishAmmo { get; set; }
        public bool AutoReplenishHangar { get; set; }
        public int Hp { get; set; }
        public long EnergyShieldEnergy { get; set; }
        public int AmmoItemId { get; set; }
        public int AmmoInc { get; set; }
        public int AmmoBulletCount { get; set; }
        public int AmmoSelectSlot { get; set; }
        public int AmmoMuzzleFire { get; set; }
        public int AmmoRoundFire { get; set; }
        public int AmmoMuzzleIndex { get; set; }
        public bool LaserActive { get; set; }
        public bool LaserRecharging { get; set; }
        public long LaserEnergy { get; set; }
        public int LaserFire { get; set; }
        public int BombFire { get; set; }
        public StorageComponent AmmoStorage { get; set; }
        public StorageComponent BombStorage { get; set; }
        public EnemyHatredTarget AmmoHatredTarget { get; set; }
        public EnemyHatredTarget LaserHatredTarget { get; set; }
        public StorageComponent FighterStorage { get; set; }
        public CombatModuleComponent GroundCombatModule { get; set; }
        public CombatModuleComponent SpaceCombatModule { get; set; }
        public void Serialize(INetDataWriter writer)
        {
            writer.Put(AutoReplenishFuel);
            writer.Put(AutoReplenishAmmo);
            writer.Put(AutoReplenishHangar);
            writer.Put(Hp);
            writer.Put(EnergyShieldEnergy);
            writer.Put(AmmoItemId);
            writer.Put(AmmoInc);
            writer.Put(AmmoBulletCount);
            writer.Put(AmmoSelectSlot);
            writer.Put(AmmoMuzzleFire);
            writer.Put(AmmoRoundFire);
            writer.Put(AmmoMuzzleIndex);
            writer.Put(LaserActive);
            writer.Put(LaserRecharging);
            writer.Put(LaserEnergy);
            writer.Put(LaserFire);
            writer.Put(BombFire);

            using var ms = new MemoryStream();
            using (var wr = new BinaryWriter(ms))
            {
                AmmoStorage.Export(wr);
                BombStorage.Export(wr);
                AmmoHatredTarget.Export(wr);
                LaserHatredTarget.Export(wr);
                FighterStorage.Export(wr);
                GroundCombatModule.Export(wr);
                SpaceCombatModule.Export(wr);
            }
            var export = ms.ToArray();
            writer.Put(export.Length);
            writer.Put(export);
        }
        public void Deserialize(INetDataReader reader)
        {
            AmmoStorage = new StorageComponent(3);
            BombStorage = new StorageComponent(1);
            AmmoHatredTarget = default(EnemyHatredTarget);
            LaserHatredTarget = default(EnemyHatredTarget);
            FighterStorage = new StorageComponent(5);
            GroundCombatModule = new CombatModuleComponent();
            SpaceCombatModule = new CombatModuleComponent();

            AutoReplenishFuel = reader.GetBool();
            AutoReplenishAmmo = reader.GetBool();
            AutoReplenishHangar = reader.GetBool();
            Hp = reader.GetInt();
            EnergyShieldEnergy = reader.GetLong();
            AmmoItemId = reader.GetInt();
            AmmoInc = reader.GetInt();
            AmmoBulletCount = reader.GetInt();
            AmmoSelectSlot = reader.GetInt();
            AmmoMuzzleFire = reader.GetInt();
            AmmoRoundFire = reader.GetInt();
            AmmoMuzzleIndex = reader.GetInt();
            LaserActive = reader.GetBool();
            LaserRecharging = reader.GetBool();
            LaserEnergy = reader.GetLong();
            LaserFire = reader.GetInt();
            BombFire = reader.GetInt();

            if (Hp == 0)
            {
                // prevent instant death, which can happen when a player joins for the first time and then exits again before sending the first mecha data update.
                // when the host saves in this situation, the Hp would be set to 0 and on every next join the client would be insta killed. lol
                Hp = GameMain.mainPlayer.mecha.hpMaxApplied;
            }

            var fightLength = reader.GetInt();
            var fightBytes = new byte[fightLength];
            reader.GetBytes(fightBytes, fightLength);
            using var ms = new MemoryStream(fightBytes);
            using var br = new BinaryReader(ms);

            AmmoStorage.Import(br);
            BombStorage.Import(br);
            AmmoHatredTarget.Import(br);
            LaserHatredTarget.Import(br);
            FighterStorage.Import(br);
            GroundCombatModule.Import(br);
            SpaceCombatModule.Import(br);
        }
        public void UpdateMech(Player destination)
        {
            destination.mecha.autoReplenishFuel = AutoReplenishFuel;
            destination.mecha.autoReplenishAmmo = AutoReplenishAmmo;
            destination.mecha.autoReplenishHangar = AutoReplenishHangar;
            destination.mecha.hp = Hp;
            destination.mecha.energyShieldEnergy = EnergyShieldEnergy;
            destination.mecha.ammoItemId = AmmoItemId;
            destination.mecha.ammoInc = AmmoInc;
            destination.mecha.ammoBulletCount = AmmoBulletCount;
            destination.mecha.ammoSelectSlot = AmmoSelectSlot;
            destination.mecha.ammoMuzzleFire = AmmoMuzzleFire;
            destination.mecha.ammoRoundFire = AmmoRoundFire;
            destination.mecha.ammoMuzzleIndex = AmmoMuzzleIndex;
            destination.mecha.laserActive = LaserActive;
            destination.mecha.laserRecharging = LaserRecharging;
            destination.mecha.laserEnergy = LaserEnergy;
            destination.mecha.laserFire = LaserFire;
            destination.mecha.bombFire = BombFire;
            destination.mecha.ammoStorage = AmmoStorage;
            destination.mecha.bombStorage = BombStorage;
            destination.mecha.ammoHatredTarget = AmmoHatredTarget;
            destination.mecha.laserHatredTarget = LaserHatredTarget;
            destination.mecha.fighterStorage = FighterStorage;
            destination.mecha.groundCombatModule = GroundCombatModule;
            destination.mecha.spaceCombatModule = SpaceCombatModule;
        }
    }
}
