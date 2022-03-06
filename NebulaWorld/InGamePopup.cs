using System;
using UnityEngine;

namespace NebulaWorld
{
    public static class InGamePopup
    {
        private static UIMessageBox displayedMessage;

        public static void FadeOut()
        {
            displayedMessage?.FadeOut();
            displayedMessage = null;
        }

        public static void UpdateMessage(in string title, string message)
        {
            if (displayedMessage != null && displayedMessage.m_TitleText.text == title)
            {
                displayedMessage.m_MessageText.horizontalOverflow = HorizontalWrapMode.Overflow;
                displayedMessage.m_MessageText.verticalOverflow = VerticalWrapMode.Overflow;
                displayedMessage.m_MessageText.text = message;
            }
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

        public static void ShowInfo(string title, string message, string btn1, string btn2, string btn3, Action resp1, Action resp2, Action resp3)
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

        public static void ShowWarning(string title, string message, string btn1, string btn2, string btn3, Action resp1, Action resp2, Action resp3)
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

        public static void ShowQuestion(string title, string message, string btn1, string btn2, string btn3, Action resp1, Action resp2, Action resp3)
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

        public static void ShowError(string title, string message, string btn1, string btn2, string btn3, Action resp1, Action resp2, Action resp3)
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
            displayedMessage = UIMessageBox.Show(title, message, btn1, btn2, type, () => { resp1?.Invoke(); }, () => { resp2?.Invoke(); });
        }

        private static void Show(int type, string title, string message, string btn1, string btn2, string btn3, Action resp1, Action resp2, Action resp3)
        {
            displayedMessage = UIMessageBox.Show(title, message, btn1, btn2, btn3, type, () => { resp1?.Invoke(); }, () => { resp2?.Invoke(); }, () => { resp3?.Invoke(); });
        }
    }
}
