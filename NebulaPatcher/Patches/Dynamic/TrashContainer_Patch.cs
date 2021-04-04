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
        [HarmonyPatch("RemoveTrash")]
        public static void RemoveTrash_Postfix(int index)
        {
            //Notify other that trash was removed
            if (SimulatedWorld.Initialized && !TrashManager.RemoveTrashFromOtherPlayers)
            {
                LocalPlayer.SendPacket(new TrashSystemTrashRemovedPacket(index));
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch("NewTrash")]
        public static void NewTrash_Postfix(int __result, TrashObject trashObj, TrashData trashData)
        {
            //Notify other that trash was created 
            if (SimulatedWorld.Initialized && !TrashManager.NewTrashFromOtherPlayers)
            {
                LocalPlayer.SendPacket(new TrashSystemNewTrashCreatedPacket(__result, trashObj, trashData, LocalPlayer.PlayerId, GameMain.mainPlayer.planetId));
            }
        }
    }
}
