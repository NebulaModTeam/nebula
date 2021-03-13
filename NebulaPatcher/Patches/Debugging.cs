using HarmonyLib;

namespace NebulaPatcher.Patches
{
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
}
