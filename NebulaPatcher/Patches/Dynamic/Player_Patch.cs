﻿using HarmonyLib;
using NebulaWorld;
using NebulaWorld.Factory;

namespace NebulaPatcher.Patches.Dynamic
{
    [HarmonyPatch(typeof(Player))]
    class Player_Patch
    {
        [HarmonyPrefix]
        [HarmonyPatch(nameof(Player.SetSandCount))]
        public static bool RemoveLayer_Prefix()
        {
            //Soil should be given in singleplayer or to the player who is author of the "Build" request, or to the host if there is no author.
            return !SimulatedWorld.Initialized || FactoryManager.PacketAuthor == LocalPlayer.PlayerId || (LocalPlayer.IsMasterClient && FactoryManager.PacketAuthor == FactoryManager.AUTHOR_NONE) || !FactoryManager.IsIncomingRequest;
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(Player.TryAddItemToPackage))]
        public static bool TryAddItemToPackage_Prefix(ref int __result)
        {
            if (!SimulatedWorld.Initialized)
            {
                return true;
            }

            // We should only add items to player if player requested
            if (FactoryManager.IsIncomingRequest && FactoryManager.PacketAuthor != LocalPlayer.PlayerId)
            {
                __result = 0;
                return false;
            }

            return true;
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(Player.UseHandItems))]
        public static bool UseHandItems_Prefix(ref int __result)
        {
            // Run normally if we are not in an MP session or StorageComponent is not player package
            if (!SimulatedWorld.Initialized)
            {
                return true;
            }

            // We should only take items to player if player requested
            if (FactoryManager.IsIncomingRequest && FactoryManager.PacketAuthor != LocalPlayer.PlayerId)
            {
                __result = 1;
                return false;
            }

            return true;
        }
    }
}
