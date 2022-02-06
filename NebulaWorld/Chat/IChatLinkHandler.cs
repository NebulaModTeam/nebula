using NebulaWorld.MonoBehaviours.Local;
using UnityEngine;

namespace NebulaWorld.Chat
{
    public interface IChatLinkHandler
    {
        void OnClick(string tipData);
        void OnHover(string tipData, ChatLinkTrigger trigger, ref MonoBehaviour tipObject);
    }
}