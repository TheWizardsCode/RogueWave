using NeoFPS;
using NeoFPS.SinglePlayer;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Playground
{
    public class BasicEnemyController : MonoBehaviour
    {
        [Header("Metadata")]
        [SerializeField, Tooltip("The name of this enemy as displayed in the UI.")]
        protected string displayName = "TBD";
        [SerializeField, Tooltip("The description of this enemy as displayed in the UI.")]
        protected string description = "TBD";

        [Header("Movement")]
        [SerializeField, Tooltip("How fast the enemy moves.")]
        protected float speed = 5f;
        [SerializeField, Tooltip("How fast the enemy rotates.")]
        protected float rotationSpeed = 1f;

        [Header("Feedback")]
        [SerializeField, Tooltip("The sound to play when the enemy is killed.")]
        protected AudioClip[] deathClips;
        [SerializeField, Tooltip("The particle system to play when the enemy is killed.")]
        protected ParticleSystem deathParticlePrefab;

        [Header("Rewards")]
        [SerializeField, Tooltip("The chance of dropping a reward when killed.")]
        protected float resourcesDropChance = 0.5f;
        [SerializeField, Tooltip("The resources this enemy drops when killed.")]
        protected ResourcesPickup resourcesPrefab;

        protected virtual void Update()
        {
            if (FpsSoloCharacter.localPlayerCharacter == null)
                return;

            Vector3 destination = FpsSoloCharacter.localPlayerCharacter.localTransform.position;
            transform.position = Vector3.MoveTowards(transform.position, destination, speed * Time.deltaTime);

            Vector3 directionToTarget = destination - transform.position;
            directionToTarget.y = 0;
            if (directionToTarget != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(directionToTarget);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            }
        }

        public void OnAliveIsChanged(bool isAlive)
        {
            if (!isAlive)
                Die();
        }

        private void Die()
        {
            // Death Feedback
            if (deathClips.Length > 0)
            {
                NeoFpsAudioManager.PlayEffectAudioAtPosition(deathClips[Random.Range(0, deathClips.Length)], transform.position);
            }

            Renderer parentRenderer = GetComponentInChildren<Renderer>();

            // TODO use pool for particles
            if (deathParticlePrefab != null)
            {
                ParticleSystem deathParticle = Instantiate(deathParticlePrefab, transform.position, Quaternion.identity);
                if (parentRenderer != null)
                {
                    var particleSystemRenderer = deathParticle.GetComponent<ParticleSystemRenderer>();
                    if (particleSystemRenderer != null)
                    {
                        particleSystemRenderer.material = parentRenderer.material;
                    }
                }
                deathParticle.Play();
            }

            // Drop resources
            if (Random.value <= resourcesDropChance)
            {
                Vector3 pos = transform.position;
                pos.y = 0;
                ResourcesPickup resources = Instantiate(resourcesPrefab, pos, Quaternion.identity);
                if (parentRenderer != null)
                {
                    var resourcesRenderer = resources.GetComponentInChildren<Renderer>();
                    if (resourcesRenderer != null)
                    {
                        resourcesRenderer.material = parentRenderer.material;
                    }
                }
            }

            Destroy(gameObject);
        }
    }
}
