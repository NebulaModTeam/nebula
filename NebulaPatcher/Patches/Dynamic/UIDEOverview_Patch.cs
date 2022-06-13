using HarmonyLib;
using NebulaModel.Packets.GameHistory;
using NebulaWorld;

namespace NebulaPatcher.Patches.Dynamic
{
    [HarmonyPatch(typeof(UIDEOverview))]
    internal class UIDEOverview_Patch
    {
        [HarmonyPostfix]
        [HarmonyPatch(nameof(UIDEOverview._OnInit))]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Original Function Name")]
        public static void _OnInit_Postfix()
        {
            NebulaModel.Logger.Log.Debug("UIDEOverview._OnInit");
            UIRoot.instance.uiGame.dysonEditor.controlPanel.topFunction.pauseButton.button.interactable = !Multiplayer.IsActive;
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(UIDEOverview._OnUpdate))]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Original Function Name")]
        public static void _OnUpdate_Prefix(UIDEOverview __instance)
        {
            if (Multiplayer.IsActive)
            {
                if (__instance.autoConstructSwitch.isOn && !GameMain.data.history.HasFeatureKey(1100002))
                {
                    Multiplayer.Session.Network.SendPacket(new GameHistoryFeatureKeyPacket(1100002, true));
                }
                else if (!__instance.autoConstructSwitch.isOn && GameMain.data.history.HasFeatureKey(1100002))
                {
                    Multiplayer.Session.Network.SendPacket(new GameHistoryFeatureKeyPacket(1100002, false));
                }
            }
        }
    }
}
