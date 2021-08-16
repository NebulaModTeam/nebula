using HarmonyLib;
using NebulaModel.Packets.Trash;
using NebulaWorld;
using NebulaWorld.Trash;

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
            if (SimulatedWorld.Initialized && !TrashManager.RemoveTrashFromOtherPlayers)
            {
                LocalPlayer.Instance.SendPacket(new TrashSystemTrashRemovedPacket(index));
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(nameof(TrashContainer.NewTrash))]
        public static void NewTrash_Postfix(int __result, TrashObject trashObj, TrashData trashData)
        {
            //Notify other that trash was created 
            if (SimulatedWorld.Initialized && !TrashManager.NewTrashFromOtherPlayers)
            {
                //Refresh trash to assign local planet Id and local position
                GameMain.data.trashSystem.Gravity(ref trashData, GameMain.data.galaxy.astroPoses, 0, 0, 0, (GameMain.data.localPlanet != null) ? GameMain.data.localPlanet.id : 0, (GameMain.data.localPlanet != null) ? GameMain.data.localPlanet.data : null);
                LocalPlayer.Instance.SendPacket(new TrashSystemNewTrashCreatedPacket(__result, trashObj, trashData, LocalPlayer.Instance.PlayerId, GameMain.mainPlayer.planetId));
            }
        }
    }
}
