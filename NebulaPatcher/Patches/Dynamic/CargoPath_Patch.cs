using HarmonyLib;
using System.IO;
using UnityEngine;
using System.Collections.Generic;
using NebulaModel;
using NebulaWorld.Factory;

namespace NebulaPatcher.Patches.Dynamic
{
    [HarmonyPatch(typeof(CargoPath))]
    class CargoPath_Patch
    {

        [HarmonyPrefix]
        [HarmonyPatch(nameof(CargoPath.Export))]
        public static bool Export_Prefix(
            ref CargoPath __instance, 
            //ref int ___capacity, 
            //ref int ___bufferLength, 
            //ref int ___chunkCapacity, 
            //ref int ___chunkCount, 
            //ref int ___updateLen, 
            BinaryWriter w
        )
        {
            //// The original implementation
            //w.Write(0);
            //w.Write(__instance.id);
            //w.Write(___capacity);
            //w.Write(___bufferLength);
            //w.Write(___chunkCapacity);
            //w.Write(___chunkCount);
            //w.Write(___updateLen);
            //w.Write(__instance.closed);
            //w.Write((__instance.outputPath == null) ? 0 : __instance.outputPath.id);
            //w.Write((__instance.outputPath == null) ? -1 : __instance.outputIndex);
            //w.Write(__instance.belts.Count);
            //w.Write(__instance.inputPaths.Count);
            //w.Write(__instance.buffer, 0, ___bufferLength);
            //for (int i = 0; i < ___chunkCount; i++)
            //{
            //    w.Write(__instance.chunks[i * 3]);
            //    w.Write(__instance.chunks[i * 3 + 1]);
            //    w.Write(__instance.chunks[i * 3 + 2]);
            //}
            //for (int j = 0; j < ___bufferLength; j++)
            //{
            //    w.Write(__instance.pointPos[j].x);
            //    w.Write(__instance.pointPos[j].y);
            //    w.Write(__instance.pointPos[j].z);
            //    w.Write(__instance.pointRot[j].x);
            //    w.Write(__instance.pointRot[j].y);
            //    w.Write(__instance.pointRot[j].z);
            //    w.Write(__instance.pointRot[j].w);
            //}
            //for (int k = 0; k < __instance.belts.Count; k++)
            //{
            //    w.Write(__instance.belts[k]);
            //}
            //for (int l = 0; l < __instance.inputPaths.Count; l++)
            //{
            //    w.Write(__instance.inputPaths[l]);
            //}

            if (!Config.Options.GlobalSavegameCompression)
            {
                // If savegame compression is not enabled skip through to the original implementation
                return true;
            }

            CustomExporters.CustomCargoPathExport(w, ref __instance);

            // Skip the original function
            return false;

        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(CargoPath.Import))]
        public static bool Import_Prefix(
            ref CargoPath __instance,
            //ref int ___capacity,
            ref int ___bufferLength,
            //ref int ___chunkCapacity,
            ref int ___chunkCount,
            ref int ___updateLen,
            BinaryReader r
        )
        {
            // We use 4 bytes at the beginning of the data that are not used in the original implementation to store the version,
            // this circumvents having to set the position of the BaseStream which is not possible if an underlying LZ4Stream is used
            var encodedVersion = r.ReadInt32();
            if (encodedVersion == CustomExporters.EncodedVersionNb00)
            {
                __instance.Free();
                //r.ReadInt32(); // Since we can't reset the BinaryReader's BaseStream to its original position and we encode the version here we skip reading these not used 4 bytes
                __instance.id = r.ReadInt32();
                __instance.SetCapacity(r.ReadInt32());
                ___bufferLength = r.ReadInt32();
                __instance.SetChunkCapacity(r.ReadInt32());
                ___chunkCount = r.ReadInt32();
                ___updateLen = r.ReadInt32();
                __instance.closed = r.ReadBoolean();
                __instance.outputPathIdForImport = r.ReadInt32();
                __instance.outputIndex = r.ReadInt32();
                int beltsCount = r.ReadInt32();
                int inputPathsCount = r.ReadInt32();
                r.BaseStream.Read(__instance.buffer, 0, ___bufferLength);
                for (int i = 0; i < ___chunkCount; i++)
                {
                    __instance.chunks[i * 3] = r.ReadInt32();
                    __instance.chunks[i * 3 + 1] = r.ReadInt32();
                    __instance.chunks[i * 3 + 2] = r.ReadInt32();
                }

                // Code that reads/calculates the rotations and positions
                for (int j = 0; j < ___bufferLength;)
                {
                    var sameRelativeRotationAsPrevious = r.ReadBoolean();
                    var repCount = r.ReadInt32();

                    var previosRelativeRotation = new Quaternion();
                    var inSequence = false;
                    var planarMatrix = new Matrix4x4();
                    for (int i = 0; i < repCount; i++)
                    {
                        if (sameRelativeRotationAsPrevious)
                        {

                            if (!inSequence)
                            {
                                previosRelativeRotation = new Quaternion(r.ReadSingle(), r.ReadSingle(), r.ReadSingle(), r.ReadSingle());
           
                                planarMatrix = new Matrix4x4(
                                    new Vector4(r.ReadSingle(), r.ReadSingle(), r.ReadSingle(), 0),
                                    new Vector4(r.ReadSingle(), r.ReadSingle(), r.ReadSingle(), 0),
                                    new Vector4(r.ReadSingle(), r.ReadSingle(), r.ReadSingle(), 0),
                                    new Vector4(r.ReadSingle(), r.ReadSingle(), r.ReadSingle(), 1)
                                );

                                inSequence = true;
                            }

                            var planarPosition = new Vector3(r.ReadSingle(), r.ReadSingle());
                            __instance.pointPos[j] = planarMatrix.MultiplyPoint3x4(planarPosition);

                            var originalPosition = __instance.pointPos[j];
                            var calculatedRotation = Quaternion.LookRotation(originalPosition, Vector3.up) * previosRelativeRotation;
                            __instance.pointRot[j] = calculatedRotation;

                        }
                        else
                        {
                            __instance.pointRot[j].x = r.ReadSingle();
                            __instance.pointRot[j].y = r.ReadSingle();
                            __instance.pointRot[j].z = r.ReadSingle();
                            __instance.pointRot[j].w = r.ReadSingle();

                            __instance.pointPos[j].x = r.ReadSingle();
                            __instance.pointPos[j].y = r.ReadSingle();
                            __instance.pointPos[j].z = r.ReadSingle();

                            inSequence = false;
                        }

                        j++;

                    }

                }

                __instance.belts = new List<int>();
                for (int k = 0; k < beltsCount; k++)
                {
                    __instance.belts.Add(r.ReadInt32());
                }
                __instance.inputPaths = new List<int>();
                for (int l = 0; l < inputPathsCount; l++)
                {
                    __instance.inputPaths.Add(r.ReadInt32());
                }

            } else
            {
                // Run a sligltly modified original implementation
                __instance.Free();
                //r.ReadInt32(); // Since we can't reset the BinaryReader's BaseStream to its original position and we encode the version here we skip reading these not used 4 bytes
                __instance.id = r.ReadInt32();
                __instance.SetCapacity(r.ReadInt32());
                ___bufferLength = r.ReadInt32();
                __instance.SetChunkCapacity(r.ReadInt32());
                ___chunkCount = r.ReadInt32();
                ___updateLen = r.ReadInt32();
                __instance.closed = r.ReadBoolean();
                __instance.outputPathIdForImport = r.ReadInt32();
                __instance.outputIndex = r.ReadInt32();
                int num = r.ReadInt32();
                int num2 = r.ReadInt32();
                r.BaseStream.Read(__instance.buffer, 0, ___bufferLength);
                for (int i = 0; i < ___chunkCount; i++)
                {
                    __instance.chunks[i * 3] = r.ReadInt32();
                    __instance.chunks[i * 3 + 1] = r.ReadInt32();
                    __instance.chunks[i * 3 + 2] = r.ReadInt32();
                }
                for (int j = 0; j < ___bufferLength; j++)
                {
                    __instance.pointPos[j].x = r.ReadSingle();
                    __instance.pointPos[j].y = r.ReadSingle();
                    __instance.pointPos[j].z = r.ReadSingle();
                    __instance.pointRot[j].x = r.ReadSingle();
                    __instance.pointRot[j].y = r.ReadSingle();
                    __instance.pointRot[j].z = r.ReadSingle();
                    __instance.pointRot[j].w = r.ReadSingle();
                }
                __instance.belts = new List<int>();
                for (int k = 0; k < num; k++)
                {
                    __instance.belts.Add(r.ReadInt32());
                }
                __instance.inputPaths = new List<int>();
                for (int l = 0; l < num2; l++)
                {
                    __instance.inputPaths.Add(r.ReadInt32());
                }
            }            

            // Skip the original function
            return false;
        }
    }
}
