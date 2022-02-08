using HarmonyLib;
using NebulaModel.Packets.Factory.Tank;
using NebulaWorld;

namespace NebulaPatcher.Patches.Dynamic
{
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

        public static void SendPacket(UISpraycoaterWindow window)
        {
            SpraycoaterComponent spraycoater = window.traffic.spraycoaterPool[window._spraycoaterId];
            int planetId = window.traffic.planet.id;
            Multiplayer.Session.Network.SendPacketToLocalStar(new SprayerStorageUpdatePacket(in spraycoater, planetId));
        }        
    }
}
