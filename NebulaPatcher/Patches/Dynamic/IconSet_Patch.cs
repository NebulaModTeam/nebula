using HarmonyLib;
using NebulaWorld.Chat;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.TextCore;

namespace NebulaPatcher.Patches.Dynamic
{
    [HarmonyPatch(typeof(IconSet))]
    public static class IconSet_Patch
    {
        [HarmonyPatch(nameof(IconSet.Create))]
        [HarmonyPostfix]
        public static void Create_Postfix(IconSet __instance)
        {
            ChatRichTextManager.Create(__instance);
        }
    }
}