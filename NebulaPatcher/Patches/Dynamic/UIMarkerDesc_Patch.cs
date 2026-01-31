#region

using HarmonyLib;
using NebulaModel.Packets.Universe;
using NebulaWorld;

#endregion

namespace NebulaPatcher.Patches.Dynamic;

[HarmonyPatch(typeof(UIMarkerDesc))]
internal class UIMarkerDesc_Patch
{
    [HarmonyPostfix]
    [HarmonyPatch(nameof(UIMarkerDesc.OnTitleInputFieldEndEdit))]
    public static void OnTitleInputFieldEndEdit_Postfix(UIMarkerDesc __instance)
    {
        if (!Multiplayer.IsActive || Multiplayer.Session.Warning.IsIncomingMarkerPacket)
        {
            return;
        }
        if (__instance.marker == null || __instance.factory == null)
        {
            return;
        }
        Multiplayer.Session.Network.SendPacket(
            new MarkerSettingUpdatePacket(__instance.factory.planetId, __instance.marker.id,
                MarkerSettingEvent.SetName, stringValue: __instance.marker.name));
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(UIMarkerDesc.OnWordInputFieldEndEdit))]
    public static void OnWordInputFieldEndEdit_Postfix(UIMarkerDesc __instance)
    {
        if (!Multiplayer.IsActive || Multiplayer.Session.Warning.IsIncomingMarkerPacket)
        {
            return;
        }
        if (__instance.marker == null || __instance.factory == null)
        {
            return;
        }
        Multiplayer.Session.Network.SendPacket(
            new MarkerSettingUpdatePacket(__instance.factory.planetId, __instance.marker.id,
                MarkerSettingEvent.SetWord, stringValue: __instance.marker.word));
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(UIMarkerDesc.OnTagsInputFieldEndEdit))]
    public static void OnTagsInputFieldEndEdit_Postfix(UIMarkerDesc __instance)
    {
        if (!Multiplayer.IsActive || Multiplayer.Session.Warning.IsIncomingMarkerPacket)
        {
            return;
        }
        if (__instance.marker == null || __instance.factory == null)
        {
            return;
        }
        Multiplayer.Session.Network.SendPacket(
            new MarkerSettingUpdatePacket(__instance.factory.planetId, __instance.marker.id,
                MarkerSettingEvent.SetTags, stringValue: __instance.marker.tags));
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(UIMarkerDesc.OnTodoInputFieldEndEdit))]
    public static void OnTodoInputFieldEndEdit_Postfix(UIMarkerDesc __instance)
    {
        if (!Multiplayer.IsActive || Multiplayer.Session.Warning.IsIncomingMarkerPacket)
        {
            return;
        }
        if (__instance.marker == null || __instance.factory == null)
        {
            return;
        }
        // Send both content and colors together - they're logically connected
        Multiplayer.Session.Network.SendPacket(
            new MarkerSettingUpdatePacket(__instance.factory.planetId, __instance.marker.id,
                MarkerSettingEvent.SetTodoContent,
                stringValue: __instance.marker.todo?.content,
                colorData: __instance.marker.todo?.contentColorIndex));
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(UIMarkerDesc.OnDigitalSignalIdEndEdit))]
    public static void OnDigitalSignalIdEndEdit(UIMarkerDesc __instance)
    {
        if (!Multiplayer.IsActive || Multiplayer.Session.Warning.IsIncomingMarkerPacket)
        {
            return;
        }
        if (__instance.marker == null || __instance.factory == null)
        {
            return;
        }
        Multiplayer.Session.Network.SendPacket(
            new MarkerSettingUpdatePacket(__instance.factory.planetId, __instance.marker.id,
                MarkerSettingEvent.SetDigitalSignalId,
                intValue: __instance.marker.digitalSignalId));
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(UIMarkerDesc.OnVisibilityBoxItemIndexChanged))]
    public static void OnVisibilityBoxItemIndexChanged_Postfix(UIMarkerDesc __instance)
    {
        if (!Multiplayer.IsActive || Multiplayer.Session.Warning.IsIncomingMarkerPacket)
        {
            return;
        }
        if (__instance.marker == null || __instance.factory == null)
        {
            return;
        }
        Multiplayer.Session.Network.SendPacket(
            new MarkerSettingUpdatePacket(__instance.factory.planetId, __instance.marker.id,
                MarkerSettingEvent.SetVisibility, intValue: (int)__instance.marker.visibility));
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(UIMarkerDesc.OnDetailLevelBoxItemIndexChanged))]
    public static void OnDetailLevelBoxItemIndexChanged_Postfix(UIMarkerDesc __instance)
    {
        if (!Multiplayer.IsActive || Multiplayer.Session.Warning.IsIncomingMarkerPacket)
        {
            return;
        }
        if (__instance.marker == null || __instance.factory == null)
        {
            return;
        }
        Multiplayer.Session.Network.SendPacket(
            new MarkerSettingUpdatePacket(__instance.factory.planetId, __instance.marker.id,
                MarkerSettingEvent.SetDetailLevel, intValue: (int)__instance.marker.detailLevel));
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(UIMarkerDesc.OnHeightSliderValueChanged))]
    public static void OnHeightSliderValueChanged_Postfix(UIMarkerDesc __instance)
    {
        if (!Multiplayer.IsActive || Multiplayer.Session.Warning.IsIncomingMarkerPacket)
        {
            return;
        }
        if (__instance.marker == null || __instance.factory == null)
        {
            return;
        }
        Multiplayer.Session.Network.SendPacket(
            new MarkerSettingUpdatePacket(__instance.factory.planetId, __instance.marker.id,
                MarkerSettingEvent.SetHeight, floatValue: __instance.marker.height));
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(UIMarkerDesc.OnRadiusSliderValueChanged))]
    public static void OnRadiusSliderValueChanged_Postfix(UIMarkerDesc __instance)
    {
        if (!Multiplayer.IsActive || Multiplayer.Session.Warning.IsIncomingMarkerPacket)
        {
            return;
        }
        if (__instance.marker == null || __instance.factory == null)
        {
            return;
        }
        Multiplayer.Session.Network.SendPacket(
            new MarkerSettingUpdatePacket(__instance.factory.planetId, __instance.marker.id,
                MarkerSettingEvent.SetRadius, floatValue: __instance.marker.radius));
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(UIMarkerDesc.OnIconSelectRightButtonClick))]
    public static void OnIconSelectRightButtonClick_Postfix(UIMarkerDesc __instance)
    {
        if (!Multiplayer.IsActive || Multiplayer.Session.Warning.IsIncomingMarkerPacket)
        {
            return;
        }
        if (__instance.marker == null || __instance.factory == null)
        {
            return;
        }
        Multiplayer.Session.Network.SendPacket(
            new MarkerSettingUpdatePacket(__instance.factory.planetId, __instance.marker.id,
                MarkerSettingEvent.SetIcon, intValue: __instance.marker.icon));
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(UIMarkerDesc.OnIconSelectLeftButtonClick))]
    public static void OnIconSelectLeftButtonClick_Postfix(UIMarkerDesc __instance)
    {
        if (!Multiplayer.IsActive || Multiplayer.Session.Warning.IsIncomingMarkerPacket)
        {
            return;
        }
        if (__instance.marker == null || __instance.factory == null)
        {
            return;
        }
        Multiplayer.Session.Network.SendPacket(
            new MarkerSettingUpdatePacket(__instance.factory.planetId, __instance.marker.id,
                MarkerSettingEvent.SetIcon, intValue: __instance.marker.icon));
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(UIMarkerDesc.OnIconSelectSignalPickerReturn))]
    public static void OnIconSelectSignalPickerReturn_Postfix(UIMarkerDesc __instance)
    {
        if (!Multiplayer.IsActive || Multiplayer.Session.Warning.IsIncomingMarkerPacket)
        {
            return;
        }
        if (__instance.marker == null || __instance.factory == null)
        {
            return;
        }
        Multiplayer.Session.Network.SendPacket(
            new MarkerSettingUpdatePacket(__instance.factory.planetId, __instance.marker.id,
                MarkerSettingEvent.SetIcon, intValue: __instance.marker.icon));
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(UIMarkerDesc.OnBriefSwitchButtonClick))]
    public static void OnBriefSwitchButtonClick_Postfix(UIMarkerDesc __instance)
    {
        if (!Multiplayer.IsActive || Multiplayer.Session.Warning.IsIncomingMarkerPacket)
        {
            return;
        }
        if (__instance.marker == null || __instance.factory == null)
        {
            return;
        }
        Multiplayer.Session.Network.SendPacket(
            new MarkerSettingUpdatePacket(__instance.factory.planetId, __instance.marker.id,
                MarkerSettingEvent.SetIcon, intValue: __instance.marker.icon));
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(UIMarkerDesc.OnColorPanelPickerReturn))]
    public static void OnColorPanelPickerReturn_Postfix(UIMarkerDesc __instance)
    {
        if (!Multiplayer.IsActive || Multiplayer.Session.Warning.IsIncomingMarkerPacket)
        {
            return;
        }
        if (__instance.marker == null || __instance.factory == null)
        {
            return;
        }
        Multiplayer.Session.Network.SendPacket(
            new MarkerSettingUpdatePacket(__instance.factory.planetId, __instance.marker.id,
                MarkerSettingEvent.SetColor, intValue: __instance.marker.color));
    }
}
