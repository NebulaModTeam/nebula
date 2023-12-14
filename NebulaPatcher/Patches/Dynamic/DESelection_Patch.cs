#region

using HarmonyLib;
using NebulaModel.Packets.Universe;
using NebulaWorld;

#endregion

namespace NebulaPatcher.Patches.Dynamic;

[HarmonyPatch(typeof(DESelection))]
internal class DESelection_Patch
{
    [HarmonyPrefix]
    [HarmonyPatch(nameof(DESelection.SetViewStar))]
    public static bool SetViewStar_Prefix(ref StarData starData)
    {
        if (!Multiplayer.IsActive || !Multiplayer.Session.LocalPlayer.IsClient)
        {
            return true;
        }
        //UIDysonEditor._OnOpen()
        if (!UIRoot.instance.uiGame.dysonEditor.sceneGroup.activeSelf)
        {
            //Request the latest list of existing dyson spheres
            Multiplayer.Session.Network.SendPacket(new DysonSphereLoadRequest(0, DysonSphereRequestEvent.List));
            if (GameMain.localStar == null)
            {
                //In outer space, set initial viewStar to the one that has dyson sphere
                foreach (var t in GameMain.data.dysonSpheres)
                {
                    if (t == null)
                    {
                        continue;
                    }
                    starData = t.starData;
                    return true;
                }
                //No dyson sphere exist, close the UI
                UIRoot.instance.uiGame.dysonEditor._Close();
                return false;
            }
            if (GameMain.data.dysonSpheres[starData.index] == null)
            {
                //Local dyson sphere hasn't loaded yet, close the UI
                UIRoot.instance.uiGame.dysonEditor._Close();
                return false;
            }
        }
        if (starData == null || GameMain.data.dysonSpheres[starData.index] != null)
        {
            return true;
        }
        if (Multiplayer.Session.DysonSpheres.RequestingIndex == -1)
        {
            Multiplayer.Session.DysonSpheres.RequestDysonSphere(starData.index);
        }
        else
        {
            InGamePopup.ShowInfo("Loading".Translate(),
                string.Format("Loading Dyson sphere {0}, please wait".Translate(), starData.displayName), null);
        }
        // Restore comboBox back to original star
        var dysonBox = UIRoot.instance.uiGame.dysonEditor.controlPanel.topFunction.dysonBox;
        var index = dysonBox.ItemsData.FindIndex(x =>
            x == UIRoot.instance.uiGame.dysonEditor.selection.viewStar?.index);
        dysonBox.itemIndex = index >= 0 ? index : 0;
        return false;
    }
}
