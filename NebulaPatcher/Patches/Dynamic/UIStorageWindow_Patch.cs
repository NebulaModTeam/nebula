using HarmonyLib;
using NebulaModel.Packets.Factory;
using NebulaWorld;
using NebulaWorld.Factory;
using UnityEngine.UI;

namespace NebulaPatcher.Patches.Dynamic
{
    [HarmonyPatch(typeof(UIStorageWindow))]
    class UIStorageWindow_Patch
    {
        [HarmonyPrefix]
        [HarmonyPatch(nameof(UIStorageWindow.OnStorageIdChange))]
        public static bool OnStorageIdChange_Prefix(UIStorageWindow __instance)
        {
            if (SimulatedWorld.Initialized && !LocalPlayer.IsMasterClient && StorageManager.WindowOpened)
            {
                UIStorageGrid storageUI = __instance.storageUI;
                StorageManager.ActiveUIStorageGrid = storageUI;
                Text titleText = __instance.titleText;
                StorageManager.ActiveStorageComponent = __instance.factoryStorage.storagePool[__instance.storageId];
                StorageManager.ActiveWindowTitle = titleText;
                StorageManager.ActiveBansSlider = __instance.bansSlider;
                StorageManager.ActiveBansValueText = __instance.bansValueText;
                titleText.text = "Loading...";
                storageUI._Free();
                storageUI._Open();
                storageUI.OnStorageDataChanged();
                LocalPlayer.SendPacket(new StorageSyncRequestPacket(GameMain.data.localPlanet.id, __instance.storageId));
                return false;
            }
            return true;
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(UIStorageWindow._OnOpen))]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Original Function Name")]
        public static bool _OnOpen_Prefix()
        {
            StorageManager.WindowOpened = true;
            return true;
        }

        [HarmonyPostfix]
        [HarmonyPatch(nameof(UIStorageWindow._OnClose))]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Original Function Name")]
        public static void _OnClose_Prefix()
        {
            StorageManager.WindowOpened = false;
            StorageManager.ActiveStorageComponent = null;
        }
    }
}
