using HarmonyLib;
using UnityEngine;

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
            for(int i = 0; i < recipe.productIds.Length; i++)
            {
                GameMain.mainPlayer.package.AddItemStacked(recipe.productIds[i], count);
            }
            //__instance.tasks.Add(recipe);
            __result = null;
            return false;
        }
    }
#endif
}
