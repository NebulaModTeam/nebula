#region

using System;
using NebulaAPI.DataStructures;
using NebulaAPI.Packets;

#endregion

namespace NebulaModel.Packets.Players;

[HidePacketInDebugLogs]
public class PlayerMovement
{
    [Flags]
    public enum EFlags : byte
    {
        isGrounded = 1,
        inWater = 2,
        warping = 4,
        hasShield = 8,
        chargeShieldBurst = 16
    }

    public PlayerMovement() { }

    public PlayerMovement(ushort playerId, int localPlanetId, Float3 localPlanetPosition, Double3 uPosition, Float3 rotation,
        Float3 bodyRotation, PlayerAnimator animator)
    {
        // Movement
        PlayerId = playerId;
        LocalPlanetId = localPlanetId;
        LocalPlanetPosition = localPlanetPosition;
        UPosition = uPosition;
        Rotation = rotation;
        BodyRotation = bodyRotation;

        // Animation
        MovementState = animator.movementState;
        HorzSpeed = animator.controller.horzSpeed;
        VertSpeed = animator.controller.vertSpeed;
        Turning = animator.turning;
        JumpWeight = animator.jumpWeight;
        JumpNormalizedTime = animator.jumpNormalizedTime;

        //Compress AnimIndex, assume their values are less than 256
        IdleAnimIndex = (byte)animator.idleAnimIndex;
        MiningAnimIndex = (byte)animator.miningAnimIndex;
        MiningWeight = animator.miningWeight;

        Flags = 0;
        if (animator.controller.actionWalk.isGrounded)
        {
            Flags |= EFlags.isGrounded;
        }
        if (animator.controller.actionDrift.inWater)
        {
            Flags |= EFlags.inWater;
        }
        if (animator.player.warping)
        {
            Flags |= EFlags.warping;
        }
        var mecha = animator.controller.mecha;
        if (mecha.energyShieldEnergy > 0)
        {
            Flags |= EFlags.hasShield;
        }
        if (mecha.energyShieldBurstProgress > 0f)
        {
            Flags |= EFlags.chargeShieldBurst;
        }
    }

    // Movement
    public ushort PlayerId { get; set; }
    public int LocalPlanetId { get; set; }
    public Float3 LocalPlanetPosition { get; set; }
    public Double3 UPosition { get; set; }
    public Float3 Rotation { get; set; }
    public Float3 BodyRotation { get; set; }

    // Animation
    public EMovementState MovementState { get; set; }
    public float HorzSpeed { get; set; }
    public float VertSpeed { get; set; }
    public float Turning { get; set; }
    public float JumpWeight { get; set; }
    public float JumpNormalizedTime { get; set; }
    public byte IdleAnimIndex { get; set; }
    public byte MiningAnimIndex { get; set; }
    public float MiningWeight { get; set; }
    public EFlags Flags { get; set; }
}
