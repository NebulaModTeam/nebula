using HarmonyLib;
using NebulaModel;
using NebulaModel.Attributes;
using NebulaModel.Logger;
using NGPT;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

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
            UIButton[] tabButtons = __instance.tabButtons;
            multiplayerTabIndex = tabButtons.Length;
            RectTransform lastTab = tabButtons[tabButtons.Length - 1].GetComponent<RectTransform>();
            RectTransform beforeLastTab = tabButtons[tabButtons.Length - 2].GetComponent<RectTransform>();
            float tabOffset = lastTab.anchoredPosition.x - beforeLastTab.anchoredPosition.x;
            multiplayerTab = Object.Instantiate(lastTab, lastTab.parent, true);
            multiplayerTab.anchoredPosition = new Vector2(lastTab.anchoredPosition.x + tabOffset, lastTab.anchoredPosition.y);
            UIButton[] newTabButtons = CollectionExtensions.AddToArray(tabButtons, multiplayerTab.GetComponent<UIButton>());
            __instance.tabButtons = newTabButtons;

            // Update multiplayer tab text
            Text tabText = multiplayerTab.GetComponentInChildren<Text>();
            tabText.GetComponent<Localizer>().enabled = false;
            tabText.text = "Multiplayer";
            Text[] tabTexts = __instance.tabTexts;
            Text[] newTabTexts = CollectionExtensions.AddToArray(tabTexts, tabText);
            __instance.tabTexts = newTabTexts;

            // Add multiplayer tab content
            Tweener[] tabTweeners = __instance.tabTweeners;
            RectTransform contentTemplate = tabTweeners[0].GetComponent<RectTransform>();
            multiplayerContent = Object.Instantiate(contentTemplate, contentTemplate.parent, true);
            multiplayerContent.name = "multiplayer-content";

            Tweener[] newContents = CollectionExtensions.AddToArray(tabTweeners, multiplayerContent.GetComponent<Tweener>());
            __instance.tabTweeners = newContents;
            UIButton[] revertButtons = __instance.revertButtons;
            RectTransform revertButton = multiplayerContent.Find("revert-button").GetComponent<RectTransform>();
            UIButton[] newRevertButtons = CollectionExtensions.AddToArray(revertButtons, revertButton.GetComponent<UIButton>());
            __instance.revertButtons = newRevertButtons;

            // Remove unwanted GameObject
            foreach (RectTransform child in multiplayerContent)
            {
                if (child != revertButton)
                {
                    Object.Destroy(child.gameObject);
                }
            }

            // Add ScrollView
            RectTransform list = Object.Instantiate(tabTweeners[3].transform.Find("list").GetComponent<RectTransform>(), multiplayerContent);
            list.offsetMax = Vector2.zero;
            RectTransform listContent = list.Find("scroll-view/viewport/content").GetComponent<RectTransform>();
            foreach (RectTransform child in listContent)
            {
                Object.Destroy(child.gameObject);
            }

            // Find control templates
            checkboxTemplate = contentTemplate.Find("fullscreen").GetComponent<RectTransform>();
            comboBoxTemplate = contentTemplate.Find("resolution").GetComponent<RectTransform>();
            sliderTemplate = contentTemplate.Find("dofblur").GetComponent<RectTransform>();

            inputTemplate = Object.Instantiate(checkboxTemplate, listContent, false);
            Object.Destroy(inputTemplate.Find("CheckBox").gameObject);
            RectTransform inputField = Object.Instantiate(UIRoot.instance.saveGameWindow.transform.Find("input-filename/InputField").GetComponent<RectTransform>(), inputTemplate, false);
            Vector2 fieldPosition = checkboxTemplate.GetChild(0).GetComponent<RectTransform>().anchoredPosition;
            inputField.anchoredPosition = new Vector2(fieldPosition.x + 6, fieldPosition.y);
            inputField.sizeDelta = new Vector2(inputField.sizeDelta.x, 35);
            inputTemplate.gameObject.SetActive(false);

            AddMultiplayerOptionsProperties(listContent);
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
                        CreateBooleanControl(displayAttr, prop, anchorPosition, container);
                    }
                    else if (prop.PropertyType == typeof(int) || prop.PropertyType == typeof(float) || prop.PropertyType == typeof(ushort))
                    {
                        CreateNumberControl(displayAttr, prop, anchorPosition, container);
                    }
                    else if (prop.PropertyType == typeof(string))
                    {
                        CreateStringControl(displayAttr, prop, anchorPosition, container);
                    }
                    else if (prop.PropertyType.IsEnum)
                    {
                        CreateEnumControl(displayAttr, prop, anchorPosition, container);
                    }
                    else
                    {
                        Log.Warn($"MultiplayerOption property \"${prop.Name}\" of type \"{prop.PropertyType}\" not supported.");
                        continue;
                    }

                    anchorPosition = new Vector2(anchorPosition.x, anchorPosition.y - 40);
                }
            }

            container.sizeDelta = new Vector2(container.sizeDelta.x, -anchorPosition.y + 40);
        }

        private static void CreateBooleanControl(DisplayNameAttribute control, PropertyInfo prop, Vector2 anchorPosition, RectTransform container)
        {
            RectTransform element = Object.Instantiate(checkboxTemplate, container, false);
            SetupUIElement(element, control, prop, anchorPosition);
            UIToggle toggle = element.GetComponentInChildren<UIToggle>();
            toggle.toggle.onValueChanged.RemoveAllListeners();
            toggle.toggle.onValueChanged.AddListener((value) => { prop.SetValue(tempMultiplayerOptions, value, null); });

            tempToUICallbacks[prop.Name] = () =>
            {
                toggle.isOn = (bool)prop.GetValue(tempMultiplayerOptions, null);
            };
        }

        private static void CreateNumberControl(DisplayNameAttribute control, PropertyInfo prop, Vector2 anchorPosition, RectTransform container)
        {
            UIRangeAttribute rangeAttr = prop.GetCustomAttribute<UIRangeAttribute>();
            bool sliderControl = rangeAttr != null && rangeAttr.Slider;

            RectTransform element = Object.Instantiate(sliderControl ? sliderTemplate : inputTemplate, container, false);
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
                input.onValueChanged.AddListener((str) =>
                {
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

        private static void CreateStringControl(DisplayNameAttribute control, PropertyInfo prop, Vector2 anchorPosition, RectTransform container)
        {
            RectTransform element = Object.Instantiate(inputTemplate, container, false);
            SetupUIElement(element, control, prop, anchorPosition);

            InputField input = element.GetComponentInChildren<InputField>();
            input.onValueChanged.RemoveAllListeners();
            input.onValueChanged.AddListener((value) => { prop.SetValue(tempMultiplayerOptions, value, null); });

            tempToUICallbacks[prop.Name] = () =>
            {
                input.text = prop.GetValue(tempMultiplayerOptions, null) as string;
            };
        }

        private static void CreateEnumControl(DisplayNameAttribute control, PropertyInfo prop, Vector2 anchorPosition, RectTransform container)
        {
            RectTransform element = Object.Instantiate(comboBoxTemplate, container, false);
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
