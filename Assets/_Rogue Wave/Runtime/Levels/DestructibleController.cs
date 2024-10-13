using UnityEngine;
using NeoFPS;
using UnityEngine.Serialization;
using RogueWave.GameStats;
using NaughtyAttributes;
using System.Collections.Generic;

namespace RogueWave
{
    [RequireComponent(typeof(BasicHealthManager))]
    public class DestructibleController : MonoBehaviour
    {
        [SerializeField, Tooltip("The particle effects to replace the object with when it is destroyed. These effects will have their colour and emitter shape adjusted to match the object being destroyed.")]
        internal PooledObject[] m_PooledScaledDestructionParticles;
        [SerializeField, Tooltip("The VFX (e.g. smoke and fire) particle effects to spawn when the object is destroyed. These effects will not have their colour adjusted to match the object being destroyed, but they will be adjusted to match shape.")]
        internal PooledObject[] m_PooledScaledFXParticles;
        [SerializeField, Tooltip("Density of particles to spawn. The higher this value the more particles will spawn."), Range(0.5f, 100)]
        float m_ParticalDensity = 25;
        [SerializeField, Tooltip("Explosive force. The higher this value the more the particles will move away from the center of the destructable object."), Range(0f, 10f)]
        float m_ExplosiveForce = 3;
        [SerializeField, Tooltip("The particle effect to replace the object with when it is destroyed. These effects will be adjusted to match the obnject being destroyed.")]
        PooledObject[] m_PooledUnscaledParticles;
        [SerializeField, Tooltip("Sound to play when the object is destroyed. One will be chosen at random from the available sounds here.")]
        AudioClip[] m_DestroyedSound;

        [Header("Fall Damage")]
        [SerializeField, Tooltip("A multiplier for fall damage taken by this object. 0 means no damage will be taken. This allows you can make the object more or less susceptible to breakage when falling.")]
        float m_ImpactDamageMultiplier = 1;

        [Header("Rewards")]
        [SerializeField, Tooltip("The chance of dropping a reward when killed.")]
        internal float resourcesDropChance = 0.5f;
        [SerializeField, Tooltip("The collection of pickup recipes from which the discovered item will be chosen. If none are valid for this player then the resources prefab will be used.")]
        internal List<AbstractRecipe> possibleDrops = null;
        [SerializeField, Tooltip("Should the material on the resources dropped match the material on the object being destroyed?")]
        internal bool inheritMaterial = true;
        [SerializeField, Tooltip("Should the resources be pulled to the player using the magnet?")]
        internal bool magnetizeResources = true;

        // Game Stats
        [SerializeField, Tooltip("The GameStat to increment when this destructible is destroyed."), Foldout("Game Stats")]
        internal IntGameStat destructibleDestroyed;

        private BasicHealthManager m_HealthManager;
        private Renderer modelRenderer;
        private MeshRenderer[] meshRenderers;
        private Collider[] colliders;
        private ParticleSystem[] particles;
        IPickup pickupPrototype;

        private void Awake()
        {
            modelRenderer = GetComponentInChildren<Renderer>();
        }

