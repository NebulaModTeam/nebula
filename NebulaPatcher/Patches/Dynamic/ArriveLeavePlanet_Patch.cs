using HarmonyLib;
using NebulaModel.Packets.Planet;
using NebulaWorld;
using UnityEngine;

using System.Security;
using System.Security.Permissions;

[module: UnverifiableCode]
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]

namespace NebulaPatcher.Patches.Dynamic
{
    [HarmonyPatch(typeof(GameData), "ArrivePlanet")]
    class ArrivePlanet_Patch
    {
        public static bool Prefix(GameData __instance, PlanetData planet)
        {
            // we need to supply our own ArrivePlanet() logic as we load the PlanetFactory from the server (if we are a client at least).
            // due to that we have a time window between the vanilla ArrivePlanet() setting the localPlanet and planetId values and
            // our code loading the factory data.
            // this results in weird planet jumps, so we need to delay this until the factory data is loaded into the game.
            if (!SimulatedWorld.Initialized || LocalPlayer.IsMasterClient)
            {
                // if we are the server continue with vanilla logic
                return true;
            }

            // it is very painfull to patch the skip prologue functionality
            // so i apply the patched logic only after that.
            // but we still need to use the original logic one time to send our localPlanet.id to the others.
            // here comes vanilla
            if (!LocalPlayer.FinishedGameLoad)
            {
                if (planet == __instance.localPlanet)
                {
                    return false;
                }
                if (__instance.localPlanet != null)
                {
                    __instance.LeavePlanet();
                }
                if (planet != null)
                {
                    if (__instance.localStar != planet.star)
                    {
                        __instance.ArriveStar(planet.star);
                    }
                    //typeof(GameData).GetField("localPlanet", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(__instance, planet);
                    __instance.localPlanet = planet;
                    __instance.mainPlayer.planetId = planet.id;
                    if (planet.loaded)
                    {
                        __instance.OnActivePlanetLoaded(planet);
                    }
                    else
                    {
                        planet.onLoaded += __instance.OnActivePlanetLoaded;
                    }
                }
                
                var packet = new localPlanetSyncPckt(__instance.localPlanet.id, false);
                packet.playerId = LocalPlayer.PlayerId;
                LocalPlayer.SendPacket(packet);

                return true;
            }
            else // we are past the prologue and initial landing so we can use the patched logic (delaying localPlanet and planetId)
            {

                if (planet == __instance.localPlanet)
                {
                    return false;
                }
                if (__instance.localPlanet != null)
                {
                    __instance.LeavePlanet();
                }
                if (planet != null && !planet.factoryLoading)
                {
                    if (__instance.localStar != planet.star)
                    {
                        __instance.ArriveStar(planet.star);
                    }
                    // skip setting local planet stuff
                    // NOTE: we also need to patch OnActivePlanetLoaded() as it relies on localPlanet but is needed to call LoadFactory() once the planet is loaded
                    if (planet.loaded)
                    {
                        __instance.OnActivePlanetLoaded(planet);
                    }
                    else
                    {
                        planet.onLoaded += __instance.OnActivePlanetLoaded;
                    }
                }
                return false;
            }
        }
    }

    [HarmonyPatch(typeof(GameData), "LeavePlanet")]
    class LeavePlanet_Patch
    {
        public static void Postfix(GameData __instance)
        {
            if (LocalPlayer.FinishedGameLoad)
            {
                var packet = new localPlanetSyncPckt(0, false);
                packet.playerId = LocalPlayer.PlayerId;
                LocalPlayer.SendPacket(packet);
            }
        }
    }
}

