using UnityEngine;

namespace Nebula.UI
{
    public class MultiplayerMenuUI : MonoBehaviour
    {
        const int PANEL_WIDTH = 400;
        const int PANEL_HEIGHT = 220;

        private string hostAddress = "127.0.0.1";

        void OnGUI()
        {
            Rect panelRect = new Rect(Screen.width / 2 - PANEL_WIDTH / 2, Screen.height / 2 - PANEL_HEIGHT / 2, PANEL_WIDTH, PANEL_HEIGHT);
            GUI.Box(panelRect, "");

            Rect groupRect = new Rect(panelRect.x + 10, panelRect.y + 10, panelRect.width - 20, panelRect.height - 20);
            GUI.BeginGroup(groupRect);

            GUI.Label(new Rect(0,0, groupRect.width, 30), "Server / Host Address");
            hostAddress = GUI.TextField(new Rect(0, 50, groupRect.width, 30), hostAddress);

            if (GUI.Button(new Rect(0, 100, groupRect.width / 2 - 5, 40), "Host Game"))
            {
                // TODO: Launch exe server avec hostAddress
            }

            if (GUI.Button(new Rect(groupRect.width / 2 + 5, 100, groupRect.width / 2 - 5, 40), "Connect"))
            {
                // TODO: Launch client exe avec en se connectant au hostAddress
            }

            if (GUI.Button(new Rect(0, 160, groupRect.width, 40), "Back"))
            {
                UIRoot.instance.OpenMainMenuUI();
                gameObject.SetActive(false);
            }
            
            GUI.EndGroup();
        }
    }
}
