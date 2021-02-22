using UnityEngine;
using NebulaModel.Logger;

namespace NebulaClient.MonoBehaviours
{
    public class NebulaBootstrapper : MonoBehaviour
    {
        internal static NebulaBootstrapper Instance;

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
            Instance = this;
            // gameObject.AddComponent<SceneCleanerPreserve>();

#if DEBUG
            EnableDeveloperFeatures();
#endif

            CreateDebugger();
        }

        private void EnableDeveloperFeatures()
        {
            Log.Info("Enabling developer console.");
            // DevConsole.disableConsole = false;
            Application.runInBackground = true;
            Log.Info($"Unity run in background set to \"{Application.runInBackground}\"");
        }

        private void CreateDebugger()
        {
            GameObject debugger = new GameObject();
            debugger.name = "Nebula - Debug manager";
            // debugger.AddComponent<NebulaDebugManager>();
            debugger.transform.SetParent(transform);
        }
    }
}
