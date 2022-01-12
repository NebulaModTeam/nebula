# NebulaUnity
(In progress)

## What is this?
This folder contains a Unity project that can be used to build custom UI elements used by this mod

## How do I use it
1. Open the Unity project using the Unity Editor.
2. You should see ThunderKit settings window, press Locate Game button and select where DSP is installed.
3. In your context menu select Window->Nebula->Install Packages and wait for opened window to close.
4. Build the main project
   `dotnet build`
5. Copy project DLLs into Unity project 
   `dotnet build -target:"CopyAssembliesToUnityProject"` (use `"Build:CopyAssembliesToUnityProject"` to rebuild main project at the same time). It's safe to ignore this error message: ```C:\Users\matts\src\nebula\dep\websocket-sharp\websocket-sharp\websocket-sharp.csproj : error MSB4057: The target "CopyAssembliesToUnityProject" does not exist in the project.```
6. Make the changes you would like to make. More can be found here on setting up your dev environment
7. After that is complete build the asset bundle (Window->DSP Utils->Build Asset Bundles)
8. Copy the generated asset bundle file named nebulabundle to NebulaWorld\Assets 
   `copy .\NebulaUnity\Assets\StreamingAssets\AssetBundles\nebulabundle .\NebulaWorld\Assets\`
9. Rebuild the main project to see your changes
10. Make sure to include changes to .\NebulaWorld\Assets\ in your PR