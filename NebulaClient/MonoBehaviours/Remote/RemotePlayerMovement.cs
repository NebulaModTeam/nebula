using NebulaClient.MonoBehaviours.Local;
using NebulaModel.DataStructures;
using NebulaModel.Packets.Players;
using NebulaModel.Utils;
using UnityEngine;

namespace NebulaClient.MonoBehaviours.Remote
{
    public class RemotePlayerMovement : MonoBehaviour
    {
        struct Snapshot
        {
            public long Timestamp { get; set; }
            public Vector3 Position { get; set; }
            public Quaternion Rotation { get; set; }
            public Quaternion BodyRotation { get; set; }
        }

        private Transform rootTransform;
        private Transform bodyTransform;

        // To have a smooth transition between position updates, we keep a buffer of states received 
        // and once the buffer is full, we start replaying the states from the oldest to the newest state.
        // This will make sure player movement is still smooth in high latency cases and even if there are dropped packets.
        Snapshot[] snapshotBuffer = new Snapshot[4];

        void Awake()
        {
            rootTransform = GetComponent<Transform>();
            bodyTransform = rootTransform.Find("Model");
        }

        public void Update()
        {
            // Wait for the entire buffer to be full before starting to interpolate the player position
            if (snapshotBuffer[0].Timestamp == 0)
                return;

            double past = (1000 / (double)LocalPlayerMovement.SEND_RATE) * (snapshotBuffer.Length - 1);
            double now = TimeUtils.CurrentUnixTimestampMilliseconds();
            double renderTime = now - past;

            for (int i = 0; i < snapshotBuffer.Length - 1; ++i)
            {
                var t1 = snapshotBuffer[i].Timestamp;
                var t2 = snapshotBuffer[i + 1].Timestamp;

                if (renderTime <= t2 && renderTime >= t1)
                {
                    var total = t2 - t1;
                    var portion = renderTime - t1;
                    var ratio = total > 0 ? portion / total : 1;
                    MoveInterpolated(snapshotBuffer[i], snapshotBuffer[i+1], (float)ratio);
                    break;
                }
                else if (i == snapshotBuffer.Length - 2 && renderTime > t2)
                {
                    MoveInterpolated(snapshotBuffer[i], snapshotBuffer[i+1], 1); 
                }
            }
        }

        public void UpdatePosition(Movement packet)
        {
            if (!rootTransform)
                return;

            for (int i = 0; i < snapshotBuffer.Length-1; ++i)
            {
                snapshotBuffer[i] = snapshotBuffer[i + 1];
            }

            snapshotBuffer[snapshotBuffer.Length - 1] = new Snapshot()
            {
                Timestamp = TimeUtils.CurrentUnixTimestampMilliseconds(),
                Position = packet.Position.ToUnity(),
                Rotation = Quaternion.Euler(packet.Rotation.ToUnity()),
                BodyRotation = Quaternion.Euler(packet.BodyRotation.ToUnity()),
            };
        }

        private void MoveInterpolated(Snapshot previous, Snapshot current, float ratio)
        {
            rootTransform.position = Vector3.Lerp(previous.Position, current.Position, ratio);
            rootTransform.rotation = Quaternion.Slerp(previous.Rotation, current.Rotation, ratio);
            bodyTransform.rotation = Quaternion.Slerp(previous.BodyRotation, current.BodyRotation, ratio);
        }
    }
}
