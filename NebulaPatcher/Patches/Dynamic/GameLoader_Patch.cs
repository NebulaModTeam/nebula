using HarmonyLib;
using NebulaModel.Logger;
using System;
using NebulaHost;
using NebulaWorld;

namespace NebulaPatcher.Patches.Dynamic
{
    [HarmonyPatch(typeof(GameLoader), "FixedUpdate")]
    class GameLoader_Patch
    {
        public static void Postfix(int ___frame)
        {
            Log.Info(___frame);
            if(___frame >= 11 && SimulatedWorld.Initialized)
            {
                SimulatedWorld.OnGameLoadCompleted();
            }
        }
    }
}
