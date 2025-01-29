#region

using System;
using NebulaModel;
using NebulaModel.Packets.Players;
using UnityEngine;
using Random = UnityEngine.Random;

#endregion

namespace NebulaWorld.MonoBehaviours.Remote;

public class RemoteWarpEffect : MonoBehaviour
{
    private static readonly int s_multiplier = Shader.PropertyToID("_Multiplier");
    private static readonly int s_distortionStrength = Shader.PropertyToID("_DistortionStrength");
    private static readonly int s_warpRotations = Shader.PropertyToID("_WarpRotations");
    public float WarpState;
    private Material astrosMat;
    private float astrosMul;

    private ParticleSystem astrosParticles;

    private ParticleSystemRenderer astrosRenderer;
    private Material distortMat;
    private float distortMul;
    private MeshRenderer distortRenderer;

    private AnimationCurve intensByState;
    private AnimationCurve intensByState_astro;
    private bool isWarping;
    private Material nebulasMat;
    private float nebulasMul;
    private ParticleSystem nebulasParticles;
    private ParticleSystemRenderer nebulasRenderer;
    private PlayerAnimator rootAnimation;
    private Transform rootTransform;

    private Material tunnelMat;

    private float tunnelMul;

    private MeshRenderer tunnelRenderer;
    private Vector3 velocity;

    private VFWarpEffect warpEffect;
    private bool warpEffectActivated;
    private Vector4[] warpRotations;

    public void Awake()
    {
        rootTransform = GetComponent<Transform>();
        rootAnimation = GetComponent<PlayerAnimator>();

        warpEffect = Instantiate(Configs.builtin.warpEffectPrefab, GetComponent<Transform>());
        warpEffect.enabled = false;

        tunnelMat = warpEffect.tunnelMat;
        distortMat = warpEffect.distortMat;
        astrosMat = warpEffect.astrosMat;
        nebulasMat = warpEffect.nebulasMat;

        astrosParticles = warpEffect.astrosParticles;
        nebulasParticles = warpEffect.nebulasParticles;

        tunnelRenderer = warpEffect.tunnelRenderer;
        distortRenderer = warpEffect.distortRenderer;

        astrosRenderer = warpEffect.astrosRenderer;
        nebulasRenderer = warpEffect.nebulasRenderer;

        tunnelMul = warpEffect.tunnelMul;
        distortMul = warpEffect.distortMul;
        astrosMul = warpEffect.astrosMul;
        nebulasMul = warpEffect.nebulasMul;

        intensByState_astro = warpEffect.intensByState_astro;
        intensByState = warpEffect.intensByState;

        warpRotations = new Vector4[24];

        tunnelMat = Instantiate(tunnelRenderer.sharedMaterial);
        distortMat = Instantiate(distortRenderer.sharedMaterial);
        astrosMat = Instantiate(astrosRenderer.sharedMaterial);
        nebulasMat = Instantiate(nebulasRenderer.sharedMaterial);

        tunnelRenderer.sharedMaterial = tunnelMat;
        distortRenderer.sharedMaterial = distortMat;
        astrosRenderer.sharedMaterial = astrosMat;
        nebulasRenderer.sharedMaterial = nebulasMat;

        tunnelMul = tunnelMat.GetFloat(s_multiplier);
        distortMul = distortMat.GetFloat(s_distortionStrength);
        astrosMul = astrosMat.GetFloat(s_multiplier);
        nebulasMul = nebulasMat.GetFloat(s_multiplier);

        for (var i = 0; i < warpRotations.Length; i++)
        {
            warpRotations[i] = new Vector4(0f, 0f, 0f, 1f);
        }

        ToggleEffect(false);
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

        var rotation = rootTransform.rotation;
        var playerRot = new Vector4(rotation.x, rotation.y, rotation.z,
            rotation.w);
        switch (WarpState)
        {
            case > 0.001f when !warpEffectActivated:
                {
                    for (var i = 0; i < warpRotations.Length; i++)
                    {
                        warpRotations[i] = playerRot;
                    }
                    ToggleEffect(true);
                    //skip "warp-begin" VFAudio for now
                    break;
                }
            case 0 when warpEffectActivated:
                ToggleEffect(false);
                //skip "warp-end" VFAudio for now
                break;
        }

        Array.Copy(warpRotations, 0, warpRotations, 1, warpRotations.Length - 1);
        warpRotations[0] = playerRot;

        var emission = astrosParticles.emission;
        var velocityOverTime = astrosParticles.velocityOverLifetime;
        var shape = astrosParticles.shape;

        var lhs = velocity.normalized;

        // to compute the emission we would need to know the players local star, so default to this for now
        emission.rateOverTime = 120f;
        velocityOverTime.speedModifierMultiplier = 20000f;
        velocityOverTime.x = lhs.x;
        velocityOverTime.y = lhs.y;
        velocityOverTime.z = lhs.z;
        shape.position = lhs * 10000.0f;
        var rotation1 = rootTransform.rotation;
        shape.rotation = rotation1.eulerAngles;

        distortRenderer.GetComponent<Transform>().localRotation = rotation1;
        nebulasRenderer.GetComponent<Transform>().localRotation = rotation1;
        var num1 = intensByState.Evaluate(WarpState);
        var num2 = intensByState_astro.Evaluate(WarpState);
        tunnelMat.SetFloat(s_multiplier, tunnelMul * num1);
        tunnelMat.SetVectorArray(s_warpRotations, warpRotations);
        distortMat.SetFloat(s_distortionStrength, distortMul * num1);
        astrosMat.SetFloat(s_multiplier, astrosMul * num2);
        nebulasMat.SetFloat(s_multiplier, nebulasMul * num2);
    }

