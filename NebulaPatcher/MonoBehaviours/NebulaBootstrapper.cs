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

            // This makes sure that even if the game is minimized, it will still receive and send packets
            Application.runInBackground = true;
        }
    }
}
