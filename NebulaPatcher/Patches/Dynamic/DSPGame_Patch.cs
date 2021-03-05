using HarmonyLib;
using NebulaModel.Logger;
using UnityEngine;

namespace NebulaPatcher.Patches.Dynamic
{
    [HarmonyPatch(typeof(DSPGame), "Update")]
    class DSPGame_Patch
    {
        public static void Postfix()
        {
            //Use this function if you need to check/do something every frame
        }
    }
}
