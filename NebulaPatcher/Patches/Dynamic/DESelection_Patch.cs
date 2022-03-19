﻿using HarmonyLib;
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
                        //No dyson sphere exist, close the UI
                        UIRoot.instance.uiGame.dysonEditor._Close();
                        return false;
                    }
                    else if (GameMain.data.dysonSpheres[starData.index] == null)
                    {
                        //Local dyson sphere hasn't loaded yet, close the UI
                        UIRoot.instance.uiGame.dysonEditor._Close();
                        return false;
                    }
                }
                if (starData != null && GameMain.data.dysonSpheres[starData.index] == null)
                {
                    if (Multiplayer.Session.DysonSpheres.RequestingIndex == -1)
                    {
                        Multiplayer.Session.DysonSpheres.RequestDysonSphere(starData.index);
                    }
                    else
                    {
                        InGamePopup.ShowInfo("Loading", $"Loading Dyson sphere {starData.displayName}, please wait...", null);
                    }
                    // Restore comboBox back to original star
                    UIComboBox dysonBox = UIRoot.instance.uiGame.dysonEditor.controlPanel.topFunction.dysonBox;
                    int index = dysonBox.ItemsData.FindIndex(x => x == UIRoot.instance.uiGame.dysonEditor.selection.viewStar?.index);
                    dysonBox.itemIndex = index >= 0 ? index : 0;
                    return false;
                }
            }
            return true;
        }
    }
}
