using HarmonyLib;
using NebulaWorld;

namespace NebulaPatcher.Patches.Dynamic
{
    [HarmonyPatch(typeof(Mecha))]
    class Mecha_Patch
    {
        [HarmonyPostfix]
        [HarmonyPatch(nameof(Mecha.GameTick))]
        public static void GameTick_Postfix(float dt)
        {
            if (Multiplayer.IsActive)
            {
                Multiplayer.Session.World.OnDronesGameTick(dt);
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(Mecha.GenerateEnergy))]
        public static bool Mecha_GenerateEnergy_Prefix(Mecha __instance)
        {
            if (!Multiplayer.IsActive) return true;

            // some players managed to break the fuel chamber on clients.
            // the game thought there is still fuel burning while not adding energy to the mecha and preventing new fuel from beeing added.
            // this checks for this corner case and resets the reactor energy to 0 (empty fuel chamber as displayed to the player)
            if (!Multiplayer.Session.LocalPlayer.IsHost && __instance.reactorEnergy > 0 && __instance.reactorItemId == 0)
            {
                __instance.reactorEnergy = 0;
            }
            return true;
        }

        // We can't do this as client as we won't be able to get_nearestPlanet() since we do not currently have all of the factory info
        [HarmonyPrefix]
        [HarmonyPatch(typeof(Mecha), nameof(Mecha.AddConsumptionStat))]
        [HarmonyPatch(typeof(Mecha), nameof(Mecha.AddProductionStat))]
        public static bool AddStat_Common_Prefix()
        {
            if (!Multiplayer.IsActive || Multiplayer.Session.LocalPlayer.IsHost)
                return true;

            // TODO: Send packet to host to add stat?
            // Easy option: just have host add stat to their closest planet, though is this better than not syncing it at all?
            // Hard option: find a way to reliably get nearest planet from client with missing data

            return false;
        }
    }
}
