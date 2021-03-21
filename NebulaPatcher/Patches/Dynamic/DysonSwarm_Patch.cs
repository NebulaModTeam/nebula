using HarmonyLib;
using NebulaHost;
using System;
using UnityEngine;

namespace NebulaPatcher.Patches.Dynamic
{
    [HarmonyPatch(typeof(DysonSwarm))]
    class DysonSwarm_Patch
    {
        //This methods prevents updating shaders for DysonSwarm in dedicated mode
        [HarmonyPrefix]
        [HarmonyPatch("Dispatch_UpdatePos")]
        static bool Prefix1()
        {
            return !MultiplayerHostSession.IsDedicated;
        }
        [HarmonyPrefix]
        [HarmonyPatch("Dispatch_UpdateVel")]
        static bool Prefix2()
        {
            return !MultiplayerHostSession.IsDedicated;
        }
        [HarmonyPrefix]
        [HarmonyPatch("Dispatch_BlitBuffer")]
        static bool Prefix3()
        {
            return !MultiplayerHostSession.IsDedicated;
        }
        [HarmonyPrefix]
        [HarmonyPatch("Dispatch_AppendNear")]
        static bool Prefix4()
        {
            return !MultiplayerHostSession.IsDedicated;
        }
    }
}