        private void Start()
        {
            meshRenderers = GetComponentsInChildren<MeshRenderer>();
            colliders = GetComponentsInChildren<Collider>();
            particles = GetComponentsInChildren<ParticleSystem>();
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

        protected virtual void OnIsAliveChanged(bool isAlive)
        {
            float ttl = 0;
            foreach (ParticleSystem ps in particles)
            {
                if (ps.main.duration > ttl)
                {
                    ttl = ps.main.duration;
                }
            }

            if (!isAlive)
            {
                if (destructibleDestroyed != null)
                {
                    destructibleDestroyed.Add(1);
                }

                if (m_PooledScaledFXParticles != null ||  m_PooledScaledDestructionParticles != null)
                {
                    int boundsMultiplier = 2 * (int)(meshRenderers[0].bounds.extents.x + meshRenderers[0].bounds.extents.y + meshRenderers[0].bounds.extents.z);
                    //Debug.Log($"Bounds multiplier {boundsMultiplier}");

                    ttl = SpawnDestructionParticles(meshRenderers[0], boundsMultiplier);

                    // Disable the mesh renderer and colliders
                    foreach (MeshRenderer meshRenderer in meshRenderers)
                    {
                        meshRenderer.enabled = false;
                    }
                    for (int i = 0; i < colliders.Length; i++)
                    {
                        colliders[i].enabled = false;
                    }

                    // Stop any particle systems
                    foreach (ParticleSystem ps in particles)
                    {
                        ParticleSystem.EmissionModule emissionModule = ps.emission;
                        emissionModule.enabled = false;
                    }

                    if (m_DestroyedSound != null)
                    {
                        if (m_DestroyedSound.Length > 0)
                        {
                            int index = Random.Range(0, m_DestroyedSound.Length);
                            NeoFpsAudioManager.PlayEffectAudioAtPosition(m_DestroyedSound[index], transform.position);
                        }
                    }
                }

                if (m_PooledUnscaledParticles != null)
                {
                    for (int d = 0; d < m_PooledUnscaledParticles.Length; d++)
                    {
                        PooledObject pooledObject = PoolManager.GetPooledObject<PooledObject>(m_PooledUnscaledParticles[d]);
                        pooledObject.transform.position += transform.position;
                    }
                }

                SetRewards();
                DropRewards();

                Destroy(gameObject, ttl + 0.5f);
            }
        }

        void SetRewards()
        {
            // Calculate weights for the possible drops, skipping any that the player already has the maximum number of.
            IItemRecipe recipe = null;
            WeightedRandom<IItemRecipe> weightedRandom = new WeightedRandom<IItemRecipe>();
            for (int i = possibleDrops.Count - 1; i >= 0; i--)
            {
                recipe = possibleDrops[i] as IItemRecipe;
                if (recipe == null)
                {
                    Debug.LogError($"{possibleDrops[i]} in a DiscoverableItem is not a valid recipe for a pickup item. Removing it.");
                    possibleDrops.RemoveAt(i);
                    continue;
                }

                if (RogueLiteManager.runData.GetCount(recipe) >= recipe.MaxStack)
                {
                    continue;
                }

                weightedRandom.Add(recipe, recipe.weight);
            }


            if (weightedRandom.Count > 0)
            {
                recipe = weightedRandom.GetRandom();
                pickupPrototype = recipe.Item.GetComponent<IPickup>();
            }

            Debug.Log($"DiscoverableController.OnIsAliveChanged: {pickupPrototype}");
        }

        void DropRewards()
        {
            if (Random.value <= resourcesDropChance && pickupPrototype != null)
            {
                Vector3 pos = transform.position;
                pos.y = 0;

                GameObject resources = null;
                if (pickupPrototype is Pickup)
                {
                    resources = Instantiate(pickupPrototype as Pickup, pos, Quaternion.identity).gameObject;

                }
                else if (pickupPrototype is ShieldPickup)
                {
                    resources = Instantiate(pickupPrototype as ShieldPickup, pos, Quaternion.identity).gameObject;
                }
                else
                {
                    Debug.LogError("Unknown resources pickup type in DestructibleController. No rewards dropped.");
                    Destroy(gameObject, 15);
                    return;
                }

                if (inheritMaterial && modelRenderer != null)
                {
                    Renderer resourcesRenderer = resources.GetComponentInChildren<Renderer>();
                    if (resourcesRenderer != null)
                    {
                        resourcesRenderer.material = modelRenderer.material;
                    }
                }

                resources.tag = magnetizeResources ? "MagneticPickup" : "Untagged";
            }
        }

        /// <summary>
        /// Spawns destruction particles based on the provided mesh renderer and bounds multiplier.
        /// </summary>
        /// <param name="meshRenderer">The mesh renderer of the object being destroyed.</param>
        /// <param name="boundsMultiplier">A multiplier based on the bounds of the object to determine particle density.</param>
        /// <returns>The time-to-live (TTL) of the longest particle system spawned.</returns>
        /// <seealso cref="SpawnScaledParticle{T}(PooledObject[], MeshRenderer, int, bool)"/>
        private float SpawnDestructionParticles(MeshRenderer meshRenderer, int boundsMultiplier)
        {
            float ttl = 0;
            if (m_PooledScaledDestructionParticles != null)
            {
                ttl = SpawnScaledParticle<PooledObject>(m_PooledScaledDestructionParticles, meshRenderer, boundsMultiplier, true);
            }
            if (m_PooledScaledFXParticles != null)
            {
                float newTTL = SpawnScaledParticle<PooledObject>(m_PooledScaledFXParticles, meshRenderer, boundsMultiplier, false);
                if (newTTL > ttl)
                {
                    ttl = newTTL;
                }
            }

            return ttl;
        }

        /// <summary>
        /// Spawns scaled particles based on the provided particle systems, mesh renderer, and bounds multiplier.
        /// </summary>
        /// <typeparam name="T">The type of the pooled object.</typeparam>
        /// <param name="particleSystems">The array of particle systems to spawn.</param>
        /// <param name="meshRenderer">The mesh renderer of the object being destroyed.</param>
        /// <param name="boundsMultiplier">A multiplier based on the bounds of the object to determine particle density.</param>
        /// <param name="inheritColour">Whether the particles should inherit the color of the mesh renderer's material.</param>
        /// <returns>The time-to-live (TTL) of the longest particle system spawned.</returns>
        /// <seealso cref="SpawnDestructionParticles(MeshRenderer, int)"/>
        private float SpawnScaledParticle<T>(PooledObject[] particleSystems,  MeshRenderer meshRenderer, int boundsMultiplier, bool inheritColour) where T : PooledObject
        {
            float ttl = 0;

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

                if (longestDuration > ttl)
                {
                    ttl = longestDuration;
                }
            }
            return ttl + 1;
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
