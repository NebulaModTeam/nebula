#region

using System;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

#endregion

namespace NebulaWorld;

public static class InGamePopup
{
    private static UIMessageBox displayedMessage;

    public static void FadeOut()
    {
        if (displayedMessage == null)
        {
            return;
        }
        displayedMessage.FadeOut();
        displayedMessage = null;
    }

    public static void UpdateMessage(in string title, string message)
    {
        if (displayedMessage == null || displayedMessage.m_TitleText.text != title)
        {
            return;
        }
        displayedMessage.m_MessageText.horizontalOverflow = HorizontalWrapMode.Overflow;
        displayedMessage.m_MessageText.verticalOverflow = VerticalWrapMode.Overflow;
        displayedMessage.m_MessageText.text = message;
    }

    // Input
    public static void AskInput(string title, string message, InputField.ContentType inputType, string inputText,
        Action<string> onConfirm, Action onCancel)
    {
        displayedMessage = UIMessageBox.Show(title, message, "取消".Translate(), "确定".Translate(),
            UIMessageBox.QUESTION, () => { onCancel?.Invoke(); }, () => { onConfirm?.Invoke(GetInputField()); });
        CreateInputField(inputType, inputText);
    }

    // Info
    public static void ShowInfo(string title, string message, string btn1, Action resp1 = null)
    {
        Show(UIMessageBox.INFO, title, message, btn1, resp1);
    }

    public static void ShowInfo(string title, string message, string btn1, string btn2, Action resp1, Action resp2)
    {
        Show(UIMessageBox.INFO, title, message, btn1, btn2, resp1, resp2);
    }

    public static void ShowInfo(string title, string message, string btn1, string btn2, string btn3, Action resp1, Action resp2,
        Action resp3)
    {
        Show(UIMessageBox.INFO, title, message, btn1, btn2, btn3, resp1, resp2, resp3);
    }

    // Warning
    public static void ShowWarning(string title, string message, string btn1, Action resp1 = null)
    {
        Show(UIMessageBox.WARNING, title, message, btn1, resp1);
    }

    public static void ShowWarning(string title, string message, string btn1, string btn2, Action resp1, Action resp2)
    {
        Show(UIMessageBox.WARNING, title, message, btn1, btn2, resp1, resp2);
    }

    public static void ShowWarning(string title, string message, string btn1, string btn2, string btn3, Action resp1,
        Action resp2, Action resp3)
    {
        Show(UIMessageBox.WARNING, title, message, btn1, btn2, btn3, resp1, resp2, resp3);
    }

    // Question
    public static void ShowQuestion(string title, string message, string btn1, Action resp1 = null)
    {
        Show(UIMessageBox.QUESTION, title, message, btn1, resp1);
    }

    public static void ShowQuestion(string title, string message, string btn1, string btn2, Action resp1, Action resp2)
    {
        Show(UIMessageBox.QUESTION, title, message, btn1, btn2, resp1, resp2);
    }

    public static void ShowQuestion(string title, string message, string btn1, string btn2, string btn3, Action resp1,
        Action resp2, Action resp3)
    {
        Show(UIMessageBox.QUESTION, title, message, btn1, btn2, btn3, resp1, resp2, resp3);
    }

    // Error
    public static void ShowError(string title, string message, string btn1, Action resp1 = null)
    {
        Show(UIMessageBox.ERROR, title, message, btn1, resp1);
    }

    public static void ShowError(string title, string message, string btn1, string btn2, Action resp1, Action resp2)
    {
        Show(UIMessageBox.ERROR, title, message, btn1, btn2, resp1, resp2);
    }

    public static void ShowError(string title, string message, string btn1, string btn2, string btn3, Action resp1,
        Action resp2, Action resp3)
    {
        Show(UIMessageBox.ERROR, title, message, btn1, btn2, btn3, resp1, resp2, resp3);
    }

    // Base
    private static void Show(int type, string title, string message, string btn1, Action resp1 = null)
    {
        displayedMessage = UIMessageBox.Show(title, message, btn1, type, () => { resp1?.Invoke(); });
    }

    private static void Show(int type, string title, string message, string btn1, string btn2, Action resp1, Action resp2)
    {
        displayedMessage = UIMessageBox.Show(title, message, btn1, btn2, type, () => { resp1?.Invoke(); },
            () => { resp2?.Invoke(); });
    }

    private static void Show(int type, string title, string message, string btn1, string btn2, string btn3, Action resp1,
        Action resp2, Action resp3)
    {
        displayedMessage = UIMessageBox.Show(title, message, btn1, btn2, btn3, type, () => { resp1?.Invoke(); },
            () => { resp2?.Invoke(); }, () => { resp3?.Invoke(); });
    }

    private static void CreateInputField(InputField.ContentType contentType, string text)
    {
        var inputObject = GameObject.Find("UI Root/Overlay Canvas/Nebula - Multiplayer Menu/Host IP Address/InputField");
        inputObject = Object.Instantiate(inputObject, displayedMessage.transform.Find("Window/Body/Client"));
        inputObject.name = "InputField";
        inputObject.transform.localPosition = new Vector3(-150, 0, 0);
        var inputField = inputObject.GetComponent<InputField>();
        inputField.contentType = contentType;
        inputField.text = text;
    }

    private static string GetInputField()
    {
        return displayedMessage.transform.Find("Window/Body/Client/InputField").GetComponent<InputField>().text;
    }
}
