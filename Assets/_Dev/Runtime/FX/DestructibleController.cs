using UnityEngine;
using NeoFPS;
using UnityEngine.Serialization;

namespace RogueWave
{
    [RequireComponent(typeof(BasicHealthManager))]
    public class DestructibleController : MonoBehaviour
    {
        [SerializeField, Tooltip("The particle effects to replace the object with when it is destroyed. These effects will have their colour and emitter shape adjusted to match the object being destroyed.")]
        PooledObject[] m_PooledScaledDestructionParticles;
        [SerializeField, Tooltip("The VFX (e.g. smoke and fire) particle effects to spawn when the object is destroyed. These effects will not have their colour adjusted to match the object being destroyed, but they will be adjusted to match shape.")]
        PooledObject[] m_PooledScaledFXParticles;
        [SerializeField, Tooltip("Density of particles to spawn. The higher this value the more particles will spawn."), Range(0.5f, 100)]
        float m_ParticalDensity = 25;
        [SerializeField, Tooltip("Explosive force. The higher this value the more the particles will move away from the center of the destructable object."), Range(0f, 10f)]
        float m_ExplosiveForce = 3;
        [SerializeField, Tooltip("The particle effect to replace the object with when it is destroyed. These effects will be adjusted to match the obnject being destroyed.")]
        PooledObject[] m_PooledUnscaledParticles;

        [Header("Fall Damage")]
        [SerializeField, Tooltip("A multiplier for fall damage taken by this object. 0 means no damage will be taken. This allows you can make the object more or less susceptible to breakage when falling.")]
        float m_ImpactDamageMultiplier = 1;

        [Header("Rewards")]
        [SerializeField, Tooltip("The chance of dropping a reward when killed.")]
        internal float resourcesDropChance = 0.5f;
        [SerializeField, Tooltip("The resources this enemy drops when killed.")]
        internal ResourcesPickup resourcesPrefab;

        private BasicHealthManager m_HealthManager;
        private Renderer renderer;

        private void Awake()
        {
            renderer = GetComponentInChildren<Renderer>();
        }

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
                if (m_PooledScaledFXParticles != null ||  m_PooledScaledDestructionParticles != null)
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

                    if (m_PooledScaledDestructionParticles != null)
                    {
                        SpawnScaledParticle<PooledObject>(m_PooledScaledDestructionParticles, meshRenderers[0], boundsMultiplier, true);
                    }

                    if (m_PooledScaledFXParticles != null)
                    {
                        SpawnScaledParticle<PooledObject>(m_PooledScaledFXParticles, meshRenderers[0], boundsMultiplier, false);
                    }
                }

                if (m_PooledUnscaledParticles != null)
                {
                    for (int d = 0; d < m_PooledUnscaledParticles.Length; d++)
                    {
                       PooledObject pooledObject = PoolManager.GetPooledObject<PooledObject>(m_PooledUnscaledParticles[d], transform.position, Quaternion.identity);
                       pooledObject.transform.localPosition = Vector3.zero;

                    }
                }

                // Drop resources
                if (Random.value <= resourcesDropChance && resourcesPrefab != null)
                {
                    Vector3 pos = transform.position;
                    pos.y = 0;
                    ResourcesPickup resources = Instantiate(resourcesPrefab, pos, Quaternion.identity);
                    if (renderer != null)
                    {
                        var resourcesRenderer = resources.GetComponentInChildren<Renderer>();
                        if (resourcesRenderer != null)
                        {
                            resourcesRenderer.material = renderer.material;
                        }
                    }
                }

                Destroy(gameObject, 15);
            }
        }

        private void SpawnScaledParticle<T>(PooledObject[] particleSystems,  MeshRenderer meshRenderer, int boundsMultiplier, bool inheritColour) where T : PooledObject
        {
            for (int d = 0; d < particleSystems.Length; d++)
            {
                T pooledObject = PoolManager.GetPooledObject<T>(particleSystems[d], transform.position, Quaternion.identity);

                ParticleSystem[] ps = pooledObject.GetComponentsInChildren<ParticleSystem>();

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

                pooledObject.ReturnToPool(longestDuration);
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
