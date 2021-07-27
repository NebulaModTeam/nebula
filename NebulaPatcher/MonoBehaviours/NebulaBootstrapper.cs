using NebulaNetwork;
using NebulaModel.Logger;
using UnityEngine;

namespace NebulaPatcher.MonoBehaviours
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
        }

        private void EnableDeveloperFeatures()
        {
            Log.Info("Enabling developer console.");
            // DevConsole.disableConsole = false;
            Application.runInBackground = true;
            Log.Info($"Unity run in background set to \"{Application.runInBackground}\"");
        }

        public MultiplayerHostSession CreateMultiplayerHostSession()
        {
            GameObject go = new GameObject();
            go.transform.SetParent(transform);
            go.name = "Nebula - Multiplayer Host Session";
            return go.AddComponent<MultiplayerHostSession>();
        }

        public MultiplayerClientSession CreateMultiplayerClientSession()
        {
            GameObject go = new GameObject();
            go.transform.SetParent(transform);
            go.name = "Nebula - Multiplayer Client Session";
            return go.AddComponent<MultiplayerClientSession>();
        }
    }
}
