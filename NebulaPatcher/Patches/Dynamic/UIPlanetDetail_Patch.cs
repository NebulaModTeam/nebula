using HarmonyLib;
using NebulaAPI;
using NebulaModel.Packets.Universe;
using NebulaWorld;
using UnityEngine;

namespace NebulaPatcher.Patches.Dynamic
{
    [HarmonyPatch(typeof(UIPlanetDetail))]
    internal class UIPlanetDetail_Patch
    {
        [HarmonyPostfix]
        [HarmonyPatch(nameof(UIPlanetDetail.OnNameInputEndEdit))]
        public static void OnNameInputEndEdit_Postfix(UIPlanetDetail __instance)
        {
            if (Multiplayer.IsActive && !Multiplayer.Session.Factories.IsIncomingRequest.Value)
            {
                if (__instance.planet != null && !string.IsNullOrEmpty(__instance.planet.overrideName))
                {
                    // Send packet with new planet name
                    Multiplayer.Session.Network.SendPacket(new NameInputPacket(__instance.planet.overrideName, NebulaModAPI.STAR_NONE, __instance.planet.id, Multiplayer.Session.LocalPlayer.Id));
                }
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(UIPlanetDetail._OnUpdate))]
        public static bool _OnUpdate_Prefix(UIPlanetDetail __instance)
        {
            if(UIRoot.instance.galaxySelect == null || UIRoot.instance.galaxySelect.starmap == null || (Multiplayer.Session != null && !Multiplayer.Session.IsInLobby) || Multiplayer.Session == null)
            {
                return true;
            }

            if (Time.frameCount % 30 == 0)
            {
                __instance.RefreshDynamicProperties();
            }
            __instance.trslBg.gameObject.SetActive(true);
            __instance.imgBg.gameObject.SetActive(true);

            for(int i = 0; i < __instance.entries.Count; i++)
            {
                if((i < 6 || i > 9) && !__instance.entries[i].valueString.Contains("-") && (!__instance.entries[i].valueString.Equals("0") && __instance.entries[i].valueString.Contains(",")))
                {
                    __instance.entries[i].valueString = "exists";
                }
            }

            return false;
        }

        // temp set universe exploration to max for planet detail display in galaxy select screen
        [HarmonyPrefix]
        [HarmonyPatch(nameof(UIPlanetDetail.OnPlanetDataSet))]
        [HarmonyPatch(nameof(UIPlanetDetail.RefreshDynamicProperties))]
        public static bool OnPlanetDataSet_Prefix(UIPlanetDetail __instance)
        {
            if(Multiplayer.IsActive && Multiplayer.Session.IsInLobby)
            {
                GameMain.history.universeObserveLevel = 3;
            }
            return true;
        }

        [HarmonyPostfix]
        [HarmonyPatch(nameof(UIPlanetDetail.OnPlanetDataSet))]
        [HarmonyPatch(nameof(UIPlanetDetail.RefreshDynamicProperties))]
        public static void OnPlanetDataSet_Postfix(UIPlanetDetail __instance)
        {
            if(Multiplayer.IsActive && Multiplayer.Session.IsInLobby)
            {
                GameMain.history.universeObserveLevel = GetUniverseObserveLevel();
            }
        }

        private static int GetUniverseObserveLevel()
        {
            int level = 0;
            // the tech ids of the 4 tiers of Universe Exploration from https://dsp-wiki.com/Upgrades
            for (int i = 4104; i >= 4101; i--)
            {
                if(GameMain.history.TechUnlocked(i))
                {
                    // set level to last digit of tech id - 1
                    level = (i % 10) - 1;
                    break;
                }
            }
            return level;
        }
    }
}
