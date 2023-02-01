using HarmonyLib;
using NebulaModel.Packets.Factory.Belt;
using NebulaWorld;

namespace NebulaPatcher.Patches.Dynamic
{
    [HarmonyPatch(typeof(UIBeltWindow))]
    internal class UIBeltWindow_Patch
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(UIBeltWindow), nameof(UIBeltWindow.OnReverseButtonClick))]
        public static void OnReverseButtonClick_Postfix(UIBeltWindow __instance)
        {
            // Notify others about belt direction reverse
            if (Multiplayer.IsActive && !Multiplayer.Session.Factories.IsIncomingRequest.Value)
            {
                Multiplayer.Session.Network.SendPacketToLocalStar(new BeltReversePacket(__instance.beltId, __instance.factory.planetId));
            }
        }
    }
}
