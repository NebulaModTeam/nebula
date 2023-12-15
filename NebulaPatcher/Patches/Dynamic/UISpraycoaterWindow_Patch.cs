#region

using HarmonyLib;
using NebulaModel.Packets.Factory.Tank;
using NebulaWorld;

#endregion

namespace NebulaPatcher.Patches.Dynamic;

[HarmonyPatch(typeof(UISpraycoaterWindow))]
internal class UISpraycoaterWindow_Patch
{
    [HarmonyPrefix]
    [HarmonyPatch(nameof(UISpraycoaterWindow.OnTakeBackPointerUp))]
    public static void OnTakeBackPointerUp_Prefix(UISpraycoaterWindow __instance)
    {
        if (Multiplayer.IsActive)
        {
            SendPacket(__instance);
        }
    }

    private static void SendPacket(UISpraycoaterWindow window)
    {
        var spraycoater = window.traffic.spraycoaterPool[window._spraycoaterId];
        var planetId = window.traffic.planet.id;
        Multiplayer.Session.Network.SendPacketToLocalStar(new SprayerStorageUpdatePacket(in spraycoater, planetId));
    }
}
