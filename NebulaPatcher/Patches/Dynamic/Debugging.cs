﻿#if DEBUG
using HarmonyLib;

namespace NebulaPatcher.Patches.Dynamic
{
    [HarmonyPatch(typeof(GameHistoryData))]
    internal class Debug_GameHistoryData_Patch
    {
        [HarmonyPostfix]
        [HarmonyPatch(nameof(GameHistoryData.EnqueueTech))]
        public static void EnqueueTech_Postfix(GameHistoryData __instance, int techId)
        {
            __instance.UnlockTech(techId);
            GameMain.mainPlayer.mecha.corePowerGen = 10000000;
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(GameHistoryData.dysonSphereSystemUnlocked), MethodType.Getter)]
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
    internal class Debug_Mecha_Patch
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
            int dummyOut;
            __instance.coreEnergyCap = 30000000000;
            __instance.coreEnergy = 30000000000;
            __instance.corePowerGen = 5000000;
            __instance.reactorPowerGen = 20000000;
            __instance.coreLevel = 5;
            __instance.thrusterLevel = 5;
            __instance.maxSailSpeed = 2000f;
            __instance.maxWarpSpeed = 1000000f;
            __instance.walkSpeed = 25f;
            __instance.player.package.AddItemStacked(1803, 40, 1, out dummyOut); //add antimatter
            __instance.player.package.AddItemStacked(1501, 600, 1, out dummyOut); //add sails
            __instance.player.package.AddItemStacked(1503, 60, 1, out dummyOut); //add rockets
            __instance.player.package.AddItemStacked(2312, 10, 1, out dummyOut); //add launching silo
            __instance.player.package.AddItemStacked(2210, 10, 1, out dummyOut); //add artifical sun
            __instance.player.package.AddItemStacked(2311, 20, 1, out dummyOut); //add railgun
            __instance.player.package.AddItemStacked(2001, 600, 1, out dummyOut); //add MK3 belts
            __instance.player.package.AddItemStacked(2002, 600, 1, out dummyOut); //add MK3 belts
            __instance.player.package.AddItemStacked(2003, 600, 1, out dummyOut); //add MK3 belts
            __instance.player.package.AddItemStacked(2013, 100, 1, out dummyOut); //add MK3 inserters
            __instance.player.package.AddItemStacked(2212, 20, 1, out dummyOut); //add satelite sub-station
        }
    }

    [HarmonyPatch(typeof(MechaForge))]
    internal class Debug_MechaForge_Patch
    {
        [HarmonyPostfix]
        [HarmonyPatch(nameof(MechaForge.TryAddTask))]
        public static void TryAddTask_Postfix(ref bool __result)
        {
            __result = true;
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(MechaForge.AddTaskIterate))]
        public static bool AddTaskIterate_Prefix(MechaForge __instance, ForgeTask __result, int recipeId, int count)
        {
            ForgeTask recipe = new ForgeTask(recipeId, count);
            int dummyOut;
            for (int i = 0; i < recipe.productIds.Length; i++)
            {
                GameMain.mainPlayer.package.AddItemStacked(recipe.productIds[i], count, 1, out dummyOut);
            }
            __result = null;
            return false;
        }
    }

    [HarmonyPatch(typeof(UIAdvisorTip))]
    internal class Debug_UIAdvisorTip_Patch
    {
        [HarmonyPrefix]
        [HarmonyPatch(nameof(UIAdvisorTip.PlayAdvisorTip))]
        public static bool PlayAdvisorTip_Prefix()
        {
            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(UIAdvisorTip.RunAdvisorTip))]
        public static bool RunAdvisorTip_Prefix()
        {
            return false;
        }
    }

    [HarmonyPatch(typeof(UITutorialTip))]
    internal class Debug_UITutorialTip_Patch
    {
        [HarmonyPrefix]
        [HarmonyPatch(nameof(UITutorialTip.PopupTutorialTip))]
        public static bool PopupTutorialTip_Prefix()
        {
            return false;
        }
    }

    [HarmonyPatch(typeof(UIMechaEditor))]
    internal class Debug_UIMechaEditor_Patch
    {
        [HarmonyPrefix]
        [HarmonyPatch(nameof(UIMechaEditor.ApplyMechaAppearance))]
        public static bool ApplyMechaAppearance_Prefix(UIMechaEditor __instance)
        {
            __instance.mecha.diyAppearance.CopyTo(__instance.mecha.appearance);
            __instance.player.mechaArmorModel.RefreshAllPartObjects();
            __instance.player.mechaArmorModel.RefreshAllBoneObjects();
            __instance.mecha.appearance.NotifyAllEvents();
            __instance.CalcMechaProperty();

            return false;
        }
    }

    [HarmonyPatch(typeof(UIMechaMatsGroup))]
    internal class Debug_UIMechaMatsGroup_Patch
    {
        [HarmonyPrefix]
        [HarmonyPatch(nameof(UIMechaMatsGroup.TestMaterialEnough))]
        public static bool TestMaterialsEnough_Prefix(UIMechaMatsGroup __instance, ref bool __result)
        {
            __result = true;
            return false;
        }
    }
}
#endif
