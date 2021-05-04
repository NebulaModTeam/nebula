using HarmonyLib;
using NebulaModel.Attributes;
using NebulaModel.Logger;
using NGPT;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using NebulaModel;
using System.ComponentModel;

namespace NebulaPatcher.Patches.Dynamic
{
    [HarmonyPatch(typeof(UIOptionWindow))]
    class UIOptionWindow_Patch
    {
        static RectTransform multiplayerTab;

        // Templates
        static RectTransform checkboxTemplate;
        static RectTransform comboBoxTemplate;
        static RectTransform sliderTemplate;
        static RectTransform inputTemplate;

        static RectTransform multiplayerContent;
        static int multiplayerTabIndex;
        static Dictionary<string, System.Action> tempToUICallbacks;

        static MultiplayerOptions tempMultiplayerOptions = new MultiplayerOptions();

        [HarmonyPostfix]
        [HarmonyPatch("_OnCreate")]
        public static void _OnCreate_Postfix(UIOptionWindow __instance)
        {
            tempToUICallbacks = new Dictionary<string, System.Action>();
            tempMultiplayerOptions = new MultiplayerOptions();

            // Add multiplayer tab button
            UIButton[] tabButtons = AccessTools.Field(__instance.GetType(), "tabButtons").GetValue(__instance) as UIButton[];
            multiplayerTabIndex = tabButtons.Length;
            RectTransform lastTab = tabButtons[tabButtons.Length - 1].GetComponent<RectTransform>();
            RectTransform beforeLastTab = tabButtons[tabButtons.Length - 2].GetComponent<RectTransform>();
            float tabOffset = lastTab.anchoredPosition.x - beforeLastTab.anchoredPosition.x;
            multiplayerTab = Object.Instantiate(lastTab, lastTab.parent, true);
            multiplayerTab.anchoredPosition = new Vector2(lastTab.anchoredPosition.x + tabOffset, lastTab.anchoredPosition.y);
            UIButton[] newTabButtons = CollectionExtensions.AddToArray(tabButtons, multiplayerTab.GetComponent<UIButton>());
            AccessTools.Field(__instance.GetType(), "tabButtons").SetValue(__instance, newTabButtons);

            // Update multiplayer tab text
            Text tabText = multiplayerTab.GetComponentInChildren<Text>();
            tabText.GetComponent<Localizer>().enabled = false;
            tabText.text = "Multiplayer";
            Text[] tabTexts = AccessTools.Field(__instance.GetType(), "tabTexts").GetValue(__instance) as Text[];
            Text[] newTabTexts = CollectionExtensions.AddToArray(tabTexts, tabText);
            AccessTools.Field(__instance.GetType(), "tabTexts").SetValue(__instance, newTabTexts);

            // Add multiplayer tab content
            Tweener[] tabTweeners = AccessTools.Field(__instance.GetType(), "tabTweeners").GetValue(__instance) as Tweener[];
            RectTransform contentTemplate = tabTweeners[0].GetComponent<RectTransform>();
            multiplayerContent = Object.Instantiate(contentTemplate, contentTemplate.parent, true);
            multiplayerContent.name = "multiplayer-content";

            Tweener[] newContents = CollectionExtensions.AddToArray(tabTweeners, multiplayerContent.GetComponent<Tweener>());
            AccessTools.Field(__instance.GetType(), "tabTweeners").SetValue(__instance, newContents);
            UIButton[] revertButtons = AccessTools.Field(__instance.GetType(), "revertButtons").GetValue(__instance) as UIButton[];
            RectTransform revertButton = multiplayerContent.Find("revert-button").GetComponent<RectTransform>();
            UIButton[] newRevertButtons = CollectionExtensions.AddToArray(revertButtons, revertButton.GetComponent<UIButton>());
            AccessTools.Field(__instance.GetType(), "revertButtons").SetValue(__instance, newRevertButtons);

            // Find control templates
            foreach (RectTransform child in multiplayerContent)
            {
                if (child != revertButton)
                {
                    Object.Destroy(child.gameObject);
                }
            }

            checkboxTemplate = contentTemplate.Find("fullscreen").GetComponent<RectTransform>();
            comboBoxTemplate = contentTemplate.Find("resolution").GetComponent<RectTransform>();
            sliderTemplate = contentTemplate.Find("dofblur").GetComponent<RectTransform>();

            inputTemplate = Object.Instantiate(checkboxTemplate, multiplayerContent, false);
            Object.Destroy(inputTemplate.Find("CheckBox").gameObject);
            RectTransform inputField = Object.Instantiate(UIRoot.instance.saveGameWindow.transform.Find("input-filename/InputField").GetComponent<RectTransform>(), inputTemplate, false);
            Vector2 fieldPosition = checkboxTemplate.GetChild(0).GetComponent<RectTransform>().anchoredPosition;
            inputField.anchoredPosition = new Vector2(fieldPosition.x + 6, fieldPosition.y);
            inputField.sizeDelta = new Vector2(inputField.sizeDelta.x, 35);
            inputTemplate.gameObject.SetActive(false);

            AddMultiplayerOptionsProperties(multiplayerContent);
        }

