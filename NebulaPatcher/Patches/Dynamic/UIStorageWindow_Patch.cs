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
        [HarmonyPatch("OnStorageIdChange")]
        public static bool OnStorageIdChange_Prefix(UIStorageWindow __instance)
        {
            if (Multiplayer.IsActive && !LocalPlayer.IsMasterClient && Multiplayer.Session.Storage.WindowOpened)
            {
                UIStorageGrid storageUI = (UIStorageGrid)AccessTools.Field(typeof(UIStorageWindow), "storageUI").GetValue(__instance);
                Multiplayer.Session.Storage.ActiveUIStorageGrid = storageUI;
                Text titleText = (Text)AccessTools.Field(typeof(UIStorageWindow), "titleText").GetValue(__instance);
                Multiplayer.Session.Storage.ActiveStorageComponent = __instance.factoryStorage.storagePool[__instance.storageId];
                Multiplayer.Session.Storage.ActiveWindowTitle = titleText;
                Multiplayer.Session.Storage.ActiveBansSlider = (Slider)AccessTools.Field(typeof(UIStorageWindow), "bansSlider").GetValue(__instance);
                Multiplayer.Session.Storage.ActiveBansValueText = (Text)AccessTools.Field(typeof(UIStorageWindow), "bansValueText").GetValue(__instance);
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
        [HarmonyPatch("_OnOpen")]
        public static bool _OnOpen_Prefix()
        {
            Multiplayer.Session.Storage.WindowOpened = true;
            return true;
        }

        [HarmonyPostfix]
        [HarmonyPatch("_OnClose")]
        public static void _OnClose_Prefix()
        {
            Multiplayer.Session.Storage.WindowOpened = false;
            Multiplayer.Session.Storage.ActiveStorageComponent = null;
        }
    }
}
