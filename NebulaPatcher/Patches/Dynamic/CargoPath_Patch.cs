//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
using HarmonyLib;
//using NebulaModel.Logger;
//using NebulaModel.Packets.Belt;
//using NebulaWorld;
//using NebulaWorld.Factory;
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
            //w.Write(1337);

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

            // Write the positions
            for (int j = 0; j < ___bufferLength; j++)
            {
                w.Write(__instance.pointPos[j].x);
                w.Write(__instance.pointPos[j].y);
                w.Write(__instance.pointPos[j].z);
            }

            // START of rotational compression code

            //Debug.Log($"___bufferLength {___bufferLength}");

            // Build the prerequisite mappings
            var surfaceRelativeRotations = new Quaternion[___bufferLength];
            var simmilarityMap = new bool[___bufferLength - 1];
            //// TODO: We might be able to do without the differentialSimmilarityMap and just use the indexes
            //var differentialSimmilarityMap = new bool[___bufferLength - 2];
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
                    //Debug.Log($"Angular difference between {j - 1} and {j}: {angularDiff}");
                    simmilarityMap[j - 1] = angularDiff == 0;
                }

                // Construct the differential simmilarity map (since changed to store indexes only)
                if (j > 1)
                {
                    var differentialSimmilarity = simmilarityMap[j - 2] == simmilarityMap[j - 1];
                    //differentialSimmilarityMap[j - 2] = differentialSimmilarity;
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

                //Debug.Log($"surfaceRelativeSequence {surfaceRelativeSequence}");
                //Debug.Log($"repCount {repCount}");
                w.Write(surfaceRelativeSequence);
                w.Write(repCount);
                if (surfaceRelativeSequence)
                {
                    //Debug.Log($"Writing relative rotation for index {startIndex}");
                    w.Write(surfaceRelativeRotations[startIndex].x);
                    w.Write(surfaceRelativeRotations[startIndex].y);
                    w.Write(surfaceRelativeRotations[startIndex].z);
                    w.Write(surfaceRelativeRotations[startIndex].w);
                } else
                {
                    for (int i = 0; i < repCount; i++)
                    {
                        //Debug.Log($"Writing original rotation for index {startIndex + i}");
                        w.Write(__instance.pointRot[startIndex + i].x);
                        w.Write(__instance.pointRot[startIndex + i].y);
                        w.Write(__instance.pointRot[startIndex + i].z);
                        w.Write(__instance.pointRot[startIndex + i].w);
                    }
                }

                startIndex = endIndex + 1;
            }

            // The end needs to be handled seperately (since it does not have a differentialIndex)
            endIndex = ___bufferLength - 1;
            //var surfaceRelativeSequenceForEnd = simmilarityMap[simmilarityMap.Length - 1];
            var surfaceRelativeSequenceForEnd = simmilarityMap[___bufferLength - 2];
            var repCountForEnd = endIndex - startIndex + 1;

            //Debug.Log($"surfaceRelativeSequenceForEnd {surfaceRelativeSequenceForEnd}");
            //Debug.Log($"repCountForEnd {repCountForEnd}");
            w.Write(surfaceRelativeSequenceForEnd);
            w.Write(repCountForEnd);
            if (surfaceRelativeSequenceForEnd)
            {
                //Debug.Log($"Writing to relative rotation for index {startIndex}");
                w.Write(surfaceRelativeRotations[startIndex].x);
                w.Write(surfaceRelativeRotations[startIndex].y);
                w.Write(surfaceRelativeRotations[startIndex].z);
                w.Write(surfaceRelativeRotations[startIndex].w);
            }
            else
            {
                for (int i = 0; i < repCountForEnd; i++)
                {
                    //Debug.Log($"Writing to original rotation for index {startIndex + i}");
                    w.Write(__instance.pointRot[startIndex + i].x);
                    w.Write(__instance.pointRot[startIndex + i].y);
                    w.Write(__instance.pointRot[startIndex + i].z);
                    w.Write(__instance.pointRot[startIndex + i].w);
                }
            }

            // END of rotational compression code

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
            //var version = r.ReadInt32();

            switch (version)
            {
                case "nb00":
                    {
                        Debug.Log($"!!!!!!!!!Importing CargoPath data with nb00 format!!!!!!!!!!");

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

                        // Code that reads/calculates the positions
                        for (int j = 0; j < ___bufferLength; j++)
                        {
                            __instance.pointPos[j].x = r.ReadSingle();
                            __instance.pointPos[j].y = r.ReadSingle();
                            __instance.pointPos[j].z = r.ReadSingle();
                        }

                        Debug.Log($"___bufferLength {___bufferLength}");

                        // Code that reads/calculates the rotations
                        for (int j = 0; j < ___bufferLength;)
                        {

                            var sameRelativeRotationAsPrevious = r.ReadBoolean();
                            var repCount = r.ReadInt32();

                            var previosRelativeRotation = new Quaternion();
                            var isThereAPreviosRelativeRotation = false;
                            Debug.Log($"repCount {repCount}");
                            for (int i = 0; i < repCount; i++)
                            {
                                Debug.Log($"Processing rotation {j}");
                                if (sameRelativeRotationAsPrevious)
                                {

                                    if (!isThereAPreviosRelativeRotation)
                                    {
                                        previosRelativeRotation = new Quaternion(r.ReadSingle(), r.ReadSingle(), r.ReadSingle(), r.ReadSingle());
                                        isThereAPreviosRelativeRotation = true;
                                    }

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

                                }

                                
                                j++;

                            }

                        }

                        //// Code that reads/calculates the rotations
                        //Quaternion firstSurfaceRelativeRotationInChain = new Quaternion();
                        //bool isThereAfirstSurfaceRelativeRotationInChain = false;
                        //for (int j = 0; j < ___bufferLength; j++)
                        //{
                            
                        //    var sameRelativeRotationAsPrevious = r.ReadBoolean();
                            
                        //    if (sameRelativeRotationAsPrevious)
                        //    {
                        //        Debug.Log($"Part {j} has same relative rotation as part {j - 1}");

                        //        //  This ensures that the relative rotation is calculated using the first rotation of the chain (this prevents compounding presision loss)
                        //        if (!isThereAfirstSurfaceRelativeRotationInChain)
                        //        {
                        //            var previousPosition = __instance.pointPos[j - 1];
                        //            var previousRotation = __instance.pointRot[j - 1];

                                    
                        //            firstSurfaceRelativeRotationInChain = Quaternion.Inverse(Quaternion.LookRotation(previousPosition, Vector3.up)) * previousRotation;
                        //            isThereAfirstSurfaceRelativeRotationInChain = true;
                        //        }


                        //        var originalPosition = __instance.pointPos[j];

                        //        var calculatedRotation =  Quaternion.LookRotation(originalPosition, Vector3.up) * firstSurfaceRelativeRotationInChain;

                        //        Debug.Log($"Calculated rotation: x {calculatedRotation.x} y {calculatedRotation.y} z {calculatedRotation.z} w {calculatedRotation.w}");

                        //        __instance.pointRot[j] = calculatedRotation;

                        //        Debug.Log($"Original rotation: x {__instance.pointRot[j].x} y {__instance.pointRot[j].y} z {__instance.pointRot[j].z} w {__instance.pointRot[j].w}");

                        //        var angularDiff = Quaternion.Angle(calculatedRotation, __instance.pointRot[j]);
                        //        Debug.Log($"Angular difference between calculated and original rotation: {angularDiff}");

                        //    } else
                        //    {
                        //        //var surfaceRelativeRotation = new Quaternion(r.ReadSingle(), r.ReadSingle(), r.ReadSingle(), r.ReadSingle());

                        //        //var originalRotation = Quaternion.LookRotation(__instance.pointPos[j], Vector3.up) * surfaceRelativeRotation;

                        //        __instance.pointRot[j].x = r.ReadSingle();
                        //        __instance.pointRot[j].y = r.ReadSingle();
                        //        __instance.pointRot[j].z = r.ReadSingle();
                        //        __instance.pointRot[j].w = r.ReadSingle();

                        //        //__instance.pointRot[j] = originalRotation;

                        //        isThereAfirstSurfaceRelativeRotationInChain = false;
                        //    }

                        //}

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
