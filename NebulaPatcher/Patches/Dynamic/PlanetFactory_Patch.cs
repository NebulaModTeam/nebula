#region

using HarmonyLib;
using NebulaAPI;
using NebulaModel.Logger;
using NebulaModel.Networking;
using NebulaModel.Packets.Combat.GroundEnemy;
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
using NebulaModel.Packets.Factory.Turret;
using NebulaModel.Packets.Logistics;
using NebulaModel.Packets.Planet;
using NebulaWorld;
using NebulaWorld.Factory;
using NebulaWorld.Player;
using UnityEngine;

#endregion

namespace NebulaPatcher.Patches.Dynamic;

[HarmonyPatch(typeof(PlanetFactory))]
internal class PlanetFactory_patch
{
    [HarmonyPrefix]
    [HarmonyPatch(nameof(PlanetFactory.FlushPools))]
    public static bool FlushPools_Prefix()
    {
        // In vanilla, FlushPools is triggered by unload planet event which will happen at different time for each player
        // So this optimize pool function is disabled in multiplayer to keep pool in sync
        return !Multiplayer.IsActive;
    }


    [HarmonyPostfix]
    [HarmonyPatch(nameof(PlanetFactory.AddPrebuildData))]
    public static void AddPrebuildData_Postfix(PlanetFactory __instance, ref int __result)
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
    public static bool BuildFinally_Prefix(PlanetFactory __instance, int prebuildId)
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
                Log.Debug(
                    $"BuildFinally was called without having a corresponding PrebuildRequest for the prebuild {prebuildId} on the planet {__instance.planetId}");
                return false;
            }

            // Remove the prebuild request from the list since we will now convert it to a real building
            Multiplayer.Session.Factories.RemovePrebuildRequest(__instance.planetId, prebuildId);
        }

        if (!Multiplayer.Session.LocalPlayer.IsHost && Multiplayer.Session.Factories.IsIncomingRequest.Value)
        {
            return Multiplayer.Session.LocalPlayer.IsHost || Multiplayer.Session.Factories.IsIncomingRequest.Value;
        }
        var author = Multiplayer.Session.Factories.PacketAuthor == NebulaModAPI.AUTHOR_NONE
            ? Multiplayer.Session.LocalPlayer.Id
            : Multiplayer.Session.Factories.PacketAuthor;
        var entityId = Multiplayer.Session.LocalPlayer.IsHost ? FactoryManager.GetNextEntityId(__instance) : -1;
        Multiplayer.Session.Network.SendPacket(new BuildEntityRequest(__instance.planetId, prebuildId, author, entityId));

        return Multiplayer.Session.LocalPlayer.IsHost || Multiplayer.Session.Factories.IsIncomingRequest.Value;
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(PlanetFactory.UpgradeFinally))]
    public static bool UpgradeFinally_Prefix(PlanetFactory __instance, int objId, ItemProto replace_item_proto)
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
            Multiplayer.Session.Network.SendPacket(new UpgradeEntityRequest(__instance.planetId, objId,
                FactoryManager.GetObjectProtoId(__instance, objId),
                replace_item_proto.ID,
                Multiplayer.Session.Factories.PacketAuthor == -1
                    ? Multiplayer.Session.LocalPlayer.Id
                    : Multiplayer.Session.Factories.PacketAuthor));
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
    public static void PasteBuildingSetting_Prefix(int objectId)
    {
        if (Multiplayer.IsActive && !Multiplayer.Session.Factories.IsIncomingRequest.Value)
        {
            Multiplayer.Session.Network.SendPacketToLocalStar(new PasteBuildingSettingUpdate(objectId,
                BuildingParameters.clipboard, GameMain.localPlanet?.id ?? -1));
        }
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(PlanetFactory.FlattenTerrainReform))]
    public static void FlattenTerrainReform_Prefix(Vector3 center, float radius, int reformSize,
        bool veinBuried, float fade0)
    {
        if (Multiplayer.IsActive && !Multiplayer.Session.Factories.IsIncomingRequest.Value)
        {
            Multiplayer.Session.Network.SendPacketToLocalStar(
                new FoundationBuildUpdatePacket(center, radius, reformSize, veinBuried, fade0));
        }
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(PlanetFactory.PlanetReformAll))]
    public static void PlanetReformAll_Prefix(PlanetFactory __instance, int type, int color, bool bury)
    {
        if (!Multiplayer.IsActive)
        {
            return;
        }
        if (!Multiplayer.Session.Planets.IsIncomingRequest)
        {
            Multiplayer.Session.Network.SendPacketToLocalStar(new PlanetReformPacket(__instance.planetId, true, type, color,
                bury));
        }
        // Stop VegeMinedPacket from sending
        Multiplayer.Session.Planets.EnableVeinPacket = false;
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
        if (!Multiplayer.IsActive)
        {
            return;
        }
        if (!Multiplayer.Session.Planets.IsIncomingRequest)
        {
            Multiplayer.Session.Network.SendPacketToLocalStar(new PlanetReformPacket(__instance.planetId, false));
        }
        // Stop VegeMinedPacket from sending
        Multiplayer.Session.Planets.EnableVeinPacket = false;
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
        if (!Multiplayer.IsActive || Multiplayer.Session.Planets.IsIncomingRequest)
        {
            return;
        }
        using BinaryUtils.Writer writer = new();
        vege.Export(writer.BinaryWriter);
        Multiplayer.Session.Network.SendPacketToLocalStar(new VegeAddPacket(__instance.planetId, false,
            writer.CloseAndGetBytes()));
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(PlanetFactory.AddVeinData))]
    public static void AddVeinData_Postfix(PlanetFactory __instance, VeinData vein)
    {
        if (!Multiplayer.IsActive || Multiplayer.Session.Planets.IsIncomingRequest)
        {
            return;
        }
        using BinaryUtils.Writer writer = new();
        vein.Export(writer.BinaryWriter);
        Multiplayer.Session.Network.SendPacketToLocalStar(new VegeAddPacket(__instance.planetId, true,
            writer.CloseAndGetBytes()));
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(PlanetFactory.RemoveVegeWithComponents))]
    public static void RemoveVegeWithComponents_Postfix(PlanetFactory __instance, int id)
    {
        if (Multiplayer.IsActive && !Multiplayer.Session.Planets.IsIncomingRequest &&
            Multiplayer.Session.Planets.EnableVeinPacket)
        {
            Multiplayer.Session.Network.SendPacketToLocalStar(new VegeMinedPacket(__instance.planetId, id, 0, false));
        }
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(PlanetFactory.RemoveVeinWithComponents))]
    public static void RemoveVeinWithComponents_Postfix(PlanetFactory __instance, int id)
    {
        if (!Multiplayer.IsActive || Multiplayer.Session.Planets.IsIncomingRequest ||
            !Multiplayer.Session.Planets.EnableVeinPacket)
        {
            return;
        }
        if (Multiplayer.Session.LocalPlayer.IsHost)
        {
            Multiplayer.Session.Network.SendPacketToStar(new VegeMinedPacket(__instance.planetId, id, 0, true),
                __instance.planet.star.id);
        }
        else
        {
            Multiplayer.Session.Network.SendPacketToLocalStar(new VegeMinedPacket(__instance.planetId, id, 0, true));
        }
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(PlanetFactory.WriteExtraInfoOnEntity))]
    public static void WriteExtraInfoOnEntity_Postfix(PlanetFactory __instance, int entityId, string info)
    {
        if (!Multiplayer.IsActive || Multiplayer.Session.Factories.IsIncomingRequest.Value)
        {
            return;
        }
        Multiplayer.Session.Network.SendPacketToLocalStar(
            new ExtraInfoUpdatePacket(__instance.planetId, entityId, info));
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(PlanetFactory.WriteExtraInfoOnPrebuild))]
    public static void WriteExtraInfoOnPrebuild_Postfix(PlanetFactory __instance, int prebuildId, string info)
    {
        if (!Multiplayer.IsActive || Multiplayer.Session.Factories.IsIncomingRequest.Value)
        {
            return;
        }
        Multiplayer.Session.Network.SendPacketToLocalStar(
            new ExtraInfoUpdatePacket(__instance.planetId, -prebuildId, info));
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(PlanetFactory.EnableEntityWarning))]
    public static void EnableEntityWarning_Postfix(PlanetFactory __instance, int entityId)
    {
        if (!Multiplayer.IsActive || entityId <= 0 || __instance.entityPool[entityId].id != entityId)
        {
            return;
        }
        if (Multiplayer.Session.LocalPlayer.IsClient)
        {
            //Because WarningSystem.NewWarningData is blocked on client, we give it a dummy warningId
            __instance.entityPool[entityId].warningId = 1;
        }
        Multiplayer.Session.Network.SendPacketToLocalStar(
            new EntityWarningSwitchPacket(__instance.planetId, entityId, true));
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(PlanetFactory.DisableEntityWarning))]
    public static void DisableEntityWarning_Postfix(PlanetFactory __instance, int entityId)
    {
        if (Multiplayer.IsActive && entityId > 0 && __instance.entityPool[entityId].id == entityId)
        {
            Multiplayer.Session.Network.SendPacketToLocalStar(new EntityWarningSwitchPacket(__instance.planetId, entityId,
                false));
        }
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(PlanetFactory.EntityFastTakeOut))]
    public static void EntityFastTakeOut_Postfix(PlanetFactory __instance, int entityId)
    {
        // belt, splitter, monitor, piler: handle by BeltFastTakeOut
        // storage: sync in StorageComponent.TakeItemFromGrid
        // powerNode, powerCon, powerAcc: no fill in interaction

        if (!Multiplayer.IsActive || entityId <= 0 || __instance.entityPool[entityId].id != entityId)
        {
            return;
        }
        var entityData = __instance.entityPool[entityId];

        if (entityData.assemblerId > 0)
        {
            var assemblerId = entityData.assemblerId;
            var assemblerPool = __instance.factorySystem.assemblerPool;
            if (assemblerPool[assemblerId].recipeId > 0)
            {
                var produced = assemblerPool[assemblerId].produced;
                for (var j = 0; j < produced.Length; j++)
                {
                    Multiplayer.Session.Network.SendPacketToLocalStar(
                        new AssemblerUpdateProducesPacket(j, produced[j], __instance.planetId, assemblerId));
                }
            }
        }
        if (entityData.dispenserId > 0)
        {
            var dispenserId = entityData.dispenserId;
            var dispenserPool = __instance.transport.dispenserPool;
            Multiplayer.Session.Network.SendPacketToLocalStar(new DispenserStorePacket(__instance.planetId,
                in dispenserPool[dispenserId]));
        }
        if (entityData.ejectorId > 0)
        {
            var ejectorId = entityData.ejectorId;
            var ejectorPool = __instance.factorySystem.ejectorPool;
            Multiplayer.Session.Network.SendPacketToLocalStar(new EjectorStorageUpdatePacket(ejectorId,
                ejectorPool[ejectorId].bulletCount, ejectorPool[ejectorId].bulletInc, __instance.planetId));
        }
        if (entityData.inserterId > 0)
        {
            var inserterId = entityData.inserterId;
            var inserterPool = __instance.factorySystem.inserterPool;
            Multiplayer.Session.Network.SendPacketToLocalStar(new InserterItemUpdatePacket(in inserterPool[inserterId],
                __instance.planetId));
        }
        if (entityData.fractionatorId > 0)
        {
            var fractionatorId = entityData.fractionatorId;
            var fractionatorPool = __instance.factorySystem.fractionatorPool;
            Multiplayer.Session.Network.SendPacketToLocalStar(
                new FractionatorStorageUpdatePacket(in fractionatorPool[fractionatorId], __instance.planetId));
        }
        if (entityData.labId > 0)
        {
            var labId = entityData.labId;
            var labPool = __instance.factorySystem.labPool;
            if (labPool[labId].matrixMode)
            {
                Multiplayer.Session.Network.SendPacketToLocalStar(
                    new LaboratoryUpdateEventPacket(-3, labId, __instance.planetId));
            }
        }
        if (entityData.minerId > 0)
        {
            var minerId = entityData.minerId;
            Multiplayer.Session.Network.SendPacketToLocalStar(new MinerStoragePickupPacket(minerId, __instance.planetId));
        }
        if (entityData.powerExcId > 0)
        {
            var powerExcId = entityData.powerExcId;
            var excPool = __instance.powerSystem.excPool;
            Multiplayer.Session.Network.SendPacketToLocalStar(new PowerExchangerStorageUpdatePacket(powerExcId,
                excPool[powerExcId].emptyCount, excPool[powerExcId].fullCount, __instance.planetId,
                excPool[powerExcId].fullInc));
        }
        if (entityData.powerGenId > 0)
        {
            var powerGenId = entityData.powerGenId;
            var genPool = __instance.powerSystem.genPool;
            Multiplayer.Session.Network.SendPacketToLocalStar(new PowerGeneratorFuelUpdatePacket(powerGenId,
                genPool[powerGenId].fuelId, genPool[powerGenId].fuelCount, genPool[powerGenId].fuelInc,
                __instance.planetId));
            if (genPool[powerGenId].productId > 0)
            {
                Multiplayer.Session.Network.SendPacketToLocalStar(
                    new PowerGeneratorProductUpdatePacket(in genPool[powerGenId], __instance.planetId));
            }
        }
        if (entityData.stationId > 0)
        {
            var stationId = entityData.stationId;
            var stationComponent = __instance.transport.stationPool[stationId];
            for (var i = 0; i < stationComponent.storage.Length; i++)
            {
                if (stationComponent.storage[i].itemId <= 0)
                {
                    continue;
                }
                var packet = new StorageUI(__instance.planetId, stationComponent.id, stationComponent.gid, i,
                    stationComponent.storage[i].count, stationComponent.storage[i].inc);
                Multiplayer.Session.Network.SendPacket(packet);
            }
            if (!stationComponent.isCollector && !stationComponent.isVeinCollector)
            {
                var packet = new StationUI(__instance.planetId, stationComponent.id, stationComponent.gid,
                    StationUI.EUISettings.SetDroneCount, stationComponent.idleDroneCount + stationComponent.workDroneCount);
                Multiplayer.Session.Network.SendPacket(packet);
            }
            if (stationComponent.isStellar && !stationComponent.isCollector && !stationComponent.isVeinCollector)
            {
                var packet = new StationUI(__instance.planetId, stationComponent.id, stationComponent.gid,
                    StationUI.EUISettings.SetShipCount, stationComponent.idleShipCount + stationComponent.workShipCount);
                Multiplayer.Session.Network.SendPacket(packet);
            }
            if (stationComponent.isStellar && !stationComponent.isCollector && !stationComponent.isVeinCollector &&
                __instance.gameData.history.logisticShipWarpDrive)
            {
                var packet = new StationUI(__instance.planetId, stationComponent.id, stationComponent.gid,
                    StationUI.EUISettings.SetWarperCount, stationComponent.warperCount);
                Multiplayer.Session.Network.SendPacket(packet);
            }
        }
        if (entityData.siloId > 0)
        {
            var siloId = entityData.siloId;
            var siloPool = __instance.factorySystem.siloPool;
            Multiplayer.Session.Network.SendPacketToLocalStar(new SiloStorageUpdatePacket(siloId,
                siloPool[siloId].bulletCount, siloPool[siloId].bulletInc, __instance.planetId));
        }
        if (entityData.turretId > 0)
        {
            var turretId = entityData.turretId;
            var turretPool = __instance.defenseSystem.turrets;
            Multiplayer.Session.Network.SendPacketToLocalStar(
                new TurretStorageUpdatePacket(in turretPool.buffer[turretId], __instance.planetId));
        }
        if (entityData.tankId <= 0)
        {
            return;
        }

        var tankId = entityData.tankId;
        var tankPool = __instance.factoryStorage.tankPool;
        Multiplayer.Session.Network.SendPacketToLocalStar(new TankStorageUpdatePacket(in tankPool[tankId],
            __instance.planetId));
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(PlanetFactory.EntityFastFillIn))]
    public static void EntityFastFillIn_Postfix(PlanetFactory __instance, int entityId)
    {
        // belt, splitter, monitor, miner, fractionator, piler: handle by BeltFastFillIn
        // storage: sync in StorageComponent.AddItemStacked
        // inserter, powerNode, powerCon, powerAcc: no fill in interaction

        if (!Multiplayer.IsActive || entityId <= 0 || __instance.entityPool[entityId].id != entityId)
        {
            return;
        }
        var entityData = __instance.entityPool[entityId];

        if (entityData.tankId > 0)
        {
            var tankId = entityData.tankId;
            var tankPool = __instance.factoryStorage.tankPool;
            Multiplayer.Session.Network.SendPacketToLocalStar(new TankStorageUpdatePacket(in tankPool[tankId],
                __instance.planetId));
        }
        if (entityData.assemblerId > 0)
        {
            var assemblerId = entityData.assemblerId;
            var assemblerPool = __instance.factorySystem.assemblerPool;
            if (assemblerPool[assemblerId].recipeId > 0)
            {
                Multiplayer.Session.Network.SendPacketToLocalStar(new AssemblerUpdateStoragePacket(__instance.planetId,
                    assemblerId, assemblerPool[assemblerId].served, assemblerPool[assemblerId].incServed));
            }
        }
        if (entityData.dispenserId > 0)
        {
            var dispenserId = entityData.dispenserId;
            var dispenserPool = __instance.transport.dispenserPool;
            var courierCount = dispenserPool[dispenserId].workCourierCount + dispenserPool[dispenserId].idleCourierCount;
            Multiplayer.Session.Network.SendPacketToLocalStar(new DispenserSettingPacket(__instance.planetId, dispenserId,
                EDispenserSettingEvent.SetCourierCount, courierCount));
        }
        if (entityData.ejectorId > 0)
        {
            var ejectorId = entityData.ejectorId;
            var ejectorPool = __instance.factorySystem.ejectorPool;
            Multiplayer.Session.Network.SendPacketToLocalStar(new EjectorStorageUpdatePacket(ejectorId,
                ejectorPool[ejectorId].bulletCount, ejectorPool[ejectorId].bulletInc, __instance.planetId));
        }
        if (entityData.siloId > 0)
        {
            var siloId = entityData.siloId;
            var siloPool = __instance.factorySystem.siloPool;
            Multiplayer.Session.Network.SendPacketToLocalStar(new SiloStorageUpdatePacket(siloId,
                siloPool[siloId].bulletCount, siloPool[siloId].bulletInc, __instance.planetId));
        }
        if (entityData.labId > 0)
        {
            var labId = entityData.labId;
            var labPool = __instance.factorySystem.labPool;

            if (labPool[labId].researchMode)
            {
                for (var matrixId = 0; matrixId < LabComponent.matrixIds.Length; matrixId++)
                {
                    Multiplayer.Session.Network.SendPacketToLocalStar(new LaboratoryUpdateCubesPacket(
                        labPool[labId].matrixServed[matrixId], labPool[labId].matrixIncServed[matrixId], matrixId, labId,
                        __instance.planetId));
                }
            }
            else if (labPool[labId].matrixMode)
            {
                for (var m = 0; m < labPool[labId].served.Length; m++)
                {
                    Multiplayer.Session.Network.SendPacketToLocalStar(
                        new LaboratoryUpdateStoragePacket(labPool[labId].served[m], labPool[labId].incServed[m], m, labId,
                            __instance.planetId));
                }
            }
        }
        if (entityData.stationId > 0)
        {
            var stationId = entityData.stationId;
            var stationComponent = __instance.transport.stationPool[stationId];
            for (var i = 0; i < stationComponent.storage.Length; i++)
            {
                if (stationComponent.storage[i].itemId <= 0)
                {
                    continue;
                }
                var packet = new StorageUI(__instance.planetId, stationComponent.id, stationComponent.gid, i,
                    stationComponent.storage[i].count, stationComponent.storage[i].inc);
                Multiplayer.Session.Network.SendPacket(packet);
            }
            if (!stationComponent.isCollector && !stationComponent.isVeinCollector)
            {
                var packet = new StationUI(__instance.planetId, stationComponent.id, stationComponent.gid,
                    StationUI.EUISettings.SetDroneCount, stationComponent.idleDroneCount + stationComponent.workDroneCount);
                Multiplayer.Session.Network.SendPacket(packet);
            }
            if (stationComponent.isStellar && !stationComponent.isCollector && !stationComponent.isVeinCollector)
            {
                var packet = new StationUI(__instance.planetId, stationComponent.id, stationComponent.gid,
                    StationUI.EUISettings.SetShipCount, stationComponent.idleShipCount + stationComponent.workShipCount);
                Multiplayer.Session.Network.SendPacket(packet);
            }
            if (stationComponent.isStellar && !stationComponent.isCollector && !stationComponent.isVeinCollector &&
                __instance.gameData.history.logisticShipWarpDrive)
            {
                var packet = new StationUI(__instance.planetId, stationComponent.id, stationComponent.gid,
                    StationUI.EUISettings.SetWarperCount, stationComponent.warperCount);
                Multiplayer.Session.Network.SendPacket(packet);
            }
        }
        if (entityData.powerGenId > 0)
        {
            var powerGenId = entityData.powerGenId;
            var genPool = __instance.powerSystem.genPool;
            Multiplayer.Session.Network.SendPacketToLocalStar(new PowerGeneratorFuelUpdatePacket(powerGenId,
                genPool[powerGenId].fuelId, genPool[powerGenId].fuelCount, genPool[powerGenId].fuelInc,
                __instance.planetId));
            if (genPool[powerGenId].gamma)
            {
                Multiplayer.Session.Network.SendPacketToLocalStar(new RayReceiverChangeLensPacket(powerGenId,
                    genPool[powerGenId].catalystPoint, genPool[powerGenId].catalystIncPoint, __instance.planetId));
            }
        }
        if (entityData.powerExcId > 0)
        {
            var powerExcId = entityData.powerExcId;
            var excPool = __instance.powerSystem.excPool;
            Multiplayer.Session.Network.SendPacketToLocalStar(new PowerExchangerStorageUpdatePacket(powerExcId,
                excPool[powerExcId].emptyCount, excPool[powerExcId].fullCount, __instance.planetId,
                excPool[powerExcId].fullInc));
        }
        if (entityData.turretId > 0)
        {
            var turretId = entityData.turretId;
            var turretPool = __instance.defenseSystem.turrets;
            Multiplayer.Session.Network.SendPacketToLocalStar(new TurretStorageUpdatePacket(in turretPool.buffer[turretId], __instance.planetId));
        }
        if (entityData.spraycoaterId <= 0)
        {
            return;
        }
        var spraycoaterId = entityData.spraycoaterId;
        var spraycoaterPool = __instance.cargoTraffic.spraycoaterPool;
        Multiplayer.Session.Network.SendPacketToLocalStar(
            new SprayerStorageUpdatePacket(spraycoaterPool[spraycoaterId], __instance.planetId));
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(PlanetFactory.EntityAutoReplenishIfNeeded))]
    [HarmonyPatch(nameof(PlanetFactory.StationAutoReplenishIfNeeded))]
    public static bool EntityAutoReplenishIfNeeded_Prefix(PlanetFactory __instance, int entityId,
        ref (int, int, int, int) __state)
    {
        if (!Multiplayer.IsActive)
        {
            return true;
        }

        // Don't auto replenish if it is from other player's packet
        if (Multiplayer.Session.Factories.IsIncomingRequest.Value &&
            Multiplayer.Session.Factories.PacketAuthor != Multiplayer.Session.LocalPlayer.Id)
        {
            return false;
        }
        if (Multiplayer.Session.StationsUI.IsIncomingRequest.Value)
        {
            return false;
        }

        __state.Item1 = 1;
        ref var ptr = ref __instance.entityPool[entityId];
        if (ptr.dispenserId > 0)
        {
            var dispenserComponent = __instance.transport.dispenserPool[ptr.dispenserId];
            __state.Item2 = dispenserComponent.idleCourierCount;
        }
        if (ptr.stationId <= 0)
        {
            return true;
        }
        var stationComponent = __instance.transport.stationPool[ptr.stationId];
        __state.Item3 = stationComponent.idleDroneCount;
        __state.Item4 = stationComponent.idleShipCount;
        return true;
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(PlanetFactory.EntityAutoReplenishIfNeeded))]
    [HarmonyPatch(nameof(PlanetFactory.StationAutoReplenishIfNeeded))]
    public static void EntityAutoReplenishIfNeeded_Postfix(PlanetFactory __instance, int entityId,
        ref (int, int, int, int) __state)
    {
        if (__state.Item1 != 1)
        {
            return;
        }

        ref var ptr = ref __instance.entityPool[entityId];
        if (ptr.dispenserId > 0)
        {
            var dispenserComponent = __instance.transport.dispenserPool[ptr.dispenserId];
            if (__state.Item2 != dispenserComponent.idleCourierCount)
            {
                Multiplayer.Session.Network.SendPacketToLocalStar(
                    new DispenserSettingPacket(__instance.planetId,
                        ptr.dispenserId,
                        EDispenserSettingEvent.SetCourierCount,
                        dispenserComponent.workCourierCount + dispenserComponent.idleCourierCount));
            }
        }
        if (ptr.stationId <= 0)
        {
            return;
        }
        var stationComponent = __instance.transport.stationPool[ptr.stationId];
        if (__state.Item3 != stationComponent.idleDroneCount)
        {
            Multiplayer.Session.Network.SendPacket(
                new StationUI(__instance.planetId,
                    stationComponent.id,
                    stationComponent.gid,
                    StationUI.EUISettings.SetDroneCount,
                    stationComponent.idleDroneCount + stationComponent.workDroneCount));

            if (Multiplayer.Session.LocalPlayer.IsClient)
            {
                // Revert drone count until host verify
                stationComponent.idleDroneCount = __state.Item3;
            }
        }

        if (__state.Item4 == stationComponent.idleShipCount)
        {
            return;
        }
        Multiplayer.Session.Network.SendPacket(
            new StationUI(__instance.planetId,
                stationComponent.id,
                stationComponent.gid,
                StationUI.EUISettings.SetShipCount,
                stationComponent.idleShipCount + stationComponent.workShipCount));

        if (Multiplayer.Session.LocalPlayer.IsClient)
        {
            // Revert drone count until host verify
            stationComponent.idleShipCount = __state.Item4;
        }
    }

    #region Combat

    [HarmonyPrefix]
    [HarmonyPatch(nameof(PlanetFactory.KillEnemyFinally))]
    public static bool KillEnemyFinally_Prefix(PlanetFactory __instance, int enemyId)
    {
        if (!Multiplayer.IsActive || enemyId <= 0)
        {
            return true;
        }
        if (Multiplayer.Session.IsServer)
        {
            var starId = __instance.planet.star.id;
            Multiplayer.Session.Network.SendPacketToStar(new DFGKillEnemyPacket(__instance.planetId, enemyId), starId);
            return true;
        }
        if (Multiplayer.Session.Combat.IsIncomingRequest.Value)
        {
            return true;
        }

        // Client: wait for server to approve the unitId and enmeyId recycle
        // Make this enemyData appear as empty
        ref var enemyPtr = ref __instance.enemyPool[enemyId];
        enemyPtr.isInvincible = true;
        enemyPtr.id = 0;
        Multiplayer.Session.Network.SendPacket(new DFGKillEnemyPacket(__instance.planetId, enemyId));

        return false;
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(PlanetFactory.RemoveEnemyFinal))]
    public static bool RemoveEnemyFinal_Prefix()
    {
        if (!Multiplayer.IsActive || Multiplayer.Session.IsServer)
        {
            return true;
        }
        // Only execute RemoveEnemyFinal when server approve on client
        // Factories.IsIncomingRequest is for RemoveBasePit called on putting down Geothermal Power Station
        return Multiplayer.Session.Combat.IsIncomingRequest.Value || Multiplayer.Session.Factories.IsIncomingRequest.Value;
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(PlanetFactory.KillEntityFinally))]
    public static bool KillEntityFinally_Prefix(PlanetFactory __instance, int objId)
    {
        if (!Multiplayer.IsActive || objId <= 0)
        {
            return true;
        }

        if (Multiplayer.Session.IsClient) // Let server decide when to kill entity
        {
            return Multiplayer.Session.Factories.IsIncomingRequest.Value;
        }

        var packet = new KillEntityRequest(__instance.planetId, objId);
        var starId = __instance.planet.star.id;
        Multiplayer.Session.Server.SendPacketToStar(packet, starId);
        return true;
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(PlanetFactory.ReconstructTargetFinally))]
    public static void ReconstructTargetFinally_Prefix(PlanetFactory __instance, int prebuildId)
    {
        if (!Multiplayer.IsActive || Multiplayer.Session.Factories.IsIncomingRequest.Value) return;

        // Broadcast the change of isDestroyed state to other players
        var packet = new PrebuildReconstructPacket(__instance.planetId, prebuildId);
        if (Multiplayer.Session.IsServer)
        {
            var starId = __instance.planet.star.id;
            Multiplayer.Session.Network.SendPacketToStar(packet, starId);
        }
        else
        {
            Multiplayer.Session.Network.SendPacket(packet);
        }
    }

    #endregion

}
