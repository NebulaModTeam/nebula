using Mirror;
using System;
using UnityEngine;

namespace NebulaNetwork
{
    class MirrorManager
    {
        public static NetworkManager SetupMirror(Type networkManagerType, string ip = "localhost", ushort port = 0)
        {
            const int MaxMessageSize = 30 * 1024 * 1024; // 30 MB
            GameObject mirrorRoot = new GameObject();
            mirrorRoot.SetActive(false);
            mirrorRoot.name = "Mirror Networking";
            NetworkManager NetworkManager = mirrorRoot.AddComponent(networkManagerType) as NetworkManager;
            NetworkManager.autoCreatePlayer = false;
            NetworkManager.networkAddress = ip;
#if DEBUG
            mirrorRoot.AddComponent(typeof(NetworkManagerHUD));
#endif

            // Telepathy
            TelepathyTransport telepathy = mirrorRoot.AddComponent<TelepathyTransport>();
            telepathy.clientMaxMessageSize = MaxMessageSize;
            telepathy.serverMaxMessageSize = MaxMessageSize;
            telepathy.port = port != 0 ? port : telepathy.port;

            mirrorRoot.SetActive(true);
            Transport.activeTransport = telepathy;

            return NetworkManager;
        }
    }
}