    public void UpdateVelocity(Vector3 vel)
    {
        velocity = vel;
    }

    private void ToggleEffect(bool toggle)
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

    public void StartWarp()
    {
        if (rootAnimation.sailWeight <= 0.001f || isWarping)
        {
            return;
        }

        isWarping = true;
    }

    public void StopWarp()
    {
        if (rootAnimation.sailWeight <= 0.001f || !isWarping)
        {
            return;
        }

        isWarping = false;
    }
}

public class RemotePlayerEffects : MonoBehaviour
{
    private readonly string[] solidSoundEvents = { "footsteps-0", "footsteps-1", "footsteps-2", "footsteps-3" };
    private readonly string waterSoundEvent = "footsteps-6";
    private Collider[] collider;
    private VFAudio driftAudio;
    private VFAudio flyAudio0, flyAudio1;
    private ParticleSystem[] FootEffect;
    private ParticleSystem[] FootLargeSmoke;
    private ParticleSystem[] FootSmallSmoke;
    private bool isGrounded, inWater;
    private int lastTriggeredFoot;

    private float maxAltitude;

    private VFAudio miningAudio;
    private ParticleSystem[] psys;
    private ParticleSystemRenderer[] psysr;
    private PlayerAnimator rootAnimation;
    private Transform rootModelTransform;
    private RemotePlayerMovement rootMovement;
    private Transform rootTransform;
    private RemoteWarpEffect rootWarp;
    private ParticleSystem torchEffect;
    private float vegeCollideColdTime;

    private ParticleSystem[] WaterEffect;

    private float shieldBurstPrepareTime;
    private GameObject shieldBurstPrepareEffect1;
    private Material shieldBurstPrepareEffectMat1;
    private GameObject shieldBurstPrepareEffect2;
    private Material shieldBurstPrepareEffectMat2;

