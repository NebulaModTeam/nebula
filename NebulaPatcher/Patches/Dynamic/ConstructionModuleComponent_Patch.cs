using System.Net.Sockets;
using HarmonyLib;
using NebulaModel.Packets.Players;
using NebulaWorld;
using NebulaWorld.Player;

namespace NebulaPatcher.Patches.Dynamic
{
    [HarmonyPatch(typeof(ConstructionModuleComponent))]
    internal class ConstructionModuleComponent_Patch
    {
        // dont give back idle construction drones to player if it was a drone owned by a remote player
        [HarmonyPostfix]
        [HarmonyPatch(nameof(ConstructionModuleComponent.RecycleDrone))]
        public static void RecycleDrone_Postfix(ConstructionModuleComponent __instance, PlanetFactory factory, ref DroneComponent drone)
        {
            if (!Multiplayer.IsActive)
            {
                return;
            }

            if (drone.owner < 0 && drone.owner * -1 != Multiplayer.Session.LocalPlayer.Id)
            {
                __instance.droneIdleCount--;
            }
        }
    }
}
