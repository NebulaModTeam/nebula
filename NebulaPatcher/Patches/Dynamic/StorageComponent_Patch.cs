using HarmonyLib;
using NebulaHost;
using NebulaModel.Packets.Factory;
using NebulaWorld;
using NebulaWorld.Factory;
using System;

namespace NebulaPatcher.Patches.Dynamic
{
    [HarmonyPatch(typeof(StorageComponent))]
    class StorageComponent_Patch
    {
        [HarmonyPrefix]
        [HarmonyPatch("AddItem", new Type[] { typeof(int), typeof(int), typeof(int), typeof(int) })]
        public static bool AddItem_Prefix(StorageComponent __instance, int itemId, int count, int startIndex, int length)
        {
            //Run only in MP, if it is not triggered remotly and if this event was triggered manually by an user
            if (SimulatedWorld.Initialized && !StorageManager.EventFromServer && !StorageManager.EventFromClient && StorageManager.IsHumanInput && GameMain.data.localPlanet != null)
            {
                HandleUserInteraction(__instance, new StorageSyncRealtimeChangePacket(__instance.id, StorageSyncRealtimeChangeEvent.AddItem2, itemId, count, startIndex, length));
            }
            return true;
        }

        [HarmonyPrefix]
        [HarmonyPatch("AddItemStacked")]
        public static bool AddItemStacked_Prefix(StorageComponent __instance, int itemId, int count)
        {
            //Run only in MP, if it is not triggered remotly and if this event was triggered manually by an user
            if (SimulatedWorld.Initialized && !StorageManager.EventFromServer && !StorageManager.EventFromClient && StorageManager.IsHumanInput && GameMain.data.localPlanet != null)
            {
                HandleUserInteraction(__instance, new StorageSyncRealtimeChangePacket(__instance.id, StorageSyncRealtimeChangeEvent.AddItemStacked, itemId, count));
            }
            return true;

        }

        [HarmonyPrefix]
        [HarmonyPatch("TakeItemFromGrid")]
        public static bool TakeItemFromGrid_Prefix(StorageComponent __instance, int gridIndex, ref int itemId, ref int count)
        {
            //Run only in MP, if it is not triggered remotly and if this event was triggered manually by an user
            if (SimulatedWorld.Initialized && !StorageManager.EventFromServer && !StorageManager.EventFromClient && StorageManager.IsHumanInput && GameMain.data.localPlanet != null)
            {
                HandleUserInteraction(__instance, new StorageSyncRealtimeChangePacket(__instance.id, StorageSyncRealtimeChangeEvent.TakeItemFromGrid, gridIndex, itemId, count));
            }
            return true;
        }

        [HarmonyPostfix]
        [HarmonyPatch("SetBans")]
        public static void SetBans_Postfix(StorageComponent __instance, int _bans)
        {
            if (SimulatedWorld.Initialized && !StorageManager.EventFromServer && !StorageManager.EventFromClient)
            {
                HandleUserInteraction(__instance, new StorageSyncSetBansPacket(__instance.id, GameMain.data.localPlanet.factoryIndex, _bans));
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch("Sort")]
        public static void Sort_Postfix(StorageComponent __instance)
        {
            if (SimulatedWorld.Initialized && !StorageManager.EventFromServer && !StorageManager.EventFromClient && GameMain.data.localPlanet != null)
            {
                HandleUserInteraction(__instance, new StorageSyncSortPacket(__instance.id, GameMain.data.localPlanet.factoryIndex));
            }
        }

        public static void HandleUserInteraction<T>(StorageComponent __instance, T packet) where T : class, new()
        {
            //Skip if change was done in player's inventory
            if (__instance.entityId == 0 && __instance.id == 0)
            {
                return;
            }

            if (LocalPlayer.IsMasterClient)
            {
                StorageSyncManager.SendToPlayersOnTheSamePlanet(packet, GameMain.data.localPlanet.id);
            }
            else
            {
                LocalPlayer.SendPacket(packet);
            }
        }
    }
}
