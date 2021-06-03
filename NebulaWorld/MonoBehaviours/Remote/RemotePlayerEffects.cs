using HarmonyLib;
using NebulaModel.Packets.Players;
using System;
using UnityEngine;

namespace NebulaWorld.MonoBehaviours.Remote
{
    public class RemoteWarpEffect : MonoBehaviour
    {
        private Transform rootTransform;

        private VFWarpEffect warpEffect = null;
        private bool isWarping = false;

        private Material tunnelMat;
        private Material distortMat;
        private Material astrosMat;
        private Material nebulasMat;

        private ParticleSystem astrosParticles;
        private ParticleSystem nebulasParticles;

        private MeshRenderer tunnelRenderer;
        private MeshRenderer distortRenderer;

        private ParticleSystemRenderer astrosRenderer;
        private ParticleSystemRenderer nebulasRenderer;

        private AnimationCurve intensByState;
        private AnimationCurve intensByState_astro;

        private float tunnelMul;
        private float distortMul;
        private float astrosMul;
        private float nebulasMul;

        public float WarpState = 0;
        private bool warpEffectActivated = false;

        Vector4[] warpRotations;
        Vector3 velocity;

        RemotePlayerAnimation rootAnimation = null;
        public void Awake()
        {
            rootTransform = GetComponent<Transform>();
            rootAnimation = GetComponent<RemotePlayerAnimation>();

            warpEffect = UnityEngine.Object.Instantiate<VFWarpEffect>(Configs.builtin.warpEffectPrefab, GetComponent<Transform>());
            warpEffect.enabled = false;

            tunnelMat = (Material)AccessTools.Field(typeof(VFWarpEffect), "tunnelMat").GetValue(warpEffect);
            distortMat = (Material)AccessTools.Field(typeof(VFWarpEffect), "distortMat").GetValue(warpEffect);
            astrosMat = (Material)AccessTools.Field(typeof(VFWarpEffect), "astrosMat").GetValue(warpEffect);
            nebulasMat = (Material)AccessTools.Field(typeof(VFWarpEffect), "nebulasMat").GetValue(warpEffect);

            astrosParticles = warpEffect.astrosParticles;
            nebulasParticles = warpEffect.nebulasParticles;

            tunnelRenderer = warpEffect.tunnelRenderer;
            distortRenderer = warpEffect.distortRenderer;

            astrosRenderer = warpEffect.astrosRenderer;
            nebulasRenderer = warpEffect.nebulasRenderer;

            tunnelMul = (float)AccessTools.Field(typeof(VFWarpEffect), "tunnelMul").GetValue(warpEffect);
            distortMul = (float)AccessTools.Field(typeof(VFWarpEffect), "distortMul").GetValue(warpEffect);
            astrosMul = (float)AccessTools.Field(typeof(VFWarpEffect), "astrosMul").GetValue(warpEffect);
            nebulasMul = (float)AccessTools.Field(typeof(VFWarpEffect), "nebulasMul").GetValue(warpEffect);

            intensByState_astro = warpEffect.intensByState_astro;
            intensByState = warpEffect.intensByState;

            warpRotations = new Vector4[24];

            tunnelMat = UnityEngine.Object.Instantiate<Material>(tunnelRenderer.sharedMaterial);
            distortMat = UnityEngine.Object.Instantiate<Material>(distortRenderer.sharedMaterial);
            astrosMat = UnityEngine.Object.Instantiate<Material>(astrosRenderer.sharedMaterial);
            nebulasMat = UnityEngine.Object.Instantiate<Material>(nebulasRenderer.sharedMaterial);

            tunnelRenderer.sharedMaterial = tunnelMat;
            distortRenderer.sharedMaterial = distortMat;
            astrosRenderer.sharedMaterial = astrosMat;
            nebulasRenderer.sharedMaterial = nebulasMat;

            tunnelMul = tunnelMat.GetFloat("_Multiplier");
            distortMul = distortMat.GetFloat("_DistortionStrength");
            astrosMul = astrosMat.GetFloat("_Multiplier");
            nebulasMul = nebulasMat.GetFloat("_Multiplier");

            for (int i = 0; i < warpRotations.Length; i++)
            {
                warpRotations[i] = new Vector4(0f, 0f, 0f, 1f);
            }

            toggleEffect(false);
        }

