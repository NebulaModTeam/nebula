using HarmonyLib;
using NebulaModel.Packets.Universe;
using NebulaWorld;
using NebulaWorld.Factory;

namespace NebulaPatcher.Patches.Dynamic
{
    [HarmonyPatch(typeof(UIStarDetail))]
    class UIStarDetail_Patch
    {
        [HarmonyPostfix]
        [HarmonyPatch(nameof(UIStarDetail.OnNameInputEndEdit))]
        public static void OnNameInputEndEdit_Postfix(UIStarDetail __instance)
        {
            if (SimulatedWorld.Initialized && !FactoryManager.IsIncomingRequest)
            {
                if (__instance.star != null && !string.IsNullOrEmpty(__instance.star.overrideName))
                {
                    // Send packet with new star name
                    LocalPlayer.SendPacket(new NameInputPacket(__instance.star.overrideName, __instance.star.id, FactoryManager.PLANET_NONE, LocalPlayer.PlayerId));
                }
            }
        }
    }
}
