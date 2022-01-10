using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEditor;
using Object = UnityEngine.Object;

namespace UnityEditor
{
#if UNITY_EDITOR
    public class AnimationBakerWindow : EditorWindow
    {
        private GameObject bakeObject;


        private MeshFilter[] filters;
        private MeshRenderer[] basicRenderers;
        private SkinnedMeshRenderer[] renderers;

        private Material[] materials;

        private VertaBuffer buffer;
        private bool bufferReady;

        private AnimationClip[] animationClips = new AnimationClip[0];
        private int frameCount;
        private bool readyToBake;
        private bool lockSelection;
        private bool bakeOnlyMesh;
        private bool debugMeshOutput;

        private float frameRate = 30f;
        private int frameStride;

        private string meshDataPath = "MeshDatas";
        private string vertaPath = "Verta";

        private string modelName = "";

        public Object targetPathObject;

        [MenuItem("Window/DSP Tools/Verta Animation Baker", false)]
        public static void DoWindow()
        {
            var window = GetWindowWithRect<AnimationBakerWindow>(new Rect(0, 0, 300, 170));
            window.SetBakeObject(Selection.activeGameObject);
            window.Show();
        }

        public AnimationBakerWindow()
        {
            titleContent.text = "Animation Baker";
        }

        //Combine all meshes together
        public Mesh CombineSimpleMeshes(GameObject gameObject)
        {
            //Get all mesh filters and combine
            CombineInstance[] combine = new CombineInstance[filters.Length + renderers.Length];

            for (int i = 0; i < filters.Length; i++)
            {
                combine[i].mesh = filters[i].sharedMesh;
                combine[i].transform = filters[i].transform.localToWorldMatrix;
                combine[i].subMeshIndex = 0;
            }

            for (int i = filters.Length; i < filters.Length + renderers.Length; i++)
            {
                combine[i].mesh = new Mesh();
                renderers[i].BakeMesh(combine[i].mesh);
                combine[i].transform = renderers[i].transform.localToWorldMatrix;
                combine[i].subMeshIndex = 0;
            }

            Mesh mesh = new Mesh();
            mesh.CombineMeshes(combine, true, true);

            return mesh;
        }

        public Mesh CombineMeshes(GameObject gameObject)
        {
            //Get all mesh filters and combine
            //CombineInstance[] combine = new CombineInstance[filters.Length];

            List<CombineInstance> combine = new List<CombineInstance>();
            List<Mesh> subMeshes = new List<Mesh>();
            Mesh mesh;

            foreach (Material mat in materials)
            {
                for (int i = 0; i < filters.Length; i++)
                {
                    int submesh = -1;
                    for (int j = 0; j < basicRenderers[i].sharedMaterials.Length; j++)
                    {
                        if (basicRenderers[i].sharedMaterials[j] != mat) continue;

                        submesh = j;
                        break;
                    }

                    if (submesh == -1) continue;

                    CombineInstance inst = new CombineInstance
                    {
                        mesh = filters[i].sharedMesh,
                        transform = filters[i].transform.localToWorldMatrix,
                        subMeshIndex = submesh
                    };
                    combine.Add(inst);
                }

                foreach (SkinnedMeshRenderer renderer in renderers)
                {
                    int submesh = -1;
                    for (int j = 0; j < renderer.sharedMaterials.Length; j++)
                    {
                        if (renderer.sharedMaterials[j] != mat) continue;

                        submesh = j;
                        break;
                    }

                    if (submesh == -1) continue;

                    Mesh bakedMesh = new Mesh();
                    renderer.BakeMesh(bakedMesh);

                    CombineInstance inst = new CombineInstance
                    {
                        mesh = bakedMesh,
                        transform = renderer.transform.localToWorldMatrix,
                        subMeshIndex = submesh
                    };
                    combine.Add(inst);
                }

                mesh = new Mesh();
                mesh.CombineMeshes(combine.ToArray(), true, true);
                subMeshes.Add(mesh);
                combine.Clear();
            }

            if (subMeshes.Count == 1)
                return subMeshes[0];

            foreach (Mesh submesh in subMeshes)
            {
                CombineInstance inst = new CombineInstance
                {
                    mesh = submesh
                };
                combine.Add(inst);
            }

            mesh = new Mesh();
            mesh.CombineMeshes(combine.ToArray(), false, false);

            return mesh;
        }

        private int GetFramesCount(AnimationClip clip)
        {
            if (clip == null) return 0;
            return Mathf.CeilToInt(clip.length * frameRate);
        }


        // Has a GameObject been selection?
        public void OnSelectionChange()
        {
            if (!lockSelection)
            {
                SetBakeObject(Selection.activeGameObject);
            }
        }

