using HarmonyLib;
using NebulaAPI;
using NebulaModel;
using NebulaModel.Packets.Players;
using NebulaWorld;

namespace NebulaPatcher.Patches.Dynamic
{
    [HarmonyPatch(typeof(UIMechaWindow))]
    internal class UIMechaWindow_Patch
    {
        [HarmonyPostfix]
        [HarmonyPatch(nameof(UIMechaWindow._OnClose))]
        public static void _OnClose_Postfix()
        {
            if (!Multiplayer.IsActive)
            {
                return;
            }

            // GameMain.mainPlayer.mecha.mainColors not there anymore
            //Multiplayer.Session.Network.SendPacket(new PlayerColorChanged(Multiplayer.Session.LocalPlayer.Id, Float4.ToFloat4(GameMain.mainPlayer.mecha.mainColors)));
            Config.Options.SetMechaAppearance();
        }
    }
}
