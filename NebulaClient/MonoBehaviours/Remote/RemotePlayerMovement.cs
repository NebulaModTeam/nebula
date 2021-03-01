using NebulaClient.MonoBehaviours.Local;
using NebulaModel.DataStructures;
using NebulaModel.Packets.Players;
using UnityEngine;

namespace NebulaClient.MonoBehaviours.Remote
{
    public class RemotePlayerMovement : MonoBehaviour
    {
        const float LERP_TIME = LocalPlayerMovement.BROADCAST_INTERVAL * 2;

        private Vector3 targetRootPosition;
        private Quaternion targetRootRotation;
        private Quaternion targetBodyRotation;

        private Vector3 lastRootPosition;
        private Quaternion lastRootRotation;
        private Quaternion lastBodyRotation;

        private float timeSinceLastFrame;

        private Transform rootTransform;
        private Transform bodyTransform;

        void Awake()
        {
            rootTransform = GetComponent<Transform>();
            bodyTransform = rootTransform.Find("Model");
        }

        public void Update()
        {
            timeSinceLastFrame += Time.deltaTime;

            if (timeSinceLastFrame <= LERP_TIME)
            {
                // use linear interpolate
                rootTransform.position = Vector3.Lerp(lastRootPosition, targetRootPosition, timeSinceLastFrame / LERP_TIME);
                rootTransform.rotation = Quaternion.Slerp(lastRootRotation, targetRootRotation, timeSinceLastFrame / LERP_TIME);
                bodyTransform.rotation = Quaternion.Slerp(lastBodyRotation, targetBodyRotation, timeSinceLastFrame / LERP_TIME);
            }
            else
            {
                // snap player to position
                rootTransform.position = targetRootPosition;
                rootTransform.rotation = targetRootRotation;
                bodyTransform.rotation = targetBodyRotation;
            }
        }

        public void UpdatePosition(Movement packet)
        {
            if (!rootTransform)
                return;

            // Set our last position / rotation to the current position / rotation
            lastRootPosition = rootTransform.position;
            lastRootRotation = rootTransform.rotation;
            lastBodyRotation = bodyTransform.rotation;

            // Set our target position / rotation
            targetRootPosition = packet.Position.ToUnity();
            targetRootRotation = Quaternion.Euler(packet.Rotation.ToUnity());
            targetBodyRotation = Quaternion.Euler(packet.BodyRotation.ToUnity());

            // Reset time since we received a new packet
            timeSinceLastFrame = 0;
        }
    }
}
