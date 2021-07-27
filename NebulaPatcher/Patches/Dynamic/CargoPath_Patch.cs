using HarmonyLib;
using System.IO;
using UnityEngine;
using System.Text;
using System;
using System.Collections.Generic;

namespace NebulaPatcher.Patches.Dynamic
{
    [HarmonyPatch(typeof(CargoPath))]
    class CargoPath_Patch
    {

        private static Matrix4x4 Get3DPoint3DPlaneAs2DPointProjectionMatrix2(Vector3 point0, Vector3 point1, Vector3 point2)
        {
            // Adapted from https://stackoverflow.com/a/52163563/13620003

            var x = point1 - point0;
            var y = point2 - point0;
            var z = Vector3.Cross(x, y);
            y = Vector3.Cross(z, x);
            x.Normalize();
            z.Normalize();
            y.Normalize();

            return new Matrix4x4(
                new Vector4(x.x, x.y, x.z, 0),
                new Vector4(y.x, y.y, y.z, 0),
                new Vector4(z.x, z.y, z.z, 0),
                new Vector4(point0.x, point0.y, point0.z, 1)
            );

        }

        [HarmonyPrefix]
        [HarmonyPatch("Export")]
        public static bool Export_Prefix(
            ref CargoPath __instance, 
            ref int ___capacity, 
            ref int ___bufferLength, 
            ref int ___chunkCapacity, 
            ref int ___chunkCount, 
            ref int ___updateLen, 
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

            

            //w.Write(0);
            // Set a special int that we will use to indentify wheter whe should process this normally or use custom optimized method
            // NOTE: This needs to be 4 chars long, else it wont work
            w.Write(BitConverter.ToInt32(Encoding.ASCII.GetBytes("nb00"), 0));

            w.Write(__instance.id);
            w.Write(___capacity);
            w.Write(___bufferLength);
            w.Write(___chunkCapacity);
            w.Write(___chunkCount);
            w.Write(___updateLen);
            w.Write(__instance.closed);
            w.Write((__instance.outputPath == null) ? 0 : __instance.outputPath.id);
            w.Write((__instance.outputPath == null) ? -1 : __instance.outputIndex);
            w.Write(__instance.belts.Count);
            w.Write(__instance.inputPaths.Count);
            w.Write(__instance.buffer, 0, ___bufferLength);
            for (int i = 0; i < ___chunkCount; i++)
            {
                w.Write(__instance.chunks[i * 3]);
                w.Write(__instance.chunks[i * 3 + 1]);
                w.Write(__instance.chunks[i * 3 + 2]);
            }

            // START of rotational and positional compression code
            {             

                // Build the prerequisite mappings
                var surfaceRelativeRotations = new Quaternion[___bufferLength];
                var simmilarityMap = new bool[___bufferLength - 1];
                var differentialIndexes = new List<int>();
                for (int j = 0; j < ___bufferLength; j++)
                {

                    var originalPosition = __instance.pointPos[j];
                    var originalRotation = __instance.pointRot[j];
                    var surfaceRelativeRotation = Quaternion.Inverse(Quaternion.LookRotation(originalPosition, Vector3.up)) * originalRotation;
                    surfaceRelativeRotations[j] = surfaceRelativeRotation;

                    // Construct the simmilairy map
                    if (j > 0)
                    {
                        // TODO: Might need to changes the rotations around
                        // TODO: Change this to use the pure dot product if possible
                        var angularDiff = Quaternion.Angle(surfaceRelativeRotations[j - 1], surfaceRelativeRotations[j]);
                        simmilarityMap[j - 1] = angularDiff == 0;
                    }

                    // Construct the differential simmilarity map (since changed to store indexes only)
                    if (j > 1)
                    {
                        var differentialSimmilarity = simmilarityMap[j - 2] == simmilarityMap[j - 1];
                        if (!differentialSimmilarity)
                        {
                            differentialIndexes.Add(j - 2);
                        }
                    }
                }

                // Write data in a compressed format
                var startIndex = 0;
                int endIndex;
                for (int j = 0; j < differentialIndexes.Count; j++)
                {
                    var differentialIndex = differentialIndexes[j];
                    var surfaceRelativeSequence = simmilarityMap[differentialIndex];

                    endIndex = differentialIndex + (surfaceRelativeSequence ? 1 : 0);
                    var repCount = endIndex - startIndex + 1;

                    // TODO: sometimes we have a repcount of 0 we can then just skip this entire sequence
                    // TODO: compress the surfaceRelativeSequence and the repCount into one int (and dont forget to do the same in the decoding)
                    // TODO: use a uint32 for the repcount instead of a int32 (so we do not waste space on negative numbers)
                    w.Write(surfaceRelativeSequence);
                    w.Write(repCount);
                    if (surfaceRelativeSequence)
                    {
                        // TODO: If the repcount is 1 (and maybe 2 too) we should just write the original rotations (and dont forget to do the same in the decoding)

                        w.Write(surfaceRelativeRotations[startIndex].x);
                        w.Write(surfaceRelativeRotations[startIndex].y);
                        w.Write(surfaceRelativeRotations[startIndex].z);
                        w.Write(surfaceRelativeRotations[startIndex].w);

                        // TODO: How we pick the points might cause problems when the cricle that this section form's more than 2/3 of the circumvence of the planet,
                        // we should compensate for this
                        var matrixM = Get3DPoint3DPlaneAs2DPointProjectionMatrix2(__instance.pointPos[startIndex + (int)Math.Ceiling(a: repCount / 2)], __instance.pointPos[startIndex], __instance.pointPos[endIndex]);
                        w.Write(matrixM[0, 0]);
                        w.Write(matrixM[1, 0]);
                        w.Write(matrixM[2, 0]);
                        //w.Write(matrixM[3, 0]); // This is always 0
                        w.Write(matrixM[0, 1]);
                        w.Write(matrixM[1, 1]);
                        w.Write(matrixM[2, 1]);
                        //w.Write(matrixM[3, 1]); // This is always 0
                        w.Write(matrixM[0, 2]);
                        w.Write(matrixM[1, 2]);
                        w.Write(matrixM[2, 2]);
                        //w.Write(matrixM[3, 2]); // This is always 0
                        w.Write(matrixM[0, 3]);
                        w.Write(matrixM[1, 3]);
                        w.Write(matrixM[2, 3]);
                        //w.Write(matrixM[3, 3]); // This is always 1

                        var matrixMInv = matrixM.inverse;
                        for (int i = 0; i < repCount; i++)
                        {
                            // Write 2D coordinates on plane
                            // TODO: For the first one we can probably just take the origin points from matrixM (not acctually the first one but the (int)Math.Ceiling(a: repCount / 2) th one)

                            var projectedPoint = matrixMInv.MultiplyPoint3x4(__instance.pointPos[startIndex + i]); // Fuck yea!!!
                            w.Write(projectedPoint.x);
                            w.Write(projectedPoint.y);
                        }
                        
                    }
                    else
                    {
                        for (int i = 0; i < repCount; i++)
                        {
                            w.Write(__instance.pointRot[startIndex + i].x);
                            w.Write(__instance.pointRot[startIndex + i].y);
                            w.Write(__instance.pointRot[startIndex + i].z);
                            w.Write(__instance.pointRot[startIndex + i].w);

                            w.Write(__instance.pointPos[startIndex + i].x);
                            w.Write(__instance.pointPos[startIndex + i].y);
                            w.Write(__instance.pointPos[startIndex + i].z);
                        }
                    }

                    startIndex = endIndex + 1;
                }

                // The end needs to be handled seperately (since it does not have a differentialIndex)
                endIndex = ___bufferLength - 1;
                var surfaceRelativeSequenceForEnd = simmilarityMap[___bufferLength - 2];
                var repCountForEnd = endIndex - startIndex + 1;

                // TODO: sometimes we have a repcount of 0 we can then just skip this entire sequence
                // TODO: compress the surfaceRelativeSequence and the repCount into one int (and dont forget to do the same in the decoding)
                // TODO: use a uint32 for the repcount instead of a int32 (so we do not waste space on negative numbers)
                w.Write(surfaceRelativeSequenceForEnd);
                w.Write(repCountForEnd);
                if (surfaceRelativeSequenceForEnd)
                {
                    // TODO: If the repcount is 1 (and maybe 2 too) we should just write the original rotations (and dont forget to do the same in the decoding)

                    w.Write(surfaceRelativeRotations[startIndex].x);
                    w.Write(surfaceRelativeRotations[startIndex].y);
                    w.Write(surfaceRelativeRotations[startIndex].z);
                    w.Write(surfaceRelativeRotations[startIndex].w);

                    // TODO: How we pick the points might cause problems when the cricle that this section form's more than 2/3 of the circumvence of the planet,
                    // we should compensate for this
                    var matrixM = Get3DPoint3DPlaneAs2DPointProjectionMatrix2(__instance.pointPos[startIndex + (int)Math.Ceiling(a: repCountForEnd / 2)], __instance.pointPos[startIndex], __instance.pointPos[endIndex]);
                    w.Write(matrixM[0, 0]);
                    w.Write(matrixM[1, 0]);
                    w.Write(matrixM[2, 0]);
                    //w.Write(matrixM[3, 0]); // This is always 0
                    w.Write(matrixM[0, 1]);
                    w.Write(matrixM[1, 1]);
                    w.Write(matrixM[2, 1]);
                    //w.Write(matrixM[3, 1]); // This is always 0
                    w.Write(matrixM[0, 2]);
                    w.Write(matrixM[1, 2]);
                    w.Write(matrixM[2, 2]);
                    //w.Write(matrixM[3, 2]); // This is always 0
                    w.Write(matrixM[0, 3]);
                    w.Write(matrixM[1, 3]);
                    w.Write(matrixM[2, 3]);
                    //w.Write(matrixM[3, 3]); // This is always 1

                    var matrixMInv = matrixM.inverse;
                    for (int i = 0; i < repCountForEnd; i++)
                    {
                        // Write 2D coordinates on plane
                        // TODO: For the first one we can probably just take the origin points from matrixM (not acctually the first one but the (int)Math.Ceiling(a: repCount / 2) th one)

                        var projectedPoint = matrixMInv.MultiplyPoint3x4(__instance.pointPos[startIndex + i]); // Fuck yea!!!
                        w.Write(projectedPoint.x);
                        w.Write(projectedPoint.y);
                    }

                }
                else
                {
                    for (int i = 0; i < repCountForEnd; i++)
                    {
                        w.Write(__instance.pointRot[startIndex + i].x);
                        w.Write(__instance.pointRot[startIndex + i].y);
                        w.Write(__instance.pointRot[startIndex + i].z);
                        w.Write(__instance.pointRot[startIndex + i].w);

                        w.Write(__instance.pointPos[startIndex + i].x);
                        w.Write(__instance.pointPos[startIndex + i].y);
                        w.Write(__instance.pointPos[startIndex + i].z);
                    }
                }
            }
            // END of rotational and positional compression code

            for (int k = 0; k < __instance.belts.Count; k++)
            {
                w.Write(__instance.belts[k]);
            }
            for (int l = 0; l < __instance.inputPaths.Count; l++)
            {
                w.Write(__instance.inputPaths[l]);
            }

            // Skip the original function
            return false;

        }