    public void Awake()
    {
        rootAnimation = GetComponent<PlayerAnimator>();
        rootTransform = GetComponent<Transform>();
        rootModelTransform = rootTransform.Find("Model");
        rootMovement = GetComponent<RemotePlayerMovement>();

        psys = new ParticleSystem[2];
        psysr = new ParticleSystemRenderer[2];
        FootEffect = new ParticleSystem[2];
        WaterEffect = new ParticleSystem[2];
        FootSmallSmoke = new ParticleSystem[2];
        FootLargeSmoke = new ParticleSystem[2];

        var VFX = rootModelTransform.Find("bip/pelvis/spine-1/spine-2/spine-3/backpack/backpack_end/VFX")
            .GetComponent<Transform>();
        psys[0] = VFX.GetChild(0).GetComponent<ParticleSystem>();
        psys[1] = VFX.GetChild(1).GetComponent<ParticleSystem>();
        psysr[0] = VFX.GetChild(0).Find("flames").GetComponent<ParticleSystemRenderer>();
        psysr[1] = VFX.GetChild(1).Find("flames").GetComponent<ParticleSystemRenderer>();
        torchEffect = rootModelTransform
            .Find("bip/pelvis/spine-1/spine-2/spine-3/r-clavicle/r-upper-arm/r-forearm/r-torch/vfx-torch/blast")
            .GetComponent<ParticleSystem>();

        WaterEffect[0] = rootTransform.Find("VFX").Find("vfx-l-footsteps/water").GetComponent<ParticleSystem>();
        WaterEffect[1] = rootTransform.Find("VFX").Find("vfx-r-footsteps/water").GetComponent<ParticleSystem>();
        FootEffect[0] = rootTransform.Find("VFX").Find("vfx-l-footsteps").GetComponent<ParticleSystem>();
        FootEffect[1] = rootTransform.Find("VFX").Find("vfx-r-footsteps").GetComponent<ParticleSystem>();
        FootSmallSmoke[0] = rootTransform.Find("VFX").Find("vfx-l-footsteps/smoke").GetComponent<ParticleSystem>();
        FootSmallSmoke[1] = rootTransform.Find("VFX").Find("vfx-r-footsteps/smoke").GetComponent<ParticleSystem>();
        FootLargeSmoke[0] = rootTransform.Find("VFX").Find("vfx-l-footsteps/smoke-2").GetComponent<ParticleSystem>();
        FootLargeSmoke[1] = rootTransform.Find("VFX").Find("vfx-r-footsteps/smoke-2").GetComponent<ParticleSystem>();

        shieldBurstPrepareEffect1 = rootModelTransform.Find("bip/pelvis/spine-1/spine-2/spine-3/upbody/vfx-shield-burst-prepare1").gameObject;
        shieldBurstPrepareEffect1.SetActive(false);
        MeshRenderer meshRenderer = shieldBurstPrepareEffect1.GetComponent<MeshRenderer>();
        shieldBurstPrepareEffectMat1 = Instantiate(meshRenderer.sharedMaterial);
        meshRenderer.material = shieldBurstPrepareEffectMat1;
        shieldBurstPrepareEffect2 = rootModelTransform.Find("bip/pelvis/spine-1/spine-2/spine-3/upbody/vfx-shield-burst-prepare2").gameObject;
        shieldBurstPrepareEffect2.SetActive(false);
        meshRenderer = shieldBurstPrepareEffect2.GetComponent<MeshRenderer>();
        shieldBurstPrepareEffectMat2 = Instantiate(meshRenderer.sharedMaterial);
        meshRenderer.material = shieldBurstPrepareEffectMat2;

        collider = new Collider[16];

        rootTransform.gameObject.AddComponent<RemoteWarpEffect>();
        rootWarp = rootTransform.gameObject.GetComponent<RemoteWarpEffect>();
#if DEBUG
        Assert.True(psys != null && psysr != null && psys[0] != null && psys[1] != null && psysr[0] != null &&
                    psysr[1] != null && torchEffect != null);
        Assert.True(WaterEffect[0] != null && WaterEffect[1] != null && FootEffect[0] != null && FootEffect[1] != null);
        Assert.True(FootSmallSmoke[0] != null && FootSmallSmoke[1] != null && FootLargeSmoke[0] != null &&
                    FootLargeSmoke[1] != null);
#endif
    }

    public void OnDestroy()
    {
        StopAllFlyAudio();
        StopAndNullAudio(ref miningAudio);
    }

