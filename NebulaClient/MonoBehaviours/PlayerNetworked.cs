using NebulaModel.Packets;
using UnityEngine;

namespace NebulaClient.MonoBehaviours
{
    public class PlayerNetworked : MonoBehaviour
    {
        Vector3 lastPosition;
        Transform playerTransform;
        Transform playerModelTransform;

        public void Init()
        {
            playerTransform = GetComponent<Transform>();
            playerModelTransform = playerTransform.Find("Model");
        }

        public void Update()
        {
            if (MultiplayerSession.instance?.Client.IsSessionJoined ?? false)
            {
                if (lastPosition != playerTransform.position)
                {
                    MultiplayerSession.instance.Client.SendPacket(new Movement(playerTransform, playerModelTransform), LiteNetLib.DeliveryMethod.Unreliable);
                    lastPosition = playerTransform.position;
                }
            }
        }
    }
}
