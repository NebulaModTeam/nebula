#region

using System.Collections.Generic;
using NebulaAPI;
using NebulaAPI.Packets;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Factory.Foundation;
using NebulaWorld;
using UnityEngine;
#pragma warning disable IDE0007 // Use implicit type

#endregion

namespace NebulaNetwork.PacketProcessors.Factory.Foundation;

[RegisterPacketProcessor]
internal class FoundationBlueprintPasteProcessor : PacketProcessor<FoundationBlueprintPastePacket>
{
    protected override void ProcessPacket(FoundationBlueprintPastePacket packet, NebulaConnection conn)
    {
        var planet = GameMain.galaxy.PlanetById(packet.PlanetId);
        var factory = planet?.factory;
        if (factory == null) return;

        using (Multiplayer.Session.Planets.IsIncomingRequest.On())
        {
            Multiplayer.Session.Factories.TargetPlanet = packet.PlanetId;
            Multiplayer.Session.Factories.AddPlanetTimer(packet.PlanetId);
            var specifyPlanet = GameMain.gpuiManager.specifyPlanet;
            GameMain.gpuiManager.specifyPlanet = planet;

            // Split BuildTool_BlueprintPaste.DetermineReforms into following functions
            SetReform(factory, packet.ReformGridIds, packet.ReformType, packet.ReformColor);
            AlterHeightMap(planet, packet.LevelChanges);
            RemoveVeges(factory, packet.LevelChanges);
            UpdateGeothermalStrength(factory);
            AlterVeinModels(factory);

            GameMain.gpuiManager.specifyPlanet = specifyPlanet;
            Multiplayer.Session.Factories.TargetPlanet = NebulaModAPI.PLANET_NONE;
        }
    }

    static void SetReform(PlanetFactory factory, Dictionary<int, byte> reformGridIds, int brushType, int brushColor)
    {
        PlatformSystem platformSystem = factory.platformSystem;
        platformSystem.EnsureReformData();

        foreach (int key in reformGridIds.Keys)
        {
            int reformIndex = factory.platformSystem.GetReformIndex(key >> 16, key & 65535);
            byte reformData = reformGridIds[key];
            int num17 = reformData >> 5;
            int num18 = (reformData & 3);
            if (reformIndex >= 0)
            {
                int reformType = platformSystem.GetReformType(reformIndex);
                int reformColor = platformSystem.GetReformColor(reformIndex);
                if (reformType != ((num17 == 0) ? brushType : num17) || reformColor != ((num18 == 0) ? brushColor : num18))
                {
                    factory.platformSystem.SetReformType(reformIndex, brushType);
                    factory.platformSystem.SetReformColor(reformIndex, brushColor);
                }
            }
        }
    }
    static void AlterHeightMap(PlanetData planet, Dictionary<int, int> heightLevelChanges)
    {
        PlanetRawData planetRawData = planet.data;
        bool isPlanetLevelized = planet.levelized;

        // Calculate the maximum height threshold for terrain modifications
        int heightThreshold = (int)(planet.realRadius * 100f + 20f) - 60;

        ushort[] heightMap = planetRawData.heightData;

        foreach (KeyValuePair<int, int> heightChange in heightLevelChanges)
        {
            if (heightChange.Value > 0)
            {
                if (isPlanetLevelized)
                {
                    if (heightMap[heightChange.Key] >= heightThreshold)
                    {
                        if (planetRawData.GetModLevel(heightChange.Key) < 3)
                        {
                            planetRawData.SetModPlane(heightChange.Key, 0);
                        }
                        planet.AddHeightMapModLevel(heightChange.Key, heightChange.Value);
                    }
                }
                else
                {
                    planet.AddHeightMapModLevel(heightChange.Key, heightChange.Value);
                }
            }
        }

        bool meshesUpdated = planet.UpdateDirtyMeshes();
        if (GameMain.isRunning && meshesUpdated && planet == GameMain.localPlanet)
        {
            planet.factory?.RenderLocalPlanetHeightmap();
        }
    }

    static void RemoveVeges(PlanetFactory planetFactory, Dictionary<int, int> heightLevelChanges)
    {
        PlanetRawData planetRawData = planetFactory.planet.data;
        VegeData[] vegePool = planetFactory.vegePool;
        int vegeCursor = planetFactory.vegeCursor;

        for (int vegetationIndex = 1; vegetationIndex < vegeCursor; vegetationIndex++)
        {
            if (vegePool[vegetationIndex].id != 0)
            {
                int terrainIndex = planetRawData.QueryIndex(vegePool[vegetationIndex].pos);
                if (heightLevelChanges.TryGetValue(terrainIndex, out int modificationLevel) && modificationLevel >= 3)
                {
                    planetFactory.RemoveVegeWithComponents(vegetationIndex);
                }
            }
        }
    }

