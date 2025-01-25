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

    [HarmonyPostfix]
    [HarmonyPatch(nameof(UIBeltWindow.OnTagCountInputEndEdit))]
    public static void OnTagCountInputEndEdit_Postfix(UIBeltWindow __instance, string str)
    {
        if (!Multiplayer.IsActive) return;
        if (__instance.event_lock || !__instance.active || __instance.beltId == 0 || __instance.factory == null) return;
        ref var beltComponent = ref __instance.traffic.beltPool[__instance.beltId];
        if (beltComponent.id != __instance.beltId) return;

        // Notify others about belt memo count changes
        if (float.TryParse(str, out var num))
        {
            Multiplayer.Session.Network.SendPacketToLocalStar(new BeltSignalNumberPacket(beltComponent.entityId, num,
                __instance.factory.planetId));
        }
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(UIBeltWindow.OnTagItemPickerReturn))]
    public static void OnTagItemPickerReturn_Postfix(UIBeltWindow __instance, int signalId)
    {
        if (!Multiplayer.IsActive) return;
        if (!__instance.active || __instance.beltId == 0 || __instance.factory == null) return;
        ref var beltComponent = ref __instance.traffic.beltPool[__instance.beltId];
        if (beltComponent.id != __instance.beltId) return;

        // Notify others about belt memo icon changes
        var sprite = LDB.signals.IconSprite(signalId);
        if (sprite == null) signalId = 0;
        Multiplayer.Session.Network.SendPacketToLocalStar(new BeltSignalIconPacket(beltComponent.entityId, signalId,
            __instance.factory.planetId));
    }
}
