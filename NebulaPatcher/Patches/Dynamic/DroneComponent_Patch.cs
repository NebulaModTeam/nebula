using System.Reflection;
using HarmonyLib;
using NebulaWorld;
using NebulaWorld.Player;
using UnityEngine;

namespace NebulaPatcher.Patches.Dynamic
{
    internal class DroneComponent_Patch
    {
        [HarmonyPatch()]
        class Get_InternalUpdate
        {
            [HarmonyTargetMethod]
            public static MethodBase GetTargetMethod()
            {
                return AccessTools.Method(
                    typeof(DroneComponent),
                    "InternalUpdate",
                    new System.Type[]
                    {
                        typeof(CraftData).MakeByRefType(),
                        typeof(PlanetFactory),
                        typeof(Vector3).MakeByRefType(),
                        typeof(float),
                        typeof(float),
                        typeof(double).MakeByRefType(),
                        typeof(double).MakeByRefType(),
                        typeof(double),
                        typeof(double),
                        typeof(float).MakeByRefType()
                    });
            }

            // Update the position of the start/return point of construction drones that are owned by other players. The game would default to the local player position if not patched.
            [HarmonyPrefix]
            public static void InternalUpdate(DroneComponent __instance, ref CraftData craft, PlanetFactory factory, ref Vector3 ejectPos, float droneSpeed, float dt, ref double mechaEnergy, ref double mechaEnergyChange, double flyEnergyRate, double repairEnergyCost, out float energyRatio)
            {
                energyRatio = 1f; // original method does this at the beginning anyways.

                if (!Multiplayer.IsActive)
                {
                    return;
                }

                if (__instance.owner < 0)
                {
                    // very inefficient, better update this in the background elsewhere
                    DroneManager.ClearCachedPositions();
                    ejectPos = DroneManager.GetPlayerPosition((ushort)(__instance.owner * -1)); // player id is stored in owner to retrieve the current position when drones are updated.
                    ejectPos = ejectPos.normalized * (ejectPos.magnitude + 2.8f); // drones return to the head of mecha not feet
                }
            }
        }
    }
}
