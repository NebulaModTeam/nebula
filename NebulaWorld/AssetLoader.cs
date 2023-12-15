#region

using System.IO;
using System.Reflection;
using NebulaModel.Logger;
using UnityEngine;

#endregion

namespace NebulaWorld;

public static class AssetLoader
{
    private static AssetBundle assetBundle;

    public static AssetBundle AssetBundle
    {
        get
        {
            if (assetBundle != null)
            {
                return assetBundle;
            }
            var pluginfolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            if (pluginfolder == null)
            {
                Log.Warn("Plugin folder is null, unable to load chat");
                return null;
            }

            var fullAssetPath = Path.Combine(pluginfolder, "nebulabundle");
            assetBundle = AssetBundle.LoadFromFile(fullAssetPath);

            return assetBundle;
        }
    }
}
