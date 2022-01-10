using System;
using System.Linq;
using System.Threading.Tasks;
using ThunderKit.Core.Data;
using UnityEditor;
using UnityEngine;

public class AutoInstallPackagesWindow : EditorWindow
{
    public static string[] Packages =
    {
        "xiaoye97-BepInEx-5.4.11", "xiaoye97-LDBTool-1.8.0", "CommonAPI-DSPModSave-1.1.3", "CommonAPI-CommonAPI-1.2.2", "CommonAPI-DSPEditorKit-1.0.0"
    };

    public bool shouldClose;
    public bool initPosition = false;

    [MenuItem("Window/Nebula/Install Packages")]
    static void Init()
    {
        AutoInstallPackagesWindow window = GetWindowWithRect<AutoInstallPackagesWindow>(new Rect(0, 0, 300, 120), false, "Installing Packages");
        window.Show();
    }

    void OnEnable()
    {
        Task.Delay(2000).Wait();
        PackageSourceSettings settings = ThunderKitSetting.GetOrCreateSettings<PackageSourceSettings>();
        shouldClose = InstallNextPackage(settings);
    }

    void OnGUI()
    {
        if (!initPosition)
        {
            Vector2 mousePos = GUIUtility.GUIToScreenPoint(Event.current.mousePosition);
            position = new Rect(mousePos.x, mousePos.y, position.width, position.height);
            initPosition = true;
        }

        GUILayout.Label("Installing needed Packages, please wait.");

        if (shouldClose)
        {
            Debug.Log("Done!");
            Close();
        }
    }

    private static bool InstallNextPackage(PackageSourceSettings settings)
    {
        foreach (string packageString in Packages)
        {
            string[] packageData = packageString.Split('-');
            foreach (PackageSource source in settings.PackageSources)
            {
                try
                {
                    PackageGroup neededPackage =
                        source.Packages.First(package => package.Author.Equals(packageData[0]) && package.PackageName.Equals(packageData[1]));
                    if (!neededPackage.Installed)
                    {
                        Debug.Log($"Installing {neededPackage.Author}-{neededPackage.PackageName}");
                        Task task = neededPackage.Source.InstallPackage(neededPackage, packageData[2]);
                        task.Wait();
                        return false;
                    }
                }
                catch (InvalidOperationException e) { }
            }
        }

        return true;
    }
}