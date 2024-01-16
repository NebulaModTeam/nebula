#region

using HarmonyLib;
using NebulaModel.Packets.Factory.Storage;
using NebulaWorld;

#endregion

namespace NebulaPatcher.Patches.Dynamic;

[HarmonyPatch(typeof(StorageComponent))]
internal class StorageComponent_Patch
{
    [HarmonyPrefix]
    [HarmonyPatch(nameof(StorageComponent.AddItem),
        new[] { typeof(int), typeof(int), typeof(int), typeof(int), typeof(int), typeof(int) },
        new[]
        {
            ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Normal,
            ArgumentType.Out
        })]
    public static bool AddItem_Prefix(StorageComponent __instance, int itemId, int count, int startIndex, int length, int inc,
        out int remainInc)
    {
        //Run only in MP, if it is not triggered remotly and if this event was triggered manually by an user
        if (Multiplayer.IsActive && !Multiplayer.Session.Storage.IsIncomingRequest &&
            Multiplayer.Session.Storage.IsHumanInput && GameMain.data.localPlanet != null)
        {
            HandleUserInteraction(__instance,
                new StorageSyncRealtimeChangePacket(__instance.id, StorageSyncRealtimeChangeEvent.AddItem2, itemId, count,
                    startIndex, length, inc));
        }
        remainInc = inc; // this is what the game does anyways so it should not change the functionality of the method
        return true;
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(StorageComponent.AddItemStacked))]
    public static bool AddItemStacked_Prefix(StorageComponent __instance, int itemId, int count, int inc, out int remainInc)
    {
        //Run only in MP, if it is not triggered remotely and if this event was triggered manually by an user
        if (Multiplayer.IsActive && !Multiplayer.Session.Storage.IsIncomingRequest &&
            Multiplayer.Session.Storage.IsHumanInput && GameMain.data.localPlanet != null)
        {
            HandleUserInteraction(__instance,
                new StorageSyncRealtimeChangePacket(__instance.id, StorageSyncRealtimeChangeEvent.AddItemStacked, itemId, count,
                    inc));
        }
        remainInc = inc; // this is what the game does anyways so it should not change the functionality of the method
        return true;
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(StorageComponent.TakeItemFromGrid))]
    public static bool TakeItemFromGrid_Prefix(StorageComponent __instance, int gridIndex, ref int itemId, ref int count,
        out int inc)
    {
        //Run only in MP, if it is not triggered remotely and if this event was triggered manually by an user
        if (Multiplayer.IsActive && !Multiplayer.Session.Storage.IsIncomingRequest &&
            Multiplayer.Session.Storage.IsHumanInput && GameMain.data.localPlanet != null)
        {
            HandleUserInteraction(__instance,
                new StorageSyncRealtimeChangePacket(__instance.id, StorageSyncRealtimeChangeEvent.TakeItemFromGrid, gridIndex,
                    itemId, count, 0));
        }
        inc = 0; // this is what the game does anyways so it should not change the functionality of the method
        return true;
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(StorageComponent.SetBans))]
    public static void SetBans_Postfix(StorageComponent __instance, int _bans)
    {
        if (Multiplayer.IsActive && !Multiplayer.Session.Storage.IsIncomingRequest &&
            Multiplayer.Session.Storage.IsHumanInput && GameMain.data.localPlanet != null)
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
    [HarmonyPatch(typeof(StorageComponent), nameof(StorageComponent.TakeTailItems),
        new[] { typeof(int), typeof(int), typeof(int), typeof(bool) },
        new[] { ArgumentType.Ref, ArgumentType.Ref, ArgumentType.Out, ArgumentType.Normal })]
    public static bool TakeTailItems_Prefix(StorageComponent __instance, ref int count)
    {
        // Run normally if we are not in an MP session or StorageComponent is not player package
        if (!Multiplayer.IsActive || __instance.id != GameMain.mainPlayer.package.id)
        {
            return true;
        }

        // We should only take items to player if player requested
        if (!Multiplayer.Session.Factories.IsIncomingRequest.Value ||
            Multiplayer.Session.Factories.PacketAuthor == Multiplayer.Session.LocalPlayer.Id)
        {
            return true;
        }
        count = 1;
        return false;

    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(StorageComponent), nameof(StorageComponent.SetFilter))]
    private static void SetFilter_Postfix(StorageComponent __instance, int gridIndex, int filterId)
    {
        //Run only in MP, if it is not triggered remotely and if this event was triggered manually by an user
        if (Multiplayer.IsActive && !Multiplayer.Session.Storage.IsIncomingRequest &&
            Multiplayer.Session.Storage.IsHumanInput && GameMain.data.localPlanet is not null)
        {
            HandleUserInteraction(__instance,
                new StorageSyncSetFilterPacket(__instance.id, GameMain.data.localPlanet.id, gridIndex, filterId, __instance.type));
        }
        // return true;
    }

    private static void HandleUserInteraction<T>(StorageComponent __instance, T packet) where T : class, new()
    {
        //Skip if change was done in player's inventory
        if (__instance.entityId == 0 && __instance.id == 0)
        {
            return;
        }

        if (Multiplayer.Session.LocalPlayer.IsHost)
        {
            // Assume storage is on the local planet, send to all clients in local star system who may have the factory loaded
            Multiplayer.Session.Network.SendPacketToStar(packet, GameMain.localStar.id);
        }
        else
        {
            Multiplayer.Session.Network.SendPacket(packet);
        }
    }
}
