using HarmonyLib;
using NebulaModel.Logger;
using NebulaModel.Packets.Universe;
using NebulaWorld;

namespace NebulaPatcher.Patches.Dynamic
{
    [HarmonyPatch(typeof(DESelection))]
    internal class DESelection_Patch
    {
        [HarmonyPrefix]
        [HarmonyPatch(nameof(DESelection.SetViewStar))]
        public static bool SetViewStar_Prefix(DESelection __instance, ref StarData starData)
        {
            if (Multiplayer.IsActive && Multiplayer.Session.LocalPlayer.IsClient)
            {
                //UIDysonEditor._OnOpen()
                if (!UIRoot.instance.uiGame.dysonEditor.sceneGroup.activeSelf)
                {
                    //Request the latest list of existing dyson spheres
                    Multiplayer.Session.Network.SendPacket(new DysonSphereLoadRequest(0, DysonSphereRequestEvent.List));
                    if (GameMain.localStar == null)
                    {
                        //In outer space, set initial viewStar to the one that has dyson sphere
                        for (int i = 0; i < GameMain.data.dysonSpheres.Length; i++)
                        {
                            if (GameMain.data.dysonSpheres[i] != null)
                            {
                                starData = GameMain.data.dysonSpheres[i].starData;
                                return true;
                            }
                        }
                    }
                }
                if (starData != null && GameMain.data.dysonSpheres[starData.index] == null)
                {
                    Log.Info($"Requesting DysonSphere for system {starData.displayName} (Index: {starData.index})");
                    Multiplayer.Session.Network.SendPacket(new DysonSphereLoadRequest(starData.index, DysonSphereRequestEvent.Load));
                    //Set viewDysonSphere to null until requested dyson sphere data is arrived.
                    __instance.ClearAllSelection();
                    __instance.viewStar = starData;
                    __instance.viewDysonSphere = null;
                    return false;
                }
            }
            return true;
        }

        [HarmonyPostfix]
        [HarmonyPatch(nameof(DESelection.ClearAllSelection))]
        public static void ClearAllSelection_Postfix()
        {
            if (Multiplayer.IsActive && Multiplayer.Session.LocalPlayer.IsClient)
            {
                //UIDysonEditor._OnClose()
                if (!UIRoot.instance.uiGame.dysonEditor.sceneGroup.activeSelf)
                {
                    //Unload remote dyson spheres
                    Multiplayer.Session.DysonSpheres.UnloadRemoteDysonSpheres();
                }
            }            
        }
    }
}