        public void updateVelocity(Vector3 vel)
        {
            velocity = vel;
        }

        private void toggleEffect(bool toggle)
        {
            if (toggle)
            {
                astrosParticles.Play();
                nebulasParticles.Play();

                warpEffectActivated = true;
            }
            else
            {
                astrosParticles.Stop();
                nebulasParticles.Stop();

                warpEffectActivated = false;
            }
            tunnelRenderer.gameObject.SetActive(toggle);
            distortRenderer.gameObject.SetActive(toggle);
            astrosRenderer.gameObject.SetActive(toggle);
            nebulasRenderer.gameObject.SetActive(toggle);
        }

        public void startWarp()
        {
            if (!rootAnimation.Sail.enabled || isWarping)
            {
                return;
            }

            isWarping = true;
        }

        public void stopWarp()
        {
            if (!rootAnimation.Sail.enabled || !isWarping)
            {
                return;
            }

            isWarping = false;
        }

        public void Update()
        {
            if (isWarping)
            {
                WarpState += 0.0055655558f;
                if (WarpState > 1f)
                {
                    WarpState = 1f;
                }
            }
            else
            {
                WarpState -= 0.06667667f;
                if (WarpState < 0f)
                {
                    WarpState = 0f;
                }
            }

            Vector4 playerRot = new Vector4(rootTransform.rotation.x, rootTransform.rotation.y, rootTransform.rotation.z, rootTransform.rotation.w);
            if (WarpState > 0.001f && !warpEffectActivated)
            {
                for (int i = 0; i < warpRotations.Length; i++)
                {
                    warpRotations[i] = playerRot;
                }
                VFAudio.Create("warp-begin", base.transform, Vector3.zero, true, 0);
                toggleEffect(true);
            }
            else if (WarpState == 0 && warpEffectActivated)
            {
                VFAudio.Create("warp-end", base.transform, Vector3.zero, true, 0);
                toggleEffect(false);
            }

            Array.Copy(warpRotations, 0, warpRotations, 1, warpRotations.Length - 1);
            warpRotations[0] = playerRot;

            ParticleSystem.EmissionModule emission = astrosParticles.emission;
            ParticleSystem.VelocityOverLifetimeModule velocityOverTime = astrosParticles.velocityOverLifetime;
            ParticleSystem.ShapeModule shape = astrosParticles.shape;

            Vector3 lhs = velocity.normalized;

            // to compute the emission we would need to know the players local star, so default to this for now
            emission.rateOverTime = 120f;
            velocityOverTime.speedModifierMultiplier = 20000f;
            velocityOverTime.x = (float)lhs.x;
            velocityOverTime.y = (float)lhs.y;
            velocityOverTime.z = (float)lhs.z;
            shape.position = lhs * 10000.0f;
            shape.rotation = rootTransform.rotation.eulerAngles;

            distortRenderer.GetComponent<Transform>().localRotation = rootTransform.rotation;
            nebulasRenderer.GetComponent<Transform>().localRotation = rootTransform.rotation;
            float num1 = intensByState.Evaluate(WarpState);
            float num2 = intensByState_astro.Evaluate(WarpState);
            tunnelMat.SetFloat("_Multiplier", tunnelMul * num1);
            tunnelMat.SetVectorArray("_WarpRotations", warpRotations);
            distortMat.SetFloat("_DistortionStrength", distortMul * num1);
            astrosMat.SetFloat("_Multiplier", astrosMul * num2);
            nebulasMat.SetFloat("_Multiplier", nebulasMul * num2);
        }
    }
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

        private VFAudio miningAudio = null;
        private VFAudio driftAudio = null;
        private VFAudio flyAudio0 = null, flyAudio1 = null;

