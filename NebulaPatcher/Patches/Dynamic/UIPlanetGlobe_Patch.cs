﻿using HarmonyLib;
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
            if (SimulatedWorld.Initialized && !FactoryManager.IsIncomingRequest)
            {
                if (GameMain.localPlanet != null && !string.IsNullOrEmpty(GameMain.localPlanet.overrideName))
                {
                    // Send packet with new planet name
                    LocalPlayer.SendPacket(new NameInputPacket(GameMain.localPlanet.overrideName, FactoryManager.STAR_NONE, GameMain.localPlanet.id, LocalPlayer.PlayerId));
                }
                else if (GameMain.localStar != null && !string.IsNullOrEmpty(GameMain.localStar.overrideName))
                {
                    // Send packet with new star name
                    LocalPlayer.SendPacket(new NameInputPacket(GameMain.localStar.overrideName, GameMain.localStar.id, FactoryManager.PLANET_NONE, LocalPlayer.PlayerId));
                }
            }
        }
    }
}
