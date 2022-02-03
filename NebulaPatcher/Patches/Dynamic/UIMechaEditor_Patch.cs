using HarmonyLib;
using NebulaModel.Networking;
using NebulaModel.Packets.Players;
using NebulaWorld;

namespace NebulaPatcher.Patches.Dynamic
{
    [HarmonyPatch(typeof(UIMechaEditor))]
    public class UIMechaEditor_Patch
    {
        [HarmonyPostfix]
        [HarmonyPatch(nameof(UIMechaEditor.ApplyMechaAppearance))]
        public static void ApplyMechaAppearance_Postfix()
        {
            if (Multiplayer.IsActive)
            {
                using (BinaryUtils.Writer writer = new BinaryUtils.Writer())
                {
                    GameMain.mainPlayer.mecha.appearance.Export(writer.BinaryWriter);
                    Multiplayer.Session.Network.SendPacket<PlayerMechaArmor>(new PlayerMechaArmor(Multiplayer.Session.LocalPlayer.Id, writer.CloseAndGetBytes()));
                }
            }
        }
    }
}
