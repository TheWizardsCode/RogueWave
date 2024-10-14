using NeoFPS;
using System;
using UnityEngine;
using Random = UnityEngine.Random;

namespace RogueWave
{
    /// <summary>
    /// FXJuicer is responsible for adding juice to a gameobject when it is spawned, injured, killed or other events.
    /// 
    /// Place it anywhere on an object and ensure that the Juices sections of the config are setup. The juices will be added at the location of this components transform.
    /// </summary>
    [Obsolete("Use More Mountains Feel instead.")]
    public class FXJuicer : PooledObject
    {
        [Header("Setup")]
        [SerializeField, Tooltip("The audio source for this enemy.")]
        AudioSource audioSource = null;

        [Header("Alive")]
        [SerializeField, Tooltip("The drone sound to play for this enemy.")]
        internal AudioClip _droneClip = null;
        [SerializeField, Tooltip("The volume of the drone sound."), Range(0,1)]
        internal float droneVolume = 1.0f;

        [Header("Death")]
        [SerializeField, Tooltip("The Game object which has the juice to add when the enemy is killed, for example any particles, sounds or explosions.")]
        internal PooledObject deathJuicePrefab;
        [SerializeField, Tooltip("The offset from the enemy's position to spawn the juice.")]
        internal Vector3 juiceOffset = Vector3.zero;
        [SerializeField, Tooltip("The sound to play when the enemy is killed.")]
        internal AudioClip[] deathClips;

        // REFACTOR: remove this coupling between the enemy controller and the juicer
        BasicEnemyController _ownerController;
        public BasicEnemyController OwnerController
        {
            get { 
                if (_ownerController == null)
                {
                    _ownerController = GetComponentInParent<BasicEnemyController>();

                    if (_ownerController == null)
                    {
                        Debug.LogError("FXJuicer cannot find the owning BasicEnemyController. " +
                            "If this is not a component in the parent hierarchy is must be explicitly set by calling `OwnerController = controller`");
                        return null;
                    }
                }
                return _ownerController; 
            }
            set {
                if (_ownerController != null && _ownerController != value)
                {
                    _ownerController.onDeath.RemoveListener(OnDeath);
                }

                _ownerController = value;
                _ownerController.onDeath.AddListener(OnDeath);
            }
        }

        /// <summary>
        /// The drone is the sound that plays when the enemy is alive.
        /// It is a looping sound.
        /// </summary>
        public AudioClip droneClip
        {
            get { return _droneClip; }
        }

        private void OnEnable()
        {
            StartAudio();
        }

        private void OnDisable()
        {
            StopAudio();
        }

        private void StartAudio()
        {
            if (droneClip == null)
            {
                return;
            }

            audioSource.clip = droneClip;
            audioSource.volume = droneVolume;
            audioSource.loop = true;
            audioSource.Play();
        }

        private void StopAudio()
        {
            if (audioSource != null)
            {
                audioSource.volume = 1;
                audioSource.Stop();
            }
        }

        public AudioClip GetDeathAudioClip()
        {
            if (deathClips.Length > 0)
            {
                return deathClips[Random.Range(0, deathClips.Length)];
            }
            else
            {
                return null;
            }
        }
        static void PlayOneShot(AudioClip clip, Vector3 position)
        {
            // OPTIMIZATION Play only a limited number of death sounds within a certain time frame. Perhaps adding chorus or similar on subsequent calls
            NeoFpsAudioManager.PlayEffectAudioAtPosition(clip, position);
        }

        void OnDeath(BasicEnemyController controller)
        {
            if (audioSource != null)
            {
                audioSource.Stop();
            }

            PlayOneShot(GetDeathAudioClip(), transform.position);

            if (deathJuicePrefab != null)
            {
                DeathVFX();
            }
        }

        private void DeathVFX()
        {
            Vector3 pos = transform.position + juiceOffset;
            ParticleSystem deathParticle = PoolManager.GetPooledObject<ParticleSystem>(deathJuicePrefab, pos, Quaternion.identity);
            if (OwnerController.parentRenderer != null)
            {
                var particleSystemRenderer = deathParticle.GetComponent<ParticleSystemRenderer>();
                if (particleSystemRenderer != null)
                {
                    particleSystemRenderer.material = OwnerController.parentRenderer.material;
                }
            }
            deathParticle.Play();

            if (OwnerController.shouldExplodeOnDeath)
            {
                PooledExplosion explosion = deathParticle.GetComponentInChildren<PooledExplosion>();
                explosion.radius = OwnerController.deathExplosionRadius;
                explosion.Explode(OwnerController.explosionDamageOnDeath, OwnerController.explosionForceOnDeath, null);
            }
        }
    }
}