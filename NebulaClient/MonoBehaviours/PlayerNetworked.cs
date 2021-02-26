using NebulaModel.Packets;
using UnityEngine;

namespace NebulaClient.MonoBehaviours
{
    public class PlayerNetworked : MonoBehaviour
    {
        public void Init()
        {
            MultiplayerSession.instance.Client.SendPacket(new PlayerSpawned(GameMain.mainPlayer.transform), LiteNetLib.DeliveryMethod.ReliableOrdered);
        }

        public void Update()
        {
            MultiplayerSession.instance.Client.SendPacket(new Movement(GameMain.mainPlayer.transform), LiteNetLib.DeliveryMethod.Unreliable);
        }
    }
}
