using NebulaWorld.MonoBehaviours.Remote;
using UnityEngine;

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
                // get effects from model
                PlayerTransform = Object.Instantiate(Resources.Load<Transform>(playerPrefabPath));
                PlayerModelTransform = PlayerTransform.Find("Model");

                ParticleSystem[] backEngineEffect = new ParticleSystem[2];
                ParticleSystemRenderer[] backEngineFlameRenderer = new ParticleSystemRenderer[2];
                ParticleSystem torchEffect = PlayerModelTransform.Find("bip/pelvis/spine-1/spine-2/spine-3/r-clavicle/r-upper-arm/r-forearm/r-torch/vfx-torch/blast").GetComponent<ParticleSystem>();

                Transform VFX = PlayerModelTransform.Find("bip/pelvis/spine-1/spine-2/spine-3/backpack/VFX").GetComponent<Transform>();

                backEngineEffect[0] = VFX.GetChild(0).GetComponent<ParticleSystem>();
                backEngineEffect[1] = VFX.GetChild(1).GetComponent<ParticleSystem>();

                backEngineFlameRenderer[0] = VFX.GetChild(0).GetComponent<ParticleSystemRenderer>();
                backEngineFlameRenderer[1] = VFX.GetChild(1).GetComponent<ParticleSystemRenderer>();

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

                Effects.setOrigParticlesValues(backEngineEffect, backEngineFlameRenderer, torchEffect);
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
