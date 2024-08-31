#region

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using BepInEx.Configuration;
using HarmonyLib;
using NebulaModel;
using NebulaModel.Attributes;
using NebulaModel.Logger;
using NebulaWorld;
using NGPT;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Object = UnityEngine.Object;

#endregion

namespace NebulaPatcher.Patches.Dynamic;

[HarmonyPatch(typeof(UIOptionWindow))]
internal class UIOptionWindow_Patch
{
    private const float SubtabOffset = 160f;

    // Templates
    private static RectTransform checkboxTemplate;
    private static RectTransform comboBoxTemplate;
    private static RectTransform sliderTemplate;
    private static RectTransform inputTemplate;
    private static RectTransform subtabTemplate;
    private static RectTransform multiplayerContent;
    private static int multiplayerTabIndex;
    private static Dictionary<string, Action> tempToUICallbacks;
    private static MultiplayerOptions tempMultiplayerOptions = new();

    // Sub tabs
    private static readonly List<UIButton> subtabButtons = [];
    private static readonly List<Text> subtabTexts = [];
    private static readonly List<Transform> subtabContents = [];
    private static RectTransform contentContainer;
    private static Image subtabSlider;
    private static int subtabIndex = -1;

    [HarmonyPostfix]
    [HarmonyPatch(nameof(UIOptionWindow._OnCreate))]
    [SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Original Function Name")]
    public static void _OnCreate_Postfix(UIOptionWindow __instance)
    {
        tempToUICallbacks = new();
        tempMultiplayerOptions = new();

        // Add multiplayer tab button
        var tabButtons = __instance.tabButtons;
        multiplayerTabIndex = tabButtons.Length;
        var lastTab = tabButtons[tabButtons.Length - 1].GetComponent<RectTransform>();
        var beforeLastTab = tabButtons[tabButtons.Length - 2].GetComponent<RectTransform>();
        var tabOffset = lastTab.anchoredPosition.x - beforeLastTab.anchoredPosition.x;
        var multiplayerTab = Object.Instantiate(lastTab, lastTab.parent, true);
        multiplayerTab.name = "tab-button-multiplayer";
        var anchoredPosition = lastTab.anchoredPosition;
        multiplayerTab.anchoredPosition = new Vector2(anchoredPosition.x + tabOffset, anchoredPosition.y);
        var newTabButtons = tabButtons.AddToArray(multiplayerTab.GetComponent<UIButton>());
        __instance.tabButtons = newTabButtons;

        // Update multiplayer tab text
        var tabText = multiplayerTab.GetComponentInChildren<Text>();
        tabText.GetComponent<Localizer>().enabled = false;
        tabText.text = "Multiplayer".Translate();
        var tabTexts = __instance.tabTexts;
        var newTabTexts = tabTexts.AddToArray(tabText);
        __instance.tabTexts = newTabTexts;

        // Add multiplayer tab content
        // Instantiate from "Miscellaneous" tab which has no scroll elements (yet)
        var tabTweeners = __instance.tabTweeners;
        var contentRectTransform = tabTweeners[4].GetComponent<RectTransform>();
        multiplayerContent = Object.Instantiate(contentRectTransform, contentRectTransform.parent, true);
        multiplayerContent.name = "multiplayer-content";
        multiplayerContent.localPosition += new Vector3(0, -65, 0);

        // Add revert button
        var newContents = tabTweeners.AddToArray(multiplayerContent.GetComponent<Tweener>());
        __instance.tabTweeners = newContents;
        var revertButtons = __instance.revertButtons;
        var revertButton = multiplayerContent.Find("revert-button").GetComponent<RectTransform>();
        var newRevertButtons = revertButtons.AddToArray(revertButton.GetComponent<UIButton>());
        __instance.revertButtons = newRevertButtons;

        // Remove unwanted GameObject
        foreach (RectTransform child in multiplayerContent)
        {
            if (child != revertButton)
            {
                Object.Destroy(child.gameObject);
            }
        }

        // Add subtab-bar for config categories
        var subtabsBar = (RectTransform)Object.Instantiate(multiplayerTab.parent, multiplayerContent);
        subtabSlider = Object.Instantiate(__instance.tabSlider, subtabsBar);
        subtabsBar.name = "subtab-line";
        subtabsBar.anchoredPosition = new Vector2(0, 25);
        subtabsBar.anchoredPosition3D = new Vector3(0, 25);

        // Set up default subtab "General"
        RectTransform subtab = null;
        foreach (RectTransform child in subtabsBar)
        {
            switch (child.name)
            {
                case "tab-button-multiplayer":
                    subtab = child;
                    break;
                case "bar":
                    subtabSlider = child.GetComponentInChildren<Image>();
                    break;
                default:
                    Object.Destroy(child.gameObject);
                    break;
            }
        }
        if (subtab != null)
        {
            subtab.localPosition = new Vector3(20, 38, 0);
            subtabButtons.Add(subtab.GetComponent<UIButton>());
            subtab.name = $"tab-button-{subtabButtons.Count}";
            var subtabText = subtab.GetComponentInChildren<Text>();
            subtabText.text = "General".Translate();
            subtabTexts.Add(subtabText);
            subtabTemplate = subtab;
        }
        subtabContents.Add(new GameObject("General").transform);

        // Add ScrollView
        var list = Object.Instantiate(tabTweeners[3].transform.Find("list").GetComponent<RectTransform>(), multiplayerContent);
        list.name = "list";
        list.offsetMax = Vector2.zero;
        var listContent = list.Find("scroll-view/viewport/content").GetComponent<RectTransform>();
        foreach (RectTransform child in listContent)
        {
            Object.Destroy(child.gameObject);
        }
        contentContainer = listContent;

        // Find control templates from tab "Video"
        checkboxTemplate = __instance.fullscreenComp.transform.parent.GetComponent<RectTransform>();
        comboBoxTemplate = __instance.resolutionComp.transform.parent.GetComponent<RectTransform>();
        sliderTemplate = __instance.dofComp.transform.parent.GetComponent<RectTransform>();
        inputTemplate = Object.Instantiate(checkboxTemplate, listContent, false);
        Object.Destroy(inputTemplate.Find("CheckBox").gameObject);
        var inputField =
            Object.Instantiate(
                UIRoot.instance.saveGameWindow.nameInput.transform.GetComponent<RectTransform>(),
                inputTemplate, false);
        var fieldPosition = checkboxTemplate.GetChild(0).GetComponent<RectTransform>().anchoredPosition;
        inputField.anchoredPosition = new Vector2(fieldPosition.x + 6, fieldPosition.y);
        inputField.sizeDelta = new Vector2(inputField.sizeDelta.x, 35);
        inputTemplate.gameObject.SetActive(false);

        AddMultiplayerOptionsProperties();

        // Attach contents to main container
        for (var i = 0; i < subtabContents.Count; i++)
        {
            subtabContents[i].SetParent(listContent);
            subtabContents[i].localPosition = Vector3.zero;
            subtabContents[i].localScale = Vector3.one;
            subtabButtons[i].data = i;
            subtabButtons[i].onClick += OnSubtabButtonClick;
        }
        SetSubtabIndex(0);
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(UIOptionWindow._OnDestroy))]
    [SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Original Function Name")]
    public static void _OnDestroy_Postfix()
    {
        tempToUICallbacks?.Clear();
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(UIOptionWindow._OnOpen))]
    [SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Original Function Name")]
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
        var properties = AccessTools.GetDeclaredProperties(typeof(MultiplayerOptions));
        foreach (var prop in properties)
        {
            if (tempToUICallbacks.TryGetValue(prop.Name, out var callback))
            {
                callback();
            }
        }
    }

