using HarmonyLib;
using NebulaWorld;
using NebulaWorld.Factory;
using System;
using UnityEngine;

namespace NebulaPatcher.Patches.Dynamic
{
    [HarmonyPatch(typeof(UIRealtimeTip))]
    class UIRealtimeTip_Patch
    {
        [HarmonyPrefix]
        [HarmonyPatch("Popup", new Type[] { typeof(string), typeof(bool), typeof(int) })]
        public static bool Popup_Prefix()
        {
            //Do not show popups if they are triggered remotely
            return !SimulatedWorld.Initialized || (!FactoryManager.EventFromServer && !FactoryManager.EventFromClient);
        }

        [HarmonyPrefix]
        [HarmonyPatch("Popup", new Type[] { typeof(string), typeof(Vector2), typeof(int) })]
        public static bool Popup_Prefix2()
        {
            //Do not show popups if they are triggered remotely
            return !SimulatedWorld.Initialized || (!FactoryManager.EventFromServer && !FactoryManager.EventFromClient);
        }

        [HarmonyPrefix]
        [HarmonyPatch("Popup", new Type[] { typeof(string), typeof(Vector3), typeof(bool), typeof(int) })]
        public static bool Popup_Prefix3()
        {
            //Do not show popups if they are triggered remotely
            return !SimulatedWorld.Initialized || (!FactoryManager.EventFromServer && !FactoryManager.EventFromClient);
        }
    }
}
