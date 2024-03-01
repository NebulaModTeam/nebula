#region

using NebulaAPI.DataStructures;
using NebulaAPI.Packets;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Combat.Mecha;
using NebulaWorld;
using UnityEngine;

#endregion

namespace NebulaNetwork.PacketProcessors.Combat.Mecha;

[RegisterPacketProcessor]
public class MechaBombProcessor : PacketProcessor<MechaBombPacket>
{
    protected override void ProcessPacket(MechaBombPacket packet, NebulaConnection conn)
    {
        if (IsHost)
        {
            if (packet.NearStarId > 0)
            {
                Multiplayer.Session.Network.SendPacketToStarExclude(packet, packet.NearStarId, conn);
            }
            else
            {
                Multiplayer.Session.Network.SendPacketExclude(packet, conn);
            }
        }

        VectorLF3 uPos;
        Quaternion uRot;
        using (Multiplayer.Session.World.GetRemotePlayersModels(out var remotePlayersModels))
        {
            if (!remotePlayersModels.TryGetValue(packet.PlayerId, out var playerModel)) return;
            // Set bomb using the model uPos and uRot that is interpolated in the past
            const double DeltaTime = 0.016666667; // 1/60 second
            uPos = playerModel.MechaInstance.skillBombingUCenter - packet.UVelocity.ToVectorLF3() * DeltaTime;
            uRot = playerModel.PlayerInstance.uRotation;
        }

        // Modify from PlayerAction.Bombing
        var skillSystem = GameMain.data.spaceSector.skillSystem;
        var itemId = packet.ProtoId;
        var itemProto = LDB.items.Select(itemId);
        if (itemProto == null) return;
        switch (itemProto.BombType)
        {
            case EBombType.Liquid:
                ref var ptr1 = ref skillSystem.liquidBombs.Add();
                ptr1.nearStarId = packet.NearStarId;
                ptr1.uPos = uPos;
                ptr1.uRot = uRot;
                ptr1.uVel = packet.UVel.ToVectorLF3();
                ptr1.uAgl = packet.UAgl.ToVector3();
                ptr1.life = 3600;
                ptr1.caster.id = packet.PlayerId;
                ptr1.caster.type = ETargetType.Player;
                ptr1.protoId = itemId;
                ptr1.ApplyConfigs();
                break;

            case EBombType.ExplosiveUnit:
                ref var ptr2 = ref skillSystem.explosiveUnitBombs.Add();
                ptr2.nearStarId = packet.NearStarId;
                ptr2.uPos = uPos;
                ptr2.uRot = uRot;
                ptr2.uVel = packet.UVel.ToVectorLF3();
                ptr2.uAgl = packet.UAgl.ToVector3();
                ptr2.life = 3600;
                ptr2.mask = ETargetTypeMask.Vegetable | ETargetTypeMask.Enemy;
                ptr2.abilityValue = (int)(itemProto.Ability * GameMain.history.blastDamageScale + 0.5f);
                ptr2.caster.id = packet.PlayerId;
                ptr2.caster.type = ETargetType.Player;
                ptr2.protoId = itemId;
                ptr2.ApplyConfigs();
                break;

            case EBombType.EMCapusle:
                ref var ptr3 = ref skillSystem.emCapsuleBombs.Add();
                ptr3.nearStarId = packet.NearStarId;
                ptr3.uPos = uPos;
                ptr3.uRot = uRot;
                ptr3.uVel = packet.UVel.ToVectorLF3();
                ptr3.uAgl = packet.UAgl.ToVector3();
                ptr3.life = 3600;
                ptr3.mask = ETargetTypeMask.Vegetable | ETargetTypeMask.Enemy;
                ptr3.abilityValue = (int)(itemProto.Ability * GameMain.history.magneticDamageScale + 0.5f);
                ptr3.caster.id = packet.PlayerId;
                ptr3.caster.type = ETargetType.Player;
                ptr3.protoId = itemId;
                ptr3.ApplyConfigs();
                break;
        }
    }
}
