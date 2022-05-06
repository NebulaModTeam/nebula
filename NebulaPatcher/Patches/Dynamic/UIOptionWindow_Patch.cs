using HarmonyLib;
using NebulaModel;
using NebulaModel.Attributes;
using NebulaModel.Logger;
using NebulaWorld;
using NGPT;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace NebulaPatcher.Patches.Dynamic
{
    [HarmonyPatch(typeof(UIOptionWindow))]
    internal class UIOptionWindow_Patch
    {
        public class Tooltip : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
        {
            public string Title = null;
            public string Text = null;
            UIButtonTip tip = null;

            public void OnPointerEnter(PointerEventData eventData)
            {
                tip = UIButtonTip.Create(true, Title, Text, 2, new Vector2(0, 0), 508, this.gameObject.transform, "", "");
            }

            public void OnPointerExit(PointerEventData eventData)
            {
                if(tip != null)
                {
                    Destroy(tip.gameObject);
                }
            }

            public void OnDisable()
            {
                if (tip != null)
                {
                    Destroy(tip.gameObject);
                }
            }
        }

        private static RectTransform multiplayerTab;

        // Templates
        private static RectTransform checkboxTemplate;
        private static RectTransform comboBoxTemplate;
        private static RectTransform sliderTemplate;
        private static RectTransform inputTemplate;
        private static RectTransform multiplayerContent;
        private static int multiplayerTabIndex;
        private static Dictionary<string, System.Action> tempToUICallbacks;
        private static MultiplayerOptions tempMultiplayerOptions = new MultiplayerOptions();

        [HarmonyPostfix]
        [HarmonyPatch(nameof(UIOptionWindow._OnCreate))]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Original Function Name")]
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
            UIButton[] newTabButtons = tabButtons.AddToArray(multiplayerTab.GetComponent<UIButton>());
            __instance.tabButtons = newTabButtons;

            // Update multiplayer tab text
            Text tabText = multiplayerTab.GetComponentInChildren<Text>();
            tabText.GetComponent<Localizer>().enabled = false;
            tabText.text = "Multiplayer";
            Text[] tabTexts = __instance.tabTexts;
            Text[] newTabTexts = tabTexts.AddToArray(tabText);
            __instance.tabTexts = newTabTexts;

            // Add multiplayer tab content
            Tweener[] tabTweeners = __instance.tabTweeners;
            RectTransform contentTemplate = tabTweeners[0].GetComponent<RectTransform>();
            multiplayerContent = Object.Instantiate(contentTemplate, contentTemplate.parent, true);
            multiplayerContent.name = "multiplayer-content";

            Tweener[] newContents = tabTweeners.AddToArray(multiplayerContent.GetComponent<Tweener>());
            __instance.tabTweeners = newContents;
            UIButton[] revertButtons = __instance.revertButtons;
            RectTransform revertButton = multiplayerContent.Find("revert-button").GetComponent<RectTransform>();
            UIButton[] newRevertButtons = revertButtons.AddToArray(revertButton.GetComponent<UIButton>());
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
            list.name = "list";
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
        [HarmonyPatch(nameof(UIOptionWindow._OnDestroy))]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Original Function Name")]
        public static void _OnDestroy_Postfix()
        {
            tempToUICallbacks?.Clear();
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(UIOptionWindow._OnOpen))]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Original Function Name")]
        public static void _OnOpen_Prefix()
        {
            tempMultiplayerOptions = (MultiplayerOptions)Config.Options.Clone();
        }

        [HarmonyPostfix]
        [HarmonyPatch(nameof(UIOptionWindow.ApplyOptions))]
        public static void ApplyOptions()
        {
            Config.Options = tempMultiplayerOptions;
            Config.SaveOptions();
            Config.OnConfigApplied?.Invoke();
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(UIOptionWindow.OnRevertButtonClick))]
        public static void OnRevertButtonClick_Prefix(int idx)
        {
            if (idx == multiplayerTabIndex)
            {
                tempMultiplayerOptions = new MultiplayerOptions();
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(nameof(UIOptionWindow.TempOptionToUI))]
        public static void TempOptionToUI_Postfix()
        {
            List<PropertyInfo> properties = AccessTools.GetDeclaredProperties(typeof(MultiplayerOptions));
            foreach (PropertyInfo prop in properties)
            {
                if (tempToUICallbacks.TryGetValue(prop.Name, out System.Action callback))
                {
                    callback();
                }
            }
        }

        private static void AddMultiplayerOptionsProperties(RectTransform container)
        {
            List<PropertyInfo> properties = AccessTools.GetDeclaredProperties(typeof(MultiplayerOptions));
            Vector2 anchorPosition = new Vector2(30, -20);

            foreach (PropertyInfo prop in properties)
            {
                DisplayNameAttribute displayAttr = prop.GetCustomAttribute<DisplayNameAttribute>();
                DescriptionAttribute descriptionAttr = prop.GetCustomAttribute<DescriptionAttribute>();
                if (displayAttr != null)
                {
                    if (prop.PropertyType == typeof(bool))
                    {
                        CreateBooleanControl(displayAttr, descriptionAttr, prop, anchorPosition, container);
                    }
                    else if (prop.PropertyType == typeof(int) || prop.PropertyType == typeof(float) || prop.PropertyType == typeof(ushort))
                    {
                        CreateNumberControl(displayAttr, descriptionAttr, prop, anchorPosition, container);
                    }
                    else if (prop.PropertyType == typeof(string))
                    {
                        CreateStringControl(displayAttr, descriptionAttr, prop, anchorPosition, container);
                    }
                    else if (prop.PropertyType.IsEnum)
                    {
                        CreateEnumControl(displayAttr, descriptionAttr, prop, anchorPosition, container);
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

        private static void CreateBooleanControl(DisplayNameAttribute control, DescriptionAttribute descriptionAttr, PropertyInfo prop, Vector2 anchorPosition, RectTransform container)
        {
            RectTransform element = Object.Instantiate(checkboxTemplate, container, false);
            SetupUIElement(element, control, descriptionAttr, prop, anchorPosition);
            UIToggle toggle = element.GetComponentInChildren<UIToggle>();
            toggle.toggle.onValueChanged.RemoveAllListeners();
            toggle.toggle.onValueChanged.AddListener((value) => {

                // lock soil setting while in multiplayer game
                if (control.DisplayName == "Sync Soil" && Multiplayer.IsActive)
                {
                    // reset to saved value if needed
                    if(value != (bool)prop.GetValue(tempMultiplayerOptions, null))
                    {
                        toggle.isOn = !value;
                        InGamePopup.ShowInfo("Info", "This setting can only be changed while not in game", "Okay");
                    }
                    return;
                }

                // Hide Ngrok Authtoken when Streamer mode is on
                if (control.DisplayName == "Streamer mode")
                {
                    InputField input = GameObject.Find("list/scroll-view/viewport/content/NgrokAuthtoken")?.GetComponentInChildren<InputField>();
                    if (input != null)
                    {
                        input.contentType = value ? InputField.ContentType.Password : InputField.ContentType.Standard;
                        input.UpdateLabel();
                    }
                }

                prop.SetValue(tempMultiplayerOptions, value, null);

            });

            tempToUICallbacks[prop.Name] = () =>
            {
                toggle.isOn = (bool)prop.GetValue(tempMultiplayerOptions, null);
            };
        }

        private static void CreateNumberControl(DisplayNameAttribute control, DescriptionAttribute descriptionAttr, PropertyInfo prop, Vector2 anchorPosition, RectTransform container)
        {
            UIRangeAttribute rangeAttr = prop.GetCustomAttribute<UIRangeAttribute>();
            bool sliderControl = rangeAttr != null && rangeAttr.Slider;

            RectTransform element = Object.Instantiate(sliderControl ? sliderTemplate : inputTemplate, container, false);
            SetupUIElement(element, control, descriptionAttr, prop, anchorPosition);

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
                        TypeConverter converter = TypeDescriptor.GetConverter(prop.PropertyType);
                        System.IComparable value = (System.IComparable)converter.ConvertFromString(str);

                        if (rangeAttr != null)
                        {
                            System.IComparable min = (System.IComparable)System.Convert.ChangeType(rangeAttr.Min, prop.PropertyType);
                            System.IComparable max = (System.IComparable)System.Convert.ChangeType(rangeAttr.Max, prop.PropertyType);
                            if (value.CompareTo(min) < 0)
                            {
                                value = min;
                            }

                            if (value.CompareTo(max) > 0)
                            {
                                value = max;
                            }

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

        private static void CreateStringControl(DisplayNameAttribute control, DescriptionAttribute descriptionAttr, PropertyInfo prop, Vector2 anchorPosition, RectTransform container)
        {
            UICharacterLimitAttribute characterLimitAttr = prop.GetCustomAttribute<UICharacterLimitAttribute>();

            RectTransform element = Object.Instantiate(inputTemplate, container, false);
            SetupUIElement(element, control, descriptionAttr, prop, anchorPosition);

            InputField input = element.GetComponentInChildren<InputField>();
            if (characterLimitAttr != null)
            {
                input.characterLimit = characterLimitAttr.Max;
            }
            input.onValueChanged.RemoveAllListeners();
            input.onValueChanged.AddListener((value) => { prop.SetValue(tempMultiplayerOptions, value, null); });

            tempToUICallbacks[prop.Name] = () =>
            {
                input.text = prop.GetValue(tempMultiplayerOptions, null) as string;
            };
        }

        private static void CreateEnumControl(DisplayNameAttribute control, DescriptionAttribute descriptionAttr, PropertyInfo prop, Vector2 anchorPosition, RectTransform container)
        {
            RectTransform element = Object.Instantiate(comboBoxTemplate, container, false);
            SetupUIElement(element, control, descriptionAttr, prop, anchorPosition);
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

        private static void SetupUIElement(RectTransform element, DisplayNameAttribute display, DescriptionAttribute descriptionAttr, PropertyInfo prop, Vector2 anchorPosition)
        {
            element.gameObject.SetActive(true);
            element.name = prop.Name;
            element.anchoredPosition = anchorPosition;
            if(descriptionAttr != null)
            {
                element.gameObject.AddComponent<Tooltip>();
                element.gameObject.GetComponent<Tooltip>().Title = display.DisplayName;
                element.gameObject.GetComponent<Tooltip>().Text = descriptionAttr.Description;
            }
            element.GetComponent<Localizer>().enabled = false;
            element.GetComponent<Text>().text = display.DisplayName;
        }
    }
}