        [HarmonyPrefix]
        [HarmonyPatch("Import")]
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
            __instance.Free();


            //r.ReadInt32();
            var version = Encoding.ASCII.GetString(r.ReadBytes(4));

            switch (version)
            {
                case "nb00":
                    {

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

                        Debug.Log($"___bufferLength {___bufferLength}");

                        // Code that reads/calculates the rotations
                        for (int j = 0; j < ___bufferLength;)
                        {

                            var sameRelativeRotationAsPrevious = r.ReadBoolean();
                            var repCount = r.ReadInt32();

                            var previosRelativeRotation = new Quaternion();
                            var isThereAPreviosRelativeRotation = false;
                            var planarMatrix = new Matrix4x4();
                            var isThereAPlanarMatrix = false;
                            for (int i = 0; i < repCount; i++)
                            {
                                if (sameRelativeRotationAsPrevious)
                                {

                                    if (!isThereAPreviosRelativeRotation)
                                    {
                                        previosRelativeRotation = new Quaternion(r.ReadSingle(), r.ReadSingle(), r.ReadSingle(), r.ReadSingle());
                                        isThereAPreviosRelativeRotation = true;
                                    }

                                    if (!isThereAPlanarMatrix)
                                    {
                                        planarMatrix = new Matrix4x4(
                                            new Vector4(r.ReadSingle(), r.ReadSingle(), r.ReadSingle(), 0),
                                            new Vector4(r.ReadSingle(), r.ReadSingle(), r.ReadSingle(), 0),
                                            new Vector4(r.ReadSingle(), r.ReadSingle(), r.ReadSingle(), 0),
                                            new Vector4(r.ReadSingle(), r.ReadSingle(), r.ReadSingle(), 1)
                                        );
                                        isThereAPlanarMatrix = true;
                                    }

                                    var planarPosition = new Vector3(r.ReadSingle(), r.ReadSingle());
                                    __instance.pointPos[j] = planarMatrix.MultiplyPoint3x4(planarPosition);

                                    var originalPosition = __instance.pointPos[j];
                                    var calculatedRotation = Quaternion.LookRotation(originalPosition, Vector3.up) * previosRelativeRotation;
                                    __instance.pointRot[j] = calculatedRotation;

                                } else
                                {

                                    __instance.pointRot[j].x = r.ReadSingle();
                                    __instance.pointRot[j].y = r.ReadSingle();
                                    __instance.pointRot[j].z = r.ReadSingle();
                                    __instance.pointRot[j].w = r.ReadSingle();

                                    if (isThereAPreviosRelativeRotation)
                                    {
                                        isThereAPreviosRelativeRotation = false;
                                    }

                                    __instance.pointPos[j].x = r.ReadSingle();
                                    __instance.pointPos[j].y = r.ReadSingle();
                                    __instance.pointPos[j].z = r.ReadSingle();

                                    if (!isThereAPlanarMatrix)
                                    {
                                        isThereAPlanarMatrix = false;
                                    }

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

                    }

                    break;
                default:
                    {
                        // The original implementation
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

                    break;
            }
            

            // Skip the original function
            return false;
        }
    }
}
