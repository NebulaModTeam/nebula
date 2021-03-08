using NebulaModel.Packets.Players;
using UnityEngine;

namespace NebulaWorld.MonoBehaviours.Remote
{
    public class RemotePlayerEffects : MonoBehaviour
    {
        private RemotePlayerAnimation rootAnimation;

        private ParticleSystem[] psys;
        private ParticleSystemRenderer[] psysr;
        private ParticleSystem torchEffect;

        public void Awake()
        {
            rootAnimation = GetComponent<RemotePlayerAnimation>();
        }

        public void UpdateState(PlayerAnimationUpdate packet)
        {
            if (rootAnimation.RunSlow.enabled || rootAnimation.RunFast.enabled || rootAnimation.Fly.enabled || rootAnimation.Sail.enabled || rootAnimation.Drift.enabled || rootAnimation.DriftF.enabled || rootAnimation.DriftL.enabled || rootAnimation.DriftR.enabled)
            {
                if (psys != null && psysr != null)
                {
                    if ((!rootAnimation.RunSlow.enabled && !rootAnimation.RunFast.enabled) || rootAnimation.Drift.enabled)
                    {
                        for (int i = 0; i < psys.Length; i++)
                        {
                            if (!psys[i].isPlaying)
                            {
                                psys[i].Play();
                            }
                        }
                    }
                    else
                    {
                        for (int j = 0; j < psys.Length; j++)
                        {
                            if (psys[j].isPlaying)
                            {
                                psys[j].Stop();
                            }
                        }
                    }
                    for (int i = 0; i < psysr.Length; i++)
                    {
                        if (rootAnimation.RunSlow.enabled || rootAnimation.RunFast.enabled)
                        {
                            if (rootAnimation.RunFast.weight != 0)
                            {
                                // when flying over the planet
                                psysr[i].lengthScale = Mathf.Lerp(-3.5f, -10f, Mathf.Max(packet.horzSpeed, packet.vertSpeed) * 0.03f);
                                Debug.Log("Flying over planet: " + rootAnimation.RunFast.weight + " (" + Mathf.Max(packet.horzSpeed, packet.vertSpeed) + ")");
                            }
                            else
                            {
                                // when "walking" over water and moving in air without button press or while "walking" over water
                                psysr[i].lengthScale = Mathf.Lerp(-3.5f, -10f, Mathf.Max(packet.horzSpeed, packet.vertSpeed) * 0.04f);
                                Debug.Log("Moving in air: " + rootAnimation.RunSlow.weight + " (" + Mathf.Max(packet.horzSpeed, packet.vertSpeed) + ")");
                            }
                        }
                        if (rootAnimation.Drift.enabled)
                        {
                            // when in air without pressing spacebar
                            psysr[i].lengthScale = -3.5f;
                            Debug.Log("Standing i air: " + rootAnimation.Drift.weight + " (" + Mathf.Max(packet.horzSpeed, packet.vertSpeed) + ")");
                        }
                        if (rootAnimation.Fly.enabled)
                        {
                            // when pressing spacebar but also when landing (Drift is disabled when landing)
                            psysr[i].lengthScale = Mathf.Lerp(-3.5f, -10f, Mathf.Max(packet.horzSpeed, packet.vertSpeed) * 0.03f);
                            Debug.Log("Starting: " + rootAnimation.Fly.weight + " (" + Mathf.Max(packet.horzSpeed, packet.vertSpeed) + ")");
                        }
                        /*
                        else
                        {
                            // todo: use right value for computation lika above.
                            psysr[i].lengthScale = Mathf.Lerp(-3.5f, -10f, Mathf.Max(horzSpeed, vertSpeed) * 0.03f);
                            Debug.Log("Drift: " + rootAnimation.Drift.weight + " | " + rootAnimation.DriftF.weight + " | " + rootAnimation.DriftL.weight + " | " + rootAnimation.DriftR.weight);
                        }
                        */
                    }
                }
            }
            else
            {
                if (psys != null && psysr != null)
                {
                    for (int i = 0; i < psys.Length; i++)
                    {
                        if (psys[i].isPlaying)
                        {
                            psys[i].Stop();
                        }
                    }
                }
            }
            
            if(torchEffect != null && rootAnimation.Mining0.weight > 0.99f)
            {
                if (!torchEffect.isPlaying)
                {
                    torchEffect.Play();
                }
            }
            else if(torchEffect != null && rootAnimation.Mining0.weight <= 0.99f)
            {
                if (torchEffect.isPlaying)
                {
                    torchEffect.Stop();
                }
            }
        }

        public void setOrigParticlesValues(ParticleSystem[] psys, ParticleSystemRenderer[] psysr, ParticleSystem torchEffect)
        {
            this.psys = psys;
            this.psysr = psysr;
            this.torchEffect = torchEffect;
        }
    }
}
