#region

using NebulaAPI.DataStructures;
using NebulaAPI.Packets;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Warning;
using NebulaWorld;
using UnityEngine;

#endregion

namespace NebulaNetwork.PacketProcessors.Warning;

[RegisterPacketProcessor]
internal class WarningBroadcastDataProcessor : PacketProcessor<WarningBroadcastDataPacket>
{
    protected override void ProcessPacket(WarningBroadcastDataPacket packet, NebulaConnection conn)
    {
        using (Multiplayer.Session.Warning.IsIncomingBroadcast.On())
        {
            var vocal = (EBroadcastVocal)packet.Vocal;
            var factoryIndex = GameMain.data.galaxy.astrosFactory[packet.AstroId]?.index ?? -1;
            var astroId = packet.AstroId;
            var content = packet.Content;
            var lpos = packet.Lpos.ToVector3();

            if (lpos == Vector3.zero)
            {
                GameMain.data.warningSystem.Broadcast(vocal, factoryIndex, astroId, content);
            }
            else
            {
                GameMain.data.warningSystem.Broadcast(vocal, factoryIndex, astroId, content, lpos);
            }
        }
    }
}
