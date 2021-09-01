using HarmonyLib;
using NebulaModel;
using NebulaWorld;

namespace NebulaPatcher.Patches.Dynamic
{
    [HarmonyPatch(typeof(PowerSystemRenderer))]
    class PowerSystemRenderer_Patch
    {
        [HarmonyPostfix]
        [HarmonyPatch(nameof(PowerSystemRenderer.Init))]
        public static void Init_Postfix()
        {
            if (!Multiplayer.IsActive || ((LocalPlayer)Multiplayer.Session.LocalPlayer).IsHost)
            {
                return;
            }

            PowerSystemRenderer.powerGraphOn = Config.Options.PowerGridEnabled;
        }
    }
}
