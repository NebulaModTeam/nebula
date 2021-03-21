using HarmonyLib;
using NebulaHost;
using System;
using UnityEngine;

namespace NebulaPatcher.Patches.Dynamic
{
    [HarmonyPatch(typeof(ComputeBuffer), "CopyCount")]
    class ComputeBuffer_Patch
    {
        static bool Prefix()
        {
            //This methods prevents computing shaders in dedicated mode
            return !MultiplayerHostSession.IsDedicated;
        }
    }
}
