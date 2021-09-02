using HarmonyLib;
using NebulaModel.Packets.Trash;
using NebulaWorld;

namespace NebulaPatcher.Patches.Dynamic
{
    [HarmonyPatch(typeof(TrashContainer))]
    public class TrashContainer_Patch
    {
        [HarmonyPostfix]
        [HarmonyPatch(nameof(TrashContainer.RemoveTrash))]
        public static void RemoveTrash_Postfix(int index)
        {
            //Notify other that trash was removed
            if (Multiplayer.IsActive && !Multiplayer.Session.Trashes.RemoveTrashFromOtherPlayers)
            {
                Multiplayer.Session.Network.SendPacket(new TrashSystemTrashRemovedPacket(index));
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(nameof(TrashContainer.NewTrash))]
        public static void NewTrash_Postfix(int __result, TrashObject trashObj, TrashData trashData)
        {
            //Notify other that trash was created 
            if (Multiplayer.IsActive && !Multiplayer.Session.Trashes.NewTrashFromOtherPlayers)
            {
                //Refresh trash to assign local planet Id and local position
                GameMain.data.trashSystem.Gravity(ref trashData, GameMain.data.galaxy.astroPoses, 0, 0, 0, (GameMain.data.localPlanet != null) ? GameMain.data.localPlanet.id : 0, (GameMain.data.localPlanet != null) ? GameMain.data.localPlanet.data : null);
                Multiplayer.Session.Network.SendPacket(new TrashSystemNewTrashCreatedPacket(__result, trashObj, trashData, ((LocalPlayer)Multiplayer.Session.LocalPlayer).Id, GameMain.mainPlayer.planetId));
            }
        }
    }
}
