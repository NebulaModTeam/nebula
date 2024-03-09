#region

using NebulaAPI.DataStructures;
using NebulaAPI.Packets;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Combat.DFRelay;
using NebulaWorld;

#endregion

namespace NebulaNetwork.PacketProcessors.Combat.DFRelay;

[RegisterPacketProcessor]
public class DFRelayLeaveDockProcessor : PacketProcessor<DFRelayLeaveDockPacket>
{
    protected override void ProcessPacket(DFRelayLeaveDockPacket packet, NebulaConnection conn)
    {
        var hiveSystem = GameMain.spaceSector.GetHiveByAstroId(packet.HiveAstroId);
        if (hiveSystem == null) return;

        var dfrelayComponent = hiveSystem.relays.buffer[packet.RelayId];
        if (dfrelayComponent?.id != packet.RelayId) return;

        ref var enemyData = ref GameMain.spaceSector.enemyPool[dfrelayComponent.enemyId];
        ref var animData = ref GameMain.spaceSector.enemyAnimPool[dfrelayComponent.enemyId];
        enemyData.astroId = packet.HiveAstroId;
        dfrelayComponent.uSpeed = 0f;
        enemyData.pos = dfrelayComponent.dock.pos;
        enemyData.rot = dfrelayComponent.dock.rot;
        enemyData.vel.x = 0f;
        enemyData.vel.y = 0f;
        enemyData.vel.z = 0f;
        animData.power = 0f;
        animData.time = 0f;

        using (Multiplayer.Session.Enemies.IsIncomingRelayRequest.On())
        {
            dfrelayComponent.targetAstroId = packet.TargetAstroId;
            dfrelayComponent.baseId = packet.BaseId;
            dfrelayComponent.targetLPos = packet.TargetLPos.ToVector3();
            dfrelayComponent.targetYaw = packet.TargetYaw;
            dfrelayComponent.baseState = 0;
            dfrelayComponent.ResetSearchStates();

            dfrelayComponent.stage = -2;
            dfrelayComponent.direction = 1;
            dfrelayComponent.LeaveDock();
        }
    }
}