        private string[] solidSoundEvents = new string[4];
        private string waterSoundEvent = "footsteps-6";

        private float maxAltitude = 0;
        private int lastTriggeredFood = 0;
        private int localPlanetId = -1;

        Collider[] collider;
        float vegeCollideColdTime = 0;

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

            psysr[0] = VFX.GetChild(0).Find("flames").GetComponent<ParticleSystemRenderer>();
            psysr[1] = VFX.GetChild(1).Find("flames").GetComponent<ParticleSystemRenderer>();

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

            collider = new Collider[16];

            rootTransform.gameObject.AddComponent<RemoteWarpEffect>();

        }

        public void OnDestroy()
        {
            stopAllFlyAudio();
            if (miningAudio != null)
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
            if (flyAudio0 != null)
            {
                flyAudio0.Stop();
                flyAudio0 = null;
            }
            if (flyAudio1 != null)
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

                if (lastTriggeredFood != normalizedTime)
                {
                    lastTriggeredFood = normalizedTime;

                    Vector3 dustPos = ((!timeIsEven) ? FootEffect[1] : FootEffect[0]).transform.position;
                    Vector3 dustPosNormalized = dustPos.normalized;
                    dustPos += dustPos.normalized;

                    Ray ray = new Ray(dustPos, -dustPosNormalized);
                    float rDist1 = 100f, rDist2 = 100f;
                    float biomo = -1f;
                    RaycastHit rHit1, rHit2;

                    if (Physics.Raycast(ray, out rHit1, 2f, 512, QueryTriggerInteraction.Collide))
                    {
                        rDist1 = rHit1.distance;
                        biomo = rHit1.textureCoord2.x;
                    }
                    if (Physics.Raycast(ray, out rHit2, 2f, 16, QueryTriggerInteraction.Collide))
                    {
                        rDist2 = rHit2.distance + 0.1f;
                    }

                    if (normalizedTimeDiff < 0.3f && rDist1 < 1.8f && (0 < lastTriggeredFood || normalizedTime < 2))
                    {
                        playFootstepSound(moveWeight, biomo, rDist2 < rDist1);
                    }
                    if (normalizedTimeDiff < 0.5f && moveWeight > 0.5f && (rDist1 < 3f || rDist2 < 3f))
                    {
                        playFootstepEffect(timeIsEven, biomo, rDist2 < rDist1);
                    }
                }
            }
        }

        private void playFootstepSound(float vol, float biomo, bool water)
        {
            if (localPlanetId < 0)
            {
                // wait for update that should happen soon
                // its updated by localPlanetSyncProcessor.cs
                return;
            }

            AmbientDesc ambientDesc = GameMain.galaxy.PlanetById(localPlanetId).ambientDesc;
            string audioName = string.Empty;

            try
            {
                if (!water)
                {
                    if ((double)biomo <= 0.8)
                    {
                        name = solidSoundEvents[ambientDesc.biomoSound0];
                    }
                    else if ((double)biomo <= 1.8)
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
                if (biomo <= 0f)
                {
                    color = ambientDesc.biomoDustColor0;
                    dustStrength = ambientDesc.biomoDustStrength0;
                }
                else if (biomo <= 1f)
                {
                    color = Color.Lerp(ambientDesc.biomoDustColor0, ambientDesc.biomoColor1, biomo);
                    dustStrength = Mathf.Lerp(ambientDesc.biomoDustStrength0, ambientDesc.biomoDustStrength1, biomo);
                }
                else if (biomo <= 2f)
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
            if (biomo >= 0f || water)
            {
                ParticleSystem.MainModule main = waterParticle.main;
                main.startColor = ((!water) ? Color.clear : Color.white);
                foreach (ParticleSystem p in smokeParticle)
                {
                    main = p.main;
                    main.startColor = color;
                }
                foreach (ParticleSystem p in FootSmallSmoke)
                {
                    main = p.main;
                    main.startLifetime = 0.8f + 0.2f * dustStrength;
                    main.startSize = 1.1f + 0.2f * dustStrength;
                }
                foreach (ParticleSystem p in FootLargeSmoke)
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

        private bool DriftDetermineInWater(PlanetData pData)
        {
            if (localPlanetId < 0)
            {
                return false;
            }

            if (pData != null)
            {
                float currAltitude = Mathf.Max(rootTransform.position.magnitude, pData.realRadius * 0.9f) - pData.realRadius;
                Vector3 origin = rootTransform.position + rootTransform.position.normalized * 10f;
                Vector3 direction = -rootTransform.position.normalized;
                float rDist1 = 0f, rDist2 = 0f;
                bool trigger = false;
                RaycastHit rHit;

                if (Physics.Raycast(new Ray(origin, direction), out rHit, 30f, 8704, QueryTriggerInteraction.Collide))
                {
                    rDist1 = rHit.distance;
                }
                else
                {
                    trigger = true;
                }
                if (Physics.Raycast(new Ray(origin, direction), out rHit, 30f, 16, QueryTriggerInteraction.Collide))
                {
                    rDist2 = rHit.distance;
                }
                else
                {
                    trigger = true;
                }

                if (!trigger && currAltitude > -2.3f + pData.waterHeight)
                {
                    if (rDist1 - rDist2 > 0.7f && currAltitude < -0.6f)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        // collision with vegetation, landing sound effect
        private void UpdateExtraSoundEffects(PlayerAnimationUpdate packet)
        {
            if (localPlanetId < 0)
            {
                return;
            }

            if (localPlanetId > 0)
            {
                PlanetData pData = GameMain.galaxy.PlanetById(localPlanetId);
                PlanetPhysics pPhys = (pData != null) ? pData.physics : null;
                PlanetFactory pFactory = (pData != null) ? pData.factory : null;
                float tmpMaxAltitude = rootTransform.localPosition.magnitude - pData.realRadius;
                if (tmpMaxAltitude > 1000f)
                {
                    tmpMaxAltitude = 1000f;
                }

                if (rootAnimation.RunSlow.enabled || rootAnimation.RunFast.enabled || rootAnimation.Drift.enabled || rootAnimation.DriftF.enabled || rootAnimation.DriftR.enabled || rootAnimation.DriftL.enabled)
                {
                    bool ground = isGrounded();

                    if (DriftDetermineInWater(pData))
                    {
                        if (maxAltitude > 1f && pData.waterItemId > 0)
                        {
                            VFAudio audio = VFAudio.Create("landing-water", base.transform, Vector3.zero, false, 0);
                            audio.volumeMultiplier = Mathf.Clamp01(maxAltitude / 5f + 0.5f);
                            audio.Play();
                            playFootstepEffect(true, 0f, true);
                            playFootstepEffect(false, 0f, true);
                        }
                        maxAltitude = 0f;
                    }
                    if (ground && maxAltitude > 3f)
                    {
                        VFAudio audio = VFAudio.Create("landing", base.transform, Vector3.zero, false, 0);
                        audio.volumeMultiplier = Mathf.Clamp01(maxAltitude / 25f + 0.5f);
                        audio.Play();
                        maxAltitude = 0f;
                    }
                    if (!ground && tmpMaxAltitude > maxAltitude)
                    {
                        maxAltitude = tmpMaxAltitude;
                    }
                }
                else
                {
                    maxAltitude = 15f;
                }

                // NOTE: the pPhys can only be loaded if the player trying to load it has the planet actually loaded (meaning he is on the same planet or near it)
                if (pPhys != null && pFactory != null && packet.horzSpeed > 5f)
                {
                    int number = Physics.OverlapSphereNonAlloc(base.transform.localPosition, 1.8f, collider, 1024, QueryTriggerInteraction.Collide);
                    for (int i = 0; i < number; i++)
                    {
                        int colId = pPhys.nearColliderLogic.FindColliderId(collider[i]);
                        ColliderData cData = pPhys.GetColliderData(colId);
                        if (cData.objType == EObjectType.Vegetable && cData.objId > 0)
                        {
                            VegeData vData = pFactory.vegePool[cData.objId];
                            VegeProto vProto = LDB.veges.Select((int)vData.protoId);
                            if (vProto != null && vProto.CollideAudio > 0 && vegeCollideColdTime <= 0)
                            {
                                VFAudio.Create(vProto.CollideAudio, base.transform, Vector3.zero, true, 0);
                                vegeCollideColdTime = UnityEngine.Random.value * 0.23f + 0.1f;
                            }
                        }
                    }
                }
            }

            if (vegeCollideColdTime > 0)
            {
                vegeCollideColdTime -= Time.deltaTime * 2;
            }
            else
            {
                vegeCollideColdTime = 0;
            }
        }

        public void UpdateState(PlayerAnimationUpdate packet)
        {
            bool anyMovingAnimationActive = rootAnimation.RunSlow.enabled || rootAnimation.RunFast.enabled || rootAnimation.Fly.enabled || rootAnimation.Sail.enabled || rootAnimation.Drift.enabled || rootAnimation.DriftF.enabled || rootAnimation.DriftL.enabled || rootAnimation.DriftR.enabled || !isGrounded();
            bool anyDriftActive = rootAnimation.Drift.enabled || rootAnimation.DriftR.enabled || rootAnimation.DriftL.enabled || rootAnimation.DriftF.enabled;
            bool fireParticleOkay = psys != null && psysr != null && (psys[0] != null && psys[1] != null && psysr[0] != null && psysr[1] != null);

            if (anyMovingAnimationActive)
            {
                UpdateExtraSoundEffects(packet);
                if (fireParticleOkay)
                {
                    if ((!rootAnimation.RunSlow.enabled && !rootAnimation.RunFast.enabled) || anyDriftActive || rootAnimation.Sail.enabled)
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
                                psysr[i].lengthScale = Mathf.Lerp(-3.5f, -8f, Mathf.Max(packet.horzSpeed, packet.vertSpeed) * 0.04f);
                            }
                            else
                            {
                                // when "walking" over water and moving in air without button press or while "walking" over water
                                psysr[i].lengthScale = Mathf.Lerp(-3.5f, -8f, Mathf.Max(packet.horzSpeed, packet.vertSpeed) * 0.03f);
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
                            if (driftAudio != null)
                            {
                                driftAudio.Stop();
                                driftAudio = null;
                            }
                        }
                        if (rootAnimation.Fly.enabled)
                        {
                            // when pressing spacebar but also when landing (Drift is disabled when landing)
                            psysr[i].lengthScale = Mathf.Lerp(-3.5f, -10f, Mathf.Max(packet.horzSpeed, packet.vertSpeed) * 0.03f);
                            if (flyAudio0 == null)
                            {
                                flyAudio0 = VFAudio.Create("fly-atmos", base.transform, Vector3.zero, false);
                                flyAudio0.Play();
                            }
                        }
                        else
                        {
                            if (flyAudio0 != null)
                            {
                                flyAudio0.Stop();
                                flyAudio0 = null;
                            }
                        }
                        if (rootAnimation.Sail.enabled)
                        {
                            psysr[i].lengthScale = Mathf.Lerp(-3.5f, -10f, Mathf.Max(packet.horzSpeed, packet.vertSpeed) * 15f);
                            if (flyAudio1 == null)
                            {
                                flyAudio1 = VFAudio.Create("fly-space", base.transform, Vector3.zero, false);
                                flyAudio1.Play();
                            }
                        }
                        else
                        {
                            if (flyAudio1 != null)
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

            if (torchEffect != null && rootAnimation.Mining0.weight > 0.99f)
            {
                if (!torchEffect.isPlaying && miningAudio == null)
                {
                    torchEffect.Play();
                    miningAudio = VFAudio.Create("mecha-mining", base.transform, Vector3.zero, false);
                    miningAudio?.Play();
                }
            }
            else if (torchEffect != null && rootAnimation.Mining0.weight <= 0.99f)
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
