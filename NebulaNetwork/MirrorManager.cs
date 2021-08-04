using Mirror;
using System;
using UnityEngine;

namespace NebulaNetwork
{
    class MirrorManager
    {
        public static NetworkManager SetupMirror(Type networkManagerType)
        {
            const int MaxMessageSize = 30 * 1024 * 1024; // 30 MB
            GameObject mirrorRoot = new GameObject();
            mirrorRoot.SetActive(false);
            mirrorRoot.name = "Mirror Networking";
            NetworkManager NetworkManager = mirrorRoot.AddComponent(networkManagerType) as NetworkManager;
            NetworkManager.autoCreatePlayer = false;
            mirrorRoot.AddComponent(typeof(NetworkManagerHUD));

            // Telepathy
            TelepathyTransport telepathy = mirrorRoot.AddComponent<TelepathyTransport>();
            telepathy.clientMaxMessageSize = MaxMessageSize;
            telepathy.serverMaxMessageSize = MaxMessageSize;

            // KCP
            kcp2k.KcpTransport kcp = mirrorRoot.AddComponent<kcp2k.KcpTransport>();
            kcp.SendWindowSize = MaxMessageSize;
            kcp.ReceiveWindowSize = MaxMessageSize;

            mirrorRoot.SetActive(true);
            Transport.activeTransport = telepathy;

            return NetworkManager;
        }
    }
}
