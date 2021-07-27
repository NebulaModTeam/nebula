﻿using HarmonyLib;
using NebulaModel;
using NebulaWorld;

namespace NebulaPatcher.Patches.Dynamic
{
    [HarmonyPatch(typeof(PowerSystemRenderer))]
    class PowerSystemRenderer_Patch
    {
        [HarmonyPostfix]
        [HarmonyPatch("Init")]
        public static void Init_Postfix(UIGameMenu __instance)
        {
            if (!SimulatedWorld.Initialized || LocalPlayer.IsMasterClient)
            {
                return;
            }

            AccessTools.StaticFieldRefAccess<bool>(typeof(PowerSystemRenderer), "powerGraphOn") = Config.Options.PowerGridEnabled;
        }
    }
}