    private static void StopAndNullAudio(ref VFAudio vfAudio)
    {
        if (vfAudio == null)
        {
            return;
        }
        vfAudio.Stop();
        vfAudio = null;
    }

    private void StopAllFlyAudio()
    {
        StopAndNullAudio(ref driftAudio);
        StopAndNullAudio(ref flyAudio0);
        StopAndNullAudio(ref flyAudio1);
    }

    private void PlayFootsteps()
    {
        var moveWeight = Mathf.Max(1f, Mathf.Pow(rootAnimation.run_slow.weight + rootAnimation.run_fast.weight, 2f));
        var trigger = (rootAnimation.run_slow.enabled || rootAnimation.run_fast.enabled) && moveWeight > 0.15f;

        if (!trigger)
        {
            return;
        }
        var normalizedTime = Mathf.FloorToInt(rootAnimation.run_fast.normalizedTime * 2f - 0.02f);
        var normalizedTimeDiff = rootAnimation.run_fast.normalizedTime * 2f - 0.02f - normalizedTime;
        var timeIsEven = normalizedTime % 2 == 0;

        if (lastTriggeredFoot == normalizedTime)
        {
            return;
        }
        lastTriggeredFoot = normalizedTime;

        var dustPos = (!timeIsEven ? FootEffect[1] : FootEffect[0]).transform.position;
        var dustPosNormalized = dustPos.normalized;
        dustPos += dustPos.normalized;

        var ray = new Ray(dustPos, -dustPosNormalized);
        float rDist1 = 100f, rDist2 = 100f;
        var biomo = -1f;

        if (Physics.Raycast(ray, out var rHit1, 2f, 512, QueryTriggerInteraction.Collide))
        {
            rDist1 = rHit1.distance;
            biomo = rHit1.textureCoord2.x;
        }
        if (Physics.Raycast(ray, out var rHit2, 2f, 16, QueryTriggerInteraction.Collide))
        {
            rDist2 = rHit2.distance + 0.1f;
        }

        if (normalizedTimeDiff < 0.3f && rDist1 < 1.8f && (0 < lastTriggeredFoot || normalizedTime < 2))
        {
            PlayFootstepSound(biomo, rDist2 < rDist1);
        }
        if (normalizedTimeDiff < 0.5f && moveWeight > 0.5f && (rDist1 < 3f || rDist2 < 3f))
        {
            PlayFootstepEffect(timeIsEven, biomo, rDist2 < rDist1);
        }
    }

    private void PlayFootstepSound(float biomo, bool water)
    {
        if (rootMovement.localPlanetId < 0)
        {
            // wait for update in RemotePlayerMovement
            return;
        }

        var ambientDesc = GameMain.galaxy.PlanetById(rootMovement.localPlanetId).ambientDesc;
        string audioName;
        if (!water)
        {
            if (biomo <= 0.8)
            {
                audioName = solidSoundEvents[ambientDesc.biomoSound0];
            }
            else if (biomo <= 1.8)
            {
                audioName = solidSoundEvents[ambientDesc.biomoSound1];
            }
            else
            {
                audioName = solidSoundEvents[ambientDesc.biomoSound2];
            }
            if (CheckPlayerInReform())
            {
                audioName = solidSoundEvents[3];
            }
        }
        else
        {
            audioName = waterSoundEvent;
        }
        var audio = VFAudio.Create(audioName, transform, Vector3.zero, false, 8);
        if (audio != null)
        {
            // skip setting audio.volumeMultiplier = vol, it makes footsteps too loud
            audio.Play();
        }
    }

