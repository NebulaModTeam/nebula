using HarmonyLib;
using NebulaWorld;

namespace NebulaPatcher.Patches.Dynamic
{
    [HarmonyPatch(typeof(Mecha))]
    class Mecha_Patch
    {
        [HarmonyPostfix]
        [HarmonyPatch("GameTick")]
        public static void GameTick_Postfix(long time, float dt)
        {
            if (SimulatedWorld.Initialized)
            {
                SimulatedWorld.OnDronesGameTick(time, dt);
            }
        }
    }
}
