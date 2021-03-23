using NebulaModel.DataStructures;
using NebulaModel.Packets.Players;
using NebulaModel.Utils;
using NebulaWorld.MonoBehaviours.Local;
using UnityEngine;

namespace NebulaWorld.MonoBehaviours.Remote
{
    public class RemotePlayerMovement : MonoBehaviour
    {
        private const int BUFFERED_SNAPSHOT_COUNT = 4;

        struct Snapshot
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
        private RemotePlayerEffects rootEffects;
        private RemoteWarpEffect rootWarp;

        public int localPlanetId;

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
            rootEffects = GetComponent<RemotePlayerEffects>();
            rootWarp = rootTransform.GetComponent<RemoteWarpEffect>();

            localPlanetId = -1;

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
                    var reminder = renderTime - t1;
                    var ratio = total > 0 ? reminder / total : 1;

                    // We interpolate to the appropriate position between our 2 known snapshot
                    MoveInterpolated(snapshotBuffer[i], snapshotBuffer[i + 1], (float)ratio);
                    break;
                }
                else if (i == snapshotBuffer.Length - 2 && renderTime > t2)
                {
                    // This will skip interpolation and will snap to the most recent position.
                    MoveInterpolated(snapshotBuffer[i], snapshotBuffer[i + 1], 1);
                }
            }
        }

        public void UpdatePosition(PlayerMovement movement)
        {
            if (!rootTransform)
                return;

            for (int i = 0; i < snapshotBuffer.Length - 1; ++i)
            {
                snapshotBuffer[i] = snapshotBuffer[i + 1];
            }

            snapshotBuffer[snapshotBuffer.Length - 1] = new Snapshot()
            {
                Timestamp = TimeUtils.CurrentUnixTimestampMilliseconds(),
                LocalPlanetId = movement.LocalPlanetId,
                LocalPlanetPosition = movement.LocalPlanetPosition,
                UPosition = movement.UPosition,
                Rotation = Quaternion.Euler(movement.Rotation.ToUnity()),
                BodyRotation = Quaternion.Euler(movement.BodyRotation.ToUnity()),
            };
        }

        private void MoveInterpolated(Snapshot previous, Snapshot current, float ratio)
        {
            Vector3 previousPosition = GetRelativePosition(previous);
            Vector3 currentPosition = GetRelativePosition(current);
            float deltaPosition = Vector3.Distance(previousPosition, currentPosition);
            Vector3 velocity = (previousPosition - currentPosition) / (previous.Timestamp - current.Timestamp);

            // needed as its null for some reason when we arrive here for the first time
            if(rootWarp == null)
            {
                rootWarp = rootTransform.GetComponent<RemoteWarpEffect>();
            }

            /*
             * 170 is round about where vanilla warping starts, for better testing lower this to something like 30
             * then you can trigger the warping animation by sailing at around 300
             * when its at 170 you will probably not be able to see the effect ingame   
             */
            if(deltaPosition >= 170 && rootWarp != null)
            {
                rootWarp.startWarp();
            }
            else if(deltaPosition < 170 && rootWarp != null && rootWarp.warpState >= 0.9)
            {
                rootWarp.stopWarp();
            }
            rootWarp.updateVelocity(velocity);

            localPlanetId = current.LocalPlanetId;

            rootTransform.position = Vector3.Lerp(previousPosition, currentPosition, ratio);
            rootTransform.rotation = Quaternion.Slerp(previous.Rotation, current.Rotation, ratio);
            bodyTransform.rotation = Quaternion.Slerp(previous.BodyRotation, current.BodyRotation, ratio);
        }

        private Vector3 GetRelativePosition(Snapshot snapshot)
        {
            // If we are on a local planet and the remote player is on the same local planet, we use the LocalPlanetPosition that is more precise.
            if (GameMain.localPlanet != null && GameMain.localPlanet.id == snapshot.LocalPlanetId)
            {
                return snapshot.LocalPlanetPosition.ToUnity();
            }

            // If the remote player is in space, we need to calculate the remote player relative position from our local player position.
            VectorLF3 uPosition = new VectorLF3(snapshot.UPosition.x, snapshot.UPosition.y, snapshot.UPosition.z);
            return Maths.QInvRotateLF(GameMain.data.relativeRot, uPosition - GameMain.data.relativePos);
        }
    }
}
