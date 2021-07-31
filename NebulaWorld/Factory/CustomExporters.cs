using HarmonyLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace NebulaWorld.Factory
{
    public class CustomExporters
    {
        // NOTE: This needs to be 4 chars long (no more no less), else it won't encode to a 4 bytes int and thus will cause weird problems
        public static readonly int EncodedVersionNb00 = BitConverter.ToInt32(Encoding.ASCII.GetBytes("nb00"), 0);

        private static Matrix4x4 Get3DPoint3DPlaneAs2DPointProjectionMatrix(Vector3 point0, Vector3 point1, Vector3 point2)
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

        public static void CustomFactoryExport(BinaryWriter w, ref PlanetFactory planetFactory)
        {
            var entityCapacity = (int)AccessTools.Field(typeof(PlanetFactory), "entityCapacity").GetValue(planetFactory);
            var entityRecycleCursor = (int)AccessTools.Field(typeof(PlanetFactory), "entityRecycleCursor").GetValue(planetFactory);
            var entityRecycle = (int[])AccessTools.Field(typeof(PlanetFactory), "entityRecycle").GetValue(planetFactory);
            var prebuildCapacity = (int)AccessTools.Field(typeof(PlanetFactory), "prebuildCapacity").GetValue(planetFactory);
            var prebuildRecycleCursor = (int)AccessTools.Field(typeof(PlanetFactory), "prebuildRecycleCursor").GetValue(planetFactory);
            var prebuildRecycle = (int[])AccessTools.Field(typeof(PlanetFactory), "prebuildRecycle").GetValue(planetFactory);
            var vegeCapacity = (int)AccessTools.Field(typeof(PlanetFactory), "vegeCapacity").GetValue(planetFactory);
            var vegeRecycleCursor = (int)AccessTools.Field(typeof(PlanetFactory), "vegeRecycleCursor").GetValue(planetFactory);
            var vegeRecycle = (int[])AccessTools.Field(typeof(PlanetFactory), "vegeRecycle").GetValue(planetFactory);
            var veinCapacity = (int)AccessTools.Field(typeof(PlanetFactory), "veinCapacity").GetValue(planetFactory);
            var veinRecycleCursor = (int)AccessTools.Field(typeof(PlanetFactory), "veinRecycleCursor").GetValue(planetFactory);
            var veinRecycle = (int[])AccessTools.Field(typeof(PlanetFactory), "veinRecycle").GetValue(planetFactory);

            w.Write(2);
            w.Write(planetFactory.planetId);
            planetFactory.planet.ExportRuntime(w);
            w.Write(entityCapacity);
            w.Write(planetFactory.entityCursor);
            w.Write(entityRecycleCursor);
            for (int i = 1; i < planetFactory.entityCursor; i++)
            {
                planetFactory.entityPool[i].Export(w);
            }
            for (int j = 1; j < planetFactory.entityCursor; j++)
            {
                w.Write(planetFactory.entityAnimPool[j].time);
                w.Write(planetFactory.entityAnimPool[j].prepare_length);
                w.Write(planetFactory.entityAnimPool[j].working_length);
                w.Write(planetFactory.entityAnimPool[j].state);
                w.Write(planetFactory.entityAnimPool[j].power);
            }
            for (int k = 1; k < planetFactory.entityCursor; k++)
            {
                w.Write((byte)planetFactory.entitySignPool[k].signType);
                w.Write((byte)planetFactory.entitySignPool[k].iconType);
                w.Write((ushort)planetFactory.entitySignPool[k].iconId0);
                w.Write(planetFactory.entitySignPool[k].x);
                w.Write(planetFactory.entitySignPool[k].y);
                w.Write(planetFactory.entitySignPool[k].z);
                w.Write(planetFactory.entitySignPool[k].w);
            }
            int num = planetFactory.entityCursor * 16;
            for (int l = 16; l < num; l++)
            {
                w.Write(planetFactory.entityConnPool[l]);
            }
            for (int m = 0; m < entityRecycleCursor; m++)
            {
                w.Write(entityRecycle[m]);
            }
            w.Write(prebuildCapacity);
            w.Write(planetFactory.prebuildCursor);
            w.Write(prebuildRecycleCursor);
            for (int n = 1; n < planetFactory.prebuildCursor; n++)
            {
                planetFactory.prebuildPool[n].Export(w);
            }
            int num2 = planetFactory.prebuildCursor * 16;
            for (int num3 = 16; num3 < num2; num3++)
            {
                w.Write(planetFactory.prebuildConnPool[num3]);
            }
            for (int num4 = 0; num4 < prebuildRecycleCursor; num4++)
            {
                w.Write(prebuildRecycle[num4]);
            }
            w.Write(vegeCapacity);
            w.Write(planetFactory.vegeCursor);
            w.Write(vegeRecycleCursor);
            for (int num5 = 1; num5 < planetFactory.vegeCursor; num5++)
            {
                planetFactory.vegePool[num5].Export(w);
            }
            for (int num6 = 0; num6 < vegeRecycleCursor; num6++)
            {
                w.Write(vegeRecycle[num6]);
            }
            w.Write(veinCapacity);
            w.Write(planetFactory.veinCursor);
            w.Write(veinRecycleCursor);
            for (int num7 = 1; num7 < planetFactory.veinCursor; num7++)
            {
                planetFactory.veinPool[num7].Export(w);
            }
            for (int num8 = 0; num8 < veinRecycleCursor; num8++)
            {
                w.Write(veinRecycle[num8]);
            }
            for (int num9 = 1; num9 < planetFactory.veinCursor; num9++)
            {
                w.Write(planetFactory.veinAnimPool[num9].time);
                w.Write(planetFactory.veinAnimPool[num9].prepare_length);
                w.Write(planetFactory.veinAnimPool[num9].working_length);
                w.Write(planetFactory.veinAnimPool[num9].state);
                w.Write(planetFactory.veinAnimPool[num9].power);
            }
            planetFactory.cargoContainer.Export(w);
            //planetFactory.cargoTraffic.Export(w);
            CustomCargoTrafficExport(w, ref planetFactory.cargoTraffic);
            planetFactory.factoryStorage.Export(w);
            planetFactory.powerSystem.Export(w);
            planetFactory.factorySystem.Export(w);
            planetFactory.transport.Export(w);
            planetFactory.monsterSystem.Export(w);
            planetFactory.platformSystem.Export(w);
        }

        public static void CustomCargoTrafficExport(BinaryWriter w, ref CargoTraffic cargoTraffic)
        {
            var beltCapacity = (int)AccessTools.Field(typeof(CargoTraffic), "beltCapacity").GetValue(cargoTraffic);
            var beltRecycleCursor = (int)AccessTools.Field(typeof(CargoTraffic), "beltRecycleCursor").GetValue(cargoTraffic);
            var splitterCapacity = (int)AccessTools.Field(typeof(CargoTraffic), "splitterCapacity").GetValue(cargoTraffic);
            var splitterRecycleCursor = (int)AccessTools.Field(typeof(CargoTraffic), "splitterRecycleCursor").GetValue(cargoTraffic);
            var pathCapacity = (int)AccessTools.Field(typeof(CargoTraffic), "pathCapacity").GetValue(cargoTraffic);
            var pathRecycleCursor = (int)AccessTools.Field(typeof(CargoTraffic), "pathRecycleCursor").GetValue(cargoTraffic);
            var beltRecycle = (int[])AccessTools.Field(typeof(CargoTraffic), "beltRecycle").GetValue(cargoTraffic);
            var splitterRecycle = (int[])AccessTools.Field(typeof(CargoTraffic), "splitterRecycle").GetValue(cargoTraffic);
            var pathRecycle = (int[])AccessTools.Field(typeof(CargoTraffic), "pathRecycle").GetValue(cargoTraffic);

            w.Write(0);
            w.Write(cargoTraffic.beltCursor);
            w.Write(beltCapacity);
            w.Write(beltRecycleCursor);
            w.Write(cargoTraffic.splitterCursor);
            w.Write(splitterCapacity);
            w.Write(splitterRecycleCursor);
            w.Write(cargoTraffic.pathCursor);
            w.Write(pathCapacity);
            w.Write(pathRecycleCursor);
            for (int i = 1; i < cargoTraffic.beltCursor; i++)
            {
                cargoTraffic.beltPool[i].Export(w);
            }
            for (int j = 0; j < beltRecycleCursor; j++)
            {
                w.Write(beltRecycle[j]);
            }
            for (int k = 1; k < cargoTraffic.splitterCursor; k++)
            {
                cargoTraffic.splitterPool[k].Export(w);
            }
            for (int l = 0; l < splitterRecycleCursor; l++)
            {
                w.Write(splitterRecycle[l]);
            }
            for (int m = 1; m < cargoTraffic.pathCursor; m++)
            {
                if (cargoTraffic.pathPool[m] != null && cargoTraffic.pathPool[m].id == m)
                {
                    w.Write(m);
                    //cargoTraffic.pathPool[m].Export(w);
                    CustomCargoPathExport(w, ref cargoTraffic.pathPool[m]);
                }
                else
                {
                    w.Write(0);
                }
            }
            for (int n = 0; n < pathRecycleCursor; n++)
            {
                w.Write(pathRecycle[n]);
            }
        }

        public static void CustomCargoPathExport(BinaryWriter w, ref CargoPath cargoPath)
        {
            var capacity = (int)AccessTools.Field(typeof(CargoPath), "capacity").GetValue(cargoPath);
            var bufferLength = (int)AccessTools.Field(typeof(CargoPath), "bufferLength").GetValue(cargoPath);
            var chunkCapacity = (int)AccessTools.Field(typeof(CargoPath), "chunkCapacity").GetValue(cargoPath);
            var chunkCount = (int)AccessTools.Field(typeof(CargoPath), "chunkCount").GetValue(cargoPath);
            var updateLen = (int)AccessTools.Field(typeof(CargoPath), "updateLen").GetValue(cargoPath);

            //w.Write(0);
            // Set a special 4 byte int that represents the version that we will use to indentify wheter whe should process this normally or use custom optimized method
            w.Write(EncodedVersionNb00);

            w.Write(cargoPath.id);
            w.Write(capacity);
            w.Write(bufferLength);
            w.Write(chunkCapacity);
            w.Write(chunkCount);
            w.Write(updateLen);
            w.Write(cargoPath.closed);
            w.Write((cargoPath.outputPath == null) ? 0 : cargoPath.outputPath.id);
            w.Write((cargoPath.outputPath == null) ? -1 : cargoPath.outputIndex);
            w.Write(cargoPath.belts.Count);
            w.Write(cargoPath.inputPaths.Count);
            w.Write(cargoPath.buffer, 0, bufferLength);
            for (int i = 0; i < chunkCount; i++)
            {
                w.Write(cargoPath.chunks[i * 3]);
                w.Write(cargoPath.chunks[i * 3 + 1]);
                w.Write(cargoPath.chunks[i * 3 + 2]);
            }

            // START of rotational and positional compression code
            {

                // Build the prerequisite mappings
                var surfaceRelativeRotations = new Quaternion[bufferLength];
                var simmilarityMap = new bool[bufferLength - 1];
                var differentialIndexes = new List<int>();
                for (int j = 0; j < bufferLength; j++)
                {

                    var originalPosition = cargoPath.pointPos[j];
                    var originalRotation = cargoPath.pointRot[j];
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

                void handleSequenceWrite(ref int repCount, ref bool surfaceRelativeSequence, ref int startIndex, ref int endIndex, ref CargoPath cargoPathLocal)
                {
                    // Sometimes we have a repcount of 0 we can then just skip this entire sequence
                    if (repCount > 0)
                    {
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
                            var matrixM = Get3DPoint3DPlaneAs2DPointProjectionMatrix(cargoPathLocal.pointPos[startIndex + (int)Math.Ceiling(a: repCount / 2)], cargoPathLocal.pointPos[startIndex], cargoPathLocal.pointPos[endIndex]);
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

                                var projectedPoint = matrixMInv.MultiplyPoint3x4(cargoPathLocal.pointPos[startIndex + i]); // Fuck yea!!!
                                w.Write(projectedPoint.x);
                                w.Write(projectedPoint.y);
                            }

                        }
                        else
                        {
                            for (int i = 0; i < repCount; i++)
                            {
                                w.Write(cargoPathLocal.pointRot[startIndex + i].x);
                                w.Write(cargoPathLocal.pointRot[startIndex + i].y);
                                w.Write(cargoPathLocal.pointRot[startIndex + i].z);
                                w.Write(cargoPathLocal.pointRot[startIndex + i].w);

                                w.Write(cargoPathLocal.pointPos[startIndex + i].x);
                                w.Write(cargoPathLocal.pointPos[startIndex + i].y);
                                w.Write(cargoPathLocal.pointPos[startIndex + i].z);
                            }
                        }
                    }
                }

                {
                    // Write data in a compressed format
                    var startIndex = 0;
                    int endIndex;
                    for (int j = 0; j < differentialIndexes.Count; j++)
                    {
                        var differentialIndex = differentialIndexes[j];
                        var surfaceRelativeSequence = simmilarityMap[differentialIndex];

                        endIndex = differentialIndex + (surfaceRelativeSequence ? 1 : 0);
                        var repCount = endIndex - startIndex + 1;

                        handleSequenceWrite(ref repCount, ref surfaceRelativeSequence, ref startIndex, ref endIndex, ref cargoPath);

                        startIndex = endIndex + 1;
                    }

                    // The end needs to be handled seperately (since it does not have a differentialIndex)
                    endIndex = bufferLength - 1;
                    var surfaceRelativeSequenceForEnd = simmilarityMap[bufferLength - 2];
                    var repCountForEnd = endIndex - startIndex + 1;

                    handleSequenceWrite(ref repCountForEnd, ref surfaceRelativeSequenceForEnd, ref startIndex, ref endIndex, ref cargoPath);

                }
                
            }
            // END of rotational and positional compression code

            for (int k = 0; k < cargoPath.belts.Count; k++)
            {
                w.Write(cargoPath.belts[k]);
            }
            for (int l = 0; l < cargoPath.inputPaths.Count; l++)
            {
                w.Write(cargoPath.inputPaths[l]);
            }
        }

    }
}
