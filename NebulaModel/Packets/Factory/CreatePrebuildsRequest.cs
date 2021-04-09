using NebulaModel.DataStructures;
using NebulaModel.Networking;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace NebulaModel.Packets.Factory
{
    public class CreatePrebuildsRequest
    {
        public int PlanetId { get; set; }
        public byte[] BuildPreviewData { get; set; }
        public Float3 PosePosition { get; set; }
        public Float4 PoseRotation { get; set; }

        public CreatePrebuildsRequest() { }

        public CreatePrebuildsRequest(int planetId, List<BuildPreview> buildPreviews, Pose pose)
        {
            PlanetId = planetId;
            PosePosition = new Float3(pose.position);
            PoseRotation = new Float4(pose.rotation);
            using (BinaryUtils.Writer writer = new BinaryUtils.Writer())
            {
                writer.BinaryWriter.Write(buildPreviews.Count);
                for (int i = 0; i < buildPreviews.Count; i++)
                {
                    SerializeBuildPreview(buildPreviews[i], buildPreviews, writer.BinaryWriter);
                }
                BuildPreviewData = writer.CloseAndGetBytes();
            }
        }

        public List<BuildPreview> GetBuildPreviews()
        {
            List<BuildPreview> result = new List<BuildPreview>();

            using (BinaryUtils.Reader reader = new BinaryUtils.Reader(BuildPreviewData))
            {
                int previewCount = reader.BinaryReader.ReadInt32();
                for (int i = 0; i < previewCount; i++)
                {
                    result.Add(new BuildPreview());
                }
                for (int i = 0; i < previewCount; i++)
                {
                    DeserializeBuildPreview(result[i], result, reader.BinaryReader);
                }
            }
            return result;
        }

        private void DeserializeBuildPreview(BuildPreview buildPreview, List<BuildPreview> list, BinaryReader br)
        {
            int outputRef = br.ReadInt32();
            buildPreview.output = outputRef == -1 ? null : list[outputRef];
            int inputRef = br.ReadInt32();
            buildPreview.input = inputRef == -1 ? null : list[inputRef];
            buildPreview.objId = br.ReadInt32();
            int num = br.ReadInt32();
            buildPreview.nearestPowerObjId = num == 0 ? null : new int[num];
            for (int i = 0; i < num; i++)
            {
                buildPreview.nearestPowerObjId[i] = br.ReadInt32();
            }
            buildPreview.coverObjId = br.ReadInt32();
            buildPreview.willCover = br.ReadBoolean();
            buildPreview.ignoreCollider = br.ReadBoolean();
            buildPreview.outputObjId = br.ReadInt32();
            buildPreview.inputObjId = br.ReadInt32();
            buildPreview.outputToSlot = br.ReadInt32();
            buildPreview.inputFromSlot = br.ReadInt32();
            buildPreview.outputFromSlot = br.ReadInt32();
            buildPreview.inputToSlot = br.ReadInt32();
            buildPreview.outputOffset = br.ReadInt32();
            buildPreview.inputOffset = br.ReadInt32();
            buildPreview.recipeId = br.ReadInt32();
            buildPreview.filterId = br.ReadInt32();
            buildPreview.isConnNode = br.ReadBoolean();
            buildPreview.desc = new PrefabDesc();
            buildPreview.desc.modelIndex = br.ReadInt32();
            buildPreview.item = new ItemProto();
            buildPreview.item.ID = br.ReadInt32();
            buildPreview.refCount = br.ReadInt32();
            buildPreview.refArr = new int[buildPreview.refCount];
            for (int i = 0; i < buildPreview.refCount; i++)
            {
                buildPreview.refArr[i] = br.ReadInt32();
            }
            buildPreview.lpos = new Vector3(br.ReadSingle(), br.ReadSingle(), br.ReadSingle());
            buildPreview.lpos2 = new Vector3(br.ReadSingle(), br.ReadSingle(), br.ReadSingle());
            buildPreview.lrot = new Quaternion(br.ReadSingle(), br.ReadSingle(), br.ReadSingle(), br.ReadSingle());
            buildPreview.lrot2 = new Quaternion(br.ReadSingle(), br.ReadSingle(), br.ReadSingle(), br.ReadSingle());
        }

        private void SerializeBuildPreview(BuildPreview buildPreview, List<BuildPreview> list, BinaryWriter bw)
        {
            bw.Write(list.IndexOf(buildPreview.output));
            bw.Write(list.IndexOf(buildPreview.input));
            bw.Write(buildPreview.objId);
            int num = buildPreview.nearestPowerObjId == null ? 0 : buildPreview.nearestPowerObjId.Length;
            bw.Write(num);
            for (int i = 0; i < num; i++)
            {
                bw.Write(buildPreview.nearestPowerObjId[i]);
            }
            bw.Write(buildPreview.coverObjId);
            bw.Write(buildPreview.willCover);
            bw.Write(buildPreview.ignoreCollider);
            bw.Write(buildPreview.outputObjId);
            bw.Write(buildPreview.inputObjId);
            bw.Write(buildPreview.outputToSlot);
            bw.Write(buildPreview.inputFromSlot);
            bw.Write(buildPreview.outputFromSlot);
            bw.Write(buildPreview.inputToSlot);
            bw.Write(buildPreview.outputOffset);
            bw.Write(buildPreview.inputOffset);
            bw.Write(buildPreview.recipeId);
            bw.Write(buildPreview.filterId);
            bw.Write(buildPreview.isConnNode);
            bw.Write(buildPreview.desc.modelIndex);
            bw.Write(buildPreview.item.ID);
            bw.Write(buildPreview.refCount);
            for (int i = 0; i < buildPreview.refCount; i++)
            {
                bw.Write(buildPreview.refArr[i]);
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
        }
    }
}
