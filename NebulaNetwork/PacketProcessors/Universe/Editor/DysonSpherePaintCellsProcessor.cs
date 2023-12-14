#region

using NebulaAPI;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Universe;
using NebulaWorld;
using UnityEngine;

#endregion

namespace NebulaNetwork.PacketProcessors.Universe;

[RegisterPacketProcessor]
public class DysonSpherePaintCellsProcessor : PacketProcessor<DysonSpherePaintCellsPacket>
{
    public override void ProcessPacket(DysonSpherePaintCellsPacket packet, NebulaConnection conn)
    {
        var sphere = GameMain.data.dysonSpheres[packet.StarIndex];
        if (sphere == null)
        {
            return;
        }
        using (Multiplayer.Session.DysonSpheres.IsIncomingRequest.On())
        {
            var layer = sphere.GetLayer(packet.LayerId);
            if (layer == null)
            {
                Multiplayer.Session.DysonSpheres.HandleDesync(packet.StarIndex, conn);
                return;
            }

            // UIDEPaintingbox.OpenGrid()
            Color32[] cellColors;
            if (layer.cellColors != null)
            {
                Assert.True(layer.cellColors.Length == packet.CellCount);
                cellColors = layer.cellColors;
            }
            else
            {
                cellColors = new Color32[packet.CellCount];
            }

            // UIDysonPaintingGrid.PaintCells()
            var paint = packet.Paint.ToColor32();
            for (var i = 0; i < packet.CursorCells.Length; i++)
            {
                var cid = packet.CursorCells[i];
                if (cid >= 0)
                {
                    var color = cellColors[cid];
                    color.a -= color.a <= 127 ? 0 : 127;
                    color.a *= 2;
                    var color2 = Color32.Lerp(color, paint, packet.Strength);
                    color2.a /= 2;
                    color2.a += paint.a > 0 ? packet.SuperBrightMode ? 127 : 0 : 0;
                    cellColors[cid] = color2;
                }
            }
            layer.SetPaintingData(cellColors);
        }
        if (IsHost)
        {
            Multiplayer.Session.DysonSpheres.SendPacketToDysonSphereExcept(packet, packet.StarIndex, conn);
        }
    }
}
