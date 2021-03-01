using UnityEngine;

namespace NebulaClient.GameLogic
{
    public class LocalPlayerModel
    {
        public global::Player Data => GameMain.mainPlayer;
        public Transform Transform => GameMain.mainPlayer.transform;
    }
}
