#region

using NebulaWorld.MonoBehaviours.Local.Chat;
using UnityEngine;

#endregion

namespace NebulaWorld.Chat.ChatLinks;

public interface IChatLinkHandler
{
    void OnClick(string data);

    void OnRightClick(string data);

    void OnHover(string data, ChatLinkTrigger trigger, ref MonoBehaviour tipObject);

    string GetIconName(string data);

    string GetDisplayRichText(string data);
}
