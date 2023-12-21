using NeoFPS;
using NeoFPS.SinglePlayer;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Playground
{
    public class EnemyController : MonoBehaviour
    {
        [SerializeField, Tooltip("How fast the enemy moves.")]
        private float speed = 5f;
        [SerializeField, Tooltip("How fast the enemy rotates.")]
        private float rotationSpeed = 1f;

        [Header("Feedback")]
        [SerializeField, Tooltip("The sound to play when the enemy is killed.")]
        private AudioClip[] deathClips;
        [SerializeField, Tooltip("The particle system to play when the enemy is killed.")]
        private ParticleSystem deathParticlePrefab;

        [Header("Rewards")]
        [SerializeField, Tooltip("The chance of dropping a reward when killed.")]
        private float resourcesDropChance = 0.5f;
        [SerializeField, Tooltip("The resources this enemy drops when killed.")]
        private ResourcesPickup resourcesPrefab;

        private void Update()
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
            // TODO use pool for particles
            if (deathParticlePrefab != null)
            {
                ParticleSystem deathParticle = Instantiate(deathParticlePrefab, transform.position, Quaternion.identity);
                deathParticle.Play();
            }

            // Drop resources
            if (Random.value <= resourcesDropChance)
            {
                Vector3 pos = transform.position;
                pos.y = 0;
                ResourcesPickup resources = Instantiate(resourcesPrefab, pos, Quaternion.identity);
            }


            Destroy(gameObject);
        }
    }
}
