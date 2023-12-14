#region

using System.Collections.Generic;
using System.IO;
using NebulaModel.Networking;
using UnityEngine;

#endregion

namespace NebulaModel.Packets.Factory;

public class CreatePrebuildsRequest
{
    public CreatePrebuildsRequest() { }

    public CreatePrebuildsRequest(int planetId, List<BuildPreview> buildPreviews, int playerId, string buildToolType,
        int prebuildId)
    {
        AuthorId = playerId;
        PlanetId = planetId;
        BuildToolType = buildToolType;
        using (var writer = new BinaryUtils.Writer())
        {
            writer.BinaryWriter.Write(buildPreviews.Count);
            foreach (var t in buildPreviews)
            {
                SerializeBuildPreview(t, buildPreviews, writer.BinaryWriter);
            }
            BuildPreviewData = writer.CloseAndGetBytes();
        }
        PrebuildId = prebuildId;
    }

    public int PlanetId { get; }
    private byte[] BuildPreviewData { get; }
    public int AuthorId { get; }
    public string BuildToolType { get; }
    public int PrebuildId { get; }

    public List<BuildPreview> GetBuildPreviews()
    {
        var result = new List<BuildPreview>();

        using var reader = new BinaryUtils.Reader(BuildPreviewData);
        var previewCount = reader.BinaryReader.ReadInt32();
        for (var i = 0; i < previewCount; i++)
        {
            result.Add(new BuildPreview());
        }
        for (var i = 0; i < previewCount; i++)
        {
            DeserializeBuildPreview(result[i], result, reader.BinaryReader);
        }
        return result;
    }

    private static void DeserializeBuildPreview(BuildPreview buildPreview, IReadOnlyList<BuildPreview> list, BinaryReader br)
    {
        var outputRef = br.ReadInt32();
        buildPreview.output = outputRef == -1 ? null : list[outputRef];
        var inputRef = br.ReadInt32();
        buildPreview.input = inputRef == -1 ? null : list[inputRef];
        buildPreview.objId = br.ReadInt32();
        var num = br.ReadInt32();
        buildPreview.nearestPowerObjId = num == 0 ? null : new int[num];
        for (var i = 0; i < num; i++)
        {
            if (buildPreview.nearestPowerObjId != null)
            {
                buildPreview.nearestPowerObjId[i] = br.ReadInt32();
            }
        }
        buildPreview.coverObjId = br.ReadInt32();
        buildPreview.willRemoveCover = br.ReadBoolean();
        buildPreview.genNearColliderArea2 = br.ReadSingle();
        buildPreview.outputObjId = br.ReadInt32();
        buildPreview.inputObjId = br.ReadInt32();
        buildPreview.outputToSlot = br.ReadInt32();
        buildPreview.inputFromSlot = br.ReadInt32();
        buildPreview.outputFromSlot = br.ReadInt32();
        buildPreview.inputToSlot = br.ReadInt32();
        buildPreview.outputOffset = br.ReadInt32();
        buildPreview.inputOffset = br.ReadInt32();
        buildPreview.needModel = br.ReadBoolean();
        buildPreview.previewIndex = br.ReadInt32();
        buildPreview.bpgpuiModelId = br.ReadInt32();
        buildPreview.bpgpuiModelInstIndex = br.ReadInt32();
        buildPreview.recipeId = br.ReadInt32();
        buildPreview.filterId = br.ReadInt32();
        buildPreview.isConnNode = br.ReadBoolean();
        buildPreview.desc = new PrefabDesc
        {
            //Import more data about the Prefab to properly validate the build condition on server-side
            assemblerRecipeType = (ERecipeType)br.ReadInt32(),
            cullingHeight = br.ReadSingle(),
            gammaRayReceiver = br.ReadBoolean(),
            inserterSTT = br.ReadInt32(),
            isAccumulator = br.ReadBoolean(),
            isAssembler = br.ReadBoolean(),
            isBelt = br.ReadBoolean(),
            isCollectStation = br.ReadBoolean(),
            isDispenser = br.ReadBoolean(),
            isEjector = br.ReadBoolean(),
            isFractionator = br.ReadBoolean(),
            isInserter = br.ReadBoolean(),
            isLab = br.ReadBoolean(),
            isPowerExchanger = br.ReadBoolean(),
            isSplitter = br.ReadBoolean(),
            isStation = br.ReadBoolean(),
            isStellarStation = br.ReadBoolean(),
            isStorage = br.ReadBoolean(),
            isTank = br.ReadBoolean(),
            isVeinCollector = br.ReadBoolean(),
            minerType = (EMinerType)br.ReadInt32(),
            modelIndex = br.ReadInt32(),
            multiLevel = br.ReadBoolean(),
            oilMiner = br.ReadBoolean(),
            stationCollectSpeed = br.ReadInt32(),
            veinMiner = br.ReadBoolean(),
            windForcedPower = br.ReadBoolean(),
            workEnergyPerTick = br.ReadInt64()
        };

        //Import information about the position of build (land / sea)
        num = br.ReadInt32();
        buildPreview.desc.landPoints = new Vector3[num];
        for (var i = 0; i < num; i++)
        {
            buildPreview.desc.landPoints[i] = new Vector3(br.ReadSingle(), br.ReadSingle(), br.ReadSingle());
        }
        num = br.ReadInt32();
        buildPreview.desc.waterPoints = new Vector3[num];
        for (var i = 0; i < num; i++)
        {
            buildPreview.desc.waterPoints[i] = new Vector3(br.ReadSingle(), br.ReadSingle(), br.ReadSingle());
        }

        //Import information about the collider to check the collisions
        buildPreview.desc.hasBuildCollider = br.ReadBoolean();
        num = br.ReadInt32();
        buildPreview.desc.buildColliders = new ColliderData[num];
        for (var i = 0; i < num; i++)
        {
            buildPreview.desc.buildColliders[i].pos = new Vector3(br.ReadSingle(), br.ReadSingle(), br.ReadSingle());
            buildPreview.desc.buildColliders[i].ext = new Vector3(br.ReadSingle(), br.ReadSingle(), br.ReadSingle());
            buildPreview.desc.buildColliders[i].q =
                new Quaternion(br.ReadSingle(), br.ReadSingle(), br.ReadSingle(), br.ReadSingle());
            buildPreview.desc.buildColliders[i].radius = br.ReadSingle();
            buildPreview.desc.buildColliders[i].idType = br.ReadInt32();
            buildPreview.desc.buildColliders[i].link = br.ReadInt32();
        }

        buildPreview.item = new ItemProto { ID = br.ReadInt32(), BuildMode = br.ReadInt32(), Grade = br.ReadInt32() };

        buildPreview.paramCount = br.ReadInt32();
        buildPreview.parameters = new int[buildPreview.paramCount];
        for (var i = 0; i < buildPreview.paramCount; i++)
        {
            buildPreview.parameters[i] = br.ReadInt32();
        }
        buildPreview.lpos = new Vector3(br.ReadSingle(), br.ReadSingle(), br.ReadSingle());
        buildPreview.lpos2 = new Vector3(br.ReadSingle(), br.ReadSingle(), br.ReadSingle());
        buildPreview.lrot = new Quaternion(br.ReadSingle(), br.ReadSingle(), br.ReadSingle(), br.ReadSingle());
        buildPreview.lrot2 = new Quaternion(br.ReadSingle(), br.ReadSingle(), br.ReadSingle(), br.ReadSingle());
        buildPreview.condition = (EBuildCondition)br.ReadInt32();
    }

