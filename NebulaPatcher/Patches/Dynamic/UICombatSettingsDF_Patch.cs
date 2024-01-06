#region

using System.Diagnostics.CodeAnalysis;
using System.Linq;
using HarmonyLib;
using NebulaAPI.Extensions;
using NebulaAPI.Networking;
using NebulaModel;
using NebulaModel.Logger;
using NebulaModel.Packets.Session;
using NebulaPatcher.Patches.Transpilers;
using NebulaWorld;
using UnityEngine;
using UnityEngine.UI;

#endregion

namespace NebulaPatcher.Patches.Dynamic
{
    [HarmonyPatch(typeof(UICombatSettingsDF))]
    internal class UICombatSettingsDF_Patch
    {
        [HarmonyPostfix]
        [HarmonyPatch(nameof(UICombatSettingsDF.ApplySettings))]
        public static void ApplySettings_Postfix(UICombatSettingsDF __instance)
        {
            if (!Multiplayer.IsInMultiplayerMenu || !Multiplayer.Session.LocalPlayer.IsHost)
            {
                return;
            }

            // syncing players are those who have not loaded into the game yet, so they might still be in the lobby. they need to check if this packet is relevant for them in the corresponding handler.
            // just remembered others cant be in game anyways when host ist still in lobby >.>
            var loadingPlayers = Multiplayer.Session.Server.Players
                .Where(p => p.Connection.ConnectionStatus is not EConnectionStatus.Connected);

            foreach (var player in loadingPlayers)
            {
                player.SendPacket(new LobbyUpdateCombatValues(__instance.gameDesc.combatSettings));
            }
        }
    }
}
