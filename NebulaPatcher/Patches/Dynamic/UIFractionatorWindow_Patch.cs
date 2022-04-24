﻿using HarmonyLib;
using NebulaModel.Packets.Factory.Fractionator;
using NebulaWorld;

namespace NebulaPatcher.Patches.Dynamic
{
    [HarmonyPatch(typeof(UIFractionatorWindow))]
    internal class UIFractionatorWindow_Patch
    {
        [HarmonyPostfix]
        [HarmonyPatch(nameof(UIFractionatorWindow.OnProductUIButtonClick))]
        public static void OnTakeBackPointerUp_Postfix(UIFractionatorWindow __instance)
        {
            if (Multiplayer.IsActive)
            {
                FractionatorComponent fractionator = __instance.factorySystem.fractionatorPool[__instance.fractionatorId];
                Multiplayer.Session.Network.SendPacketToLocalStar(new FractionatorStorageUpdatePacket(in fractionator, __instance.factory.planetId));
            }
        }
    }
}
