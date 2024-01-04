#region

using System.Diagnostics.CodeAnalysis;
using HarmonyLib;
using NebulaModel.Packets.GameHistory;
using NebulaWorld;

#endregion

namespace NebulaPatcher.Patches.Dynamic;

[HarmonyPatch(typeof(UIDEOverview))]
internal class UIDEOverview_Patch
{
    [HarmonyPostfix]
    [HarmonyPatch(nameof(UIDEOverview._OnInit))]
    [SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Original Function Name")]
    public static void _OnInit_Postfix()
    {
        UIRoot.instance.uiGame.dysonEditor.controlPanel.topFunction.pauseButton.button.interactable = !Multiplayer.IsActive;
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(UIDEOverview._OnUpdate))]
    [SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Original Function Name")]
    public static void _OnUpdate_Prefix(UIDEOverview __instance)
    {
        if (!Multiplayer.IsActive)
        {
            return;
        }
        switch (__instance.autoConstructSwitch.isOn)
        {
            case true when !GameMain.data.history.HasFeatureKey(1100002):
                Multiplayer.Session.Network.SendPacket(new GameHistoryFeatureKeyPacket(1100002, true));
                break;
            case false when GameMain.data.history.HasFeatureKey(1100002):
                Multiplayer.Session.Network.SendPacket(new GameHistoryFeatureKeyPacket(1100002, false));
                break;
        }
    }
}