    private void PlayFootstepEffect(bool lr, float biomo, bool water)
    {
        if (CheckPlayerInReform() || rootMovement.localPlanetId < 0)
        {
            return;
        }

        var waterParticle = !lr ? WaterEffect[0] : WaterEffect[1];
        var smokeParticle = !lr ? FootSmallSmoke : FootLargeSmoke;
        var footEffect = !lr ? FootEffect[0] : FootEffect[1];
        var ambientDesc = GameMain.galaxy.PlanetById(rootMovement.localPlanetId).ambientDesc;
        var color = Color.clear;
        var dustStrength = 1f;

        if (!water)
        {
            switch (biomo)
            {
                case <= 0f:
                    color = ambientDesc.biomoDustColor0;
                    dustStrength = ambientDesc.biomoDustStrength0;
                    break;
                case <= 1f:
                    color = Color.Lerp(ambientDesc.biomoDustColor0, ambientDesc.biomoColor1, biomo);
                    dustStrength = Mathf.Lerp(ambientDesc.biomoDustStrength0, ambientDesc.biomoDustStrength1, biomo);
                    break;
                case <= 2f:
                    color = Color.Lerp(ambientDesc.biomoDustColor1, ambientDesc.biomoColor2, biomo - 1f);
                    dustStrength = Mathf.Lerp(ambientDesc.biomoDustStrength1, ambientDesc.biomoDustStrength2, biomo - 1f);
                    break;
                default:
                    color = ambientDesc.biomoDustColor2;
                    dustStrength = ambientDesc.biomoDustStrength2;
                    break;
            }
        }
        if (!(biomo >= 0f) && !water)
        {
            return;
        }
        var main = waterParticle.main;
        main.startColor = !water ? Color.clear : Color.white;
        foreach (var p in smokeParticle)
        {
            main = p.main;
            main.startColor = color;
        }
        foreach (var p in FootSmallSmoke)
        {
            main = p.main;
            main.startLifetime = 0.8f + 0.2f * dustStrength;
            main.startSize = 1.1f + 0.2f * dustStrength;
        }
        foreach (var p in FootLargeSmoke)
        {
            main = p.main;
            main.startLifetime = 1.2f * dustStrength;
            main.startSize = 2.2f + 2.4f * dustStrength;
        }
        footEffect.Play();
    }

    private bool CheckPlayerInReform()
    {
        var result = false;
        try
        {
            var localPlanet = GameMain.galaxy.PlanetById(rootMovement.localPlanetId);
            var platformSystem = localPlanet?.factory?.platformSystem;
            if (platformSystem?.reformData != null)
            {
                var reformIndexForPosition = platformSystem.GetReformIndexForPosition(rootTransform.position);
                if (reformIndexForPosition > -1)
                {
                    var reformType = platformSystem.GetReformType(reformIndexForPosition);
                    result = platformSystem.IsTerrainMapping(reformType);
                }
                else
                {
                    result = true;
                }
            }
        }
        catch
        {
            // TODO: Why empty catch?
        }
        return result;
    }

