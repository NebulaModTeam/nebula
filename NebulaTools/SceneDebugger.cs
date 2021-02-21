using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace NebulaTools
{
    public class SceneDebugger
    {
        const string logsFile = "scene-debugger-logs.txt";

        public static void DumpSceneHierarchy()
        {
            List<GameObject> rootObjects = new List<GameObject>();

            Scene scene = SceneManager.GetActiveScene();
            scene.GetRootGameObjects(rootObjects);

            string filename = Path.Combine(Application.dataPath, logsFile);

            using (StreamWriter writer = new StreamWriter(filename, false))
            {
                for (int i = 0; i < rootObjects.Count; ++i)
                {
                    GameObject gameObject = rootObjects[i];
                    DumpGameObject(gameObject, writer, "/", "");
                }
            }

            Debug.LogFormat("DumpSceneHierarchy in file: {0}", filename);
        }

        private static void DumpGameObject(GameObject gameObject, StreamWriter writer, string indent, string parentName)
        {
            if (gameObject.transform.childCount > 0)
            {
                if (parentName != "")
                {
                    if (!parentName.EndsWith("/"))
                        parentName = parentName + "/";
                }
                parentName = parentName + gameObject.name;
            }
            else
            {
                parentName = parentName + gameObject.name;
            }

            if (parentName.EndsWith("/"))
            {
                writer.WriteLine(parentName + gameObject.name);
            }
            else
            {
                writer.WriteLine(parentName);
            }

            foreach (Transform child in gameObject.transform)
            {
                if (!parentName.EndsWith("/"))
                    parentName = parentName + "/";

                DumpGameObject(child.gameObject, writer, indent, parentName);
            }
        }
    }
}
