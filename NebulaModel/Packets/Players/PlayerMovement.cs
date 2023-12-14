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
        warping = 4
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
    }

    // Movement
    public ushort PlayerId { get; }
    public int LocalPlanetId { get; }
    public Float3 LocalPlanetPosition { get; }
    public Double3 UPosition { get; }
    public Float3 Rotation { get; }
    public Float3 BodyRotation { get; }

    // Animation
    public EMovementState MovementState { get; }
    public float HorzSpeed { get; }
    public float VertSpeed { get; }
    public float Turning { get; }
    public float JumpWeight { get; }
    public float JumpNormalizedTime { get; }
    public byte IdleAnimIndex { get; }
    public byte MiningAnimIndex { get; }
    public float MiningWeight { get; }
    public EFlags Flags { get; }
}
