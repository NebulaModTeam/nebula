using NebulaModel.Packets.Players;
using UnityEngine;

namespace NebulaWorld.MonoBehaviours.Remote
{
    public class RemotePlayerEffects : MonoBehaviour
    {
        private RemotePlayerAnimation rootAnimation;
        private Transform rootTransform;
        private Transform rootModelTransform;

        private ParticleSystem[] WaterEffect;
        private ParticleSystem[][] FootSmokeEffect;
        private ParticleSystem[] FootSmallSmoke;
        private ParticleSystem[] FootLargeSmoke;
        private ParticleSystem[] FootEffect;
        private ParticleSystem[] psys;
        private ParticleSystemRenderer[] psysr;
        private ParticleSystem torchEffect;

        VFAudio miningAudio = null;
        VFAudio driftAudio = null;
        VFAudio flyAudio0 = null, flyAudio1 = null;

        string[] solidSoundEvents = new string[4];
        string waterSoundEvent = "footsteps-6";

        int lastTriggeredFood = 0;
        int localPlanetId = -1;

        public void Awake()
        {
            rootAnimation = GetComponent<RemotePlayerAnimation>();
            rootTransform = GetComponent<Transform>();
            rootModelTransform = rootTransform.Find("Model");

            psys = new ParticleSystem[2];
            psysr = new ParticleSystemRenderer[2];
            torchEffect = rootModelTransform.Find("bip/pelvis/spine-1/spine-2/spine-3/r-clavicle/r-upper-arm/r-forearm/r-torch/vfx-torch/blast").GetComponent<ParticleSystem>();
            FootEffect = new ParticleSystem[2];
            WaterEffect = new ParticleSystem[2];
            FootSmokeEffect = new ParticleSystem[2][];
            FootSmokeEffect[0] = new ParticleSystem[2];
            FootSmokeEffect[1] = new ParticleSystem[2];
            FootSmallSmoke = new ParticleSystem[2];
            FootLargeSmoke = new ParticleSystem[2];

            Transform VFX = rootModelTransform.Find("bip/pelvis/spine-1/spine-2/spine-3/backpack/VFX").GetComponent<Transform>();

            psys[0] = VFX.GetChild(0).GetComponent<ParticleSystem>();
            psys[1] = VFX.GetChild(1).GetComponent<ParticleSystem>();

            psysr[0] = VFX.GetChild(0).GetComponent<ParticleSystemRenderer>();
            psysr[1] = VFX.GetChild(1).GetComponent<ParticleSystemRenderer>();

            WaterEffect[0] = rootModelTransform.Find("bip/pelvis/l-thigh/l-calf/l-ankle/l-foot/vfx-footsteps/water").GetComponent<ParticleSystem>();
            WaterEffect[1] = rootModelTransform.Find("bip/pelvis/r-thigh/r-calf/r-ankle/r-foot/vfx-footsteps/water").GetComponent<ParticleSystem>();
            FootSmokeEffect[0][0] = rootModelTransform.Find("bip/pelvis/l-thigh/l-calf/l-ankle/l-foot/vfx-footsteps/smoke").GetComponent<ParticleSystem>();
            FootSmokeEffect[0][1] = rootModelTransform.Find("bip/pelvis/l-thigh/l-calf/l-ankle/l-foot/vfx-footsteps/smoke-2").GetComponent<ParticleSystem>();
            FootSmokeEffect[1][0] = rootModelTransform.Find("bip/pelvis/r-thigh/r-calf/r-ankle/r-foot/vfx-footsteps/smoke").GetComponent<ParticleSystem>();
            FootSmokeEffect[1][1] = rootModelTransform.Find("bip/pelvis/r-thigh/r-calf/r-ankle/r-foot/vfx-footsteps/smoke-2").GetComponent<ParticleSystem>();
            FootEffect[0] = rootModelTransform.Find("bip/pelvis/l-thigh/l-calf/l-ankle/l-foot/vfx-footsteps").GetComponent<ParticleSystem>();
            FootEffect[1] = rootModelTransform.Find("bip/pelvis/r-thigh/r-calf/r-ankle/r-foot/vfx-footsteps").GetComponent<ParticleSystem>();
            FootSmallSmoke[0] = rootModelTransform.Find("bip/pelvis/l-thigh/l-calf/l-ankle/l-foot/vfx-footsteps/smoke").GetComponent<ParticleSystem>();
            FootSmallSmoke[1] = rootModelTransform.Find("bip/pelvis/r-thigh/r-calf/r-ankle/r-foot/vfx-footsteps/smoke").GetComponent<ParticleSystem>();
            FootLargeSmoke[0] = rootModelTransform.Find("bip/pelvis/l-thigh/l-calf/l-ankle/l-foot/vfx-footsteps/smoke-2").GetComponent<ParticleSystem>();
            FootLargeSmoke[1] = rootModelTransform.Find("bip/pelvis/r-thigh/r-calf/r-ankle/r-foot/vfx-footsteps/smoke-2").GetComponent<ParticleSystem>();

            solidSoundEvents[0] = "footsteps-0";
            solidSoundEvents[1] = "footsteps-1";
            solidSoundEvents[2] = "footsteps-2";
            solidSoundEvents[3] = "footsteps-3";

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

        private void playFootsteps()
        {
            float moveWeight = Mathf.Max(1f, Mathf.Pow(rootAnimation.RunSlow.weight + rootAnimation.RunFast.weight, 2f));
            bool trigger = (rootAnimation.RunSlow.enabled || rootAnimation.RunFast.enabled) && moveWeight > 0.15f;

            if (trigger)
            {
                int normalizedTime = Mathf.FloorToInt(rootAnimation.RunFast.normalizedTime * 2f - 0.02f);
                float normalizedTimeDiff = (rootAnimation.RunFast.normalizedTime * 2f - 0.02f) - (float)normalizedTime;
                bool timeIsEven = normalizedTime % 2 == 0;

                if(lastTriggeredFood != normalizedTime)
                {
                    lastTriggeredFood = normalizedTime;

                    Vector3 dustPos = ((!timeIsEven) ? FootEffect[1] : FootEffect[0]).transform.position;
                    Vector3 dustPosNormalized = dustPos.normalized;
                    dustPos += dustPos.normalized;

                    Ray ray = new Ray(dustPos, -dustPosNormalized);
                    float rDist1 = 100f, rDist2 = 100f;
                    float biomo = -1f;
                    RaycastHit rHit1, rHit2;

                    if(Physics.Raycast(ray, out rHit1, 2f, 512, QueryTriggerInteraction.Collide))
                    {
                        rDist1 = rHit1.distance;
                        biomo = rHit1.textureCoord2.x;
                    }
                    if(Physics.Raycast(ray, out rHit2, 2f, 16, QueryTriggerInteraction.Collide))
                    {
                        rDist2 = rHit2.distance + 0.1f;
                    }

                    if(normalizedTimeDiff < 0.3f && rDist1 < 1.8f && (0 < lastTriggeredFood || normalizedTime < 2))
                    {
                        playFootstepSound(moveWeight, biomo, rDist2 < rDist1);
                    }
                    if(normalizedTimeDiff < 0.5f && moveWeight > 0.5f && (rDist1 < 3f || rDist2 < 3f))
                    {
                        playFootstepEffect(timeIsEven, biomo, rDist2 < rDist1);
                    }
                }
            }
        }

        private void playFootstepSound(float vol, float biomo, bool water)
        {
            if(localPlanetId < 0)
            {
                // wait for update that should happen soon
                return;
            }

            AmbientDesc ambientDesc = GameMain.galaxy.PlanetById(localPlanetId).ambientDesc;
            string audioName = string.Empty;

            try
            {
                if (!water)
                {
                    if((double)biomo <= 0.8)
                    {
                        name = solidSoundEvents[ambientDesc.biomoSound0];
                    }
                    else if((double)biomo <= 1.8)
                    {
                        name = solidSoundEvents[ambientDesc.biomoSound1];
                    }
                    else
                    {
                        name = solidSoundEvents[ambientDesc.biomoSound2];
                    }
                    if (CheckPlayerInReform())
                    {
                        name = solidSoundEvents[3];
                    }
                }
                else
                {
                    name = waterSoundEvent;
                }
            }
            catch
            {
                name = string.Empty;
            }
            VFAudio audio = VFAudio.Create(name, base.transform, Vector3.zero, false, 0);
            audio.volumeMultiplier = vol;
            audio.Play();
        }

        private void playFootstepEffect(bool lr, float biomo, bool water)
        {
            if (CheckPlayerInReform() || localPlanetId < 0)
            {
                return;
            }

            ParticleSystem waterParticle = (!lr) ? WaterEffect[0] : WaterEffect[1];
            ParticleSystem[] smokeParticle = (!lr) ? FootSmokeEffect[0] : FootSmokeEffect[1];
            ParticleSystem footEffect = (!lr) ? FootEffect[0] : FootEffect[1];
            AmbientDesc ambientDesc = GameMain.galaxy.PlanetById(localPlanetId).ambientDesc;
            Color color = Color.clear;
            float dustStrength = 1f;

            if (!water)
            {
                if(biomo <= 0f)
                {
                    color = ambientDesc.biomoDustColor0;
                    dustStrength = ambientDesc.biomoDustStrength0;
                }
                else if(biomo <= 1f)
                {
                    color = Color.Lerp(ambientDesc.biomoDustColor0, ambientDesc.biomoColor1, biomo);
                    dustStrength = Mathf.Lerp(ambientDesc.biomoDustStrength0, ambientDesc.biomoDustStrength1, biomo);
                }
                else if(biomo <= 2f)
                {
                    color = Color.Lerp(ambientDesc.biomoDustColor1, ambientDesc.biomoColor2, biomo - 1f);
                    dustStrength = Mathf.Lerp(ambientDesc.biomoDustStrength1, ambientDesc.biomoDustStrength2, biomo - 1f);
                }
                else
                {
                    color = ambientDesc.biomoDustColor2;
                    dustStrength = ambientDesc.biomoDustStrength2;
                }
            }
            if(biomo >= 0f || water)
            {
                ParticleSystem.MainModule main = waterParticle.main;
                main.startColor = ((!water) ? Color.clear : Color.white);
                foreach(ParticleSystem p in smokeParticle)
                {
                    main = p.main;
                    main.startColor = color;
                }
                foreach(ParticleSystem p in FootSmallSmoke)
                {
                    main = p.main;
                    main.startLifetime = 0.8f + 0.2f * dustStrength;
                    main.startSize = 1.1f + 0.2f * dustStrength;
                }
                foreach(ParticleSystem p in FootLargeSmoke)
                {
                    main = p.main;
                    main.startLifetime = 1.2f * dustStrength;
                    main.startSize = 2.2f + 2.4f * dustStrength;
                }
                footEffect.Play();
            }
        }

        private bool CheckPlayerInReform()
        {
            PlanetData localPlanet = GameMain.galaxy.PlanetById(localPlanetId);
            bool result = false;
            if (localPlanet != null)
            {
                PlanetFactory factory = localPlanet.factory;
                if (factory != null)
                {
                    PlatformSystem platformSystem = factory.platformSystem;
                    if (platformSystem.reformData != null)
                    {
                        int reformIndexForPosition = platformSystem.GetReformIndexForPosition(rootTransform.position);
                        if (reformIndexForPosition > -1)
                        {
                            int reformType = platformSystem.GetReformType(reformIndexForPosition);
                            result = platformSystem.IsTerrainMapping(reformType);
                        }
                        else
                        {
                            result = true;
                        }
                    }
                    else
                    {
                        result = false;
                    }
                }
            }
            return result;
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
                        playFootsteps();
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

        public void UpdatePlanet(int localPlanet)
        {
            localPlanetId = localPlanet;
        }
    }
}
