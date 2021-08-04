using Mirror;
using System;
using UnityEngine;

namespace NebulaNetwork
{
    class MirrorManager
    {
        public static NetworkManager SetupMirror(Type networkManagerType)
        {
            GameObject mirrorRoot = new GameObject();
            mirrorRoot.SetActive(false);
            mirrorRoot.name = "Mirror Networking";
            NetworkManager NetworkManager = mirrorRoot.AddComponent(networkManagerType) as NetworkManager;
            NetworkManager.autoCreatePlayer = false;
            TelepathyTransport telepathy = (TelepathyTransport)mirrorRoot.AddComponent(typeof(TelepathyTransport));
            telepathy.clientMaxMessageSize = 30 * 1024 * 1024;
            telepathy.serverMaxMessageSize = 30 * 1024 * 1024;
            mirrorRoot.AddComponent(typeof(NetworkManagerHUD));
            mirrorRoot.SetActive(true);
            Transport.activeTransport = telepathy;

            return NetworkManager;
        }
    }
}
