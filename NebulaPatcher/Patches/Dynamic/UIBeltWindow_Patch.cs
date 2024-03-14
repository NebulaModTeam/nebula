#region

using HarmonyLib;
using NebulaModel.Packets.Factory.Belt;
using NebulaWorld;

#endregion

namespace NebulaPatcher.Patches.Dynamic;

[HarmonyPatch(typeof(UIBeltWindow))]
internal class UIBeltWindow_Patch
{
    [HarmonyPrefix]
    [HarmonyPatch(typeof(UIBeltWindow), nameof(UIBeltWindow.OnReverseButtonClick))]
    public static bool OnReverseButtonClick_Prefix(UIBeltWindow __instance)
    {
        if (!Multiplayer.IsActive) return true;
        if (Multiplayer.Session.Factories.IsIncomingRequest.Value) return true;

        // Notify others about belt direction reverse
        var packet = new BeltReverseRequestPacket(__instance.beltId, __instance.factory.planetId, Multiplayer.Session.LocalPlayer.Id);
        if (Multiplayer.Session.IsServer)
        {
            var starId = __instance.factory.planetId / 100;
            Multiplayer.Session.Server.SendPacketToStar(packet, starId);
            return true;
        }
        else
        {
            // Request reverse change and wait for server to approve
            Multiplayer.Session.Client.SendPacket(packet);
            return false;
        }
    }
}
