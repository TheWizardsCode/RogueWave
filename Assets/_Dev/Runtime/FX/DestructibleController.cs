using UnityEngine;
using NeoFPS;
using UnityEngine.Serialization;

namespace RogueWave
{
    [RequireComponent(typeof(BasicHealthManager))]
    public class DestructibleController : MonoBehaviour
    {
        [SerializeField, Tooltip("The particle effects to replace the object with when it is destroyed. These effects will have their colour and emitter shape adjusted to match the object being destroyed.")]
        GameObject[] m_ScaledDestructionParticles;
        [SerializeField, Tooltip("The VFX (e.g. smoke and fire) particle effects to spawn when the object is destroyed. These effects will not have their colour adjusted to match the object being destroyed, but they will be adjusted to match shape.")]
        GameObject[] m_ScaledFXParticles;
        [SerializeField, Tooltip("Density of particles to spawn. The higher this value the more particles will spawn."), Range(0.5f, 100)]
        float m_ParticalDensity = 25;
        [SerializeField, Tooltip("Explosive force. The higher this value the more the particles will move away from the center of the destructable object."), Range(0f, 10f)]
        float m_ExplosiveForce = 3;
        [SerializeField, Tooltip("The particle effect to replace the object with when it is destroyed. These effects will be adjusted to match the obnject being destroyed.")]
        GameObject[] m_UnscaledParticles;

        [Header("Fall Damage")]
        [SerializeField, Tooltip("A multiplier for fall damage taken by this object. 0 means no damage will be taken. This allows you can make the object more or less susceptible to breakage when falling.")]
        float m_ImpactDamageMultiplier = 1;

        private BasicHealthManager m_HealthManager;

        private void OnEnable()
        {
            m_HealthManager = GetComponent<BasicHealthManager>();
            m_HealthManager.onIsAliveChanged += OnIsAliveChanged;
        }

        private void OnDisable()
        {
            m_HealthManager.onIsAliveChanged -= OnIsAliveChanged;
        }

        protected void OnIsAliveChanged(bool isAlive)
        {
            if (!isAlive)
            {
                if (m_ScaledFXParticles != null ||  m_ScaledDestructionParticles != null)
                {
                    //OPTIMIZATION: cache on start
                    MeshRenderer[] meshRenderers = GetComponentsInChildren<MeshRenderer>();
                    foreach (MeshRenderer meshRenderer in meshRenderers)
                    {
                        meshRenderer.enabled = false;
                    }

                    //OPTIMIZATION: cache on start
                    Collider[] colliders = GetComponentsInChildren<Collider>();
                    for (int i = 0; i < colliders.Length; i++)
                    {
                        colliders[i].enabled = false;
                    }

                    int boundsMultiplier = 2 * (int)(meshRenderers[0].bounds.extents.x + meshRenderers[0].bounds.extents.y + meshRenderers[0].bounds.extents.z);
                    //Debug.Log($"Bounds multiplier {boundsMultiplier}");

                    if (m_ScaledDestructionParticles != null)
                    {
                        SpawnScaledParticle(m_ScaledDestructionParticles, meshRenderers[0], boundsMultiplier, true);
                    }

                    if (m_ScaledFXParticles != null)
                    {
                        SpawnScaledParticle(m_ScaledFXParticles, meshRenderers[0], boundsMultiplier, false);
                    }
                }

                if (m_UnscaledParticles != null)
                {
                    //OPTIMIZATION: Use a pool system for the destruction particles
                    for (int d = 0; d < m_UnscaledParticles.Length; d++)
                    {
                        GameObject go = Instantiate(m_UnscaledParticles[d]);
                        go.transform.position = transform.position;
                        go.transform.localPosition = Vector3.zero;

                    }
                }

                Destroy(gameObject, 15);
            }
        }

        private void SpawnScaledParticle(GameObject[] particleSystems,  MeshRenderer meshRenderer, int boundsMultiplier, bool inheritColour)
        {
            for (int d = 0; d < particleSystems.Length; d++)
            {
                //OPTIMIZATION: Use Neo FPS Pool system
                GameObject go = Instantiate(particleSystems[d]);
                go.transform.position = transform.position;

                ParticleSystem[] ps = go.GetComponentsInChildren<ParticleSystem>();

                ParticleSystem.ShapeModule shape;
                ParticleSystem.EmissionModule emission;
                ParticleSystem.Burst burst;
                ParticleSystem.ForceOverLifetimeModule force;
                float longestDuration = 0;
                for (int i = 0; i < ps.Length; i++)
                {
                    if (ps[i].main.duration > longestDuration)
                    {
                        longestDuration = ps[i].main.duration;
                    }

                    shape = ps[i].shape;
                    if (shape.shapeType == ParticleSystemShapeType.MeshRenderer)
                    {
                        shape.meshRenderer = meshRenderer;
                        if (inheritColour)
                        {
                            ps[i].GetComponent<ParticleSystemRenderer>().material = meshRenderer.material;
                        }
                    }
                    else if (shape.shapeType == ParticleSystemShapeType.Mesh)
                    {
                        shape.mesh = meshRenderer.GetComponent<MeshFilter>().mesh;
                        if (inheritColour)
                        {
                            ps[i].GetComponent<ParticleSystemRenderer>().material = meshRenderer.material;
                        }
                    }

                    emission = ps[i].emission;
                    burst = emission.GetBurst(0);
                    burst.count = m_ParticalDensity * boundsMultiplier;
                    emission.SetBurst(0, burst);

                    force = ps[i].forceOverLifetime;
                    if (m_ExplosiveForce > 0)
                    {
                        force.enabled = true;
                        ParticleSystem.MinMaxCurve curve = new ParticleSystem.MinMaxCurve(-m_ExplosiveForce, m_ExplosiveForce);
                        force.x = curve;
                        force.y = curve;
                        force.z = curve;

                    }
                    else
                    {
                        force.enabled = false;
                    }
                }

                Destroy(go, longestDuration);
            }
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (m_ImpactDamageMultiplier == 0) return;

            ContactPoint contact = collision.contacts[0];
            Vector3 normal = contact.normal;
            Vector3 relativeVelocity = collision.relativeVelocity;
            float damage = Vector3.Dot(normal, relativeVelocity) * m_ImpactDamageMultiplier;
            m_HealthManager.AddDamage(damage);
        }
    }
}
