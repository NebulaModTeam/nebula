#region

using NebulaAPI.DataStructures;
using NebulaModel.Packets.Players;
using UnityEngine;

#endregion

namespace NebulaWorld.MonoBehaviours.Local;

public class LocalPlayerMovement : MonoBehaviour
{
    public const int SEND_RATE = 10;
    private const float BROADCAST_INTERVAL = 1f / SEND_RATE;
    private Transform bodyTransform;
    private PlayerAnimator playerAnimator;
    private Transform rootTransform;

    private float time;

    private void Awake()
    {
        rootTransform = GetComponent<Transform>();
        bodyTransform = rootTransform.Find("Model");
        playerAnimator = GetComponent<PlayerAnimator>();
    }

    private void Update()
    {
        // update navigation indicator
        Multiplayer.Session.Gizmos.OnUpdate();

        time += Time.deltaTime;

        if (!(time >= BROADCAST_INTERVAL))
        {
            return;
        }
        time = 0;

        var rotation = new Float3(rootTransform.eulerAngles);
        var bodyRotation = new Float3(bodyTransform.eulerAngles);

        var uPosition = new Double3(GameMain.mainPlayer.uPosition.x, GameMain.mainPlayer.uPosition.y,
            GameMain.mainPlayer.uPosition.z);
        var position = rootTransform.position;
        Multiplayer.Session.Network.SendPacket(new PlayerMovement(Multiplayer.Session.LocalPlayer.Id,
            GameMain.localPlanet?.id ?? -1, position.ToFloat3(), uPosition, rotation, bodyRotation,
            playerAnimator));

        var playerData = Multiplayer.Session.LocalPlayer.Data;
        playerData.BodyRotation = bodyRotation;
        playerData.LocalPlanetId = GameMain.localPlanet?.id ?? -1;
        playerData.LocalPlanetPosition = position.ToFloat3();
        playerData.LocalStarId = GameMain.localStar?.id ?? -1;
        playerData.Rotation = rotation;
        playerData.UPosition = uPosition;
    }
}
