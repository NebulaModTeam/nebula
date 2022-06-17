using HarmonyLib;
using NebulaAPI;
using NebulaModel.Logger;
using NebulaModel.Networking;
using NebulaModel.Packets.Factory;
using NebulaModel.Packets.Factory.Assembler;
using NebulaModel.Packets.Factory.Ejector;
using NebulaModel.Packets.Factory.Foundation;
using NebulaModel.Packets.Factory.Fractionator;
using NebulaModel.Packets.Factory.Inserter;
using NebulaModel.Packets.Factory.Laboratory;
using NebulaModel.Packets.Factory.Miner;
using NebulaModel.Packets.Factory.PowerExchanger;
using NebulaModel.Packets.Factory.PowerGenerator;
using NebulaModel.Packets.Factory.RayReceiver;
using NebulaModel.Packets.Factory.Silo;
using NebulaModel.Packets.Factory.Tank;
using NebulaModel.Packets.Logistics;
using NebulaModel.Packets.Planet;
using NebulaWorld;
using UnityEngine;

namespace NebulaPatcher.Patches.Dynamic
{
    [HarmonyPatch(typeof(PlanetFactory))]
    internal class PlanetFactory_patch
    {
        [HarmonyPostfix]
        [HarmonyPatch(nameof(PlanetFactory.AddPrebuildData))]
        public static void AddPrebuildData_Postfix(PlanetFactory __instance, PrebuildData prebuild, ref int __result)
        {
            if (!Multiplayer.IsActive)
            {
                return;
            }

            // If the host game called the method, we need to compute the PrebuildId ourself
            if (Multiplayer.Session.LocalPlayer.IsHost)
            {
                Multiplayer.Session.Factories.SetPrebuildRequest(__instance.planetId, __result, Multiplayer.Session.LocalPlayer.Id);
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(PlanetFactory.BuildFinally))]
        public static bool BuildFinally_Prefix(PlanetFactory __instance, Player player, int prebuildId)
        {
            if (!Multiplayer.IsActive)
            {
                return true;
            }

            if (Multiplayer.Session.LocalPlayer.IsHost)
            {
                if (!Multiplayer.Session.Factories.ContainsPrebuildRequest(__instance.planetId, prebuildId))
                {
                    // This prevents duplicating the entity when multiple players trigger the BuildFinally for the same entity at the same time.
                    // If it occurs in any other circumstances, it means that we have some desynchronization between clients and host prebuilds buffers.
                    Log.Warn($"BuildFinally was called without having a corresponding PrebuildRequest for the prebuild {prebuildId} on the planet {__instance.planetId}");
                    return false;
                }

                // Remove the prebuild request from the list since we will now convert it to a real building
                Multiplayer.Session.Factories.RemovePrebuildRequest(__instance.planetId, prebuildId);
            }

            if (Multiplayer.Session.LocalPlayer.IsHost || !Multiplayer.Session.Factories.IsIncomingRequest.Value)
            {
                int author = Multiplayer.Session.Factories.PacketAuthor == NebulaModAPI.AUTHOR_NONE ? Multiplayer.Session.LocalPlayer.Id : Multiplayer.Session.Factories.PacketAuthor;
                int entityId = Multiplayer.Session.LocalPlayer.IsHost ? NebulaWorld.Factory.FactoryManager.GetNextEntityId(__instance) : -1;
                Multiplayer.Session.Network.SendPacket(new BuildEntityRequest(__instance.planetId, prebuildId, author, entityId));
            }

            if (!Multiplayer.Session.LocalPlayer.IsHost && !Multiplayer.Session.Factories.IsIncomingRequest.Value && !Multiplayer.Session.Drones.IsPendingBuildRequest(-prebuildId))
            {
                Multiplayer.Session.Drones.AddBuildRequestSent(-prebuildId);
            }

            return Multiplayer.Session.LocalPlayer.IsHost || Multiplayer.Session.Factories.IsIncomingRequest.Value;
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(PlanetFactory.UpgradeFinally))]
        public static bool UpgradeFinally_Prefix(PlanetFactory __instance, Player player, int objId, ItemProto replace_item_proto)
        {
            if (!Multiplayer.IsActive)
            {
                return true;
            }

            if (objId == 0 || replace_item_proto == null)
            {
                return false;
            }

            if (Multiplayer.Session.LocalPlayer.IsHost || !Multiplayer.Session.Factories.IsIncomingRequest.Value)
            {
                Multiplayer.Session.Network.SendPacket(new UpgradeEntityRequest(__instance.planetId, objId, replace_item_proto.ID, Multiplayer.Session.Factories.PacketAuthor == -1 ? Multiplayer.Session.LocalPlayer.Id : Multiplayer.Session.Factories.PacketAuthor));
            }

            return Multiplayer.Session.LocalPlayer.IsHost || Multiplayer.Session.Factories.IsIncomingRequest.Value;
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(PlanetFactory.GameTick))]
        public static bool InternalUpdate_Prefix()
        {
            if (Multiplayer.IsActive)
            {
                Multiplayer.Session.Storage.IsHumanInput = false;
            }
            return true;
        }

        [HarmonyPostfix]
        [HarmonyPatch(nameof(PlanetFactory.GameTick))]
        public static void InternalUpdate_Postfix()
        {
            if (Multiplayer.IsActive)
            {
                Multiplayer.Session.Storage.IsHumanInput = true;
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(PlanetFactory.PasteBuildingSetting))]
        public static void PasteBuildingSetting_Prefix(PlanetFactory __instance, int objectId)
        {
            if (Multiplayer.IsActive && !Multiplayer.Session.Factories.IsIncomingRequest.Value)
            {
                Multiplayer.Session.Network.SendPacketToLocalStar(new PasteBuildingSettingUpdate(objectId, BuildingParameters.clipboard, GameMain.localPlanet?.id ?? -1));
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(PlanetFactory.FlattenTerrainReform))]
        public static void FlattenTerrainReform_Prefix(PlanetFactory __instance, Vector3 center, float radius, int reformSize, bool veinBuried, float fade0)
        {
            if (Multiplayer.IsActive && !Multiplayer.Session.Factories.IsIncomingRequest.Value)
            {
                Multiplayer.Session.Network.SendPacketToLocalStar(new FoundationBuildUpdatePacket(radius, reformSize, veinBuried, fade0));
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(PlanetFactory.PlanetReformAll))]
        public static void PlanetReformAll_Prefix(PlanetFactory __instance, int type, int color, bool bury)
        {
            if (Multiplayer.IsActive)
            {
                if (!Multiplayer.Session.Planets.IsIncomingRequest)
                {
                    Multiplayer.Session.Network.SendPacketToLocalStar(new PlanetReformPacket(__instance.planetId, true, type, color, bury));
                }
                // Stop VegeMinedPacket from sending
                Multiplayer.Session.Planets.EnableVeinPacket = false;
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(nameof(PlanetFactory.PlanetReformAll))]
        public static void PlanetReformAll_Postfix()
        {
            if (Multiplayer.IsActive)
            {
                Multiplayer.Session.Planets.EnableVeinPacket = true;
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(PlanetFactory.PlanetReformRevert))]
        public static void PlanetReformRevert_Prefix(PlanetFactory __instance)
        {
            if (Multiplayer.IsActive)
            {
                if (!Multiplayer.Session.Planets.IsIncomingRequest)
                {
                    Multiplayer.Session.Network.SendPacketToLocalStar(new PlanetReformPacket(__instance.planetId, false));
                }
                // Stop VegeMinedPacket from sending
                Multiplayer.Session.Planets.EnableVeinPacket = false;
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(nameof(PlanetFactory.PlanetReformRevert))]
        public static void PlanetReformRevert_Postfix()
        {
            if (Multiplayer.IsActive)
            {
                Multiplayer.Session.Planets.EnableVeinPacket = true;
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(nameof(PlanetFactory.AddVegeData))]
        public static void AddVegeData_Postfix(PlanetFactory __instance, VegeData vege)
        {
            if (Multiplayer.IsActive && !Multiplayer.Session.Planets.IsIncomingRequest)
            {
                using (BinaryUtils.Writer writer = new())
                {
                    vege.Export(writer.BinaryWriter);
                    Multiplayer.Session.Network.SendPacketToLocalStar(new VegeAddPacket(__instance.planetId, false, writer.CloseAndGetBytes()));
                }
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(nameof(PlanetFactory.AddVeinData))]
        public static void AddVeinData_Postfix(PlanetFactory __instance, VeinData vein)
        {
            if (Multiplayer.IsActive && !Multiplayer.Session.Planets.IsIncomingRequest)
            {
                using (BinaryUtils.Writer writer = new())
                {
                    vein.Export(writer.BinaryWriter);
                    Multiplayer.Session.Network.SendPacketToLocalStar(new VegeAddPacket(__instance.planetId, true, writer.CloseAndGetBytes()));
                }
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(nameof(PlanetFactory.RemoveVegeWithComponents))]
        public static void RemoveVegeWithComponents_Postfix(PlanetFactory __instance, int id)
        {
            if (Multiplayer.IsActive && !Multiplayer.Session.Planets.IsIncomingRequest && Multiplayer.Session.Planets.EnableVeinPacket)
            {
                Multiplayer.Session.Network.SendPacketToLocalStar(new VegeMinedPacket(__instance.planetId, id, 0, false));
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(nameof(PlanetFactory.RemoveVeinWithComponents))]
        public static void RemoveVeinWithComponents_Postfix(PlanetFactory __instance, int id)
        {
            if (Multiplayer.IsActive && !Multiplayer.Session.Planets.IsIncomingRequest && Multiplayer.Session.Planets.EnableVeinPacket)
            {
                if (Multiplayer.Session.LocalPlayer.IsHost)
                {
                    Multiplayer.Session.Network.SendPacketToStar(new VegeMinedPacket(__instance.planetId, id, 0, true), __instance.planet.star.id);
                }
                else
                {
                    Multiplayer.Session.Network.SendPacketToLocalStar(new VegeMinedPacket(__instance.planetId, id, 0, true));
                }
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(nameof(PlanetFactory.EnableEntityWarning))]
        public static void EnableEntityWarning_Postfix(PlanetFactory __instance, int entityId)
        {
            if (Multiplayer.IsActive && entityId > 0 && __instance.entityPool[entityId].id == entityId)
            {
                if (Multiplayer.Session.LocalPlayer.IsClient)
                {
                    //Becasue WarningSystem.NewWarningData is blocked on client, we give it a dummy warningId
                    __instance.entityPool[entityId].warningId = 1;
                }
                Multiplayer.Session.Network.SendPacketToLocalStar(new EntityWarningSwitchPacket(__instance.planetId, entityId, true));
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(nameof(PlanetFactory.DisableEntityWarning))]
        public static void DisableEntityWarning_Postfix(PlanetFactory __instance, int entityId)
        {
            if (Multiplayer.IsActive && entityId > 0 && __instance.entityPool[entityId].id == entityId)
            {
                Multiplayer.Session.Network.SendPacketToLocalStar(new EntityWarningSwitchPacket(__instance.planetId, entityId, false));
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(nameof(PlanetFactory.EntityFastTakeOut))]
        public static void EntityFastTakeOut_Postfix(PlanetFactory __instance, int entityId)
        {
            // belt, splitter, monitor, piler: handle by BeltFastTakeOut
            // storage: sync in StorageComponent.TakeItemFromGrid
            // powerNode, powerCon, powerAcc: no fill in interaction

            if (Multiplayer.IsActive && entityId > 0 && __instance.entityPool[entityId].id == entityId)
            {
                EntityData entityData = __instance.entityPool[entityId];

                if (entityData.assemblerId > 0)
                {
                    int assemblerId = entityData.assemblerId;
                    AssemblerComponent[] assemblerPool = __instance.factorySystem.assemblerPool;
                    if (assemblerPool[assemblerId].recipeId > 0)
                    {
                        int[] produced = assemblerPool[assemblerId].produced;
                        for (int j = 0; j < produced.Length; j++)
                        {
                            Multiplayer.Session.Network.SendPacketToLocalStar(new AssemblerUpdateProducesPacket(j, produced[j], __instance.planetId, assemblerId));
                        }
                    }
                }
                if (entityData.ejectorId > 0)
                {
                    int ejectorId = entityData.ejectorId;
                    EjectorComponent[] ejectorPool = __instance.factorySystem.ejectorPool;
                    Multiplayer.Session.Network.SendPacketToLocalStar(new EjectorStorageUpdatePacket(ejectorId, ejectorPool[ejectorId].bulletCount, ejectorPool[ejectorId].bulletInc, __instance.planetId));
                }
                if (entityData.inserterId > 0)
                {
                    int inserterId = entityData.inserterId;
                    InserterComponent[] inserterPool = __instance.factorySystem.inserterPool;
                    Multiplayer.Session.Network.SendPacketToLocalStar(new InserterItemUpdatePacket(in inserterPool[inserterId], __instance.planetId));
                }
                if (entityData.fractionatorId > 0)
                {
                    int fractionatorId = entityData.fractionatorId;
                    FractionatorComponent[] fractionatorPool = __instance.factorySystem.fractionatorPool;
                    Multiplayer.Session.Network.SendPacketToLocalStar(new FractionatorStorageUpdatePacket(in fractionatorPool[fractionatorId], __instance.planetId));
                }
                if (entityData.labId > 0)
                {
                    int labId = entityData.labId;
                    LabComponent[] labPool = __instance.factorySystem.labPool;
                    if (labPool[labId].matrixMode)
                    {
                        Multiplayer.Session.Network.SendPacketToLocalStar(new LaboratoryUpdateEventPacket(-3, labId, __instance.planetId));
                    }
                }
                if (entityData.minerId > 0)
                {
                    int minerId = entityData.minerId;
                    MinerComponent[] minerPool = __instance.factorySystem.minerPool;
                    Multiplayer.Session.Network.SendPacketToLocalStar(new MinerStoragePickupPacket(minerId, __instance.planetId));
                }
                if (entityData.powerExcId > 0)
                {
                    int powerExcId = entityData.powerExcId;
                    PowerExchangerComponent[] excPool = __instance.powerSystem.excPool;
                    Multiplayer.Session.Network.SendPacketToLocalStar(new PowerExchangerStorageUpdatePacket(powerExcId, excPool[powerExcId].emptyCount, excPool[powerExcId].fullCount, __instance.planetId));
                }
                if (entityData.powerGenId > 0)
                {
                    int powerGenId = entityData.powerGenId;
                    PowerGeneratorComponent[] genPool = __instance.powerSystem.genPool;
                    Multiplayer.Session.Network.SendPacketToLocalStar(new PowerGeneratorFuelUpdatePacket(powerGenId, genPool[powerGenId].fuelId, genPool[powerGenId].fuelCount, genPool[powerGenId].fuelInc, __instance.planetId));
                    if (genPool[powerGenId].productId > 0)
                    {
                        Multiplayer.Session.Network.SendPacketToLocalStar(new PowerGeneratorProductUpdatePacket(in genPool[powerGenId], __instance.planetId));
                    }
                }
                if (entityData.stationId > 0)
                {
                    int stationId = entityData.stationId;
                    StationComponent stationComponent = __instance.transport.stationPool[stationId];
                    for (int i = 0; i < stationComponent.storage.Length; i++)
                    {
                        if (stationComponent.storage[i].itemId > 0)
                        {
                            StorageUI packet = new StorageUI(__instance.planetId, stationComponent.id, stationComponent.gid, i, stationComponent.storage[i].count, stationComponent.storage[i].inc);
                            Multiplayer.Session.Network.SendPacket(packet);
                        }
                    }
                    if (!stationComponent.isCollector && !stationComponent.isVeinCollector)
                    {
                        StationUI packet = new StationUI(__instance.planetId, stationComponent.id, stationComponent.gid, StationUI.EUISettings.SetDroneCount, stationComponent.idleDroneCount + stationComponent.workDroneCount);
                        Multiplayer.Session.Network.SendPacket(packet);
                    }
                    if (stationComponent.isStellar && !stationComponent.isCollector && !stationComponent.isVeinCollector)
                    {
                        StationUI packet = new StationUI(__instance.planetId, stationComponent.id, stationComponent.gid, StationUI.EUISettings.SetShipCount, stationComponent.idleShipCount + stationComponent.workShipCount);
                        Multiplayer.Session.Network.SendPacket(packet);
                    }
                    if (stationComponent.isStellar && !stationComponent.isCollector && !stationComponent.isVeinCollector && __instance.gameData.history.logisticShipWarpDrive)
                    {
                        StationUI packet = new StationUI(__instance.planetId, stationComponent.id, stationComponent.gid, StationUI.EUISettings.SetWarperCount, stationComponent.warperCount);
                        Multiplayer.Session.Network.SendPacket(packet);
                    }
                }
                if (entityData.siloId > 0)
                {
                    int siloId = entityData.siloId;
                    SiloComponent[] siloPool = __instance.factorySystem.siloPool;
                    Multiplayer.Session.Network.SendPacketToLocalStar(new SiloStorageUpdatePacket(siloId, siloPool[siloId].bulletCount, siloPool[siloId].bulletInc, __instance.planetId));
                }
                if (entityData.tankId > 0)
                {
                    int tankId = entityData.tankId;
                    TankComponent[] tankPool = __instance.factoryStorage.tankPool;
                    Multiplayer.Session.Network.SendPacketToLocalStar(new TankStorageUpdatePacket(in tankPool[tankId], __instance.planetId));
                }
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(nameof(PlanetFactory.EntityFastFillIn))]
        public static void EntityFastFillIn_Postfix(PlanetFactory __instance, int entityId)
        {
            // belt, splitter, monitor, miner, fractionator, piler: handle by BeltFastFillIn
            // storage: sync in StorageComponent.AddItemStacked
            // inserter, powerNode, powerCon, powerAcc: no fill in interaction

            if (Multiplayer.IsActive && entityId > 0 && __instance.entityPool[entityId].id == entityId)
            {
                EntityData entityData = __instance.entityPool[entityId];

                if (entityData.tankId > 0)
                {
                    int tankId = entityData.tankId;
                    TankComponent[] tankPool = __instance.factoryStorage.tankPool;
                    Multiplayer.Session.Network.SendPacketToLocalStar(new TankStorageUpdatePacket(in tankPool[tankId], __instance.planetId));
                }
                if (entityData.assemblerId > 0)
                {
                    int assemblerId = entityData.assemblerId;
                    AssemblerComponent[] assemblerPool = __instance.factorySystem.assemblerPool;
                    if (assemblerPool[assemblerId].recipeId > 0)
                    {
                        Multiplayer.Session.Network.SendPacketToLocalStar(new AssemblerUpdateStoragePacket(__instance.planetId, assemblerId, assemblerPool[assemblerId].served, assemblerPool[assemblerId].incServed));
                    }
                }
                if (entityData.ejectorId > 0)
                {
                    int ejectorId = entityData.ejectorId;
                    EjectorComponent[] ejectorPool = __instance.factorySystem.ejectorPool;
                    Multiplayer.Session.Network.SendPacketToLocalStar(new EjectorStorageUpdatePacket(ejectorId, ejectorPool[ejectorId].bulletCount, ejectorPool[ejectorId].bulletInc, __instance.planetId));
                }
                if (entityData.siloId > 0)
                {
                    int siloId = entityData.siloId;
                    SiloComponent[] siloPool = __instance.factorySystem.siloPool;
                    Multiplayer.Session.Network.SendPacketToLocalStar(new SiloStorageUpdatePacket(siloId, siloPool[siloId].bulletCount, siloPool[siloId].bulletInc, __instance.planetId));
                }
                if (entityData.labId > 0)
                {
                    int labId = entityData.labId;
                    LabComponent[] labPool = __instance.factorySystem.labPool;

                    if (labPool[labId].researchMode)
                    {
                        for (int matrixId = 0; matrixId < LabComponent.matrixIds.Length; matrixId++)
                        {
                            Multiplayer.Session.Network.SendPacketToLocalStar(new LaboratoryUpdateCubesPacket(labPool[labId].matrixServed[matrixId], labPool[labId].matrixIncServed[matrixId], matrixId, labId, __instance.planetId));
                        }
                    }
                    else if (labPool[labId].matrixMode)
                    {
                        for (int m = 0; m < labPool[labId].served.Length; m++)
                        {
                            Multiplayer.Session.Network.SendPacketToLocalStar(new LaboratoryUpdateStoragePacket(labPool[labId].served[m], labPool[labId].incServed[m], m, labId, __instance.planetId));
                        }
                    }
                }
                if (entityData.stationId > 0)
                {
                    int stationId = entityData.stationId;
                    StationComponent stationComponent = __instance.transport.stationPool[stationId];
                    for (int i = 0; i < stationComponent.storage.Length; i++)
                    {
                        if (stationComponent.storage[i].itemId > 0)
                        {
                            StorageUI packet = new StorageUI(__instance.planetId, stationComponent.id, stationComponent.gid, i, stationComponent.storage[i].count, stationComponent.storage[i].inc);
                            Multiplayer.Session.Network.SendPacket(packet);
                        }
                    }
                    if (!stationComponent.isCollector && !stationComponent.isVeinCollector)
                    {
                        StationUI packet = new StationUI(__instance.planetId, stationComponent.id, stationComponent.gid, StationUI.EUISettings.SetDroneCount, stationComponent.idleDroneCount + stationComponent.workDroneCount);
                        Multiplayer.Session.Network.SendPacket(packet);
                    }
                    if (stationComponent.isStellar && !stationComponent.isCollector && !stationComponent.isVeinCollector)
                    {
                        StationUI packet = new StationUI(__instance.planetId, stationComponent.id, stationComponent.gid, StationUI.EUISettings.SetShipCount, stationComponent.idleShipCount + stationComponent.workShipCount);
                        Multiplayer.Session.Network.SendPacket(packet);
                    }
                    if (stationComponent.isStellar && !stationComponent.isCollector && !stationComponent.isVeinCollector && __instance.gameData.history.logisticShipWarpDrive)
                    {
                        StationUI packet = new StationUI(__instance.planetId, stationComponent.id, stationComponent.gid, StationUI.EUISettings.SetWarperCount, stationComponent.warperCount);
                        Multiplayer.Session.Network.SendPacket(packet);
                    }
                }
                if (entityData.powerGenId > 0)
                {
                    int powerGenId = entityData.powerGenId;
                    PowerGeneratorComponent[] genPool = __instance.powerSystem.genPool;
                    Multiplayer.Session.Network.SendPacketToLocalStar(new PowerGeneratorFuelUpdatePacket(powerGenId, genPool[powerGenId].fuelId, genPool[powerGenId].fuelCount, genPool[powerGenId].fuelInc, __instance.planetId));
                    if (genPool[powerGenId].gamma)
                    {
                        Multiplayer.Session.Network.SendPacketToLocalStar(new RayReceiverChangeLensPacket(powerGenId, genPool[powerGenId].catalystPoint, genPool[powerGenId].catalystIncPoint, __instance.planetId));
                    }
                }
                if (entityData.powerExcId > 0)
                {
                    int powerExcId = entityData.powerExcId;
                    PowerExchangerComponent[] excPool = __instance.powerSystem.excPool;
                    Multiplayer.Session.Network.SendPacketToLocalStar(new PowerExchangerStorageUpdatePacket(powerExcId, excPool[powerExcId].emptyCount, excPool[powerExcId].fullCount, __instance.planetId));
                }
                if (entityData.spraycoaterId > 0)
                {
                    int spraycoaterId = entityData.spraycoaterId;
                    SpraycoaterComponent[] spraycoaterPool = __instance.cargoTraffic.spraycoaterPool;
                    Multiplayer.Session.Network.SendPacketToLocalStar(new SprayerStorageUpdatePacket(spraycoaterPool[spraycoaterId], __instance.planetId));
                }
            }
        }
    }
}
