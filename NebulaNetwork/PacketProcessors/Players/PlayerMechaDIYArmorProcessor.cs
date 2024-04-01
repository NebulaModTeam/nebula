#region

using NebulaAPI.Packets;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Players;

#endregion

namespace NebulaNetwork.PacketProcessors.Players;

[RegisterPacketProcessor]
public class PlayerMechaDIYArmorProcessor : PacketProcessor<PlayerMechaDIYArmor>
{
    protected override void ProcessPacket(PlayerMechaDIYArmor packet, NebulaConnection conn)
    {
        // store DIYAppearance and items to serve them when player connects again
        if (IsHost)
        {
            using var reader = new BinaryUtils.Reader(packet.DIYAppearanceData);
            var player = Players.Get(conn);
            if (player == null)
            {
                return;
            }
            if (player.Data.DIYAppearance == null)
            {
                player.Data.DIYAppearance = new MechaAppearance();
                player.Data.DIYAppearance.Init();
            }
            player.Data.DIYAppearance.Import(reader.BinaryReader);

            player.Data.DIYItemId = packet.DIYItemId;
            player.Data.DIYItemValue = packet.DIYItemValue;
        }
        else
        {
            // load DIYAppearance received from host to display in mecha appearance editor
            using var reader = new BinaryUtils.Reader(packet.DIYAppearanceData);
            var player = GameMain.mainPlayer;
            var editor = UIRoot.instance.uiMechaEditor;

            if (player.mecha.diyAppearance == null)
            {
                player.mecha.diyAppearance = new MechaAppearance();
                player.mecha.diyAppearance.Init();
            }
            player.mecha.diyAppearance.Import(reader.BinaryReader);

            if (packet.DIYItemId.Length > 0)
            {
                player.mecha.diyItems.Clear();
                for (var i = 0; i < packet.DIYItemId.Length; i++)
                {
                    player.mecha.diyItems.items[packet.DIYItemId[i]] = packet.DIYItemValue[i];
                }
            }

            if (editor == null)
            {
                return;
            }
            editor.selection.ClearSelection();
            editor.saveGroup._Close();
            if (editor.mecha.diyAppearance == null)
            {
                editor.mecha.diyAppearance = new MechaAppearance();
                editor.mecha.diyAppearance.Init();
            }
            // Modify from UIMechaEditor.ApplyMechaAppearance
            GameMain.mainPlayer.mecha.diyAppearance.CopyTo(editor.mecha.diyAppearance);
            editor.mechaArmorModel.RefreshAllPartObjects();
            editor.mechaArmorModel.RefreshAllBoneObjects();
            editor.mecha.diyAppearance.NotifyAllEvents();
            editor.CalcMechaProperty(); // set mecha.hp and mecha.energyConsumptionCoef
            editor._left_content_height_max = 0f;
            editor.SetLeftScrollTop();
            editor.saveGroup._Open();
        }
    }
}
