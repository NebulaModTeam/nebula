using NebulaModel.Logger;
using NebulaWorld.MonoBehaviours.Local;
using System.IO;
using System.Reflection;
using UnityEngine;

namespace NebulaWorld
{
    public static class InGameChatAssetLoader
    {
        private static ChatManager chatManager;
        private static AssetBundle assetBundle;

        public static ChatManager ChatManager()
        {
            if (chatManager != null)
                return chatManager;
            if (assetBundle != null)
            {
                Log.Warn($"Asset bundle loaded but ChatManager instance was not, possible invalid state");
            }
            else
            {
                var pluginfolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                if (pluginfolder == null)
                {
                    Log.Warn($"plugin folder is null, unable to load chat");
                    return null;
                }
                var fullAssetPath = Path.Combine(pluginfolder, "Assets", "nebulabundle");
                assetBundle = AssetBundle.LoadFromFile(fullAssetPath);
            }

            GameObject prefab = assetBundle.LoadAsset<GameObject>("Assets/Prefab/ChatV2.prefab");
            var uiGameInventory = UIRoot.instance.uiGame.inventory;
            var gameObject = Object.Instantiate(prefab, uiGameInventory.transform.parent, false);
            
            chatManager = gameObject.transform.parent.GetComponentInChildren<ChatManager>();
            return chatManager;
        }

    }
}
