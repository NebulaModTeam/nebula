﻿using HarmonyLib;
using NebulaModel.Packets.Factory;
using NebulaNetwork;
using NebulaWorld;
using System;

namespace NebulaPatcher.Patches.Dynamic
{
    [HarmonyPatch(typeof(StorageComponent))]
    internal class StorageComponent_Patch
    {
        [HarmonyPrefix]
        [HarmonyPatch(nameof(StorageComponent.AddItem), new Type[] { typeof(int), typeof(int), typeof(int), typeof(int) })]
        public static bool AddItem_Prefix(StorageComponent __instance, int itemId, int count, int startIndex, int length)
        {
            //Run only in MP, if it is not triggered remotly and if this event was triggered manually by an user
            if (Multiplayer.IsActive && !Multiplayer.Session.Storage.IsIncomingRequest && Multiplayer.Session.Storage.IsHumanInput && GameMain.data.localPlanet != null)
            {
                HandleUserInteraction(__instance, new StorageSyncRealtimeChangePacket(__instance.id, StorageSyncRealtimeChangeEvent.AddItem2, itemId, count, startIndex, length));
            }
            return true;
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(StorageComponent.AddItemStacked))]
        public static bool AddItemStacked_Prefix(StorageComponent __instance, int itemId, int count)
        {
            //Run only in MP, if it is not triggered remotly and if this event was triggered manually by an user
            if (Multiplayer.IsActive && !Multiplayer.Session.Storage.IsIncomingRequest && Multiplayer.Session.Storage.IsHumanInput && GameMain.data.localPlanet != null)
            {
                HandleUserInteraction(__instance, new StorageSyncRealtimeChangePacket(__instance.id, StorageSyncRealtimeChangeEvent.AddItemStacked, itemId, count));
            }
            return true;

        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(StorageComponent.TakeItemFromGrid))]
        public static bool TakeItemFromGrid_Prefix(StorageComponent __instance, int gridIndex, ref int itemId, ref int count)
        {
            //Run only in MP, if it is not triggered remotly and if this event was triggered manually by an user
            if (Multiplayer.IsActive && !Multiplayer.Session.Storage.IsIncomingRequest && Multiplayer.Session.Storage.IsHumanInput && GameMain.data.localPlanet != null)
            {
                HandleUserInteraction(__instance, new StorageSyncRealtimeChangePacket(__instance.id, StorageSyncRealtimeChangeEvent.TakeItemFromGrid, gridIndex, itemId, count));
            }
            return true;
        }

        [HarmonyPostfix]
        [HarmonyPatch(nameof(StorageComponent.SetBans))]
        public static void SetBans_Postfix(StorageComponent __instance, int _bans)
        {
            if (Multiplayer.IsActive && !Multiplayer.Session.Storage.IsIncomingRequest && GameMain.data.localPlanet != null)
            {
                HandleUserInteraction(__instance, new StorageSyncSetBansPacket(__instance.id, GameMain.data.localPlanet.id, _bans));
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(nameof(StorageComponent.Sort))]
        public static void Sort_Postfix(StorageComponent __instance)
        {
            if (Multiplayer.IsActive && !Multiplayer.Session.Storage.IsIncomingRequest && GameMain.data.localPlanet != null)
            {
                HandleUserInteraction(__instance, new StorageSyncSortPacket(__instance.id, GameMain.data.localPlanet.id));
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(StorageComponent), nameof(StorageComponent.TakeTailItems), new Type[] { typeof(int), typeof(int), typeof(bool) }, new ArgumentType[] { ArgumentType.Ref, ArgumentType.Ref, ArgumentType.Normal })]
        public static bool TakeTailItems_Prefix(StorageComponent __instance, ref int count)
        {
            // Run normally if we are not in an MP session or StorageComponent is not player package
            if (!Multiplayer.IsActive || __instance.id != GameMain.mainPlayer.package.id)
            {
                return true;
            }

            // We should only take items to player if player requested
            if (Multiplayer.Session.Factories.IsIncomingRequest.Value && Multiplayer.Session.Factories.PacketAuthor != Multiplayer.Session.LocalPlayer.Id)
            {
                count = 1;
                return false;
            }

            return true;
        }

        public static void HandleUserInteraction<T>(StorageComponent __instance, T packet) where T : class, new()
        {
            //Skip if change was done in player's inventory
            if (__instance.entityId == 0 && __instance.id == 0)
            {
                return;
            }

            if (Multiplayer.Session.LocalPlayer.IsHost)
            {
                StorageSyncManager.SendToPlayersOnTheSamePlanet(packet, GameMain.data.localPlanet.id);
            }
            else
            {
                Multiplayer.Session.Network.SendPacket(packet);
            }
        }
    }
}
