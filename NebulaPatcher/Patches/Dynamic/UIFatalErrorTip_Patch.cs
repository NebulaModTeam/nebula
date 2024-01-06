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
    private static GameObject button;

    [HarmonyPostfix]
    [HarmonyPatch(nameof(UIFatalErrorTip._OnRegEvent))]
    [SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Original Function Name")]
    public static void _OnRegEvent_Postfix()
    {
        // If there is errer message before game begin, we will show to user here
        if (Log.LastErrorMsg == null)
        {
            return;
        }
        UIFatalErrorTip.instance.ShowError("[Nebula Error] " + Log.LastErrorMsg, "");
        Log.LastErrorMsg = null;
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(UIFatalErrorTip._OnOpen))]
    [SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Original Function Name")]
    public static void _OnOpen_Postfix()
    {
        try
        {
            if (button == null)
            {
                button = GameObject.Find(
                    "UI Root/Overlay Canvas/In Game/Windows/Dyson Sphere Editor/Dyson Editor Control Panel/hierarchy/layers/blueprint-group/blueprint-2/copy-button");
                var errorPanel = GameObject.Find("UI Root/Overlay Canvas/Fatal Error/errored-panel/");
                errorPanel.transform.Find("tip-text-0").GetComponent<Text>().text = Title();
                Object.Destroy(errorPanel.transform.Find("tip-text-0").GetComponent<Localizer>());
                errorPanel.transform.Find("tip-text-1").GetComponent<Text>().text = Title();
                Object.Destroy(errorPanel.transform.Find("tip-text-1").GetComponent<Localizer>());
                button = Object.Instantiate(button, errorPanel.transform);
                button.name = "Copy & Close button";
                button.transform.localPosition =
                    errorPanel.transform.Find("icon").localPosition + new Vector3(30, -35, 0); //-885 -30 //-855 -60
                button.GetComponent<Image>().color = new Color(0.3113f, 0f, 0.0097f, 0.6f);
                button.GetComponent<UIButton>().BindOnClickSafe(OnClick);
                ref var tips = ref button.GetComponent<UIButton>().tips;
                tips.tipTitle = "Copy & Close Error";
                tips.tipText = "Copy the message to clipboard and close error.";
                tips.corner = 1;
            }

            DedicatedServerReportError();
        }
        catch (Exception e)
        {
            Log.Warn($"UIFatalErrorTip button did not patch! {e}");
        }
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
        Multiplayer.Session.Network.SendToAll(new NewChatMessagePacket(ChatMessageType.SystemWarnMessage, log,
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

    private static void OnClick(int id)
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
            if (str.StartsWith("NebulaModel.Packets.PacketProcessor"))
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
        UIFatalErrorTip.ClearError();
        Object.Destroy(button);
        button = null;
    }
}
