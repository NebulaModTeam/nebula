using HarmonyLib;
using NebulaModel.Packets.PowerSystem;
using NebulaWorld;
using System.Collections.Generic;
using UnityEngine;

namespace NebulaPatcher.Patches.Dynamic
{
    [HarmonyPatch(typeof(PowerSystem))]
    internal class PowerSystem_Patch
    {
        private const float REQUEST_INTERVAL = 1;
        private static float timePassed;

        [HarmonyPrefix]
        [HarmonyPatch(nameof(PowerSystem.GameTick))]
        public static bool PowerSystem_GameTick_Prefix(PowerSystem __instance, long time, bool isActive, bool isMultithreadMode)
        {
            if (Multiplayer.IsActive && Multiplayer.Session.LocalPlayer.IsClient)
            {
                // if player is not in a solar system he has no factories loaded, thus nothing to sync here
                if(GameMain.localStar == null)
                {
                    return false;
                }

                timePassed += Time.deltaTime;

                if(timePassed >= REQUEST_INTERVAL)
                {
                    timePassed = 0;

                    List<int> pIDs = new List<int>();
                    for(int i = 0; i < GameMain.localStar.planetCount; i++)
                    {
                        if(GameMain.localStar.planets[i]?.factory != null)
                        {
                            pIDs.Add(GameMain.localStar.planets[i].id);
                        }
                    }

                    Multiplayer.Session.Network.SendPacket(new PowerSystemUpdateRequest(pIDs.ToArray()));
                }

                return false;
            }
            return true;
        }
    }
}
