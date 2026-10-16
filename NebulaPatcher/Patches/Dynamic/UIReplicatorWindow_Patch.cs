#region

using System;
using HarmonyLib;
using NebulaAPI;
using NebulaModel.Logger;
using NebulaModel.Packets.Players;
using NebulaWorld;
using NebulaWorld.GameStates;

#endregion

namespace NebulaPatcher.Patches.Dynamic;

[HarmonyPatch(typeof(UIReplicatorWindow))]
internal class UIReplicatorWindow_Patch
{
    [HarmonyPostfix]
    [HarmonyPatch(nameof(UIReplicatorWindow._OnInit))]
    public static void _OnInit_Postfix(UIReplicatorWindow __instance)
    {
        try
        {
            GameStatesManager.RestoreReplicatorMultipliers(__instance);
        }
        catch (Exception e)
        {
            Log.Warn($"Failed to restore replicator multipliers on _OnInit: {e}");
        }
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(UIReplicatorWindow._OnOpen))]
    public static void _OnOpen_Postfix(UIReplicatorWindow __instance)
    {
        try
        {
            GameStatesManager.RestoreReplicatorMultipliers(__instance);
        }
        catch (Exception e)
        {
            Log.Warn($"Failed to restore replicator multipliers on _OnOpen: {e}");
        }
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(UIReplicatorWindow._OnClose))]
    public static void _OnClose_Postfix(UIReplicatorWindow __instance)
    {
        OnMultiplierChanged(__instance);
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(UIReplicatorWindow.OnPlusButtonClick))]
    public static void OnPlusButtonClick_Postfix(UIReplicatorWindow __instance)
    {
        OnMultiplierChanged(__instance);
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(UIReplicatorWindow.OnMinusButtonClick))]
    public static void OnMinusButtonClick_Postfix(UIReplicatorWindow __instance)
    {
        OnMultiplierChanged(__instance);
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(UIReplicatorWindow.OnSelectedRecipeChange))]
    public static void OnSelectedRecipeChange_Postfix(UIReplicatorWindow __instance)
    {
        try
        {
            if (__instance != null && __instance.selectedRecipe != null && __instance.multiValueText != null)
            {
                int multi = 1;
                if (__instance.multipliers != null && __instance.multipliers.TryGetValue(__instance.selectedRecipe.ID, out int val) && val > 1)
                {
                    multi = val;
                }
                __instance.multiValueText.text = $"{multi}x";
            }
        }
        catch (Exception e)
        {
            Log.Warn($"Failed to update multiValueText on recipe change: {e}");
        }
    }

    public static void OnMultiplierChanged(UIReplicatorWindow instance)
    {
        if (instance == null)
        {
            return;
        }

        try
        {
            // Sync to GameMain.data.preferences
            if (GameMain.data?.preferences != null && instance.multipliers != null)
            {
                GameMain.data.preferences.replicatorMultipliers ??= new System.Collections.Generic.Dictionary<int, int>();
                foreach (var kv in instance.multipliers)
                {
                    GameMain.data.preferences.replicatorMultipliers[kv.Key] = kv.Value;
                }
            }

            var bytes = GameStatesManager.ExportReplicatorMultipliers();
            SyncMultipliersToServer(bytes);
        }
        catch (Exception e)
        {
            Log.Warn($"Failed to process multiplier change: {e}");
        }
    }

    public static void SyncMultipliersToServer(byte[] data = null)
    {
        if (!Multiplayer.IsActive)
        {
            return;
        }

        data ??= GameStatesManager.ExportReplicatorMultipliers();
        if (data == null || data.Length == 0)
        {
            return;
        }

        if (Multiplayer.Session.LocalPlayer.IsHost)
        {
            if (Multiplayer.Session.LocalPlayer.Data is NebulaModel.DataStructures.PlayerData hostData)
            {
                hostData.ReplicatorMultipliersData = data;
            }
        }
        else
        {
            try
            {
                Multiplayer.Session.Network.SendPacket(
                    new PlayerReplicatorMultipliersPacket(Multiplayer.Session.LocalPlayer.Id, data));
            }
            catch (Exception e)
            {
                Log.Warn($"Failed to sync replicator multipliers to server: {e}");
            }
        }
    }
}
