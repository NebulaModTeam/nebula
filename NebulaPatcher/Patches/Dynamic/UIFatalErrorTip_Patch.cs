﻿using BepInEx;
using BepInEx.Bootstrap;
using HarmonyLib;
using NebulaModel.Logger;
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
        [HarmonyPatch(nameof(UIFatalErrorTip._OnOpen))]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Original Function Name")]
        public static void _OnOpen_Postfix()
        {
            try
            {
                if (button == null)
                {
                    button = GameObject.Find("UI Root/Overlay Canvas/In Game/Windows/Dyson Sphere Editor/Dyson Editor Control Panel/hierarchy/layers/blueprint-group/blueprint-2/copy-button");
                    GameObject errorPanel = GameObject.Find("UI Root/Overlay Canvas/Fatal Error/errored-panel/");
                    errorPanel.transform.Find("tip-text-1").GetComponent<Text>().text = Title();
                    GameObject.Destroy(errorPanel.transform.Find("tip-text-1").GetComponent<Localizer>());
                    button = GameObject.Instantiate(button, errorPanel.transform);
                    button.name = "Copy & Close button";
                    button.transform.localPosition = errorPanel.transform.Find("icon").localPosition + new Vector3(30, -35, 0); //-885 -30 //-855 -60
                    button.GetComponent<Image>().color = new Color(0.3113f, 0f, 0.0097f, 0.6f);
                    button.GetComponent<UIButton>().BindOnClickSafe(OnClick);
                    button.GetComponent<UIButton>().tips = new UIButton.TipSettings();
                }
            }
            catch (Exception e)
            {
                Log.Warn($"UIFatalErrorTip button did not patch! {e}");
            }
        }

        private static string Title()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append("This game has occured an error! Game version ");
            stringBuilder.Append(GameConfig.gameVersion.ToString());
            stringBuilder.Append('.');
            stringBuilder.Append(GameConfig.gameVersion.Build);
            stringBuilder.AppendLine();
            stringBuilder.Append("Mods use: ");
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
            stringBuilder.AppendLine("```");
            stringBuilder.AppendLine(Title());
            stringBuilder.Append(UIFatalErrorTip.instance.errorLogText.text);
            stringBuilder.AppendLine("```");
            // Copy string to clipboard
            GUIUtility.systemCopyBuffer = stringBuilder.ToString();
            UIFatalErrorTip.ClearError();
            GameObject.Destroy(button);
            button = null;
        }
    }
}
