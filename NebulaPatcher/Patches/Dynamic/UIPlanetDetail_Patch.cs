using HarmonyLib;
using NebulaAPI;
using NebulaModel.Packets.Universe;
using NebulaWorld;
using NebulaWorld.Factory;

namespace NebulaPatcher.Patches.Dynamic
{
    [HarmonyPatch(typeof(UIPlanetDetail))]
    class UIPlanetDetail_Patch
    {
        [HarmonyPostfix]
        [HarmonyPatch(nameof(UIPlanetDetail.OnNameInputEndEdit))]
        public static void OnNameInputEndEdit_Postfix(UIPlanetDetail __instance)
        {
            if (SimulatedWorld.Instance.Initialized && !FactoryManager.Instance.IsIncomingRequest.Value)
            {
                if (__instance.planet != null && !string.IsNullOrEmpty(__instance.planet.overrideName))
                {
                    // Send packet with new planet name
                    LocalPlayer.Instance.SendPacket(new NameInputPacket(__instance.planet.overrideName, NebulaModAPI.STAR_NONE, __instance.planet.id, LocalPlayer.Instance.PlayerId));
                }
            }
        }
    }
}
