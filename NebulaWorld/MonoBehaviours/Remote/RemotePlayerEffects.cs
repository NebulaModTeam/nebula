using UnityEngine;

namespace NebulaWorld.MonoBehaviours.Remote
{
    public class RemotePlayerEffects : MonoBehaviour
    {
        private RemotePlayerAnimation rootAnimation;
        private Transform rootTransform;
        public int PlanetId = (int)GameMain.localPlanet?.id; // this needs to be updated!!

        private ParticleSystem[] psys;
        private ParticleSystemRenderer[] psysr;

        public void Awake()
        {
            rootAnimation = GetComponent<RemotePlayerAnimation>();
            rootTransform = GetComponent<Transform>();
        }

        public void UpdateState()
        {
            /*
            Quaternion relRot = new Quaternion();
            VectorLF3 relPos = VectorLF3.zero, lVelocity, uVelocity, rhs = VectorLF3.zero;
            float vertSpeed, horzSpeed;
            Vector3 horzVel;

            PlanetData pData = GameMain.galaxy?.PlanetById(PlanetId);

            relRot.x = relRot.y = relRot.z = 0;
            relRot.w = 1f;
            if (pData == null)
            {
                relPos.x = rootTransform.position.x;
                relPos.y = rootTransform.position.y;
                relPos.z = rootTransform.position.z;
            }
            else
            {
                relRot.x = pData.runtimeRotation.x;
                relRot.y = pData.runtimeRotation.y;
                relRot.z = pData.runtimeRotation.z;
                relRot.w = pData.runtimeRotation.w;
                relPos.x = pData.uPosition.x;
                relPos.y = pData.uPosition.y;
                relPos.z = pData.uPosition.z;
                rhs = pData.GetUniversalVelocityAtLocalPoint(GameMain.gameTime, rootTransform.position);
            }

            if (rootAnimation.Sail.enabled)
            {
                if (pData != null)
                {

                    Vector3 vec = Maths.QInvRotateLF(relRot, (VectorLF3)rootTransform.position - relPos);
                    // cant take uVelocity here so taking from rigidbody again
                    lVelocity = Quaternion.Inverse(relRot) * ((VectorLF3)rootTransform.GetComponent<Rigidbody>().velocity - pData.GetUniversalVelocityAtLocalPoint(GameMain.gameTime, vec));

                }
                else
                {
                    // this is 'lVelocity = uVelocity' but as this is not possible here i take the rigids velocity
                    //lVelocity = Maths.QRotateLF(relRot, rootTransform.GetComponent<Rigidbody>().velocity);
                    lVelocity = rootTransform.GetComponent<Rigidbody>().velocity;
                }
                uVelocity = lVelocity;
            }
            else
            {
                // this is potentially not needed as its nulled below
                lVelocity = rootTransform.GetComponent<Rigidbody>().velocity;
                uVelocity = Maths.QRotateLF(relRot, lVelocity) + rhs;
            }

            if (pData != null)
            {
                if (rootAnimation.Sail.enabled)
                {
                    uVelocity = Maths.QInvRotateLF(relRot, uVelocity);
                }
                else
                {
                    uVelocity = Vector3.zero;
                }
            }

            vertSpeed = Vector3.Dot(base.transform.up, uVelocity);
            horzVel = (Vector3)uVelocity - vertSpeed * base.transform.up;
            horzSpeed = horzVel.magnitude;
            */
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
                                psysr[i].lengthScale = Mathf.Lerp(-3.5f, -10f, rootAnimation.RunFast.weight * 0.4f);
                                Debug.Log("Flying over planet: " + rootAnimation.RunFast.weight);
                            }
                            else
                            {
                                // when "walking" over water and moving in air without button press or while "walking" over water
                                psysr[i].lengthScale = Mathf.Lerp(-3.5f, -10f, rootAnimation.RunSlow.weight * 1f);
                                Debug.Log("Moving in air: " + rootAnimation.RunSlow.weight);
                            }
                        }
                        if (rootAnimation.Drift.enabled)
                        {
                            // when in air without pressing spacebar
                            psysr[i].lengthScale = -3.5f;
                            Debug.Log("Standing i air: " + rootAnimation.Drift.weight);
                        }
                        if (rootAnimation.Fly.enabled)
                        {
                            // when pressing spacebar but also when landing (Drift is disabled when landing)
                            psysr[i].lengthScale = Mathf.Lerp(-3.5f, -10f, rootAnimation.Fly.weight * 0.5f);
                            Debug.Log("Starting: " + rootAnimation.Fly.weight);
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
            // other effects come here
        }

        public void setOrigParticlesValues(ParticleSystem[] psys, ParticleSystemRenderer[] psysr)
        {
            this.psys = psys;
            this.psysr = psysr;
        }
    }
}
