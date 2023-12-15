#region

using HarmonyLib;
using NebulaAPI;
using NebulaModel;
using NebulaModel.Packets.Players;
using NebulaWorld;

#endregion

namespace NebulaPatcher.Patches.Dynamic;

[HarmonyPatch(typeof(Player))]
internal class Player_Patch
{
    [HarmonyPrefix]
    [HarmonyPatch(nameof(Player.SetSandCount))]
    public static bool SetSandCount_Prefix(long newSandCount)
    {
        if (!Config.Options.SyncSoil)
        {
            return !Multiplayer.IsActive || Multiplayer.Session.Factories.PacketAuthor == Multiplayer.Session.LocalPlayer.Id ||
                   Multiplayer.Session.LocalPlayer.IsHost &&
                   Multiplayer.Session.Factories.PacketAuthor == NebulaModAPI.AUTHOR_NONE ||
                   !Multiplayer.Session.Factories.IsIncomingRequest.Value;
        }
        switch (Multiplayer.IsActive)
        {
            //Soil should be given in singleplayer or to the host who then syncs it back to all players.
            case true when Multiplayer.Session.LocalPlayer.IsHost:
                Multiplayer.Session.Network.PlayerManager.UpdateSyncedSandCount((int)(newSandCount - GameMain.mainPlayer.sandCount));
                Multiplayer.Session.Network.SendPacket(new PlayerSandCount(newSandCount));
                break;
            //Or client that use reform tool
            case true when GameMain.mainPlayer.controller.actionBuild.reformTool.drawing:
                Multiplayer.Session.Network.SendPacket(new PlayerSandCount(newSandCount));
                break;
        }
        return !Multiplayer.IsActive || Multiplayer.Session.LocalPlayer.IsHost;
        //Soil should be given in singleplayer or to the player who is author of the "Build" request, or to the host if there is no author.
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
}