    private static void SerializeBuildPreview(BuildPreview buildPreview, IList<BuildPreview> list, BinaryWriter bw)
    {
        bw.Write(list.IndexOf(buildPreview.output));
        bw.Write(list.IndexOf(buildPreview.input));
        bw.Write(buildPreview.objId);
        var num = buildPreview.nearestPowerObjId?.Length ?? 0;
        bw.Write(num);
        for (var i = 0; i < num; i++)
        {
            bw.Write(buildPreview.nearestPowerObjId![i]);
        }
        bw.Write(buildPreview.coverObjId);
        bw.Write(buildPreview.willRemoveCover);
        bw.Write(buildPreview.genNearColliderArea2);
        bw.Write(buildPreview.outputObjId);
        bw.Write(buildPreview.inputObjId);
        bw.Write(buildPreview.outputToSlot);
        bw.Write(buildPreview.inputFromSlot);
        bw.Write(buildPreview.outputFromSlot);
        bw.Write(buildPreview.inputToSlot);
        bw.Write(buildPreview.outputOffset);
        bw.Write(buildPreview.inputOffset);
        bw.Write(buildPreview.needModel);
        bw.Write(buildPreview.previewIndex);
        bw.Write(buildPreview.bpgpuiModelId);
        bw.Write(buildPreview.bpgpuiModelInstIndex);
        bw.Write(buildPreview.recipeId);
        bw.Write(buildPreview.filterId);
        bw.Write(buildPreview.isConnNode);

        //Export more data about the Prefab to properly validate the build condition on server-side
        bw.Write((int)buildPreview.desc.assemblerRecipeType);
        bw.Write(buildPreview.desc.cullingHeight);
        bw.Write(buildPreview.desc.gammaRayReceiver);
        bw.Write(buildPreview.desc.inserterSTT);
        bw.Write(buildPreview.desc.isAccumulator);
        bw.Write(buildPreview.desc.isAssembler);
        bw.Write(buildPreview.desc.isBelt);
        bw.Write(buildPreview.desc.isCollectStation);
        bw.Write(buildPreview.desc.isDispenser);
        bw.Write(buildPreview.desc.isEjector);
        bw.Write(buildPreview.desc.isFractionator);
        bw.Write(buildPreview.desc.isInserter);
        bw.Write(buildPreview.desc.isLab);
        bw.Write(buildPreview.desc.isPowerExchanger);
        bw.Write(buildPreview.desc.isSplitter);
        bw.Write(buildPreview.desc.isStation);
        bw.Write(buildPreview.desc.isStellarStation);
        bw.Write(buildPreview.desc.isStorage);
        bw.Write(buildPreview.desc.isTank);
        bw.Write(buildPreview.desc.isVeinCollector);
        bw.Write((int)buildPreview.desc.minerType);
        bw.Write(buildPreview.desc.modelIndex);
        bw.Write(buildPreview.desc.multiLevel);
        bw.Write(buildPreview.desc.oilMiner);
        bw.Write(buildPreview.desc.stationCollectSpeed);
        bw.Write(buildPreview.desc.veinMiner);
        bw.Write(buildPreview.desc.windForcedPower);
        bw.Write(buildPreview.desc.workEnergyPerTick);

        //Export information about the position of build (land / sea)
        num = buildPreview.desc.landPoints.Length;
        bw.Write(num);
        for (var i = 0; i < num; i++)
        {
            bw.Write(buildPreview.desc.landPoints[i].x);
            bw.Write(buildPreview.desc.landPoints[i].y);
            bw.Write(buildPreview.desc.landPoints[i].z);
        }
        num = buildPreview.desc.waterPoints.Length;
        bw.Write(num);
        for (var i = 0; i < num; i++)
        {
            bw.Write(buildPreview.desc.waterPoints[i].x);
            bw.Write(buildPreview.desc.waterPoints[i].y);
            bw.Write(buildPreview.desc.waterPoints[i].z);
        }
        //Export information about the collider to check the collisions
        bw.Write(buildPreview.desc.hasBuildCollider);
        num = buildPreview.desc.buildColliders.Length;
        bw.Write(num);
        for (var i = 0; i < num; i++)
        {
            bw.Write(buildPreview.desc.buildColliders[i].pos.x);
            bw.Write(buildPreview.desc.buildColliders[i].pos.y);
            bw.Write(buildPreview.desc.buildColliders[i].pos.z);
            bw.Write(buildPreview.desc.buildColliders[i].ext.x);
            bw.Write(buildPreview.desc.buildColliders[i].ext.y);
            bw.Write(buildPreview.desc.buildColliders[i].ext.z);
            bw.Write(buildPreview.desc.buildColliders[i].q.x);
            bw.Write(buildPreview.desc.buildColliders[i].q.y);
            bw.Write(buildPreview.desc.buildColliders[i].q.z);
            bw.Write(buildPreview.desc.buildColliders[i].q.w);
            bw.Write(buildPreview.desc.buildColliders[i].radius);
            bw.Write(buildPreview.desc.buildColliders[i].idType);
            bw.Write(buildPreview.desc.buildColliders[i].link);
        }

        bw.Write(buildPreview.item.ID);
        bw.Write(buildPreview.item.BuildMode);
        bw.Write(buildPreview.item.Grade);
        bw.Write(buildPreview.paramCount);
        for (var i = 0; i < buildPreview.paramCount; i++)
        {
            bw.Write(buildPreview.parameters[i]);
        }
        bw.Write(buildPreview.lpos.x);
        bw.Write(buildPreview.lpos.y);
        bw.Write(buildPreview.lpos.z);
        bw.Write(buildPreview.lpos2.x);
        bw.Write(buildPreview.lpos2.y);
        bw.Write(buildPreview.lpos2.z);
        bw.Write(buildPreview.lrot.x);
        bw.Write(buildPreview.lrot.y);
        bw.Write(buildPreview.lrot.z);
        bw.Write(buildPreview.lrot.w);
        bw.Write(buildPreview.lrot2.x);
        bw.Write(buildPreview.lrot2.y);
        bw.Write(buildPreview.lrot2.z);
        bw.Write(buildPreview.lrot2.w);
        bw.Write((int)buildPreview.condition);
    }
}
