using System;
using System.Linq;
using System.Threading.Tasks;
using ThunderKit.Core.Data;
using UnityEditor;
using UnityEngine;
using PackageSource = ThunderKit.Core.Data.PackageSource;


public class AutoInstallPackagesWindow : EditorWindow
{
    public static string[] Packages =
    {
        "CommonAPI-CommonAPI-1.6.5", 
        "CommonAPI-DSPEditorKit-1.0.3"
    };

    public bool shouldClose;
    public bool initPosition = false;
    public bool currentlyInstalling = false;
    public string installName;

    [MenuItem("Window/Nebula/Install Packages")]
    private static void Init()
    {
        PackageSource.SourcesInitialized -= PackageSource_SourceInitialized;
        PackageSource.SourcesInitialized += PackageSource_SourceInitialized;
        PackageSource.LoadAllSources();
    }

    private static void PackageSource_SourceInitialized(object sender, EventArgs e)
    {
		Task.Delay(3000).Wait();
        AutoInstallPackagesWindow window = GetWindowWithRect<AutoInstallPackagesWindow>(new Rect(0, 0, 300, 120), false, "Installing Packages");
        window.Show();
    }

    private void OnEnable()
    {
        currentlyInstalling = false;
        Task.Delay(2000).Wait();
        PackageSourceSettings settings = ThunderKitSetting.GetOrCreateSettings<PackageSourceSettings>();
        shouldClose = InstallNextPackage(settings);
    }

    private void Update()
    {
        if (currentlyInstalling || EditorApplication.isCompiling) return;

        PackageSourceSettings settings = ThunderKitSetting.GetOrCreateSettings<PackageSourceSettings>();
        shouldClose = InstallNextPackage(settings);
    }

    private void OnGUI()
    {
        if (!initPosition)
        {
            Vector2 mousePos = GUIUtility.GUIToScreenPoint(Event.current.mousePosition);
            position = new Rect(mousePos.x, mousePos.y, position.width, position.height);
            initPosition = true;
        }

        GUILayout.Label("Installing needed Packages, please wait.");
        
        if(currentlyInstalling)
            GUILayout.Label($"Installing {installName}");
        if (EditorApplication.isCompiling)
            GUILayout.Label("Waiting for compilation!");

        if (shouldClose)
        {
            Debug.Log("Done!");
            Close();
        }
    }

    private bool InstallNextPackage(PackageSourceSettings settings)
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
                        currentlyInstalling = true;
                        installName = $"{neededPackage.Author}-{neededPackage.PackageName}";
                        Debug.Log($"Installing {installName}");
                        
                        Task task = neededPackage.Source.InstallPackage(neededPackage, packageData[2]);
                        task.Wait();
                        return false;
                    }
                }
                catch (InvalidOperationException) { }
            }
        }

        return true;
    }
}
