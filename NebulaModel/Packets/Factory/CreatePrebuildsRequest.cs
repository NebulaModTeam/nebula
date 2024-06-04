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

    public int PlanetId { get; set; }
    public byte[] BuildPreviewData { get; set; }
    public int AuthorId { get; set; }
    public string BuildToolType { get; set; }
    public int PrebuildId { get; set; }

    public bool TryGetBuildPreviews(out List<BuildPreview> buildPreviews)
    {
        buildPreviews = new();

        try
        {
            using var reader = new BinaryUtils.Reader(BuildPreviewData);
            var previewCount = reader.BinaryReader.ReadInt32();
            for (var i = 0; i < previewCount; i++)
            {
                buildPreviews.Add(new BuildPreview());
            }
            for (var i = 0; i < previewCount; i++)
            {
                DeserializeBuildPreview(buildPreviews[i], buildPreviews, reader.BinaryReader);
            }
            return true;
        }
        catch (System.Exception e)
        {
            Logger.Log.WarnInform("DeserializeBuildPreview parse error\n" + e);
            return false;
        }
    }

    public static void DeserializeBuildPreview(BuildPreview buildPreview, IReadOnlyList<BuildPreview> list, BinaryReader br)
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

        var modelIndex = br.ReadInt32();
        if (modelIndex >= 0 && modelIndex < LDB.models.modelArray.Length)
        {
            var modelProto = LDB.models.modelArray[modelIndex];
            if (modelProto != null)
            {
                buildPreview.desc = modelProto.prefabDesc;
            }
        }
        if (buildPreview.desc == null)
        {
            throw new System.Exception("DeserializeBuildPreview: can't find modelIndx " + modelIndex);
        }
        var itemId = br.ReadInt32();
        buildPreview.item = LDB.items.Select(itemId);
        if (buildPreview.item == null)
        {
            throw new System.Exception("DeserializeBuildPreview: can't find itemId " + itemId);
        }

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
        buildPreview.tilt = br.ReadSingle();
        buildPreview.condition = (EBuildCondition)br.ReadInt32();
    }

    public static void SerializeBuildPreview(BuildPreview buildPreview, IList<BuildPreview> list, BinaryWriter bw)
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

        //Export the index of PrefabDesc and fetch in LDB.models on the receiving end
        bw.Write(buildPreview.desc.modelIndex);
        //Export the index of ItemProto and featch by LDB.items on the receiving end
        bw.Write(buildPreview.item.ID);

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
        bw.Write(buildPreview.tilt);
        bw.Write((int)buildPreview.condition);
    }
}
