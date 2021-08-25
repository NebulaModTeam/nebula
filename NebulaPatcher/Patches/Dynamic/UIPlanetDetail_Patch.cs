using HarmonyLib;
using NebulaModel.Packets.Universe;
using NebulaWorld;
using NebulaWorld.Factory;

namespace NebulaPatcher.Patches.Dynamic
{
    [HarmonyPatch(typeof(UIPlanetDetail))]
    class UIPlanetDetail_Patch
    {
        [HarmonyPostfix]
        [HarmonyPatch("OnNameInputEndEdit")]
        public static void OnNameInputEndEdit_Postfix(UIPlanetDetail __instance)
        {
            if (Multiplayer.IsActive && !Multiplayer.Session.Factories.IsIncomingRequest)
            {
                if (__instance.planet != null && !string.IsNullOrEmpty(__instance.planet.overrideName))
                {
                    // Send packet with new planet name
                    LocalPlayer.SendPacket(new NameInputPacket(__instance.planet.overrideName, FactoryManager.STAR_NONE, __instance.planet.id, LocalPlayer.PlayerId));
                }
            }
        }
    }
}
