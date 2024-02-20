#region

using NebulaAPI.Packets;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Combat.GroundEnemy;
using NebulaWorld;
using static UnityEngine.UI.CanvasScaler;

#endregion

namespace NebulaNetwork.PacketProcessors.Combat.GroundEnemy;

[RegisterPacketProcessor]
public class DFGFormationAddUnitProcessor : PacketProcessor<DFGFormationAddUnitPacket>
{
    protected override void ProcessPacket(DFGFormationAddUnitPacket packet, NebulaConnection conn)
    {
        var factory = GameMain.galaxy.PlanetById(packet.PlanetId)?.factory;
        if (factory == null) return;

        var dFBase = factory.enemySystem.bases.buffer[packet.BaseId];
        using (Multiplayer.Session.Combat.IsIncomingRequest.On())
        {
            var portId = dFBase.forms[packet.FormId].AddUnit();
#if DEBUG
            if (portId != packet.PortId)
            {
                NebulaModel.Logger.Log.Warn($"DFGFormationAddUnitPacket wrong id {packet.PortId} => {portId}");
            }
#endif
        }
    }
}
