#region

using HarmonyLib;
using NebulaModel;
using NebulaWorld;

#endregion

namespace NebulaPatcher.Patches.Dynamic;

[HarmonyPatch(typeof(PowerSystemRenderer))]
internal class PowerSystemRenderer_Patch
{
    [HarmonyPostfix]
    [HarmonyPatch(nameof(PowerSystemRenderer.Init))]
    public static void Init_Postfix()
    {
        if (!Multiplayer.IsActive || Multiplayer.Session.LocalPlayer.IsHost)
        {
            return;
        }

        PowerSystemRenderer.powerGraphOn = Config.Options.PowerGridEnabled;
    }
}
