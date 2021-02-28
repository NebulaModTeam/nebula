using NebulaClient.MonoBehaviours.Remote;
using UnityEngine;

namespace NebulaClient
{
    public class Player
    {
        public ushort PlayerId { get; protected set; }
        public Transform PlayerTransform { get; set; }
        public Transform PlayerModelTransform { get; set; }
        public RemotePlayerMovement Movement {get; set;}
        public RemotePlayerAnimation Animator {get; set; }

        public Player(ushort playerId)
        {
            PlayerId = playerId;
        }

        public void Destroy()
        {
            GameObject.Destroy(PlayerTransform.gameObject);
        }
    }
}