    // collision with vegetation, landing sound effect
    private void UpdateExtraSoundEffects(ref RemotePlayerAnimation.Snapshot packet)
    {
        switch (rootMovement.localPlanetId)
        {
            case < 0:
                return;
            case > 0:
                {
                    var pData = GameMain.galaxy.PlanetById(rootMovement.localPlanetId);
                    var pPhys = pData?.physics;
                    var pFactory = pData?.factory;
                    if (pData != null)
                    {
                        var tmpMaxAltitude = rootTransform.localPosition.magnitude - pData.realRadius;
                        if (tmpMaxAltitude > 1000f)
                        {
                            tmpMaxAltitude = 1000f;
                        }

                        if (rootAnimation.movementState < EMovementState.Fly)
                        {
                            if (inWater)
                            {
                                if (maxAltitude > 1f && pData.waterItemId > 0)
                                {
                                    var audio = VFAudio.Create("landing-water", transform, Vector3.zero);
                                    audio.volumeMultiplier = Mathf.Clamp01(maxAltitude / 5f + 0.5f);
                                    audio.Play();
                                    PlayFootstepEffect(true, 0f, true);
                                    PlayFootstepEffect(false, 0f, true);
                                }
                                maxAltitude = 0f;
                            }
                            switch (isGrounded)
                            {
                                case true:
                                    {
                                        if (maxAltitude > 3f)
                                        {
                                            var audio = VFAudio.Create("landing", transform, Vector3.zero);
                                            audio.volumeMultiplier = Mathf.Clamp01(maxAltitude / 25f + 0.5f);
                                            audio.Play();
                                        }
                                        maxAltitude = 0f;
                                        break;
                                    }
                                case false when tmpMaxAltitude > maxAltitude:
                                    maxAltitude = tmpMaxAltitude;
                                    break;
                            }
                        }
                        else
                        {
                            maxAltitude = 15f;
                        }
                    }

                    // NOTE: the pPhys can only be loaded if the player trying to load it has the planet actually loaded (meaning he is on the same planet or near it)
                    if (pPhys != null && pFactory != null && packet.HorzSpeed > 5f)
                    {
                        var number = Physics.OverlapSphereNonAlloc(transform.localPosition, 1.8f, collider, 1024,
                            QueryTriggerInteraction.Collide);
                        for (var i = 0; i < number; i++)
                        {
                            var colId = pPhys.nearColliderLogic.FindColliderId(collider[i]);
                            var cData = pPhys.GetColliderData(colId);
                            if (cData.objType != EObjectType.Vegetable || cData.objId <= 0)
                            {
                                continue;
                            }
                            var vData = pFactory.vegePool[cData.objId];
                            var vProto = LDB.veges.Select(vData.protoId);
                            if (vProto is not { CollideAudio: > 0 } || !(vegeCollideColdTime <= 0))
                            {
                                continue;
                            }
                            VFAudio.Create(vProto.CollideAudio, transform, Vector3.zero, true);
                            vegeCollideColdTime = Random.value * 0.23f + 0.1f;
                        }
                    }
                    break;
                }
        }

        vegeCollideColdTime = vegeCollideColdTime > 0 ? vegeCollideColdTime - Time.deltaTime * 2 : 0;
    }

    public void UpdateState(ref RemotePlayerAnimation.Snapshot packet)
    {
        var allowSounds = GameMain.localPlanet?.id == rootMovement.localPlanetId && Config.Options.EnableOtherPlayerSounds;
        var runActive = rootAnimation.runWeight > 0.001f;
        var driftActive = rootAnimation.driftWeight > 0.001f;
        var flyActive = rootAnimation.flyWeight > 0.001f;
        var sailActive = rootAnimation.sailWeight > 0.001f;
        isGrounded = (packet.Flags & PlayerMovement.EFlags.isGrounded) == PlayerMovement.EFlags.isGrounded;
        inWater = (packet.Flags & PlayerMovement.EFlags.inWater) == PlayerMovement.EFlags.inWater;
        var warping = (packet.Flags & PlayerMovement.EFlags.warping) == PlayerMovement.EFlags.warping;
        var chargeShieldBurst = (packet.Flags & PlayerMovement.EFlags.chargeShieldBurst) == PlayerMovement.EFlags.chargeShieldBurst;

        if ((runActive || !isGrounded || maxAltitude > 0) && allowSounds)
        {
            UpdateExtraSoundEffects(ref packet);
        }
        if (runActive && isGrounded && allowSounds)
        {
            PlayFootsteps();
        }
        if ((driftActive || flyActive || sailActive || !isGrounded) && allowSounds)
        {
            foreach (var t in psys)
            {
                if (!t.isPlaying)
                {
                    t.Play();
                }
            }
            foreach (var t in psysr)
            {
                if (runActive)
                {
                    // when flying over the planet
                    t.lengthScale = rootAnimation.run_fast.weight != 0
                        ? Mathf.Lerp(-3.5f, -8f, Mathf.Max(packet.HorzSpeed, packet.VertSpeed) * 0.04f)
                        :
                        // when "walking" over water and moving in air without button press or while "walking" over water
                        Mathf.Lerp(-3.5f, -8f, Mathf.Max(packet.HorzSpeed, packet.VertSpeed) * 0.03f);
                }
                if (driftActive)
                {
                    // when in air without pressing spacebar
                    t.lengthScale = -3.5f;
                    driftAudio = driftAudio != null ? driftAudio : VFAudio.Create("drift", transform, Vector3.zero, true);
                }
                else
                {
                    StopAndNullAudio(ref driftAudio);
                }
                if (flyActive)
                {
                    // when pressing spacebar but also when landing (Drift is disabled when landing)
                    t.lengthScale = Mathf.Lerp(-3.5f, -10f, Mathf.Max(packet.HorzSpeed, packet.VertSpeed) * 0.03f);
                    flyAudio0 = flyAudio0 != null ? flyAudio0 : VFAudio.Create("fly-atmos", transform, Vector3.zero, true);
                }
                else
                {
                    StopAndNullAudio(ref flyAudio0);
                }
                if (sailActive)
                {
                    t.lengthScale = Mathf.Lerp(-3.5f, -10f, Mathf.Max(packet.HorzSpeed, packet.VertSpeed) * 15f);
                    flyAudio1 = flyAudio1 != null ? flyAudio1 : VFAudio.Create("fly-space", transform, Vector3.zero, true);
                }
                else
                {
                    StopAndNullAudio(ref flyAudio1);
                }
            }
        }
        else
        {
            foreach (var t in psys)
            {
                if (t.isPlaying)
                {
                    t.Stop();
                }
            }
            StopAllFlyAudio();
        }

        if (torchEffect != null && rootAnimation.miningWeight > 0.99f && allowSounds)
        {
            if (!torchEffect.isPlaying)
            {
                torchEffect.Play();
                miningAudio = VFAudio.Create("mecha-mining", transform, Vector3.zero, true);
            }
        }
        else if (torchEffect != null && rootAnimation.miningWeight <= 0.99f)
        {
            if (torchEffect.isPlaying)
            {
                torchEffect.Stop();
                StopAndNullAudio(ref miningAudio);
            }
        }

        if (warping)
        {
            rootWarp.StartWarp();
        }
        else
        {
            rootWarp.StopWarp();
        }

        UpdateShieldBrustEffect(chargeShieldBurst);
    }

