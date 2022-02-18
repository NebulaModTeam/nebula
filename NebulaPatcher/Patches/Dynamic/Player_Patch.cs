﻿using HarmonyLib;
using NebulaAPI;
using NebulaModel;
using NebulaModel.Packets.Players;
using NebulaWorld;

namespace NebulaPatcher.Patches.Dynamic
{
    [HarmonyPatch(typeof(Player))]
    internal class Player_Patch
    {
        [HarmonyPrefix]
        [HarmonyPatch(nameof(Player.SetSandCount))]
        public static bool SetSandCount_Prefix(int newSandCount)
        {
            if (Config.Options.SyncSoil)
            {
                //Soil should be given in singleplayer or to the host who then syncs it back to all players.
                if(Multiplayer.IsActive && Multiplayer.Session.LocalPlayer.IsHost)
                {
                    Multiplayer.Session.Network.PlayerManager.UpdateSyncedSandCount(newSandCount - GameMain.mainPlayer.sandCount);
                    Multiplayer.Session.Network.SendPacket(new PlayerSandCount(newSandCount));
                }
                //Or client that use reform tool
                else if (Multiplayer.IsActive && GameMain.mainPlayer.controller.actionBuild.reformTool.drawing == true)
                {
                    Multiplayer.Session.Network.SendPacket(new PlayerSandCount(newSandCount));
                }
                return !Multiplayer.IsActive || Multiplayer.Session.LocalPlayer.IsHost;
            }
            else
            {
                //Soil should be given in singleplayer or to the player who is author of the "Build" request, or to the host if there is no author.
                return !Multiplayer.IsActive || Multiplayer.Session.Factories.PacketAuthor == Multiplayer.Session.LocalPlayer.Id || (Multiplayer.Session.LocalPlayer.IsHost && Multiplayer.Session.Factories.PacketAuthor == NebulaModAPI.AUTHOR_NONE) || !Multiplayer.Session.Factories.IsIncomingRequest.Value;
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(Player.TryAddItemToPackage))]
        public static bool TryAddItemToPackage_Prefix(ref int __result)
        {
            if (!Multiplayer.IsActive)
            {
                return true;
            }

            // We should only add items to player if player requested
            if (Multiplayer.Session.Factories.IsIncomingRequest.Value && Multiplayer.Session.Factories.PacketAuthor != Multiplayer.Session.LocalPlayer.Id)
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
            if (!Multiplayer.IsActive)
            {
                return true;
            }

            // We should only take items to player if player requested
            if (Multiplayer.Session.Factories.IsIncomingRequest.Value && Multiplayer.Session.Factories.PacketAuthor != Multiplayer.Session.LocalPlayer.Id)
            {
                __result = 1;
                return false;
            }

            return true;
        }
    }
}
