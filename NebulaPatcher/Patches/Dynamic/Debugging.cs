using HarmonyLib;

namespace NebulaPatcher.Patches.Dynamic
{
#if DEBUG

    [HarmonyPatch(typeof(GameHistoryData), "EnqueueTech")]
    class patch
    {
        public static void Postfix(GameHistoryData __instance, int techId)
        {
            __instance.UnlockTech(techId);
            GameMain.mainPlayer.mecha.corePowerGen = 10000000;
        }
    }

    [HarmonyPatch(typeof(Mecha), "UseWarper")]
    class patch2
    {
        public static void Postfix(ref bool __result)
        {
            __result = true;
        }
    }

    [HarmonyPatch(typeof(MechaForge), "TryAddTask")]
    class patch3
    {
        public static void Postfix(ref bool __result)
        {
            __result = true;
        }
    }

    [HarmonyPatch(typeof(MechaForge), "AddTaskIterate")]
    class patch4
    {
        public static bool Prefix(MechaForge __instance, ForgeTask __result, int recipeId, int count)
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

    [HarmonyPatch(typeof(Mecha), "SetForNewGame")]
    class patch5
    {
        public static void Postfix(Mecha __instance)
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

    [HarmonyPatch(typeof(GameHistoryData), "dysonSphereSystemUnlocked", MethodType.Getter)]
    class patch6
    {
        public static bool Prefix(GameHistoryData __instance, ref bool __result)
        {
            __result = true;
            return false;
        }
    }

    [HarmonyPatch(typeof(GameHistoryData), "SetForNewGame")]
    class patch7
    {
        public static void Postfix(GameHistoryData __instance)
        {
            __instance.dysonNodeLatitude = 90f;
        }
    }

    [HarmonyPatch(typeof(UIAdvisorTip), "PlayAdvisorTip")]
    class patch8
    {
        public static bool Prefix()
        {
            return false;
        }
    }

    [HarmonyPatch(typeof(UIAdvisorTip), "RunAdvisorTip")]
    class patch9
    {
        public static bool Prefix()
        {
            return false;
        }
    }

    [HarmonyPatch(typeof(UITutorialTip), "PopupTutorialTip")]
    class patch10
    {
        public static bool Prefix()
        {
            return false;
        }
    }
#endif
}
