﻿using NebulaAPI;
using NebulaModel.Packets.Players;
using NebulaWorld.MonoBehaviours.Local;
using System;
using UnityEngine;

namespace NebulaWorld.MonoBehaviours.Remote
{
    public class RemotePlayerMovement : MonoBehaviour
    {
        private const int BUFFERED_SNAPSHOT_COUNT = 4;
        private const double INTERPOLATION_TIME = (1000 / (double)LocalPlayerMovement.SEND_RATE) * (BUFFERED_SNAPSHOT_COUNT - 1);

        public struct Snapshot
        {
            public long Timestamp { get; set; }
            public int LocalPlanetId { get; set; }
            public Float3 LocalPlanetPosition { get; set; }
            public Double3 UPosition { get; set; }
            public Quaternion Rotation { get; set; }
            public Quaternion BodyRotation { get; set; }
        }

        private Transform rootTransform;
        private Transform bodyTransform;

        public int localPlanetId;
        public VectorLF3 absolutePosition;

#if DEBUG
        private GameObject positionDebugger;
#endif

        // To have a smooth transition between position updates, we keep a buffer of states received 
        // and once the buffer is full, we start replaying the states from the oldest to the newest state.
        // This will make sure player movement is still smooth in high latency cases and even if there are dropped packets.
        private readonly Snapshot[] snapshotBuffer = new Snapshot[BUFFERED_SNAPSHOT_COUNT];

        private void Awake()
        {
            rootTransform = GetComponent<Transform>();
            bodyTransform = rootTransform.Find("Model");

            localPlanetId = -1;
            absolutePosition = Vector3.zero;
#if DEBUG
            positionDebugger = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            GameObject.Destroy(positionDebugger.GetComponent<SphereCollider>());
            positionDebugger.transform.SetParent(rootTransform, false);
            positionDebugger.transform.localScale = Vector3.one * 30;
            positionDebugger.GetComponent<MeshRenderer>().material = null;
            positionDebugger.SetActive(false);
#endif
        }

        private void Update()
        {
#if DEBUG
            if (Input.GetKeyDown(KeyCode.F10))
            {
                positionDebugger.SetActive(!positionDebugger.activeSelf);
            }
#endif

            // Wait for the entire buffer to be full before starting to interpolate the player position
            if (snapshotBuffer[0].Timestamp == 0)
            {
                return;
            }

            double renderTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - INTERPOLATION_TIME;

            for (int i = 0; i < snapshotBuffer.Length - 1; ++i)
            {
                long t1 = snapshotBuffer[i].Timestamp;
                long t2 = snapshotBuffer[i + 1].Timestamp;

                if (renderTime <= t2 && renderTime >= t1)
                {
                    long total = t2 - t1;
                    double reminder = renderTime - t1;
                    double ratio = total > 0 ? reminder / total : 1;

                    // We interpolate to the appropriate position between our 2 known snapshot
                    MoveInterpolated(snapshotBuffer[i], snapshotBuffer[i + 1], (float)ratio);
                    break;
                }

                if (i == snapshotBuffer.Length - 2 && renderTime > t2)
                {
                    // This will skip interpolation and will snap to the most recent position.
                    MoveInterpolated(snapshotBuffer[i], snapshotBuffer[i + 1], 1);
                }
            }
        }

        public void UpdatePosition(PlayerMovement movement)
        {
            if (!rootTransform)
            {
                return;
            }

            for (int i = 0; i < snapshotBuffer.Length - 1; ++i)
            {
                snapshotBuffer[i] = snapshotBuffer[i + 1];
            }

            snapshotBuffer[snapshotBuffer.Length - 1] = new Snapshot()
            {
                Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                LocalPlanetId = movement.LocalPlanetId,
                LocalPlanetPosition = movement.LocalPlanetPosition,
                UPosition = movement.UPosition,
                Rotation = Quaternion.Euler(movement.Rotation.ToVector3()),
                BodyRotation = Quaternion.Euler(movement.BodyRotation.ToVector3()),
            };
        }

        private void MoveInterpolated(Snapshot previous, Snapshot current, float ratio)
        {
            Vector3 previousRelativePosition = GetRelativePosition(previous);
            Vector3 currentRelativePosition = GetRelativePosition(current);
            Vector3 previousAbsolutePosition = GetAbsolutePosition(previous);
            Vector3 currentAbsolutePosition = GetAbsolutePosition(current);

            localPlanetId = current.LocalPlanetId;

            rootTransform.position = Vector3.Lerp(previousRelativePosition, currentRelativePosition, ratio);
            rootTransform.rotation = Quaternion.Slerp(previous.Rotation, current.Rotation, ratio);
            bodyTransform.rotation = Quaternion.Slerp(previous.BodyRotation, current.BodyRotation, ratio);

            absolutePosition = Vector3.Lerp(previousAbsolutePosition, currentAbsolutePosition, ratio);
        }

        private Vector3 GetRelativePosition(Snapshot snapshot)
        {
            if (GameMain.data == null)
            {
                return Vector3.zero;
            }

            // If we are on a local planet and the remote player is on the same local planet, we use the LocalPlanetPosition that is more precise.
            if (GameMain.localPlanet != null && GameMain.localPlanet.id == snapshot.LocalPlanetId)
            {
                return snapshot.LocalPlanetPosition.ToVector3();
            }

            // If the remote player is in space, we need to calculate the remote player relative position from our local player position.
            VectorLF3 uPosition = new VectorLF3(snapshot.UPosition.x, snapshot.UPosition.y, snapshot.UPosition.z);
            return Maths.QInvRotateLF(GameMain.data.relativeRot, uPosition - GameMain.data.relativePos);
        }

        private Vector3 GetAbsolutePosition(Snapshot snapshot)
        {
            // We only need to bother with uPos for this part for now
            return new VectorLF3(snapshot.UPosition.x, snapshot.UPosition.y, snapshot.UPosition.z);
        }

        public Snapshot GetLastPosition()
        {
            return snapshotBuffer[snapshotBuffer.Length - 1];
        }
    }
}
