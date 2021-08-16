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
        [HarmonyPatch("OnNameInputEndEdit")]
        public static void OnNameInputEndEdit_Postfix(UIStarDetail __instance)
        {
            if (SimulatedWorld.Initialized && !FactoryManager.Instance.IsIncomingRequest.Value)
            {
                if (__instance.star != null && !string.IsNullOrEmpty(__instance.star.overrideName))
                {
                    // Send packet with new star name
                    LocalPlayer.Instance.SendPacket(new NameInputPacket(__instance.star.overrideName, __instance.star.id, FactoryManager.Instance.PLANET_NONE, LocalPlayer.Instance.PlayerId));
                }
            }
        }
    }
}
