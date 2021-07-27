#if DEBUG
using HarmonyLib;

namespace NebulaPatcher.Patches.Dynamic
{
    [HarmonyPatch(typeof(GameHistoryData))]
    class Debug_GameHistoryData_Patch
    {
        [HarmonyPostfix]
        [HarmonyPatch(nameof(GameHistoryData.EnqueueTech))]
        public static void EnqueueTech_Postfix(GameHistoryData __instance, int techId)
        {
            __instance.UnlockTech(techId);
            GameMain.mainPlayer.mecha.corePowerGen = 10000000;
        }

        [HarmonyPrefix]
        [HarmonyPatch("dysonSphereSystemUnlocked", MethodType.Getter)]
        public static bool DysonSphereSystemUnlocked_Prefix(GameHistoryData __instance, ref bool __result)
        {
            __result = true;
            return false;
        }

        [HarmonyPostfix]
        [HarmonyPatch(nameof(GameHistoryData.SetForNewGame))]
        public static void SetForNewGame_Postfix(GameHistoryData __instance)
        {
            __instance.dysonNodeLatitude = 90f;
        }
    }

    [HarmonyPatch(typeof(Mecha))]
    class Debug_Mecha_Patch
    {
        [HarmonyPostfix]
        [HarmonyPatch(nameof(Mecha.UseWarper))]
        public static void UseWarper_Postfix(ref bool __result)
        {
            __result = true;
        }

        [HarmonyPostfix]
        [HarmonyPatch(nameof(Mecha.SetForNewGame))]
        public static void SetForNewGame_Postfix(Mecha __instance)
        {
            __instance.coreEnergyCap = 30000000000;
            __instance.coreEnergy = 30000000000;
            __instance.corePowerGen = 5000000;
            __instance.reactorPowerGen = 20000000;
            __instance.coreLevel = 5;
            __instance.thrusterLevel = 5;
            __instance.maxSailSpeed = 2000f;
            __instance.maxWarpSpeed = 1000000f;
            __instance.walkSpeed = 25f;
            __instance.player.package.AddItemStacked(1803, 40); //add antimatter
            __instance.player.package.AddItemStacked(1501, 600); //add sails
            __instance.player.package.AddItemStacked(1503, 60); //add rockets
            __instance.player.package.AddItemStacked(2312, 10); //add launching silo
            __instance.player.package.AddItemStacked(2210, 10); //add artifical sun
            __instance.player.package.AddItemStacked(2311, 20); //add railgun
            __instance.player.package.AddItemStacked(2001, 600); //add MK3 belts
            __instance.player.package.AddItemStacked(2002, 600); //add MK3 belts
            __instance.player.package.AddItemStacked(2003, 600); //add MK3 belts
            __instance.player.package.AddItemStacked(2013, 100); //add MK3 inserters
            __instance.player.package.AddItemStacked(2212, 20); //add satelite sub-station
        }
    }

    [HarmonyPatch(typeof(MechaForge))]
    class Debug_MechaForge_Patch
    {
        [HarmonyPostfix]
        [HarmonyPatch(nameof(MechaForge.TryAddTask))]
        public static void TryAddTask_Postfix(ref bool __result)
        {
            __result = true;
        }

        [HarmonyPrefix]
        [HarmonyPatch("AddTaskIterate")]
        public static bool AddTaskIterate_Prefix(MechaForge __instance, ForgeTask __result, int recipeId, int count)
        {
            ForgeTask recipe = new ForgeTask(recipeId, count);
            for (int i = 0; i < recipe.productIds.Length; i++)
            {
                GameMain.mainPlayer.package.AddItemStacked(recipe.productIds[i], count);
            }
            __result = null;
            return false;
        }
    }

    [HarmonyPatch(typeof(UIAdvisorTip))]
    class Debug_UIAdvisorTip_Patch
    {
        [HarmonyPrefix]
        [HarmonyPatch("PlayAdvisorTip")]
        public static bool PlayAdvisorTip_Prefix()
        {
            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch("RunAdvisorTip")]
        public static bool RunAdvisorTip_Prefix()
        {
            return false;
        }
    }

    [HarmonyPatch(typeof(UITutorialTip))]
    class Debug_UITutorialTip_Patch
    {
        [HarmonyPrefix]
        [HarmonyPatch("PopupTutorialTip")]
        public static bool PlayAdvisorTip_Prefix()
        {
            return false;
        }
    }
}
#endif
