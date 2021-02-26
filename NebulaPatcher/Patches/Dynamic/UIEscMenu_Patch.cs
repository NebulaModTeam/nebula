using HarmonyLib;
using NebulaClient.MonoBehaviours;
using UnityEngine;

namespace NebulaPatcher.Patches.Dynamic
{
    [HarmonyPatch(typeof(UIEscMenu), "OnButton5Click")]
    [HarmonyPatch(typeof(UIEscMenu), "OnButton6Click")]
    class UIEscMenu_Patch
    {
        public static void Prefix()
        {
            GameObject.FindObjectOfType<MultiplayerSession>()?.Disconnect();
        }
    }
}
