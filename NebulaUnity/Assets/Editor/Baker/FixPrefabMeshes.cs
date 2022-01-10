using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace UnityEditor
{
#if UNITY_EDITOR
    public class FixPrefabMeshes
    {
        [MenuItem("Window/DSP Tools/Fix prefab(Can crash unity!)")]
        public static void FixPrefabs()
        {
            GameObject gameObject = Selection.activeGameObject;
            
            if (gameObject == null)
            {
                Debug.Log("You have nothing selected!");
                return;
            }
            
            RemoveMissingScripts(gameObject);

            if (gameObject.GetComponent<BuildConditionConfig>() == null)
            {
                gameObject.AddComponent<BuildConditionConfig>();
                gameObject.AddComponent<SlotConfig>();
                gameObject.AddComponent<AnimDesc>();
                gameObject.AddComponent<MinimapConfig>();
                gameObject.AddComponent<PowerDesc>();

                Transform lodTrs = gameObject.transform.Find("LOD");
                if (lodTrs != null)
                {
                    lodTrs.gameObject.AddComponent<LODModelDesc>();
                }
            }


            MeshFilter[] filters = gameObject.GetComponentsInChildren<MeshFilter>();

            foreach (MeshFilter filter in filters)
            {
                if (filter.sharedMesh == null)
                {
                    string[] found = AssetDatabase.FindAssets(filter.gameObject.name + " t:mesh");
                    if (found.Length > 0)
                    {
                        Debug.Log("Found mesh with name: " + AssetDatabase.GUIDToAssetPath(found[0]));
                        //Mesh mesh = (Mesh) Resources.Load(AssetDatabase.GUIDToAssetPath(found[0]), typeof(Mesh));

                        Mesh mesh = AssetDatabase.LoadAssetAtPath<Mesh>(AssetDatabase.GUIDToAssetPath(found[0]));

                        filter.sharedMesh = mesh;
                        
                        UnityEditor.EditorUtility.SetDirty(filter.gameObject);
                    }
                }
            }
        }

        
        private static void DoIterate(GameObject gameObject)
        {
            foreach (Transform child in gameObject.transform)
            {
                handleChild(child.gameObject);
                
                DoIterate(child.gameObject);
            }
        }

        private static void handleChild(GameObject gameObject)
        {
            var components = gameObject.GetComponents<Component>();
         
            // Create a serialized object so that we can edit the component list
            var serializedObject = new SerializedObject(gameObject);
            // Find the component list property
            var prop = serializedObject.FindProperty("m_Component");
         
            int r = 0;
         
            for(int j = 0; j < components.Length; j++)
            {
                if(components[j] == null)
                {
                    prop.DeleteArrayElementAtIndex(j-r);
                    r++;
                }
            }
         
            // Apply our changes to the game object
            serializedObject.ApplyModifiedProperties();
            UnityEditor.EditorUtility.SetDirty(gameObject);
        }
        
        private static void RemoveMissingScripts(GameObject target)
        {

            Queue<GameObject> queue = new Queue<GameObject>();
            queue.Enqueue(target);

            while (queue.Count > 0)
            {
                GameObject gameObject = queue.Dequeue();
                handleChild(gameObject);
                foreach (Transform child in gameObject.transform)
                {
                    queue.Enqueue(child.gameObject);
                }
            }
            
        }
    }
#endif
}