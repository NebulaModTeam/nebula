#region

using HarmonyLib;
using NebulaAPI;
using NebulaModel;
using NebulaModel.Packets.Combat.Mecha;
using NebulaModel.Packets.Players;
using NebulaWorld;

#endregion

namespace NebulaPatcher.Patches.Dynamic;

[HarmonyPatch(typeof(Player))]
internal class Player_Patch
{
    [HarmonyPrefix]
    [HarmonyPatch(nameof(Player.ExchangeSand))]
    public static bool ExchangeSand_Prefix(Player __instance)
    {
        if (!Multiplayer.IsActive)
        {
            return true;
        }

        var gainedSand = 0;
        for (var i = 0; i < __instance.package.size; i++)
        {
            if (__instance.package.grids[i].itemId == 1099) // 1099: enemy drop sand item
            {
                gainedSand += __instance.package.grids[i].count;
                __instance.package.grids[i].itemId = 0;
                __instance.package.grids[i].filter = 0;
                __instance.package.grids[i].count = 0;
                __instance.package.grids[i].inc = 0;
                __instance.package.grids[i].stackSize = 0;
            }
        }

        // Only call SetSandCount when there is sand change in client
        if (gainedSand > 0)
        {
            if (Config.Options.SyncSoil && Multiplayer.Session.IsClient)
            {
                // Report to server to add sand in shared pool
                Multiplayer.Session.Client.SendPacket(new PlayerSandCount(gainedSand, true));
            }
            else
            {
                __instance.SetSandCount(__instance.sandCount + gainedSand);
            }
        }
        return false;
    }


    [HarmonyPrefix]
    [HarmonyPatch(nameof(Player.SetSandCount))]
    public static bool SetSandCount_Prefix(long newSandCount)
    {
        if (!Multiplayer.IsActive)
        {
            return true;
        }

        if (!Config.Options.SyncSoil)
        {
            return Multiplayer.Session.Factories.PacketAuthor == Multiplayer.Session.LocalPlayer.Id ||
                   Multiplayer.Session.LocalPlayer.IsHost &&
                   Multiplayer.Session.Factories.PacketAuthor == NebulaModAPI.AUTHOR_NONE ||
                   !Multiplayer.Session.Factories.IsIncomingRequest.Value;
        }

        if (Multiplayer.Session.LocalPlayer.IsHost)
        {
            //Soil should be given in singleplayer or to the host who then syncs it back to all players.
            var deltaSandCount = (int)(newSandCount - GameMain.mainPlayer.sandCount);
            if (deltaSandCount != 0)
            {
                UpdateSyncedSandCount(deltaSandCount);
                Multiplayer.Session.Server.SendPacket(new PlayerSandCount(newSandCount));
            }
        }
        else
        {
            //Or client that use reform tool
            if (GameMain.mainPlayer.controller.actionBuild.reformTool.drawing)
            {
                Multiplayer.Session.Client.SendPacket(new PlayerSandCount(newSandCount));
            }
        }

        return Multiplayer.Session.LocalPlayer.IsHost;
        //Soil should be given in singleplayer or to the player who is author of the "Build" request, or to the host if there is no author.
    }

    private static void UpdateSyncedSandCount(long deltaSandCount)
    {
        var connectedPlayers = Multiplayer.Session.Server.Players.Connected;
        foreach (var kvp in connectedPlayers)
        {
            kvp.Value.Data.Mecha.SandCount += deltaSandCount / (connectedPlayers.Count + 1);
            // dont be too picky here, a little bit more or less sand is ignorable i guess
            if (kvp.Value.Data.Mecha.SandCount < 0)
            {
                kvp.Value.Data.Mecha.SandCount = 0;
            }
        }

        Multiplayer.Session.LocalPlayer.Data.Mecha.SandCount += deltaSandCount / (connectedPlayers.Count + 1);
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
        if (!Multiplayer.Session.Factories.IsIncomingRequest.Value ||
            Multiplayer.Session.Factories.PacketAuthor == Multiplayer.Session.LocalPlayer.Id)
        {
            return true;
        }

        __result = 0;
        return false;
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
        if (!Multiplayer.Session.Factories.IsIncomingRequest.Value ||
            Multiplayer.Session.Factories.PacketAuthor == Multiplayer.Session.LocalPlayer.Id)
        {
            return true;
        }

        __result = 1;
        return false;
    }

    #region Combat

    [HarmonyPrefix]
    [HarmonyPatch(nameof(Player.Kill))]
    public static bool Kill_Prefix(Player __instance)
    {
        if (!Multiplayer.IsActive) return true;
        if (__instance != GameMain.mainPlayer) return false;

        if (__instance.isAlive)
        {
            Multiplayer.Session.Network.SendPacket(new MechaAliveEventPacket(
                Multiplayer.Session.LocalPlayer.Id, MechaAliveEventPacket.EStatus.Kill));
            ThrowItemsInInventory(__instance);
        }
        return true;
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(Player.PrepareRedeploy))]
    public static bool PrepareRedeploy_Prefix(Player __instance)
    {
        if (!Multiplayer.IsActive) return true;

        // Don't drop item when Redeploy
        __instance.mecha.PrepareRedeploy();
        return false;
    }

    private static void ThrowItemsInInventory(Player player)
    {
        // Balance: Drop half of item in inventory when player killed
        const float DROP_RATE = 0.5f;
        for (var i = 0; i < player.package.size; i++)
        {
            var itemId = 0;
            var itemCount = (int)(player.package.grids[i].count * DROP_RATE);
            player.package.TakeItemFromGrid(i, ref itemId, ref itemCount, out var itemInc);
            if (itemId > 0 && itemCount > 0)
            {
                player.ThrowTrash(itemId, itemCount, itemInc, 0, 0);
            }
        }
    }

    #endregion
}
