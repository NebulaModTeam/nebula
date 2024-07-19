using NebulaAPI.Packets;
using NebulaModel.Logger;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Combat.DFRelay;
using UnityEngine;

namespace NebulaNetwork.PacketProcessors.Combat.DFRelay
{
    [RegisterPacketProcessor]
    public class DFRelayDirectionStateChangeProcessor : PacketProcessor<DFRelayDirectionStateChangePacket>
    {
        protected override void ProcessPacket(DFRelayDirectionStateChangePacket packet, NebulaConnection conn)
        {
            var hiveSystem = GameMain.spaceSector.GetHiveByAstroId(packet.HiveAstroId);
            if (hiveSystem == null) return;

            var dfRelayComponent = hiveSystem.relays.buffer[packet.RelayId];
            if (dfRelayComponent?.id != packet.RelayId) return;

            switch (packet.NewDirection)
            {
                case -1: //Relay is being sent home
                    dfRelayComponent.targetAstroId = 0;
                    dfRelayComponent.targetLPos = Vector3.zero;
                    dfRelayComponent.targetYaw = 0f;
                    dfRelayComponent.baseState = 0;
                    dfRelayComponent.baseId = 0;
                    dfRelayComponent.baseTicks = 0;
                    dfRelayComponent.baseEvolve = default;
                    dfRelayComponent.baseRespawnCD = 0;
                    dfRelayComponent.direction = -1;
                    dfRelayComponent.param0 = 0f;
                    dfRelayComponent.stage = packet.Stage;

                    Log.Debug($"Relay {dfRelayComponent.id} returning home");
                    break;
            }
        }
    }
}
