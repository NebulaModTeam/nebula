using BepInEx.Bootstrap;
using HarmonyLib;
using NebulaModel.DataStructures;
using NebulaModel.Logger;
using NebulaModel.Packets.Players;
using NebulaWorld;
using System;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace NebulaPatcher.Patches.Dynamic
{
    [HarmonyPatch(typeof(UIFatalErrorTip))]
    internal class UIFatalErrorTip_Patch
    {
        static GameObject button;

        [HarmonyPostfix]
        [HarmonyPatch(nameof(UIFatalErrorTip._OnRegEvent))]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Original Function Name")]
        public static void _OnRegEvent_Postfix()
        {
            // If there is errer message before game begin, we will show to user here
            if (Log.LastErrorMsg != null)
            {
                UIFatalErrorTip.instance.ShowError("[Nebula Error] " + Log.LastErrorMsg, "");
                Log.LastErrorMsg = null;
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(nameof(UIFatalErrorTip._OnOpen))]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Original Function Name")]
        public static void _OnOpen_Postfix(UIFatalErrorTip __instance)
        {
            try
            {
                if (button == null)
                {
                    button = GameObject.Find("UI Root/Overlay Canvas/In Game/Windows/Dyson Sphere Editor/Dyson Editor Control Panel/hierarchy/layers/blueprint-group/blueprint-2/copy-button");
                    GameObject errorPanel = GameObject.Find("UI Root/Overlay Canvas/Fatal Error/errored-panel/");
                    errorPanel.transform.Find("tip-text-0").GetComponent<Text>().text = Title();
                    GameObject.Destroy(errorPanel.transform.Find("tip-text-0").GetComponent<Localizer>());
                    errorPanel.transform.Find("tip-text-1").GetComponent<Text>().text = Title();
                    GameObject.Destroy(errorPanel.transform.Find("tip-text-1").GetComponent<Localizer>());
                    button = GameObject.Instantiate(button, errorPanel.transform);
                    button.name = "Copy & Close button";
                    button.transform.localPosition = errorPanel.transform.Find("icon").localPosition + new Vector3(30, -35, 0); //-885 -30 //-855 -60
                    button.GetComponent<Image>().color = new Color(0.3113f, 0f, 0.0097f, 0.6f);
                    button.GetComponent<UIButton>().BindOnClickSafe(OnClick);
                    button.GetComponent<UIButton>().tips = new UIButton.TipSettings();
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
            if (Multiplayer.IsDedicated && Multiplayer.IsActive)
            {
                string log = "Server report an error: \n" + UIFatalErrorTip.instance.errorLogText.text;
                Log.Warn(log);
                Multiplayer.Session.Network.SendPacket(new NewChatMessagePacket(ChatMessageType.SystemWarnMessage, log, DateTime.Now, ""));
            }
        }

        private static string Title()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append("An error has occurred! Game version ");
            stringBuilder.Append(GameConfig.gameVersion.ToString());
            stringBuilder.Append('.');
            stringBuilder.Append(GameConfig.gameVersion.Build);
            if (Multiplayer.IsActive)
            {
                stringBuilder.Append(Multiplayer.Session.LocalPlayer.IsHost ? " (Host)" : " (Client)");
            }
            stringBuilder.AppendLine();
            stringBuilder.Append("Mods used: ");
            foreach (BepInEx.PluginInfo pluginInfo in Chainloader.PluginInfos.Values)
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
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.AppendLine("```ini");
            stringBuilder.AppendLine(Title());
            string[] subs = UIFatalErrorTip.instance.errorLogText.text.Split('\n', '\r');
            foreach (string str in subs)
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
                int start = str.LastIndexOf(" <");
                int end = str.LastIndexOf(">:");
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
            GameObject.Destroy(button);
            button = null;
        }
    }
}
