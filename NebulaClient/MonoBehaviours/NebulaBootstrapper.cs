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

#if DEBUG
            EnableDeveloperFeatures();
#endif

            CreateMultiplayer();
        }

        private void EnableDeveloperFeatures()
        {
            Log.Info("Enabling developer console.");
            // DevConsole.disableConsole = false;
            Application.runInBackground = true;
            Log.Info($"Unity run in background set to \"{Application.runInBackground}\"");
        }

        private void CreateMultiplayer()
        {
            GameObject go = new GameObject();
            go.transform.SetParent(transform);
            go.name = "Nebula - Multiplayer";
            go.AddComponent<MultiplayerSession>();
        }
    }
}
