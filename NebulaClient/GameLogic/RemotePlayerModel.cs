using NebulaClient.MonoBehaviours.Remote;
using UnityEngine;

using System.Security;
using System.Security.Permissions;

[module: UnverifiableCode]
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]

namespace NebulaClient.GameLogic
{
    public class RemotePlayerModel
    {
        const int PLAYER_PROTO_ID = 1;

        public Transform PlayerTransform { get; set; }
        public Transform PlayerModelTransform { get; set; }
        public RemotePlayerMovement Movement { get; set; }
        public RemotePlayerAnimation Animator { get; set; }
        public RemotePlayerEffects Effects { get; set; }

        public RemotePlayerModel(ushort playerId)
        {
            // Spawn remote player model by cloning the player prefab and replacing local player script by remote player ones.
            string playerPrefabPath = LDB.players.Select(PLAYER_PROTO_ID).PrefabPath;
            if (playerPrefabPath != null)
            {
                PlayerTransform = Object.Instantiate(Resources.Load<Transform>(playerPrefabPath));
                PlayerModelTransform = PlayerTransform.Find("Model");

                ParticleSystem[] backEngineEffect = PlayerTransform.GetComponent<PlayerEffect>().backEngineEffect;
                ParticleSystemRenderer[] backEngineFlameRenderer = PlayerTransform.GetComponent<PlayerEffect>().backEngineFlameRenderer;

                // Remove local player components
                Object.Destroy(PlayerTransform.GetComponent<PlayerFootsteps>());
                Object.Destroy(PlayerTransform.GetComponent<PlayerEffect>());
                Object.Destroy(PlayerTransform.GetComponent<PlayerAudio>());
                Object.Destroy(PlayerTransform.GetComponent<PlayerAnimator>());
                Object.Destroy(PlayerTransform.GetComponent<PlayerController>());
                PlayerTransform.GetComponent<Rigidbody>().isKinematic = true;

                // Add remote player components
                Movement = PlayerTransform.gameObject.AddComponent<RemotePlayerMovement>();
                Animator = PlayerTransform.gameObject.AddComponent<RemotePlayerAnimation>();
                Effects = PlayerTransform.gameObject.AddComponent<RemotePlayerEffects>();

                Effects.setOrigParticlesValues(backEngineEffect, backEngineFlameRenderer);
            }

            PlayerTransform.gameObject.name = $"Remote Player ({playerId})";
        }

        public void Destroy()
        {
            Object.Destroy(PlayerTransform.gameObject);
            PlayerTransform = null;
            PlayerModelTransform = null;
            Movement = null;
            Animator = null;
        }
    }
}
