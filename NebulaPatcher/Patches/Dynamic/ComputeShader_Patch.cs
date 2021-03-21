using HarmonyLib;
using NebulaHost;
using System;
using UnityEngine;

namespace NebulaPatcher.Patches.Dynamic
{
    [HarmonyPatch(typeof(ComputeShader))]
    class ComputeShader_Patch
    {
        //This methods prevents computing shaders in dedicated mode 
        [HarmonyPrefix]
        [HarmonyPatch("FindKernel")]
        static bool Prefix1(ref int __result)
        {
            if (MultiplayerHostSession.IsDedicated)
            {
                __result = 0;
                return false;
            }
            return true;
        }

        [HarmonyPrefix]
        [HarmonyPatch("SetBuffer", new Type[] { typeof(int), typeof(string), typeof(ComputeBuffer) })]
        static bool Prefix2()
        {
            return !MultiplayerHostSession.IsDedicated;
        }

        [HarmonyPrefix]
        [HarmonyPatch("SetBuffer", new Type[] { typeof(int), typeof(int), typeof(ComputeBuffer) })]
        static bool Prefix3()
        {
            return !MultiplayerHostSession.IsDedicated;
        }
        [HarmonyPrefix]
        [HarmonyPatch("SetFloat", new Type[] { typeof(int), typeof(float) })]
        static bool Prefix4()
        {
            return !MultiplayerHostSession.IsDedicated;
        }
        [HarmonyPrefix]
        [HarmonyPatch("SetInt", new Type[] { typeof(int), typeof(int) })]
        static bool Prefix5()
        {
            return !MultiplayerHostSession.IsDedicated;
        }
        [HarmonyPrefix]
        [HarmonyPatch("GetKernelThreadGroupSizes")]
        static bool Prefix6()
        {
            return !MultiplayerHostSession.IsDedicated;
        }
    }
} 