        [HarmonyPostfix]
        [HarmonyPatch("_OnDestroy")]
        public static void _OnDestroy_Postfix()
        {
            tempToUICallbacks?.Clear();
        }

        [HarmonyPrefix]
        [HarmonyPatch("_OnOpen")]
        public static void _OnOpen_Prefix()
        {
            tempMultiplayerOptions = (MultiplayerOptions)Config.Options.Clone();
        }

        [HarmonyPostfix]
        [HarmonyPatch("ApplyOptions")]
        public static void ApplyOptions()
        {
            Config.Options = tempMultiplayerOptions;
            Config.SaveOptions();
        }

        [HarmonyPrefix]
        [HarmonyPatch("OnRevertButtonClick")]
        public static void OnRevertButtonClick_Prefix(int idx)
        {
            if (idx == multiplayerTabIndex)
            {
                tempMultiplayerOptions = new MultiplayerOptions();
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch("TempOptionToUI")]
        public static void TempOptionToUI_Postfix()
        {
            List<PropertyInfo> properties = AccessTools.GetDeclaredProperties(typeof(MultiplayerOptions));
            foreach (var prop in properties)
            {
                if (tempToUICallbacks.TryGetValue(prop.Name, out var callback))
                {
                    callback();
                }
            }
        }

        private static void AddMultiplayerOptionsProperties(RectTransform container)
        {
            List<PropertyInfo> properties = AccessTools.GetDeclaredProperties(typeof(MultiplayerOptions));
            Vector2 anchorPosition = new Vector2(30, -20);

            foreach (var prop in properties)
            {
                DisplayNameAttribute displayAttr = prop.GetCustomAttribute<DisplayNameAttribute>();
                if (displayAttr != null)
                {
                    if (prop.PropertyType == typeof(bool))
                    {
                        CreateBooleanControl(displayAttr, prop, anchorPosition);
                    }
                    else if (prop.PropertyType == typeof(int) || prop.PropertyType == typeof(float) || prop.PropertyType == typeof(ushort))
                    {
                        CreateNumberControl(displayAttr, prop, anchorPosition);
                    }
                    else if (prop.PropertyType == typeof(string))
                    {
                        CreateStringControl(displayAttr, prop, anchorPosition);
                    }
                    else if (prop.PropertyType.IsEnum)
                    {
                        CreateEnumControl(displayAttr, prop, anchorPosition);
                    }
                    else
                    {
                        Log.Warn($"MultiplayerOption property \"${prop.Name}\" of type \"{prop.PropertyType}\" not supported.");
                        continue;
                    }

                    anchorPosition = new Vector2(anchorPosition.x, anchorPosition.y - 40);
                }
            }
        }

        private static void CreateBooleanControl(DisplayNameAttribute control, PropertyInfo prop, Vector2 anchorPosition)
        {
            RectTransform element = Object.Instantiate(checkboxTemplate, multiplayerContent, false);
            SetupUIElement(element, control, prop, anchorPosition);
            UIToggle toggle = element.GetComponentInChildren<UIToggle>();
            toggle.toggle.onValueChanged.RemoveAllListeners();
            toggle.toggle.onValueChanged.AddListener((value) => { prop.SetValue(tempMultiplayerOptions, value, null); });

            tempToUICallbacks[prop.Name] = () =>
            {
                toggle.isOn = (bool)prop.GetValue(tempMultiplayerOptions, null);
            };
        }

        private static void CreateNumberControl(DisplayNameAttribute control, PropertyInfo prop, Vector2 anchorPosition)
        {
            UIRangeAttribute rangeAttr = prop.GetCustomAttribute<UIRangeAttribute>();
            bool sliderControl = rangeAttr != null && rangeAttr.Slider;

            RectTransform element = Object.Instantiate(sliderControl ? sliderTemplate : inputTemplate, multiplayerContent, false);
            SetupUIElement(element, control, prop, anchorPosition);

            bool isFloatingPoint = prop.PropertyType == typeof(float) || prop.PropertyType == typeof(double);

            if (sliderControl)
            {
                Slider slider = element.GetComponentInChildren<Slider>();
                slider.minValue = rangeAttr.Min;
                slider.maxValue = rangeAttr.Max;
                slider.wholeNumbers = !isFloatingPoint;
                Text sliderThumbText = slider.GetComponentInChildren<Text>();
                slider.onValueChanged.RemoveAllListeners();
                slider.onValueChanged.AddListener((value) => 
                {
                    prop.SetValue(tempMultiplayerOptions, value, null);
                    sliderThumbText.text = value.ToString(isFloatingPoint ? "0.00" : "0");
                });

                tempToUICallbacks[prop.Name] = () =>
                {
                    slider.value = (float)prop.GetValue(tempMultiplayerOptions, null);
                    sliderThumbText.text = slider.value.ToString(isFloatingPoint ? "0.00" : "0");
                };
            }
            else
            {
                InputField input = element.GetComponentInChildren<InputField>();
                
                input.onValueChanged.RemoveAllListeners();
                input.onValueChanged.AddListener((str) => {
                    try
                    {
                        var converter = TypeDescriptor.GetConverter(prop.PropertyType);
                        System.IComparable value = (System.IComparable)converter.ConvertFromString(str);

                        if (rangeAttr != null)
                        {
                            System.IComparable min = (System.IComparable)System.Convert.ChangeType(rangeAttr.Min, prop.PropertyType);
                            System.IComparable max = (System.IComparable)System.Convert.ChangeType(rangeAttr.Max, prop.PropertyType);
                            if (value.CompareTo(min) < 0) value = min;
                            if (value.CompareTo(max) > 0) value = max;
                            input.text = value.ToString();
                        }

                        prop.SetValue(tempMultiplayerOptions, value, null);
                    } 
                    catch
                    {
                        // If the char is not a number, rollback to previous value
                        input.text = prop.GetValue(tempMultiplayerOptions, null).ToString();
                    }
                });

                tempToUICallbacks[prop.Name] = () =>
                {
                    input.text = prop.GetValue(tempMultiplayerOptions, null).ToString();
                };
            }
        }

        private static void CreateStringControl(DisplayNameAttribute control, PropertyInfo prop, Vector2 anchorPosition)
        {
            RectTransform element = Object.Instantiate(inputTemplate, multiplayerContent, false);
            SetupUIElement(element, control, prop, anchorPosition);

            InputField input = element.GetComponentInChildren<InputField>();
            input.onValueChanged.RemoveAllListeners();
            input.onValueChanged.AddListener((value) => { prop.SetValue(tempMultiplayerOptions, value, null); });

            tempToUICallbacks[prop.Name] = () =>
            {
                input.text = prop.GetValue(tempMultiplayerOptions, null) as string;
            };
        }

        private static void CreateEnumControl(DisplayNameAttribute control, PropertyInfo prop, Vector2 anchorPosition)
        {
            RectTransform element = Object.Instantiate(comboBoxTemplate, multiplayerContent, false);
            SetupUIElement(element, control, prop, anchorPosition);
            UIComboBox combo = element.GetComponentInChildren<UIComboBox>();
            combo.Items = System.Enum.GetNames(prop.PropertyType).ToList();
            combo.ItemsData = System.Enum.GetValues(prop.PropertyType).OfType<int>().ToList();
            combo.onItemIndexChange.RemoveAllListeners();
            combo.onItemIndexChange.AddListener(() => { prop.SetValue(tempMultiplayerOptions, combo.itemIndex, null); });

            tempToUICallbacks[prop.Name] = () =>
            {
                combo.itemIndex = (int)prop.GetValue(tempMultiplayerOptions, null);
            };
        }

        private static void SetupUIElement(RectTransform element, DisplayNameAttribute display, PropertyInfo prop, Vector2 anchorPosition)
        {
            element.gameObject.SetActive(true);
            element.name = prop.Name;
            element.anchoredPosition = anchorPosition;
            element.GetComponent<Localizer>().enabled = false;
            element.GetComponent<Text>().text = display.DisplayName;
        }
    }
}
