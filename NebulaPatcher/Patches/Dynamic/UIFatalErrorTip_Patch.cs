#region

using System;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using BepInEx.Bootstrap;
using HarmonyLib;
using NebulaModel.DataStructures.Chat;
using NebulaModel.Logger;
using NebulaModel.Packets.Chat;
using NebulaWorld;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

#endregion

namespace NebulaPatcher.Patches.Dynamic;

[HarmonyPatch(typeof(UIFatalErrorTip))]
internal class UIFatalErrorTip_Patch
{
    private static UIButton btnClose;
    private static UIButton btnCopy;

    [HarmonyPostfix]
    [HarmonyPatch(nameof(UIFatalErrorTip._OnRegEvent))]
    [SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Original Function Name")]
    public static void _OnRegEvent_Postfix()
    {
        // If there is error message before game begin, we will show to user here
        if (Log.LastErrorMsg == null)
        {
            return;
        }
        UIFatalErrorTip.instance.ShowError("[Nebula Error] " + Log.LastErrorMsg, "");
        Log.LastErrorMsg = null;
    }

    [HarmonyPostfix, HarmonyAfter("aaa.dsp.plugin.ErrorAnalyzer")]
    [HarmonyPatch(nameof(UIFatalErrorTip._OnOpen))]
    [SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Original Function Name")]
    public static void _OnOpen_Postfix(UIFatalErrorTip __instance)
    {
        try
        {
            TryCreateButton(() => CreateCloseBtn(__instance), "Close Button");
            TryCreateButton(() => CreateCopyBtn(__instance), "Copy Button");
            __instance.transform.Find("tip-text-0").GetComponent<Text>().text = Title();
            __instance.transform.Find("tip-text-1").GetComponent<Text>().text = Title();
            Object.Destroy(__instance.transform.Find("tip-text-0").GetComponent<Localizer>());
            Object.Destroy(__instance.transform.Find("tip-text-1").GetComponent<Localizer>());

            DedicatedServerReportError();
        }
        catch (Exception e)
        {
            Log.Warn($"UIFatalErrorTip button did not patch! {e}");
        }
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(UIFatalErrorTip._OnClose))]
    public static void _OnClose_Postfix()
    {
        if (btnClose != null)
        {
            Object.Destroy(btnClose.gameObject);
            btnClose = null;
        }
        if (btnCopy != null)
        {
            Object.Destroy(btnCopy.gameObject);
            btnCopy = null;
        }
    }

    private static void TryCreateButton(Action createAction, string buttonName)
    {
        try
        {
            createAction();
        }
        catch (Exception e)
        {
            Log.Warn($"{buttonName} did not patch!\n{e}");
        }
    }

    private static UIButton CreateButton(string path, Transform parent, Vector3 positionOffset, Action<int> onClickAction)
    {
        var go = GameObject.Find(path);
        return CreateButton(go, parent, positionOffset, onClickAction);
    }

    private static UIButton CreateButton(GameObject originalGo, Transform parent, Vector3 positionOffset, Action<int> onClickAction)
    {
        if (originalGo != null)
        {
            var go = Object.Instantiate(originalGo, parent);
            var rect = (RectTransform)go.transform;
            rect.anchorMin = Vector2.up;
            rect.anchorMax = Vector2.up;
            rect.pivot = Vector2.up;
            rect.anchoredPosition = positionOffset;
            go.SetActive(true);

            var button = go.GetComponent<UIButton>();
            button.onClick += onClickAction;
            button.tips.corner = 1;
            return button;
        }
        return null;
    }

    private static void CreateCloseBtn(UIFatalErrorTip __instance)
    {
        if (btnClose != null) return;

        const string PATH = "UI Root/Overlay Canvas/In Game/Common Tools/Color Palette Panel/panel-bg/btn-box/close-wnd-btn";
        btnClose = CreateButton(PATH, __instance.transform, new Vector3(-5, 0, 0), OnCloseClick);
    }

    private static void CreateCopyBtn(UIFatalErrorTip __instance)
    {
        if (btnCopy != null) return;

        const string PATH = "UI Root/Overlay Canvas/In Game/Windows/Dyson Sphere Editor/Dyson Editor Control Panel/hierarchy/layers/blueprint-group/blueprint-2/copy-button";
        btnCopy = CreateButton(PATH, __instance.transform, new Vector3(5, -55, 0), OnCopyClick);
        btnCopy.tips.tipTitle = "Copy Error".Translate();
        btnCopy.tips.tipText = "Copy the message to clipboard".Translate();
    }

    private static void DedicatedServerReportError()
    {
        // OnOpen only run once for the first error report
        if (!Multiplayer.IsDedicated || !Multiplayer.IsActive)
        {
            return;
        }
        var log = "Server report an error: \n" + UIFatalErrorTip.instance.errorLogText.text;
        Log.Warn(log);
        Multiplayer.Session.Network.SendPacket(new NewChatMessagePacket(ChatMessageType.SystemWarnMessage, log,
            DateTime.Now, ""));
    }

    private static string Title()
    {
        var stringBuilder = new StringBuilder();
        stringBuilder.Append("An error has occurred! Game version ");
        stringBuilder.Append(GameConfig.gameVersion.ToString());
        stringBuilder.Append('.');
        stringBuilder.Append(GameConfig.gameVersion.Build);
        if (Multiplayer.IsActive)
        {
            stringBuilder.Append(Multiplayer.Session.LocalPlayer.IsHost ? " (Host)" : " (Client)");
        }
        stringBuilder.AppendLine();
        stringBuilder.Append(Chainloader.PluginInfos.Values.Count + " Mods used: ");
        foreach (var pluginInfo in Chainloader.PluginInfos.Values)
        {
            stringBuilder.Append('[');
            stringBuilder.Append(pluginInfo.Metadata.Name);
            stringBuilder.Append(pluginInfo.Metadata.Version);
            stringBuilder.Append("] ");
        }
        return stringBuilder.ToString();
    }

    private static void OnCloseClick(int _)
    {
        UIFatalErrorTip.ClearError();
    }

    private static void OnCopyClick(int id)
    {
        var stringBuilder = new StringBuilder();
        stringBuilder.AppendLine("```ini");
        stringBuilder.AppendLine(Title());
        var subs = UIFatalErrorTip.instance.errorLogText.text.Split('\n', '\r');
        foreach (var str in subs)
        {
            if (string.IsNullOrEmpty(str))
            {
                continue;
            }

            // Nebula only: skip the message after PacketProcessor
            if (str.StartsWith("NebulaModel.Packets.PacketProcessor") || str.StartsWith("  at NebulaModel.Packets.PacketProcessor"))
            {
                break;
            }

            // Remove hash string
            var start = str.LastIndexOf(" <", StringComparison.Ordinal);
            var end = str.LastIndexOf(">:", StringComparison.Ordinal);
            if (start != -1 && end > start)
            {
                stringBuilder.AppendLine(str.Remove(start, end - start + 2));
            }
            else
            {
                stringBuilder.AppendLine(str);
            }
        }
        // Apply format for ini code style
        stringBuilder.Replace(" (at", ";(");
        stringBuilder.Replace(" inIL_", " ;IL_");
        stringBuilder.AppendLine("```");

        // Copy string to clipboard
        GUIUtility.systemCopyBuffer = stringBuilder.ToString();
    }
}