    private void UpdateShieldBrustEffect(bool chargeShieldBurst)
    {
        if (chargeShieldBurst)
        {
            shieldBurstPrepareTime += 2f * Time.deltaTime;
        }
        else
        {
            shieldBurstPrepareTime -= 2f * Time.deltaTime;
        }
        if (shieldBurstPrepareTime > 1f)
        {
            shieldBurstPrepareTime = 1f;
        }
        if (shieldBurstPrepareTime < 0f)
        {
            shieldBurstPrepareTime = 0f;
        }
        shieldBurstPrepareEffect1.SetActive(shieldBurstPrepareTime > 0f);
        shieldBurstPrepareEffect2.SetActive(shieldBurstPrepareTime > 0f);
        if (shieldBurstPrepareTime > 0f)
        {
            var num6 = Mathf.Repeat(Time.time / 2f, 1f);
            var num7 = Mathf.Repeat(Time.time / 2f + 0.5f, 1f);
            var num8 = ((num6 < 0.5f) ? (num6 * 2f) : (-num6 * 2f + 2f));
            var num9 = 1f - num8;
            var num10 = 0.5f + 0.5f * 3f; // assume mecha.energyShieldBurstProgress is half full
            shieldBurstPrepareEffectMat1.SetFloat("_Ani1", num6);
            shieldBurstPrepareEffectMat2.SetFloat("_Ani1", num7);
            shieldBurstPrepareEffectMat1.SetFloat("_AlphaMultiplier", num8 * shieldBurstPrepareTime);
            shieldBurstPrepareEffectMat2.SetFloat("_AlphaMultiplier", num9 * shieldBurstPrepareTime);
            shieldBurstPrepareEffectMat1.SetFloat("_Radius", num10);
            shieldBurstPrepareEffectMat2.SetFloat("_Radius", num10);
            shieldBurstPrepareEffectMat1.SetVector("_FlareSize", new Vector4(30f, 30f, 40f, 0f));
            shieldBurstPrepareEffectMat2.SetVector("_FlareSize", new Vector4(50f, 50f, 40f, 0f));
        }
    }
}
