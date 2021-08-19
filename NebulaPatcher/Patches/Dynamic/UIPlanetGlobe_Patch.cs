using HarmonyLib;
using NebulaAPI;
using NebulaModel.Packets.Universe;
using NebulaWorld;
using NebulaWorld.Factory;

namespace NebulaPatcher.Patches.Dynamic
{
    [HarmonyPatch(typeof(UIPlanetGlobe))]
    class UIPlanetGlobe_Patch
    {
        [HarmonyPostfix]
        [HarmonyPatch("OnNameInputEndEdit")]
        public static void OnNameInputEndEdit_Postfix()
        {
            if (SimulatedWorld.Instance.Initialized && !FactoryManager.Instance.IsIncomingRequest.Value)
            {
                if (GameMain.localPlanet != null && !string.IsNullOrEmpty(GameMain.localPlanet.overrideName))
                {
                    // Send packet with new planet name
                    LocalPlayer.Instance.SendPacket(new NameInputPacket(GameMain.localPlanet.overrideName, NebulaModAPI.STAR_NONE, GameMain.localPlanet.id, LocalPlayer.Instance.PlayerId));
                }
                else if (GameMain.localStar != null && !string.IsNullOrEmpty(GameMain.localStar.overrideName))
                {
                    // Send packet with new star name
                    LocalPlayer.Instance.SendPacket(new NameInputPacket(GameMain.localStar.overrideName, GameMain.localStar.id, NebulaModAPI.PLANET_NONE, LocalPlayer.Instance.PlayerId));
                }
            }
        }
    }
}
