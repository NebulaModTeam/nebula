# NebulaUnity
(In progress)

## What is this?
This folder contains a Unity project that can be used to build custom UI elements used by this mod

## How do I use it

1. First build the main project `dotnet build`
2. Copy project DLLs into Unity project ` dotnet build -target:CopyAssembliesToUnityProject`
3. Open the Unity project using the Unity Editor and make the changes you would like to make. [More can be found here](https://github.com/kremnev8/DSP-Mods/wiki/Setting-up-development-environment) on setting up your dev environment
4. After that is complete build the asset bundle (Window->DSP Utils->Build Asset Bundles)
5. Copy the generated asset bundle file named `ncht` to `NebulaWorld\Assets`
   `copy .\NebulaUnity\Assets\StreamingAssets\AssetBundles\ncht .\NebulaWorld\Assets\` 
6. Rebuild the main project to see your changes


