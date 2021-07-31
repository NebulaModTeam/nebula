using Mirror;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace NebulaPatcher.MonoBehaviours
{
    public class MirrorBootstrapper : MonoBehaviour
    {
        internal static MirrorBootstrapper instance;

        private void Awake()
        {
            instance = this;

            gameObject.AddComponent(typeof(NetworkManager));
            gameObject.AddComponent(typeof(kcp2k.KcpTransport));
            gameObject.AddComponent(typeof(NetworkManagerHUD));
        }
    }
}
