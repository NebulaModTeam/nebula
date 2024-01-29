#region

using NebulaModel.Networking;
using NebulaWorld;
using UnityEngine;

#endregion

namespace NebulaPatcher.MonoBehaviours;

public class NebulaBootstrapper : MonoBehaviour
{
    private void Awake()
    {
        DontDestroyOnLoad(gameObject);

        // This makes sure that even if the game is minimized, it will still receive and send packets
        Application.runInBackground = true;
        // make sure chat window starts out closed
        //InGameChatAssetLoader.ChatManager()?.Toggle(forceClosed: true);
    }

    private void LateUpdate()
    {
        if (Multiplayer.IsActive)
        {
            (Multiplayer.Session.Network as IServer)?.Update();
            (Multiplayer.Session.Network as IClient)?.Update();
        }
    }
}
