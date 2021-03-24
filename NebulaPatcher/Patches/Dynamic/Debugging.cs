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
            __instance.maxSailSpeed = 10000f;
            __instance.maxWarpSpeed = 1000000f;
            __instance.walkSpeed = 25f;
        }
    }
#endif
}
