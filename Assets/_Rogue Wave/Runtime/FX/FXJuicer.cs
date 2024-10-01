using NeoFPS;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using Random = UnityEngine.Random;

namespace RogueWave
{
    /// <summary>
    /// FXJuicer is responsible for adding juice to a gameobject when it is spawned, injured, killed or other events.
    /// 
    /// Place it anywhere on an object and ensure that the Juices sections of the config are setup. The juices will be added at the location of this components transform.
    /// </summary>
    public class FXJuicer : MonoBehaviour
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
        BasicEnemyController controller;

        /// <summary>
        /// The drone is the sound that plays when the enemy is alive.
        /// It is a looping sound.
        /// </summary>
        public AudioClip droneClip
        {
            get { return _droneClip; }
        }

        private void Awake()
        {
            controller = GetComponentInParent<BasicEnemyController>();
        }

        private void OnEnable()
        {
            controller.onDeath.AddListener(OnDeath);
            StartAudio();
        }

        private void OnDisable()
        {
            controller.onDeath.RemoveListener(OnDeath);
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
            if (controller.parentRenderer != null)
            {
                var particleSystemRenderer = deathParticle.GetComponent<ParticleSystemRenderer>();
                if (particleSystemRenderer != null)
                {
                    particleSystemRenderer.material = controller.parentRenderer.material;
                }
            }
            deathParticle.Play();

            if (controller.shouldExplodeOnDeath)
            {
                PooledExplosion explosion = deathParticle.GetComponentInChildren<PooledExplosion>();
                explosion.radius = controller.deathExplosionRadius;
                explosion.Explode(controller.explosionDamageOnDeath, controller.explosionForceOnDeath, null);
            }
        }
    }
}