    static void UpdateGeothermalStrength(PlanetFactory factory)
    {
        if (factory.planet.waterItemId != -1) return;

        PowerSystem powerSystem = factory.powerSystem;
        PowerGeneratorComponent[] generatorPool = powerSystem.genPool;
        int generatorCount = powerSystem.genCursor;
        EntityData[] entityPool = factory.entityPool;

        for (int generatorIndex = 1; generatorIndex < generatorCount; generatorIndex++)
        {
            ref PowerGeneratorComponent generator = ref generatorPool[generatorIndex];
            if (generator.id == generatorIndex && generator.geothermal && generator.entityId > 0)
            {
                generator.gthStrength = powerSystem.CalculateGeothermalStrenth(
                    entityPool[generator.entityId].pos,
                    entityPool[generator.entityId].rot,
                    generator.baseRuinId
                );
            }
        }
    }

    static void AlterVeinModels(PlanetFactory factory)
    {
        PlanetData planet = factory.planet;
        PlanetRawData data = planet.data;

        VeinData[] veinPool = factory.veinPool;
        int veinCursor = factory.veinCursor;
        float veinSurfaceThreshold = planet.realRadius - 50f + 5f;
        PlanetPhysics physics = planet.physics;
        for (int veinId = 1; veinId < veinCursor; veinId++)
        {
            ref VeinData veinPtr = ref veinPool[veinId];
            if (veinPtr.id == veinId)
            {
                Vector3 pos = veinPtr.pos;
                float magnitude = pos.magnitude;
                if (magnitude >= veinSurfaceThreshold)
                {
                    float veinHeight = data.QueryModifiedHeight(pos) - 0.13f;
                    int colliderId = veinPtr.colliderId;
                    Vector3 vector = physics.GetColliderData(colliderId).pos.normalized * (veinHeight + 0.4f);
                    int chunkId = colliderId >> 20;
                    colliderId &= 1048575;
                    physics.colChunks[chunkId].colliderPool[colliderId].pos = vector;
                    Vector3 vectorNormal = pos / magnitude;
                    veinPtr.pos = vectorNormal * veinHeight;
                    physics.SetPlanetPhysicsColliderDirty();
                    if (Mathf.Abs(magnitude - veinSurfaceThreshold) > 0.1f)
                    {
                        Quaternion quaternion = Maths.SphericalRotation(pos, Random.value * 360f);
                        GameMain.gpuiManager.AlterModel(veinPtr.modelIndex, veinPtr.modelId, veinId, pos, quaternion, false);
                    }
                    else
                    {
                        GameMain.gpuiManager.AlterModel(veinPtr.modelIndex, veinPtr.modelId, veinId, pos, false);
                    }
                    VeinProto veinProto = LDB.veins.Select((int)veinPool[veinId].type);
                    if (veinProto != null)
                    {
                        if (veinPool[veinId].minerId0 > 0)
                        {
                            GameMain.gpuiManager.AlterModel(veinProto.MinerBaseModelIndex, veinPtr.minerBaseModelId, veinPtr.minerId0, vectorNormal * (veinHeight + 0.1f), false);
                            GameMain.gpuiManager.AlterModel(veinProto.MinerCircleModelIndex, veinPtr.minerCircleModelId0, veinPtr.minerId0, vectorNormal * (veinHeight + 0.4f), false);
                        }
                        if (veinPool[veinId].minerId1 > 0)
                        {
                            GameMain.gpuiManager.AlterModel(veinProto.MinerCircleModelIndex, veinPtr.minerCircleModelId1, veinPtr.minerId1, vectorNormal * (veinHeight + 0.6f), false);
                        }
                        if (veinPool[veinId].minerId2 > 0)
                        {
                            GameMain.gpuiManager.AlterModel(veinProto.MinerCircleModelIndex, veinPtr.minerCircleModelId2, veinPtr.minerId2, vectorNormal * (veinHeight + 0.8f), false);
                        }
                        if (veinPool[veinId].minerId3 > 0)
                        {
                            GameMain.gpuiManager.AlterModel(veinProto.MinerCircleModelIndex, veinPtr.minerCircleModelId3, veinPtr.minerId3, vectorNormal * (veinHeight + 1f), false);
                        }
                    }
                }
            }
        }
    }


}
