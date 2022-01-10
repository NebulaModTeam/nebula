# NebulaUnity
(In progress)

## What is this?
This folder contains a Unity project that can be used to build custom UI elements used by this mod

## How do I use it
1. 
2. Open the Unity project using the Unity Editor.
3. You should see ThunderKit settings window, press Locate Game button and select where DSP is installed.
4. In your context menu select Window->Nebula->Install Packages and wait for opened window to close.
5. Build the main project dotnet build
6. Copy project DLLs into Unity project dotnet build -target:CopyAssembliesToUnityProject
7. Make the changes you would like to make. More can be found here on setting up your dev environment
8. After that is complete build the asset bundle (Window->DSP Utils->Build Asset Bundles)
9. Copy the generated asset bundle file named ncht to NebulaWorld\Assets
10. copy .\NebulaUnity\Assets\StreamingAssets\AssetBundles\ncht .\NebulaWorld\Assets\
11. Rebuild the main project to see your changes