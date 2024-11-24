#region

using System.Diagnostics.CodeAnalysis;
using HarmonyLib;
using NebulaModel.Packets.Factory.Storage;
using NebulaWorld;

#endregion

namespace NebulaPatcher.Patches.Dynamic;

[HarmonyPatch(typeof(UIStorageWindow))]
internal class UIStorageWindow_Patch
{
    [HarmonyPrefix]
    [HarmonyPatch(nameof(UIStorageWindow.OnStorageIdChange))]
    public static bool OnStorageIdChange_Prefix(UIStorageWindow __instance)
    {
        if (!Multiplayer.IsActive || Multiplayer.Session.LocalPlayer.IsHost || !Multiplayer.Session.Storage.WindowOpened)
        {
            return true;
        }
        var storageComponent = __instance.factoryStorage?.storagePool[__instance.storageId];
        if (storageComponent == null || storageComponent.entityId == 0)
        {
            return true;
        }
        var storageUI = __instance.storageUI;
        Multiplayer.Session.Storage.ActiveUIStorageGrid = storageUI;
        var titleText = __instance.titleText;
        Multiplayer.Session.Storage.ActiveStorageComponent = storageComponent;
        Multiplayer.Session.Storage.ActiveWindowTitle = titleText;
        Multiplayer.Session.Storage.ActiveBansSlider = __instance.bansSlider;
        Multiplayer.Session.Storage.ActiveBansValueText = __instance.bansValueText;
        titleText.text = "Loading...";
        storageUI._Free();
        storageUI._Open();
        storageUI.OnStorageDataChanged();
        Multiplayer.Session.Network.SendPacket(new StorageSyncRequestPacket(__instance.factoryStorage.planet.id,
            __instance.storageId));
        return false;
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(UIStorageWindow._OnOpen))]
    [SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Original Function Name")]
    public static bool _OnOpen_Prefix()
    {
        if (Multiplayer.IsActive)
        {
            Multiplayer.Session.Storage.WindowOpened = true;
        }

        return true;
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(UIStorageWindow._OnClose))]
    [SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Original Function Name")]
    public static void _OnClose_Prefix()
    {
        if (!Multiplayer.IsActive)
        {
            return;
        }
        Multiplayer.Session.Storage.WindowOpened = false;
        Multiplayer.Session.Storage.ActiveStorageComponent = null;
    }
}
