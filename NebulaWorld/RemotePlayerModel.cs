#region

using NebulaWorld.MonoBehaviours.Remote;
using UnityEngine;
using UnityEngine.UI;

#endregion

namespace NebulaWorld;

public class RemotePlayerModel
{
    private const int PLAYER_PROTO_ID = 1;

    public RemotePlayerModel(ushort playerId, string username)
    {
        // Spawn remote player model by cloning the player prefab and replacing local player script by remote player ones.
        var playerPrefabPath = LDB.players.Select(PLAYER_PROTO_ID).PrefabPath;
        if (playerPrefabPath != null)
        {
            PlayerTransform = Object.Instantiate(Resources.Load<Transform>(playerPrefabPath));
            PlayerModelTransform = PlayerTransform.Find("Model");

            // Remove local player components
            Object.Destroy(PlayerTransform.GetComponent<PlayerFootsteps>());
            Object.Destroy(PlayerTransform.GetComponent<PlayerEffect>());
            Object.Destroy(PlayerTransform.GetComponent<PlayerAudio>());
            Object.Destroy(PlayerTransform.GetComponent<PlayerController>());
            PlayerTransform.GetComponent<Rigidbody>().isKinematic = true;

            // Add remote player components
            Movement = PlayerTransform.gameObject.AddComponent<RemotePlayerMovement>();
            PlayerTransform.gameObject.AddComponent<RemotePlayerEffects>();
            Animator = PlayerTransform.gameObject.AddComponent<RemotePlayerAnimation>();

            Movement.Username = username;
            Movement.PlayerID = playerId;

            PlayerTransform.GetComponent<PlayerAnimator>().Start();
            PlayerTransform.GetComponent<PlayerAnimator>().enabled = false;
        }

        if (PlayerTransform != null)
        {
            PlayerTransform.gameObject.name = $"Remote Player ({playerId})";

            PlayerInstance = new global::Player { transform = PlayerTransform };
        }
        if (Animator != null)
        {
            Animator.PlayerAnimator.player = PlayerInstance;
        }
        MechaInstance = new Mecha();
        if (PlayerInstance != null)
        {
            PlayerInstance.mecha = MechaInstance;
            MechaInstance.Init(GameMain.data, PlayerInstance);

            //Fix MechaDroneRenderers
            //todo:replace
            //MechaInstance.droneRenderer.mat_0 = new Material(Configs.builtin.mechaDroneMat.shader);
            //var mat = MechaInstance.droneRenderer.mat_0;
            //mat.CopyPropertiesFromMaterial(Configs.builtin.mechaDroneMat);

            //Fix MechaArmorModel
            if (PlayerModelTransform != null)
            {
                PlayerInstance.mechaArmorModel = PlayerModelTransform.GetComponent<MechaArmorModel>();
                var mechaArmorModel = PlayerInstance.mechaArmorModel;
                mechaArmorModel.data = PlayerInstance;
                mechaArmorModel.player = PlayerInstance;
                mechaArmorModel.mecha = MechaInstance;
                mechaArmorModel._OnCreate();
                mechaArmorModel._OnInit();

                for (var i = 0; i < PlayerModelTransform.childCount; i++)
                {
                    PlayerModelTransform.GetChild(i).gameObject.SetActive(true);
                }
            }
        }

        PlayerId = playerId;
        Username = username;
    }

    public string Username { get; set; }
    public ushort PlayerId { get; set; }
    public Transform PlayerTransform { get; set; }
    public Transform PlayerModelTransform { get; set; }
    public RemotePlayerMovement Movement { get; set; }
    public RemotePlayerAnimation Animator { get; set; }
    public GameObject InGameNameText { get; set; }
    public Text StarmapNameText { get; set; }
    public Transform StarmapTracker { get; set; }

    public global::Player PlayerInstance { get; set; }
    public Mecha MechaInstance { get; set; }

    public void Destroy()
    {
        Object.Destroy(PlayerTransform.gameObject);
        PlayerTransform = null;
        PlayerModelTransform = null;
        Movement = null;
        Animator = null;
        PlayerInstance.Free();
        PlayerInstance = null;
        if (StarmapTracker != null)
        {
            Object.Destroy(StarmapTracker.gameObject);
        }

        if (StarmapNameText != null)
        {
            Object.Destroy(StarmapNameText);
        }

        if (InGameNameText != null)
        {
            Object.Destroy(InGameNameText);
        }

        StarmapTracker = null;
        StarmapNameText = null;
        InGameNameText = null;
    }
}
