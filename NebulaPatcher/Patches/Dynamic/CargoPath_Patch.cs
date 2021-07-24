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
using NebulaModel.DataStructures;

namespace NebulaPatcher.Patches.Dynamic
{
    [HarmonyPatch(typeof(CargoPath))]
    class CargoPath_Patch
    {

        //public static Matrix4x4 Get2DCoordMatrix(Vector3[] triangle)
        //{
        //    // Adapted from https://forum.unity.com/threads/convert-3d-coords-of-point-on-plane-to-2d-relative-to-this-plane.869314/

        //    var normal = Vector3.Cross(triangle[1] - triangle[0], triangle[2] - triangle[0]).normalized;

        //    var upwards = (triangle[1] - triangle[0]).normalized;
        //    var rotation = Quaternion.LookRotation(-normal, upwards);

        //    return Matrix4x4.TRS(triangle[0], rotation, Vector3.one);
        //}

        private static Matrix4x4 Get3DPoint3DPlaneAs2DPointProjectionMatrix2(Vector3 point0, Vector3 point1, Vector3 point2)
        {
            // Adapted from https://stackoverflow.com/a/52163563/13620003

            //var x = point1 - point0;
            //x.Normalize();
            //var y = point2 - point0;
            //z = Vector3.Cross(x, y);
            //z.Normalize();
            //y = Vector3.Cross(z, x);
            //y.Normalize();
            //var o = point0;

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


        private static readonly Vector4 columnVector1 = new Vector4(0, 0, 0, 1);
        private static readonly Matrix4x4 matrixD = new Matrix4x4(
                columnVector1,
                new Vector4(1, 0, 0, 1),
                new Vector4(0, 1, 0, 1),
                new Vector4(0, 0, 1, 1)
        );

        private static Matrix4x4 Get3DPoint3DPlaneAs2DPointProjectionMatrix(Vector3 pointA, Vector3 pointB, Vector3 pointC, out Vector3 vectoruN)
        {
            // Adapted from https://stackoverflow.com/questions/49769459/convert-points-on-a-3d-plane-to-2d-coordinates

            var vectorAB = pointB - pointA;
            var vectorAC = pointC - pointA;

            vectoruN = Vector3.Cross(vectorAB, vectorAC).normalized;
            var vectorU = vectorAB.normalized;
            var vectorV = Vector3.Cross(vectorU, vectoruN);

            var vectoru = pointA + vectorU;
            var vectorv = pointA + vectorV;
            var vectorn = pointA + vectoruN;

            var matrixS = new Matrix4x4(
                new Vector4(pointA.x, pointA.y, pointA.z, 1),
                new Vector4(vectoru.x, vectoru.y, vectoru.z, 1),
                new Vector4(vectorv.x, vectorv.y, vectorv.z, 1),
                new Vector4(vectorn.x, vectorn.y, vectorn.z, 1)
            );


            //var matrixD = new Matrix4x4(
            //    new Vector4(0, 0, 0, 1),
            //    new Vector4(1, 0, 0, 1),
            //    new Vector4(0, 1, 0, 1),
            //    new Vector4(0, 0, 1, 1)
            //);

            var matrixM = matrixD * matrixS.inverse;

            return matrixM;

        }
        //private static Matrix4x4 Get3DPoint3DPlaneAs2DPointProjectionMatrix(Vector3 pointA, Vector3 pointB, Vector3 vectoruN)
        //{
        //    // Adapted from https://stackoverflow.com/questions/49769459/convert-points-on-a-3d-plane-to-2d-coordinates

        //    var vectorAB = pointB - pointA;
        //    //var vectorAC = pointC - pointA;

        //    //vectoruN = Vector3.Cross(vectorAB, vectorAC).normalized;
        //    var vectorU = vectorAB.normalized;
        //    var vectorV = Vector3.Cross(vectorU, vectoruN);

        //    var vectoru = pointA + vectorU;
        //    var vectorv = pointA + vectorV;
        //    var vectorn = pointA + vectoruN;

        //    var matrixS = new Matrix4x4(
        //        new Vector4(pointA.x, pointA.y, pointA.z, 1),
        //        new Vector4(vectoru.x, vectoru.y, vectoru.z, 1),
        //        new Vector4(vectorv.x, vectorv.y, vectorv.z, 1),
        //        new Vector4(vectorn.x, vectorn.y, vectorn.z, 1)
        //    );


        //    //var matrixD = new Matrix4x4(
        //    //    new Vector4(0, 0, 0, 1),
        //    //    new Vector4(1, 0, 0, 1),
        //    //    new Vector4(0, 1, 0, 1),
        //    //    new Vector4(0, 0, 1, 1)
        //    //);

        //    var matrixM = matrixD * matrixS.inverse;

        //    return matrixM;

        //}

        private static Matrix4x4 Get3DPoint3DPlaneAs2DPointProjectionMatrix(Vector3 pointA, Vector3 pointB, Vector3 pointC)
        {
            Vector3 vectoruN;
            return Get3DPoint3DPlaneAs2DPointProjectionMatrix(pointA, pointB, pointC, out vectoruN);
        }

        private static Matrix4x4 Get3DPoint3DPlaneAs2DPointProjectionMatrixTroughOrigin(Vector3 pointB, Vector3 pointC, out Vector3 vectoruN)
        {
            // Adapted from https://stackoverflow.com/questions/49769459/convert-points-on-a-3d-plane-to-2d-coordinates

            vectoruN = Vector3.Cross(pointB, pointC).normalized;
            var vectorU = pointB.normalized;
            var vectorV = Vector3.Cross(vectorU, vectoruN);

            var matrixS = new Matrix4x4(
                columnVector1,
                new Vector4(vectorU.x, vectorU.y, vectorU.z, 1),
                new Vector4(vectorV.x, vectorV.y, vectorV.z, 1),
                new Vector4(vectoruN.x, vectoruN.y, vectoruN.z, 1)
            );

            //var matrixD = new Matrix4x4(
            //    new Vector4(0, 0, 0, 1),
            //    new Vector4(1, 0, 0, 1),
            //    new Vector4(0, 1, 0, 1),
            //    new Vector4(0, 0, 1, 1)
            //);

            var matrixM = matrixD * matrixS.inverse;

            return matrixM;
        }

        private static Matrix4x4 Get3DPoint3DPlaneAs2DPointProjectionMatrixTroughOrigin(Vector3 pointB, Vector3 vectoruN)
        {
            // Adapted from https://stackoverflow.com/questions/49769459/convert-points-on-a-3d-plane-to-2d-coordinates

            var vectorU = pointB.normalized;
            var vectorV = Vector3.Cross(vectorU, vectoruN);

            var matrixS = new Matrix4x4(
                new Vector4(0, 0, 0, 1),
                new Vector4(vectorU.x, vectorU.y, vectorU.z, 1),
                new Vector4(vectorV.x, vectorV.y, vectorV.z, 1),
                new Vector4(vectoruN.x, vectoruN.y, vectoruN.z, 1)
            );

            //var matrixD = new Matrix4x4(
            //    new Vector4(0, 0, 0, 1),
            //    new Vector4(1, 0, 0, 1),
            //    new Vector4(0, 1, 0, 1),
            //    new Vector4(0, 0, 1, 1)
            //);

            var matrixM = matrixD * matrixS.inverse;

            return matrixM;

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

            bool isEqual(float a, float b, int multiplier = 1)
            {
                if (a >= b - (Mathf.Epsilon * multiplier) && a <= b + (Mathf.Epsilon * multiplier))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }

            double calcSphericalDistance(VectorLF3 position1, VectorLF3 position2)
            {
                return Math.Acos(VectorLF3.Dot(position1.normalized, position2.normalized)) * ((position1.magnitude + position2.magnitude) / 2);
            }


            VectorLF3 VectorLF3Project(VectorLF3 vector, VectorLF3 onNormal)
            {
                double num = VectorLF3.Dot(onNormal, onNormal);
                VectorLF3 result;
                if (num < double.Epsilon)
                {
                    result = VectorLF3.zero;
                }
                else
                {
                    result = onNormal * VectorLF3.Dot(vector, onNormal) / num;
                }
                return result;
            }

            VectorLF3 VectorLF3ProjectOnPlane(VectorLF3 vector, VectorLF3 planeNormal)
            {
                return vector - VectorLF3Project(vector, planeNormal);
            }

            //Matrix4x4 get3DPoint3DPlaneAs2DPointProjectionMatrix(Vector3 pointA, Vector3 pointB, Vector3 pointC)
            //{
            //    // Adapted from https://stackoverflow.com/questions/49769459/convert-points-on-a-3d-plane-to-2d-coordinates

            //    var vectorAB = pointB - pointA;
            //    var vectorAC = pointC - pointA;

            //    var vectoruN = Vector3.Cross(vectorAB, vectorAC).normalized;
            //    var vectorU = vectorAB.normalized;
            //    var vectorV = Vector3.Cross(vectorU, vectoruN);

            //    var vectoru = pointA + vectorU;
            //    var vectorv = pointA + vectorV;
            //    var vectorn = pointA + vectoruN;

            //    var matrixS = new Matrix4x4(
            //        new Vector4(pointA.x, pointA.y, pointA.z, 1),
            //        new Vector4(vectoru.x, vectoru.y, vectoru.z, 1),
            //        new Vector4(vectorv.x, vectorv.y, vectorv.z, 1),
            //        new Vector4(vectorn.x, vectorn.y, vectorn.z, 1)
            //    );

            //    var matrixD = new Matrix4x4(
            //        new Vector4(0, 0, 0, 1),
            //        new Vector4(1, 0, 0, 1),
            //        new Vector4(0, 1, 0, 1),
            //        new Vector4(0, 0, 1, 1)
            //    );

            //    var matrixM = matrixD * matrixS.inverse;

            //    return matrixM;

            //}

            //Matrix4x4 get3DPoint3DPlaneAs2DPointProjectionMatrixTroughOrigin(Vector3 pointB, Vector3 pointC, out Vector3 vectoruN)
            //{
            //    // Adapted from https://stackoverflow.com/questions/49769459/convert-points-on-a-3d-plane-to-2d-coordinates

            //    vectoruN = Vector3.Cross(pointB, pointC).normalized;
            //    var vectorU = pointB.normalized;
            //    var vectorV = Vector3.Cross(vectorU, vectoruN);

            //    var matrixS = new Matrix4x4(
            //        new Vector4(0, 0, 0, 1),
            //        new Vector4(vectorU.x, vectorU.y, vectorU.z, 1),
            //        new Vector4(vectorV.x, vectorV.y, vectorV.z, 1),
            //        new Vector4(vectoruN.x, vectoruN.y, vectoruN.z, 1)
            //    );

            //    var matrixD = new Matrix4x4(
            //        new Vector4(0, 0, 0, 1),
            //        new Vector4(1, 0, 0, 1),
            //        new Vector4(0, 1, 0, 1),
            //        new Vector4(0, 0, 1, 1)
            //    );

            //    var matrixM = matrixD * matrixS.inverse;

            //    return matrixM;

            //}
            ////Matrix4x4 get3DPoint3DPlaneAs2DPointProjectionMatrixTroughOrigin2(Vector3 pointB, Vector3 pointC)
            ////{
            ////    Vector3 vectoruN;
            ////    return get3DPoint3DPlaneAs2DPointProjectionMatrixTroughOrigin(pointB, pointC, out vectoruN);
            ////}
            //Matrix4x4 get3DPoint3DPlaneAs2DPointProjectionMatrixTroughOrigin3(Vector3 pointB, Vector3 vectoruN)
            //{
            //    // Adapted from https://stackoverflow.com/questions/49769459/convert-points-on-a-3d-plane-to-2d-coordinates

            //    var vectorU = pointB.normalized;
            //    var vectorV = Vector3.Cross(vectorU, vectoruN);

            //    var matrixS = new Matrix4x4(
            //        new Vector4(0, 0, 0, 1),
            //        new Vector4(vectorU.x, vectorU.y, vectorU.z, 1),
            //        new Vector4(vectorV.x, vectorV.y, vectorV.z, 1),
            //        new Vector4(vectoruN.x, vectoruN.y, vectoruN.z, 1)
            //    );

            //    var matrixD = new Matrix4x4(
            //        new Vector4(0, 0, 0, 1),
            //        new Vector4(1, 0, 0, 1),
            //        new Vector4(0, 1, 0, 1),
            //        new Vector4(0, 0, 1, 1)
            //    );

            //    var matrixM = matrixD * matrixS.inverse;

            //    return matrixM;

            //}

            // START FAILED ATTEMPT!!!
            // Start of positional compression code
            {
                var distances = new double[___bufferLength - 1];
                var sphericalDistances = new double[___bufferLength - 1];
                var distanceSimmilarity = new bool[___bufferLength - 2];
                var lookRotations = new Quaternion[___bufferLength];
                var planeNormals = new Vector3[___bufferLength - 1];
                var distanceToOrigins = new double[___bufferLength];
                var deltaDistanceToOrigins = new double[___bufferLength - 1];
                var planarSimilarity = new bool[___bufferLength - 2];
                var planarSimilarityDifferentialIndexes = new List<int>();
                for (int j = 0; j < ___bufferLength; j++)
                {
                    w.Write(__instance.pointPos[j].x);
                    w.Write(__instance.pointPos[j].y);
                    w.Write(__instance.pointPos[j].z);

                    //Debug.Log($"Position of {j} is ({__instance.pointPos[j].x}, {__instance.pointPos[j].y}, {__instance.pointPos[j].z})");

                    var lookRotation = Quaternion.LookRotation(__instance.pointPos[j], Vector3.up);
                    //Debug.Log($"Position As rotation of {j} is ({lookRotation.x}, {lookRotation.y}, {lookRotation.z}, {lookRotation.w})");
                    lookRotations[j] = lookRotation;

                    var origin = new VectorLF3(0, 0, 0);
                    var distanceToOrigin = origin.Distance(__instance.pointPos[j]);
                    //Debug.Log($"Distance between origin (0,0,0) and {j} is {distanceToOrigin} rounded too 2 decimals {Math.Round(distanceToOrigin, 2)}");
                    distanceToOrigins[j] = distanceToOrigin;

                    if (j > 0)
                    {
                        var distance = new VectorLF3(__instance.pointPos[j - 1]).Distance(__instance.pointPos[j]);
                        //Debug.Log($"Distance between {j-1} and {j} is {distance}");
                        distances[j - 1] = distance;

                        var sphericalDistance = calcSphericalDistance(__instance.pointPos[j - 1], __instance.pointPos[j]);
                        //Debug.Log($"Spherical Distance between {j - 1} and {j} is {sphericalDistance}");
                        sphericalDistances[j - 1] = sphericalDistance;

                        var lookRotationDelta = lookRotations[j - 1] * Quaternion.Inverse(lookRotations[j]);
                        //Debug.Log($"lookRotationDelta between {j - 1} and {j} is ({lookRotationDelta.x}, {lookRotationDelta.y}, {lookRotationDelta.z}, {lookRotationDelta.w})");

                        var lookRotationDeltaAsAngle = Quaternion.Angle(lookRotations[j - 1], lookRotations[j]);
                        //Debug.Log($"lookRotationDeltaAsAngle between {j - 1} and {j} is {lookRotationDeltaAsAngle}");

                        planeNormals[j - 1] = Vector3.Cross(__instance.pointPos[j - 1], __instance.pointPos[j]);

                        var deltaDistanceToOrgin = distanceToOrigins[j - 1] - distanceToOrigins[j];
                        //Debug.Log($"deltaDistanceToOrgin between {j - 1} and {j} is {deltaDistanceToOrgin}");
                        deltaDistanceToOrigins[j - 1] = deltaDistanceToOrgin;
                    }

                    if (j > 1)
                    {
                        //distanceSimmilarity[j - 2] = Mathf.Approximately(distances[j - 2], distances[j - 1]);
                        distanceSimmilarity[j - 2] = Mathf.Approximately((float)distances[j - 2], (float)distances[j - 1]);
                        //Debug.Log($"distanceSimmilarity between {j - 2}, {j - 1} and {j} is {distanceSimmilarity[j - 2]}");

                        //var planeNormal = Vector3.Cross(__instance.pointPos[j - 2], __instance.pointPos[j - 1]);
                        var planeNormal = planeNormals[j - 2];
                        var projectionOnPlane = Vector3.ProjectOnPlane(__instance.pointPos[j], planeNormal);
                        var distanceFromPlane = Vector3.Distance(__instance.pointPos[j], projectionOnPlane);

                        //var planeNormal = VectorLF3.Cross(__instance.pointPos[j - 2], __instance.pointPos[j - 1]);
                        //var projectionOnPlane = VectorLF3ProjectOnPlane(__instance.pointPos[j], planeNormal);                    
                        //var distanceFromPlane = projectionOnPlane.Distance(__instance.pointPos[j]);
                        //Debug.Log($"Distance of {j} to plane formed by {j - 2} and {j - 1} is {distanceFromPlane} and regarded as 0 {distanceFromPlane < 1E-04f}");

                        // THIS is the best way to detect wheter they are in the same plane
                        var angularDiff = Quaternion.Angle(Quaternion.LookRotation(planeNormals[j - 2]), Quaternion.LookRotation(planeNormals[j - 1]));
                        //Debug.Log($"Angle between normals of {j - 2}, {j - 1}  and {j - 1}, {j} is {angularDiff} is {angularDiff == 0}");

                        planarSimilarity[j - 2] = angularDiff == 0;
                    }

                    if (j > 2)
                    {
                        var planarSimilarityDifferentialSimilarity = planarSimilarity[j - 3] == planarSimilarity[j - 2];

                        if (!planarSimilarityDifferentialSimilarity)
                        {
                            planarSimilarityDifferentialIndexes.Add(j - 3);
                        }
                    }
                }

                // Write positinal data in a compressed format
                var startIndex = 0;
                int endIndex;
                for (int j = 0; j < planarSimilarityDifferentialIndexes.Count; j++)
                {
                    var differentialIndex = planarSimilarityDifferentialIndexes[j];
                    var planarSequence = planarSimilarity[differentialIndex];

                    endIndex = differentialIndex + (planarSequence ? 2 : 0);
                    var repCount = endIndex - startIndex + 1;

                    //w.Write(planarSequence);
                    //w.Write(repCount);
                    if (planarSequence)
                    {
                        // TODO: Fall back to non plane logic if the repcount is 1 (or even 2 or 3 maybe)

                        //// Write normal of the plane (or we just store the entire projection matrix here? )
                        //var matrixM = get3DPoint3DPlaneAs2DPointProjectionMatrixTroughOrigin(__instance.pointPos[startIndexP], __instance.pointPos[endIndexP]);
                        ////matrixM[]
                        // It is probably enough to just write the begin and end points, or the begin point and the plane normal
                        // TODO: Finish this
                        var pointA = __instance.pointPos[startIndex + (int)Math.Ceiling(a: repCount / 2)];
                        Debug.Log($"pointA is {pointA.x}, {pointA.y}, {pointA.z}");
                        var pointB = __instance.pointPos[startIndex];
                        Debug.Log($"pointB is {pointB.x}, {pointB.y}, {pointB.z}");
                        var pointC = __instance.pointPos[endIndex];
                        Debug.Log($"pointC is {pointC.x}, {pointC.y}, {pointC.z}");
                        //Vector3 vectoruN;
                        //var matrixM = Get3DPoint3DPlaneAs2DPointProjectionMatrixTroughOrigin(__instance.pointPos[startIndexP], __instance.pointPos[endIndexP], out vectoruN); // We should not use this ^, because we risk that the points are linear
                        //var matrixM = Get3DPoint3DPlaneAs2DPointProjectionMatrixTroughOrigin(__instance.pointPos[startIndex], __instance.pointPos[startIndex + 1], out vectoruN);
                        var matrixM = Get3DPoint3DPlaneAs2DPointProjectionMatrix2(__instance.pointPos[startIndex + (int)Math.Ceiling(a: repCount / 2)], __instance.pointPos[startIndex], __instance.pointPos[endIndex]);
                        //w.Write(vectoruN.x);
                        //w.Write(vectoruN.y);
                        //w.Write(vectoruN.z);
                        //w.Write(__instance.pointPos[startIndexP].x);
                        //w.Write(__instance.pointPos[startIndexP].y);
                        //w.Write(__instance.pointPos[startIndexP].z);

                        //Debug.Log($"Matrix M is {matrixM}");

                        //var projection = Vector3.Project(__instance.pointPos[startIndex + (int)Math.Ceiling(a: repCount / 2)], vectoruN);
                        //Debug.Log($"vectoruN is {vectoruN.x}, {vectoruN.y}, {vectoruN.z}");
                        //var offset = Vector3.Dot(projection, vectoruN);
                        //Debug.Log($"Distance offset along the normal is {offset}");
                        //var vectorN = vectoruN * offset;
                        //Debug.Log($"vectorN is {vectorN.x}, {vectorN.y}, {vectorN.z}");

                        //matrixM = new Matrix4x4(
                        //    new Vector4(1, 0, 0, 0),
                        //    new Vector4(0, 1, 0, 0),
                        //    new Vector4(0, 0, 1, 0),
                        //    new Vector4(vectorN.x, vectorN.y, vectorN.z, 1)
                        //) * matrixM;

                        //var negativeTranslationMatrix = new Matrix4x4(
                        //    new Vector4(1, 0, 0, 0),
                        //    new Vector4(0, 1, 0, 0),
                        //    new Vector4(0, 0, 1, 0),
                        //    new Vector4(projection.x, projection.y, projection.z, 1)
                        //);

                        //var planarPolarAngle = Vector3.Angle(vectoruN, Vector3.up);
                        //Debug.Log($"planarPolarAngle for sequence {j} is {planarPolarAngle} is 90 is {Mathf.Approximately(planarPolarAngle, 90)}");

                        for (int i = 0; i < repCount; i++)
                        {
                            // Write 2D coordinates on plane

                            //var test1 = __instance.pointPos[startIndex + i] - projection;
                            //var test2 = negativeTranslationMatrix * __instance.pointPos[startIndex + i];
                            //Debug.Log($"test1 is ({test1.x}, {test1.y}, {test1.z})");
                            //Debug.Log($"test2 is ({test2.x}, {test2.y}, {test2.z})");

                            // TODO: For the first one we can probably just take the already written point
                            //var projectedPoint = matrixM * (__instance.pointPos[startIndex + i] - projection); // Works
                            //var projectedPoint = matrixM * Matrix4x4.Translate(-projection).MultiplyPoint3x4(__instance.pointPos[startIndex + i]);
                            //var projectedPoint = (Matrix4x4.Translate(-projection) * matrixM).MultiplyPoint3x4(__instance.pointPos[startIndex + i]);
                            //var projectedPoint = (matrixM * __instance.pointPos[startIndex + i]) - (matrixM * projection); // Works
                            var projectedPoint = matrixM.inverse.MultiplyPoint3x4(__instance.pointPos[startIndex + i]); // Fuck yea!!!
                            //w.Write(__instance.pointPos[startIndexP].x);
                            //w.Write(__instance.pointPos[startIndexP].y);
                            Debug.Log($"point {startIndex + i} in planar sequense has projection point ({projectedPoint.x}, {projectedPoint.y}, {projectedPoint.z})");

                            //var calculatedPoint = matrixM.inverse * (projectedPoint + (matrixM * projection)); // Works
                            //var calculatedPoint = (matrixM.inverse * projectedPoint) + (matrixM.inverse * matrixM * projection); // Works
                            //var calculatedPoint = matrixM.inverse.MultiplyPoint3x4(projectedPoint) + projection; // Works
                            var calculatedPoint = matrixM.MultiplyPoint3x4(projectedPoint); // Fuck yea!!!
                            Debug.Log($"point {startIndex + i} is ({__instance.pointPos[startIndex + i].x}, {__instance.pointPos[startIndex + i].y}, {__instance.pointPos[startIndex + i].z})");
                            Debug.Log($"calculcated point {startIndex + i} is ({calculatedPoint.x}, {calculatedPoint.y}, {calculatedPoint.z})");
                        }
                    }
                    else
                    {
                        for (int i = 0; i < repCount; i++)
                        {
                            //w.Write(__instance.pointPos[startIndexP + i].x);
                            //w.Write(__instance.pointPos[startIndexP + i].y);
                            //w.Write(__instance.pointPos[startIndexP + i].z);
                        }
                    }

                    startIndex = endIndex + 1;
                }
                // The end needs to be handled seperately (since it does not have a differentialIndex)
                endIndex = ___bufferLength - 1;
                var planarSequenceForEnd = planarSimilarity[___bufferLength - 3];
                var repCountForEndP = endIndex - startIndex + 1;
                //w.Write(planarSequence);
                //w.Write(repCount);
                if (planarSequenceForEnd)
                {
                    // TODO: Fall back to non plane logic if the repcount is 1 (or even 2 or 3 maybe)

                    //// Write normal of the plane (or we just store the entire projection matrix here? )
                    //var matrixM = get3DPoint3DPlaneAs2DPointProjectionMatrixTroughOrigin(__instance.pointPos[startIndexP], __instance.pointPos[endIndexP]);
                    ////matrixM[]
                    // It is probably enough to just write the begin and end points, or the begin point and the plane normal
                    // TODO: Finish this
                    var pointA = __instance.pointPos[startIndex + (int)Math.Ceiling(a: repCountForEndP / 2)];
                    Debug.Log($"pointA is {pointA.x}, {pointA.y}, {pointA.z}");
                    var pointB = __instance.pointPos[startIndex];
                    Debug.Log($"pointB is {pointB.x}, {pointB.y}, {pointB.z}");
                    var pointC = __instance.pointPos[endIndex];
                    Debug.Log($"pointC is {pointC.x}, {pointC.y}, {pointC.z}");
                    //Vector3 vectoruN;
                    //var matrixM = Get3DPoint3DPlaneAs2DPointProjectionMatrixTroughOrigin(__instance.pointPos[startIndexP], __instance.pointPos[endIndexP], out vectoruN); // We should not use this, because we risk that the points are linear
                    //var matrixM = Get3DPoint3DPlaneAs2DPointProjectionMatrixTroughOrigin(__instance.pointPos[startIndex], __instance.pointPos[startIndex + 1], out vectoruN);
                    var matrixM = Get3DPoint3DPlaneAs2DPointProjectionMatrix2(__instance.pointPos[startIndex + (int)Math.Ceiling(a: repCountForEndP/2)], __instance.pointPos[startIndex], __instance.pointPos[endIndex]);
                    //w.Write(vectoruN.x);
                    //w.Write(vectoruN.y);
                    //w.Write(vectoruN.z);
                    //w.Write(__instance.pointPos[startIndexP].x);
                    //w.Write(__instance.pointPos[startIndexP].y);
                    //w.Write(__instance.pointPos[startIndexP].z);

                    //Debug.Log($"Matrix M is {matrixM}");

                    

                    //var projection = Vector3.Project(__instance.pointPos[startIndex + (int)Math.Ceiling(a: repCountForEndP / 2)], vectoruN);
                    //Debug.Log($"vectoruN is {vectoruN.x}, {vectoruN.y}, {vectoruN.z}");
                    //var offset = Vector3.Dot(projection, vectoruN);
                    //Debug.Log($"Distance offset along the normal is {offset}");
                    //var vectorN = vectoruN * offset;
                    //Debug.Log($"vectorN is {vectorN.x}, {vectorN.y}, {vectorN.z}");

                    //matrixM = new Matrix4x4(
                    //    new Vector4(1, 0, 0, 0),
                    //    new Vector4(0, 1, 0, 0),
                    //    new Vector4(0, 0, 1, 0),
                    //    new Vector4(vectorN.x, vectorN.y, vectorN.z, 1)
                    //) * matrixM;

                    //var negativeTranslationMatrix = new Matrix4x4(
                    //    new Vector4(1, 0, 0, 0),
                    //    new Vector4(0, 1, 0, 0),
                    //    new Vector4(0, 0, 1, 0),
                    //    new Vector4(projection.x, projection.y, projection.z, 1)
                    //);

                    //var planarPolarAngle = Vector3.Angle(vectoruN, Vector3.up);
                    //Debug.Log($"planarPolarAngle for sequence {planarSimilarityDifferentialIndexes.Count} is {planarPolarAngle} is 90 is {Mathf.Approximately(planarPolarAngle, 90)}");

                    for (int i = 0; i < repCountForEndP; i++)
                    {
                        // Write 2D coordinates on plane

                        //var test1 = __instance.pointPos[startIndex + i] - projection;
                        //var test2 = negativeTranslationMatrix * __instance.pointPos[startIndex + i];
                        //Debug.Log($"test1 is ({test1.x}, {test1.y}, {test1.z})");
                        //Debug.Log($"test2 is ({test2.x}, {test2.y}, {test2.z})");

                        // TODO: For the first one we can probably just take the already written point
                        //var projectedPoint = matrixM * (__instance.pointPos[startIndex + i] - projection);
                        //var projectedPoint = matrixM * Matrix4x4.Translate(-projection).MultiplyPoint3x4(__instance.pointPos[startIndex + i]);
                        //var projectedPoint = (Matrix4x4.Translate(-projection) * matrixM).MultiplyPoint3x4(__instance.pointPos[startIndex + i]);
                        //var projectedPoint = (matrixM * __instance.pointPos[startIndex + i]) - (matrixM * projection);
                        var projectedPoint = matrixM.inverse.MultiplyPoint3x4(__instance.pointPos[startIndex + i]);
                        //w.Write(__instance.pointPos[startIndexP].x);
                        //w.Write(__instance.pointPos[startIndexP].y);
                        Debug.Log($"point {startIndex + i} in planar sequense has projection point ({projectedPoint.x}, {projectedPoint.y}, {projectedPoint.z})");

                        //var calculatedPoint = matrixM.inverse * (projectedPoint + (matrixM * projection));
                        //var calculatedPoint = (matrixM.inverse * projectedPoint) + (matrixM.inverse * matrixM * projection);
                        var calculatedPoint = matrixM.MultiplyPoint3x4(projectedPoint);
                        Debug.Log($"point {startIndex + i} is ({__instance.pointPos[startIndex + i].x}, {__instance.pointPos[startIndex + i].y}, {__instance.pointPos[startIndex + i].z})");
                        Debug.Log($"calculcated point {startIndex + i} is ({calculatedPoint.x}, {calculatedPoint.y}, {calculatedPoint.z})");
                    }
                }
                else
                {
                    for (int i = 0; i < repCountForEndP; i++)
                    {
                        //w.Write(__instance.pointPos[startIndexP + i].x);
                        //w.Write(__instance.pointPos[startIndexP + i].y);
                        //w.Write(__instance.pointPos[startIndexP + i].z);
                    }
                }
            }

            // END FAILED ATTEMPT!!!

            //// Start of positional compression code
            //{
            //    var interPointVectors = new Vector3[___bufferLength - 1];
            //    var tripointPlanarNormals = new Vector3[___bufferLength - 2];
            //    var planarSimmilarity = new bool[___bufferLength - 3];
            //    var planarSimilarityDifferentialIndexes = new List<int>();
            //    for (int j = 0; j < ___bufferLength; j++)
            //    {
            //        w.Write(__instance.pointPos[j].x);
            //        w.Write(__instance.pointPos[j].y);
            //        w.Write(__instance.pointPos[j].z);

            //        if (j > 0)
            //        {
            //            interPointVectors[j - 1] = __instance.pointPos[j - 1] - __instance.pointPos[j];
            //        }

            //        if (j > 1)
            //        {
            //            //tripointPlanarNormals[j - 2] = Vector3.Cross(interPointVectors[j - 2], interPointVectors[j - 1]);
            //            tripointPlanarNormals[j - 2] = Vector3.Cross(__instance.pointPos[j - 1] - __instance.pointPos[j - 2], __instance.pointPos[j] - __instance.pointPos[j - 2]);
            //        }

            //        if (j > 2)
            //        {
            //            var angularDiff = Quaternion.Angle(Quaternion.LookRotation(tripointPlanarNormals[j - 3]), Quaternion.LookRotation(tripointPlanarNormals[j - 2]));
            //            //var angularDiff = Vector3.Angle(tripointPlanarNormals[j - 3], tripointPlanarNormals[j - 2]);
            //            Debug.Log($"Angle between tripointPlanarNormals from points {j - 3}, {j - 2}, {j - 1}  and {j - 2}, {j - 1}, {j} is {angularDiff} is {angularDiff == 0}");

            //            planarSimmilarity[j - 3] = angularDiff == 0;
            //        }

            //        if (j > 3)
            //        {
            //            var planarSimilarityDifferentialSimilarity = planarSimmilarity[j - 4] == planarSimmilarity[j - 3];

            //            if (!planarSimilarityDifferentialSimilarity)
            //            {
            //                planarSimilarityDifferentialIndexes.Add(j - 4);
            //            }
            //        }
            //    }
            //}




            // START of rotational compression code
            {             
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
                    // TODO: compress the surfaceRelativeSequence and the repCount into one int (and dont forget to do the same in the decoding)
                    // TODO: use a uint32 for the repcount instead of a int32 (so we do not waste space on negative numbers)
                    w.Write(surfaceRelativeSequence);
                    w.Write(repCount);
                    if (surfaceRelativeSequence)
                    {

                        // TODO: If the repcount is 1 (and maybe 2 too) we should just write the original rotations (and dont forget to do the same in the decoding)

                        //Debug.Log($"Writing relative rotation for index {startIndex}");
                        w.Write(surfaceRelativeRotations[startIndex].x);
                        w.Write(surfaceRelativeRotations[startIndex].y);
                        w.Write(surfaceRelativeRotations[startIndex].z);
                        w.Write(surfaceRelativeRotations[startIndex].w);
                    }
                    else
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