    private static void OnSubtabButtonClick(int idx)
    {
        SetSubtabIndex(idx);
    }

    private static void SetSubtabIndex(int index)
    {
        if (subtabIndex != index)
        {
            for (var i = 0; i < subtabButtons.Count; i++)
            {
                if (i == index)
                {
                    subtabTexts[i].color = Color.white;
                    subtabContents[i].gameObject.SetActive(true);
                    contentContainer.sizeDelta =
                        new Vector2(contentContainer.sizeDelta.x, 40 * (subtabContents[i].childCount + 1));
                }
                else
                {
                    subtabTexts[i].color = new Color(1f, 1f, 1f, 0.55f);
                    subtabContents[i].gameObject.SetActive(false);
                }
            }
            subtabSlider.rectTransform.anchoredPosition =
                new Vector2(SubtabOffset * index, subtabSlider.rectTransform.anchoredPosition.y);
        }
        subtabIndex = index;
    }

    private static void AddMultiplayerOptionsProperties()
    {
        var properties = AccessTools.GetDeclaredProperties(typeof(MultiplayerOptions));

        foreach (var prop in properties)
        {
            var displayAttr = prop.GetCustomAttribute<DisplayNameAttribute>();
            var descriptionAttr = prop.GetCustomAttribute<DescriptionAttribute>();
            var categoryAttribute = prop.GetCustomAttribute<CategoryAttribute>();
            if (displayAttr == null)
            {
                continue;
            }
            var index = 0;
            if (categoryAttribute != null)
            {
                index = subtabTexts.FindIndex(text => text.text.Translate() == categoryAttribute.Category.Translate());
                if (index == -1)
                {
                    CreateSubtab(categoryAttribute.Category.Translate());
                    index = subtabTexts.Count - 1;
                }
            }
            var container = subtabContents[index];
            var anchorPosition = new Vector2(30, -40 * container.childCount);

            if (prop.PropertyType == typeof(bool))
            {
                CreateBooleanControl(displayAttr, descriptionAttr, prop, anchorPosition, container);
            }
            else if (prop.PropertyType == typeof(int) || prop.PropertyType == typeof(float) ||
                     prop.PropertyType == typeof(ushort))
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
            else if (prop.PropertyType == typeof(KeyboardShortcut))
            {
                CreateHotkeyControl(displayAttr, descriptionAttr, prop, anchorPosition, container);
            }
            else
            {
                Log.Warn($"MultiplayerOption property \"${prop.Name}\" of type \"{prop.PropertyType}\" not supported.");
            }
        }
    }