        public void SetBakeObject(GameObject newTarget)
        {
            if (newTarget == null) return;

            if (modelName.Equals("") || bakeObject != null && modelName.Equals(bakeObject.name))
            {
                modelName = newTarget.name;
            }

            Animation animation = newTarget.GetComponent<Animation>();
            if (animation != null && animation.clip != null)
            {
                animationClips = new AnimationClip[animation.GetClipCount()];
                int i = 0;
                foreach (AnimationState state in animation)
                {
                    animationClips[i] = state.clip;
                    i++;
                }
            }


            bakeObject = newTarget;
            Repaint();
        }

        // Main editor window
        public void OnGUI()
        {
            if (AnimationMode.InAnimationMode())
                AnimationMode.StopAnimationMode();

            int needSize = 110;

            // Wait for user to select a GameObject
            if (bakeObject == null)
            {
                EditorGUILayout.HelpBox("Please select a GameObject", MessageType.Info);
                return;
            }

            if (buffer == null)
            {
                buffer = new VertaBuffer();
            }

            EditorGUILayout.BeginVertical();
            EditorGUILayout.LabelField("Selected object: " + bakeObject.name);

            modelName = EditorGUILayout.TextField("Model Name", modelName);
            
            targetPathObject = EditorGUILayout.ObjectField("Output folder", targetPathObject, typeof(Object), true);

            bakeOnlyMesh = EditorGUILayout.Toggle("No animations", bakeOnlyMesh);

            if (!bakeOnlyMesh)
            {
                needSize += 20; 
            }

            debugMeshOutput = EditorGUILayout.Toggle("Output debug mesh", debugMeshOutput);


            if (!bakeOnlyMesh)
            {
                int newLen = EditorGUILayout.IntField("Number of clips:", animationClips.Length);
                newLen = Math.Min(newLen, 5);
                if (newLen != animationClips.Length)
                {
                    Array.Resize(ref animationClips, newLen);
                }

                EditorGUILayout.BeginVertical();
                for (int i = 0; i < animationClips.Length; i++)
                {
                    animationClips[i] = EditorGUILayout.ObjectField(animationClips[i], typeof(AnimationClip), false) as AnimationClip;
                    needSize += 19;
                }
                EditorGUILayout.EndVertical();

                if (animationClips != null && animationClips.Length > 0)
                {
                    int tmpFrameCount = 0;
                    foreach (AnimationClip clip in animationClips)
                    {
                        tmpFrameCount += GetFramesCount(clip);
                    }

                    frameCount = tmpFrameCount;
                    EditorGUILayout.LabelField("Frames to bake: " + frameCount);
                    needSize += 10;
                }
            }
            else
            {
                frameCount = 1;
            }
            
            minSize = new Vector2(300, needSize);
            maxSize = new Vector2(300, needSize);

            bool clipsReady = animationClips != null && animationClips.Length > 0;
            if (clipsReady)
            {
                clipsReady = animationClips.All(clip => clip != null);
            }

            readyToBake = (clipsReady || bakeOnlyMesh) && !EditorApplication.isPlaying &&
                          !modelName.Equals("");

            if (GUILayout.Button("Bake mesh animations.") && readyToBake)
            {
                lockSelection = true;
                BakeMesh();
                lockSelection = false;
            }

            EditorGUILayout.EndVertical();
        }

