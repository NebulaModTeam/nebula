using HarmonyLib;
using NebulaModel.Networking;
using NebulaModel.Packets.Players;
using NebulaWorld;
using System.Linq;

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
                    Multiplayer.Session.Network.SendPacket(new PlayerMechaArmor(Multiplayer.Session.LocalPlayer.Id, writer.CloseAndGetBytes()));
                }
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(nameof(UIMechaEditor._OnClose))]
        public static void OnClose_Postfix()
        {
            if (Multiplayer.IsActive && Multiplayer.Session.LocalPlayer.IsClient)
            {
                using (BinaryUtils.Writer writer = new BinaryUtils.Writer())
                {
                    GameMain.mainPlayer.mecha.diyAppearance.Export(writer.BinaryWriter);
                    Multiplayer.Session.Network.SendPacket(new PlayerMechaDIYArmor(writer.CloseAndGetBytes(), GameMain.mainPlayer.mecha.diyItems.items.Keys.ToArray(), GameMain.mainPlayer.mecha.diyItems.items.Values.ToArray()));
                }
            }
        }
    }
}