    private static void CreateSubtab(string subtabName)
    {
        var subtab = Object.Instantiate(subtabTemplate, subtabTemplate.parent);
        var anchoredPosition = subtabTemplate.anchoredPosition;
        subtab.anchoredPosition = new Vector2(anchoredPosition.x + SubtabOffset * subtabButtons.Count,
            anchoredPosition.y);
        subtabButtons.Add(subtab.GetComponent<UIButton>());
        subtab.name = $"tab-button-{subtabButtons.Count}";
        var subtabText = subtab.GetComponentInChildren<Text>();
        subtabText.text = subtabName;
        subtabTexts.Add(subtabText);

        subtabContents.Add(new GameObject(subtabName).transform);
    }

    private static void CreateBooleanControl(DisplayNameAttribute control, DescriptionAttribute descriptionAttr,
        PropertyInfo prop, Vector2 anchorPosition, Transform container)
    {
        var element = Object.Instantiate(checkboxTemplate, container, false);
        SetupUIElement(element, control, descriptionAttr, prop, anchorPosition);
        var toggle = element.GetComponentInChildren<UIToggle>();
        toggle.toggle.onValueChanged.RemoveAllListeners();
        toggle.toggle.onValueChanged.AddListener(value =>
        {
            // lock soil setting while in multiplayer game
            if (control.DisplayName == "Sync Soil" && Multiplayer.IsActive)
            {
                // reset to saved value if needed
                if (value == (bool)prop.GetValue(tempMultiplayerOptions, null))
                {
                    return;
                }
                toggle.isOn = !value;
                InGamePopup.ShowInfo("Unavailable".Translate(),
                    "This setting can only be changed while not in game".Translate(), "OK".Translate());
                return;
            }

            prop.SetValue(tempMultiplayerOptions, value, null);
        });

        tempToUICallbacks[prop.Name] = () =>
        {
            toggle.isOn = (bool)prop.GetValue(tempMultiplayerOptions, null);
        };
    }

