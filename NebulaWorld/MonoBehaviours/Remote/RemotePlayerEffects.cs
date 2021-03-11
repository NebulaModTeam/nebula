using NebulaModel.Packets.Players;
using UnityEngine;

namespace NebulaWorld.MonoBehaviours.Remote
{
    public class RemotePlayerEffects : MonoBehaviour
    {
        private RemotePlayerAnimation rootAnimation;
        private Transform rootTransform;

        private ParticleSystem[] psys;
        private ParticleSystemRenderer[] psysr;
        private ParticleSystem torchEffect;

        VFAudio miningAudio = null;
        VFAudio driftAudio = null;
        VFAudio flyAudio0 = null, flyAudio1 = null;

        public void Awake()
        {
            rootAnimation = GetComponent<RemotePlayerAnimation>();
            rootTransform = GetComponent<Transform>();
        }

        public void OnDestroy()
        {
            stopAllFlyAudio();
            if(miningAudio != null)
            {
                miningAudio.Stop();
                miningAudio = null;
            }
        }

        private void stopAllFlyAudio()
        {
            if (driftAudio != null)
            {
                driftAudio.Stop();
                driftAudio = null;
            }
            if(flyAudio0 != null)
            {
                flyAudio0.Stop();
                flyAudio0 = null;
            }
            if(flyAudio1 != null)
            {
                flyAudio1.Stop();
                flyAudio1 = null;
            }
        }

        private bool isGrounded()
        {
            Vector3 pos = rootTransform.position + rootTransform.position.normalized * 0.15f;
            if (Physics.CheckSphere(pos, 0.35f, 15873))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public void UpdateState(PlayerAnimationUpdate packet)
        {
            bool anyMovingAnimationActive = rootAnimation.RunSlow.enabled || rootAnimation.RunFast.enabled || rootAnimation.Fly.enabled || rootAnimation.Sail.enabled || rootAnimation.Drift.enabled || rootAnimation.DriftF.enabled || rootAnimation.DriftL.enabled || rootAnimation.DriftR.enabled || !isGrounded();
            bool anyDriftActive = rootAnimation.Drift.enabled || rootAnimation.DriftR.enabled || rootAnimation.DriftL.enabled || rootAnimation.DriftF.enabled;
            bool fireParticleOkay = psys != null && psysr != null && (psys[0] != null && psys[1] != null && psysr[0] != null && psysr[1] != null);

            if (anyMovingAnimationActive)
            {
                if (fireParticleOkay)
                {
                    if ((!rootAnimation.RunSlow.enabled && !rootAnimation.RunFast.enabled) || anyDriftActive)
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
                        stopAllFlyAudio();
                    }
                    for (int i = 0; i < psysr.Length; i++)
                    {
                        if (rootAnimation.RunSlow.enabled || rootAnimation.RunFast.enabled)
                        {
                            if (rootAnimation.RunFast.weight != 0)
                            {
                                // when flying over the planet
                                psysr[i].lengthScale = Mathf.Lerp(-3.5f, -10f, Mathf.Max(packet.horzSpeed, packet.vertSpeed) * 0.03f);
                            }
                            else
                            {
                                // when "walking" over water and moving in air without button press or while "walking" over water
                                psysr[i].lengthScale = Mathf.Lerp(-3.5f, -10f, Mathf.Max(packet.horzSpeed, packet.vertSpeed) * 0.04f);
                            }
                        }
                        if (rootAnimation.Drift.enabled)
                        {
                            // when in air without pressing spacebar
                            psysr[i].lengthScale = -3.5f;
                            if (driftAudio == null)
                            {
                                driftAudio = VFAudio.Create("drift", base.transform, Vector3.zero, false);
                                driftAudio?.Play();
                            }
                        }
                        else
                        {
                            if(driftAudio != null){
                                driftAudio.Stop();
                                driftAudio = null;
                            }
                        }
                        if (rootAnimation.Fly.enabled)
                        {
                            // when pressing spacebar but also when landing (Drift is disabled when landing)
                            psysr[i].lengthScale = Mathf.Lerp(-3.5f, -10f, Mathf.Max(packet.horzSpeed, packet.vertSpeed) * 0.03f);
                            if(flyAudio0 == null)
                            {
                                flyAudio0 = VFAudio.Create("fly-atmos", base.transform, Vector3.zero, false);
                                flyAudio0.Play();
                            }
                        }
                        else
                        {
                            if(flyAudio0 != null)
                            {
                                flyAudio0.Stop();
                                flyAudio0 = null;
                            }
                        }
                        if (rootAnimation.Sail.enabled)
                        {
                            psysr[i].lengthScale = Mathf.Lerp(-3.5f, -10f, Mathf.Max(packet.horzSpeed, packet.vertSpeed) * 0.03f);
                            if (flyAudio1 == null)
                            {
                                flyAudio1 = VFAudio.Create("fly-space", base.transform, Vector3.zero, false);
                                flyAudio1.Play();
                            }
                        }
                        else
                        {
                            if(flyAudio1 != null)
                            {
                                flyAudio1.Stop();
                                flyAudio1 = null;
                            }
                        }
                    }
                }
            }
            else
            {
                if (fireParticleOkay)
                {
                    for (int i = 0; i < psys.Length; i++)
                    {
                        if (psys[i].isPlaying)
                        {
                            psys[i].Stop();
                        }
                    }
                    stopAllFlyAudio();
                }
            }
            
            if(torchEffect != null && rootAnimation.Mining0.weight > 0.99f)
            {
                if (!torchEffect.isPlaying && miningAudio == null)
                {
                    torchEffect.Play();
                    miningAudio = VFAudio.Create("mecha-mining", base.transform, Vector3.zero, false);
                    miningAudio?.Play();
                }
            }
            else if(torchEffect != null && rootAnimation.Mining0.weight <= 0.99f)
            {
                if (torchEffect.isPlaying)
                {
                    torchEffect.Stop();
                    miningAudio?.Stop();
                    miningAudio = null;
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