        private void BakeMesh()
        {
            if (bakeObject == null)
                return;

            if (animationClips.Any(clip => clip == null) && !bakeOnlyMesh)
                return;

            // There is a bug in AnimationMode.SampleAnimationClip which crashes
            // Unity if there is no valid controller attached
            Animator animator = bakeObject.GetComponent<Animator>();
            if (animator != null && animator.runtimeAnimatorController == null && !bakeOnlyMesh)
                return;

            //Collect information about gameObject
            List<MeshFilter> tmpFilters = bakeObject.GetComponentsInChildren<MeshFilter>().ToList();
            List<SkinnedMeshRenderer> tmpMeshRenderers = new List<SkinnedMeshRenderer>();

            for (int i = 0; i < tmpFilters.Count; i++)
            {
                MeshFilter filter = tmpFilters[i];
                SkinnedMeshRenderer meshRenderer = filter.GetComponent<SkinnedMeshRenderer>();

                if (meshRenderer != null)
                {
                    tmpFilters.RemoveAt(i);
                    tmpMeshRenderers.Add(meshRenderer);
                    i--;
                }
            }

            filters = tmpFilters.ToArray();
            basicRenderers = filters.Select(filter => filter.GetComponent<MeshRenderer>()).ToArray();
            List<Material> _materials = new List<Material>();
            foreach (MeshRenderer renderer in basicRenderers)
            {
                foreach (Material mat in renderer.sharedMaterials)
                {
                    if (!_materials.Contains(mat))
                    {
                        _materials.Add(mat);
                    }
                }
            }

            materials = _materials.ToArray();
            renderers = tmpMeshRenderers.ToArray();

            //Temporarily set position to zero to make matrix math easier
            Vector3 position = bakeObject.transform.position;
            bakeObject.transform.position = Vector3.zero;

            Mesh firstFrame = new Mesh();

            EditorUtility.DisplayProgressBar("Mesh Animation Baker", "Baking", 0f);

            //Now bake
            AnimationMode.StartAnimationMode();
            AnimationMode.BeginSampling();
            int totalFrame = 0;

            int BakeClip(AnimationClip animationClip)
            {
                int clipFrameCount = bakeOnlyMesh ? 1 : GetFramesCount(animationClip);
                for (int frame = 0; frame < clipFrameCount; frame++)
                {
                    EditorUtility.DisplayProgressBar("Mesh Animation Baker", "Baking mesh animations",
                        1f * (totalFrame + frame) / frameCount);

                    if (!bakeOnlyMesh)
                        AnimationMode.SampleAnimationClip(bakeObject, animationClip, frame / frameRate);
                    Mesh bakedMesh = CombineMeshes(bakeObject);

                    if (!bufferReady)
                    {
                        buffer.Expand(VertType.VNT, bakedMesh.vertexCount, frameCount);
                        frameStride = buffer.vertexSize * buffer.vertexCount;
                        bufferReady = true;
                    }

                    for (int j = 0; j < bakedMesh.vertexCount; j++)
                    {
                        int vertStart = j * buffer.vertexSize;
                        int globalVertStart = frameStride * (totalFrame + frame) + vertStart;
                        buffer.data[globalVertStart] = bakedMesh.vertices[j].x;
                        buffer.data[globalVertStart + 1] = bakedMesh.vertices[j].y;
                        buffer.data[globalVertStart + 2] = bakedMesh.vertices[j].z;

                        buffer.data[globalVertStart + 3] = bakedMesh.normals[j].x;
                        buffer.data[globalVertStart + 4] = bakedMesh.normals[j].y;
                        buffer.data[globalVertStart + 5] = bakedMesh.normals[j].z;

                        buffer.data[globalVertStart + 6] = bakedMesh.tangents[j].x;
                        buffer.data[globalVertStart + 7] = bakedMesh.tangents[j].y;
                        buffer.data[globalVertStart + 8] = bakedMesh.tangents[j].z;
                    }

                    if (totalFrame + frame == 0)
                    {
                        firstFrame = bakedMesh;
                    }
                }

                return bakeOnlyMesh ? 1 : GetFramesCount(animationClip);
            }


            if (!bakeOnlyMesh)
            {
                foreach (AnimationClip clip in animationClips)
                {
                    totalFrame += BakeClip(clip);
                }
            }
            else
            {
                BakeClip(null);
            }

            //Return to original position
            bakeObject.transform.position = position;

            string filePath = Path.Combine(Application.dataPath, vertaPath);

            FileInfo fileInfo = new FileInfo(filePath);
            if (!Directory.Exists(fileInfo.Directory.FullName))
                Directory.CreateDirectory(fileInfo.Directory.FullName);

            filePath += $"/{modelName}.verta";

            if (!bakeOnlyMesh)
                buffer.SaveToFile(filePath);

            if (targetPathObject == null)
            {
                filePath = Path.Combine("Assets", meshDataPath);
            }
            else
            {
                filePath = AssetDatabase.GetAssetPath(targetPathObject);
            }

            fileInfo = new FileInfo(filePath);
            if (!Directory.Exists(fileInfo.Directory.FullName))
                Directory.CreateDirectory(fileInfo.Directory.FullName);

            if (debugMeshOutput)
                AssetDatabase.CreateAsset(firstFrame, filePath + $"/{modelName}-mesh.asset");

            byte[] bytes = MeshDataAssetEditor.saveMeshToMeshAsset(firstFrame);

            MeshDataAsset asset = new MeshDataAsset();
            asset.bytes = bytes;
            asset.materials = materials.ToArray();

            AssetDatabase.CreateAsset(asset, filePath + $"/{modelName}.asset");

            EditorUtility.ClearProgressBar();

            AnimationMode.EndSampling();
            AnimationMode.StopAnimationMode();
        }
    }
#endif
}