    private static void CreateNumberControl(DisplayNameAttribute control, DescriptionAttribute descriptionAttr,
        PropertyInfo prop, Vector2 anchorPosition, Transform container)
    {
        var rangeAttr = prop.GetCustomAttribute<UIRangeAttribute>();
        var sliderControl = rangeAttr is { Slider: true };

        var element = Object.Instantiate(sliderControl ? sliderTemplate : inputTemplate, container, false);
        SetupUIElement(element, control, descriptionAttr, prop, anchorPosition);

        var isFloatingPoint = prop.PropertyType == typeof(float) || prop.PropertyType == typeof(double);

        if (sliderControl)
        {
            var slider = element.GetComponentInChildren<Slider>();
            slider.minValue = rangeAttr.Min;
            slider.maxValue = rangeAttr.Max;
            slider.wholeNumbers = !isFloatingPoint;
            var sliderThumbText = slider.GetComponentInChildren<Text>();
            slider.onValueChanged.RemoveAllListeners();
            slider.onValueChanged.AddListener(value =>
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
            var input = element.GetComponentInChildren<InputField>();

            input.onValueChanged.RemoveAllListeners();
            input.onValueChanged.AddListener(str =>
            {
                try
                {
                    var converter = TypeDescriptor.GetConverter(prop.PropertyType);
                    var value = (IComparable)converter.ConvertFromString(str);

                    if (rangeAttr != null)
                    {
                        var min = (IComparable)Convert.ChangeType(rangeAttr.Min, prop.PropertyType);
                        var max = (IComparable)Convert.ChangeType(rangeAttr.Max, prop.PropertyType);
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

    private static void CreateStringControl(DisplayNameAttribute control, DescriptionAttribute descriptionAttr,
        PropertyInfo prop, Vector2 anchorPosition, Transform container)
    {
        var characterLimitAttr = prop.GetCustomAttribute<UICharacterLimitAttribute>();
        var contentTypeAttr = prop.GetCustomAttribute<UIContentTypeAttribute>();

        var element = Object.Instantiate(inputTemplate, container, false);
        SetupUIElement(element, control, descriptionAttr, prop, anchorPosition);

        var input = element.GetComponentInChildren<InputField>();
        if (characterLimitAttr != null)
        {
            input.characterLimit = characterLimitAttr.Max;
        }
        if (contentTypeAttr != null)
        {
            input.contentType = contentTypeAttr.ContentType;
        }
        if (control?.DisplayName != null)
        {
            tempMultiplayerOptions.ModifyInputFieldAtCreation(control.DisplayName, ref input);
        }

        input.onValueChanged.RemoveAllListeners();
        input.onValueChanged.AddListener(value => { prop.SetValue(tempMultiplayerOptions, value, null); });

        tempToUICallbacks[prop.Name] = () =>
        {
            input.text = prop.GetValue(tempMultiplayerOptions, null) as string;
        };
    }

    private static void CreateEnumControl(DisplayNameAttribute control, DescriptionAttribute descriptionAttr, PropertyInfo prop,
        Vector2 anchorPosition, Transform container)
    {
        var element = Object.Instantiate(comboBoxTemplate, container, false);
        SetupUIElement(element, control, descriptionAttr, prop, anchorPosition);
        var combo = element.GetComponentInChildren<UIComboBox>();
        combo.Items = Enum.GetNames(prop.PropertyType).ToList();
        combo.ItemsData = Enum.GetValues(prop.PropertyType).OfType<int>().ToList();
        combo.onItemIndexChange.RemoveAllListeners();
        combo.onItemIndexChange.AddListener(() => { prop.SetValue(tempMultiplayerOptions, combo.itemIndex, null); });

        tempToUICallbacks[prop.Name] = () =>
        {
            combo.itemIndex = (int)prop.GetValue(tempMultiplayerOptions, null);
        };
    }

    private static void CreateHotkeyControl(DisplayNameAttribute control, DescriptionAttribute descriptionAttr,
        PropertyInfo prop, Vector2 anchorPosition, Transform container)
    {
        var element = KeyBinder.CreateKeyBinder(control.DisplayName.Translate());
        element.SetParent(container);
        element.anchoredPosition = anchorPosition;
        if (descriptionAttr != null)
        {
            element.gameObject.AddComponent<Tooltip>();
            element.gameObject.GetComponent<Tooltip>().Title = control.DisplayName.Translate();
            element.gameObject.GetComponent<Tooltip>().Text = descriptionAttr.Description.Translate();
        }

        var keybinder = element.GetComponent<KeyBinder>();
        keybinder.OnEdit += hotkey => prop.SetValue(tempMultiplayerOptions, hotkey, null);
        tempToUICallbacks[prop.Name] = () =>
        {
            keybinder.SetShortcut((KeyboardShortcut)prop.GetValue(tempMultiplayerOptions, null));
        };
    }

    private static void SetupUIElement(RectTransform element, DisplayNameAttribute display,
        DescriptionAttribute descriptionAttr, PropertyInfo prop, Vector2 anchorPosition)
    {
        element.gameObject.SetActive(true);
        element.name = prop.Name;
        element.anchoredPosition = anchorPosition;
        if (descriptionAttr != null)
        {
            element.gameObject.AddComponent<Tooltip>();
            element.gameObject.GetComponent<Tooltip>().Title = display.DisplayName.Translate();
            element.gameObject.GetComponent<Tooltip>().Text = descriptionAttr.Description.Translate();
        }
        element.GetComponent<Localizer>().enabled = false;
        element.GetComponent<Text>().text = display.DisplayName.Translate();
    }

    public class Tooltip : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        public string Title;
        public string Text;
        private UIButtonTip tip;

        public void OnDisable()
        {
            if (tip != null)
            {
                Destroy(tip.gameObject);
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            tip = UIButtonTip.Create(true, Title, Text, 2, new Vector2(0, 0), 508, gameObject.transform, "", "");
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (tip != null)
            {
                Destroy(tip.gameObject);
            }
        }
    }

    // MyKeyBinder modified from LSTM: https://github.com/hetima/DSP_LSTM/blob/main/LSTM/MyKeyBinder.cs
    public class KeyBinder : MonoBehaviour
    {
        public Action<KeyboardShortcut> OnEdit;

        private Text functionText;
        private Text keyText;
        private UnityEngine.UI.Toggle setTheKeyToggle;
        private UIButton inputUIButton;
        private Text waitingText;
        private bool nextNotOn;
        private KeyboardShortcut shortcut;

        public static RectTransform CreateKeyBinder(string label = "")
        {
            var optionWindow = UIRoot.instance.optionWindow;
            var uikeyEntry = Instantiate(optionWindow.entryPrefab);
            uikeyEntry.gameObject.SetActive(true);

            var go = uikeyEntry.gameObject;
            go.name = "keybinder";
            var kb = go.AddComponent<KeyBinder>();

            kb.functionText = uikeyEntry.functionText;
            kb.functionText.text = label;
            kb.functionText.fontSize = 18;

            kb.keyText = uikeyEntry.keyText;
            kb.keyText.fontSize = 18;
            kb.keyText.transform.localPosition += new Vector3(-200f, 0f);

            kb.setTheKeyToggle = uikeyEntry.setTheKeyToggle;
            kb.inputUIButton = uikeyEntry.inputUIButton;
            kb.inputUIButton.onClick += kb.OnInputUIButtonClick;
            kb.waitingText = uikeyEntry.waitingText;
            kb.inputUIButton.transform.parent.localPosition += new Vector3(-350f, 0f);

            Destroy(uikeyEntry.setDefaultUIButton.gameObject);
            Destroy(uikeyEntry.setNoneKeyUIButton.gameObject);
            Destroy(uikeyEntry);
            var rect = go.transform as RectTransform;
            rect.anchorMax = new Vector2(0, 0);
            rect.anchorMin = new Vector2(0, 0);
            return rect;
        }

        public void SetShortcut(KeyboardShortcut keyboardShortcut)
        {
            shortcut = keyboardShortcut;
            keyText.text = shortcut.Serialize();
            OnEdit.Invoke(shortcut);
        }

        private void Update()
        {
            if (!setTheKeyToggle.isOn && inputUIButton.highlighted)
            {
                setTheKeyToggle.isOn = true;
            }
            if (setTheKeyToggle.isOn)
            {
                if (!inputUIButton._isPointerEnter && Input.GetKeyDown(KeyCode.Mouse0))
                {
                    inputUIButton.highlighted = false;
                    setTheKeyToggle.isOn = false;
                    Reset();
                }
                else if (!inputUIButton.highlighted)
                {
                    setTheKeyToggle.isOn = false;
                    Reset();
                }
                else
                {
                    waitingText.gameObject.SetActive(true);
                    if (TrySetValue())
                    {
                        setTheKeyToggle.isOn = false;
                        inputUIButton.highlighted = false;
                        Reset();
                    }
                }
            }
        }

        public bool TrySetValue()
        {
            if (Input.GetKey(KeyCode.Escape))
            {
                VFInput.UseEscape();
                return true;
            }
            if (Input.GetKey(KeyCode.Mouse0) || Input.GetKey(KeyCode.Mouse1))
            {
                return true;
            }
            var anyKey = IsKeyInput();
            if (!anyKey && lastKey != KeyCode.None)
            {
                var k = GetPressedKeysString();
                if (string.IsNullOrEmpty(k))
                {
                    return false;
                }
                lastKey = KeyCode.None;

                SetShortcut(KeyboardShortcut.Deserialize(k));
                return true;
            }

            return false;
        }

        private KeyCode lastKey;
        [SuppressMessage("Style", "IDE0300")]
        private static readonly KeyCode[] modKeys = { KeyCode.RightShift, KeyCode.LeftShift,
                 KeyCode.RightControl, KeyCode.LeftControl,
                 KeyCode.RightAlt, KeyCode.LeftAlt,
                 KeyCode.LeftCommand,  KeyCode.LeftApple, KeyCode.LeftWindows,
                 KeyCode.RightCommand,  KeyCode.RightApple, KeyCode.RightWindows };

        public string GetPressedKeysString()
        {
            var key = lastKey.ToString();
            if (string.IsNullOrEmpty(key))
            {
                return null;
            }
            var mod = "";
            foreach (var modKey in modKeys)
            {
                if (Input.GetKey(modKey))
                {
                    mod += "+" + modKey.ToString();
                }
            }

            if (!string.IsNullOrEmpty(mod))
            {
                key += mod;
            }
            return key;
        }

        public bool IsKeyInput()
        {
            var isPressed = false;
            foreach (KeyCode item in Enum.GetValues(typeof(KeyCode)))
            {
                if (item != KeyCode.None && !modKeys.Contains(item) && Input.GetKey(item))
                {
                    lastKey = item;
                    isPressed = true;
                }
            }
            return isPressed;
        }

        public void Reset()
        {
            waitingText.gameObject.SetActive(false);
            lastKey = KeyCode.None;
        }

        public void OnInputUIButtonClick(int data)
        {
            inputUIButton.highlighted = true;

            if (nextNotOn)
            {
                nextNotOn = false;
                inputUIButton.highlighted = false;
                setTheKeyToggle.isOn = false;
                waitingText.gameObject.SetActive(false);
            }
        }
    }
}
