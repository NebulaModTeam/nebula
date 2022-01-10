using System;
using System.IO;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
// ReSharper disable Unity.PerformanceCriticalCodeNullComparison
// ReSharper disable Unity.PerformanceCriticalCodeInvocation

#endif

namespace UnityEditor
{
#if UNITY_EDITOR
    [CustomEditor(typeof(MeshDataAsset))]
    public class MeshDataAssetEditor : Editor
    {
        public Mesh mesh;
        
        public static byte[] saveMeshToMeshAsset(Mesh mesh)
        {
            MeshData data = new MeshData();
            data.InitMeshData(mesh);

            try
            {
                using (MemoryStream stream = new MemoryStream())
                {
                    using (BinaryWriter binaryWriter = new BinaryWriter(stream))
                    {
                        binaryWriter.Write(data.vertexCount);
                        binaryWriter.Write(2704);
                        binaryWriter.Write(data.submeshCount);
                        for (int i = 0; i < data.submeshCount; i++)
                        {
                            binaryWriter.Write(data.indices[i].Length);
                        }

                        binaryWriter.Write(data.normals != null);
                        binaryWriter.Write(data.tangents != null);
                        binaryWriter.Write(data.colors != null);
                        binaryWriter.Write(data.uvs != null);
                        binaryWriter.Write(data.uv2s != null);
                        binaryWriter.Write(data.uv3s != null);
                        binaryWriter.Write(data.uv4s != null);

                        for (int j = 0; j < data.vertices.Length; j++)
                        {
                            binaryWriter.Write(data.vertices[j].x);
                            binaryWriter.Write(data.vertices[j].y);
                            binaryWriter.Write(data.vertices[j].z);
                        }

                        if (data.normals != null)
                        {
                            for (int j = 0; j < data.normals.Length; j++)
                            {
                                binaryWriter.Write(data.normals[j].x);
                                binaryWriter.Write(data.normals[j].y);
                                binaryWriter.Write(data.normals[j].z);
                            }
                        }

                        if (data.tangents != null)
                        {
                            for (int j = 0; j < data.tangents.Length; j++)
                            {
                                binaryWriter.Write(data.tangents[j].x);
                                binaryWriter.Write(data.tangents[j].y);
                                binaryWriter.Write(data.tangents[j].z);
                                binaryWriter.Write(data.tangents[j].w);
                            }
                        }

                        if (data.colors != null)
                        {
                            for (int j = 0; j < data.colors.Length; j++)
                            {
                                binaryWriter.Write(data.colors[j].r);
                                binaryWriter.Write(data.colors[j].g);
                                binaryWriter.Write(data.colors[j].b);
                                binaryWriter.Write(data.colors[j].a);
                            }
                        }

                        if (data.uvs != null)
                        {
                            for (int j = 0; j < data.uvs.Length; j++)
                            {
                                binaryWriter.Write(data.uvs[j].x);
                                binaryWriter.Write(data.uvs[j].y);
                            }
                        }

                        if (data.uv2s != null)
                        {
                            for (int j = 0; j < data.uv2s.Length; j++)
                            {
                                binaryWriter.Write(data.uv2s[j].x);
                                binaryWriter.Write(data.uv2s[j].y);
                            }
                        }

                        if (data.uv3s != null)
                        {
                            for (int j = 0; j < data.uv3s.Length; j++)
                            {
                                binaryWriter.Write(data.uv3s[j].x);
                                binaryWriter.Write(data.uv3s[j].y);
                            }
                        }

                        if (data.uv4s != null)
                        {
                            for (int j = 0; j < data.uv4s.Length; j++)
                            {
                                binaryWriter.Write(data.uv4s[j].x);
                                binaryWriter.Write(data.uv4s[j].y);
                            }
                        }

                        for (int j = 0; j < data.submeshCount; j++)
                        {
                            binaryWriter.Write(data.indices[j].Length);
                            for (int k = 0; k < data.indices[j].Length; k++)
                            {
                                binaryWriter.Write(data.indices[j][k]);
                            }
                        }

                        binaryWriter.Close();
                    }

                    return stream.ToArray();
                }
            }
            catch (Exception ex)
            {
                Debug.LogError("MeshData saving failed:\r\n" + ex.ToString());
            }

            return null;
        }
        
        public override void OnInspectorGUI()
        {
            MeshDataAsset trg = (MeshDataAsset) target;
            EditorGUILayout.LabelField("Editor that hides the bytes(And saves the world):");
            EditorGUILayout.LabelField("Byte count: " + trg.bytes.Length);
            
            var serializedObject = new SerializedObject(trg);
            var property = serializedObject.FindProperty("materials");
            serializedObject.Update();
            EditorGUILayout.PropertyField(property, true);
            serializedObject.ApplyModifiedProperties();
            
            mesh = (Mesh)EditorGUILayout.ObjectField(mesh, typeof(Mesh), false);
            
            if (GUILayout.Button("Save mesh to data aseet"))
            {
                if (mesh != null)
                {
                    trg.bytes = saveMeshToMeshAsset(mesh);
                }
            }

        }
    }
#endif
}