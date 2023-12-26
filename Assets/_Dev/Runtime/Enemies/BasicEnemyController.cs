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
        [SerializeField, TextArea, Tooltip("The description of this enemy as displayed in the UI.")]
        protected string description = "TBD";

        [Header("Movement")]
        [SerializeField, Tooltip("How fast the enemy moves.")]
        protected float speed = 5f;
        [SerializeField, Tooltip("How fast the enemy rotates.")]
        protected float rotationSpeed = 1f;
        [SerializeField, Tooltip("How long the enemy will seek out the player for after losing sight of them.")]
        float seekDuration = 7;
        [SerializeField, Tooltip("The maximum distance the enemy will wander from their spawn point.")]
        float wanderRange = 15f;

        [Header("Behaviour")]
        [SerializeField, Tooltip("The maximum distance the enemy will wander from their spawn point. The enemy will move further away than this when they are chasing the player but will return to within this range if they go back to a wandering state.")]
        float maxWanderRange = 30f;
        [SerializeField, Tooltip("If true, the enemy will only move towards the player if they have line of sight. If false they will always seek out the player.")]
        bool requireLineOfSight = true;
        [SerializeField, Tooltip("The maximum distance the character can see")]
        float viewDistance = 30f;
        [SerializeField, Tooltip("The distance the enemy will try to avoid obstacles by.")]
        float obstacleAvoidanceDistance = 2f;
        [SerializeField, Tooltip("The source of the sensor array for this enemy. Note this must be inside the enemies collider.")]
        Transform sensor;
        [SerializeField, Tooltip("The layers the character can see")]
        LayerMask sensorMask = 0;

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

        Transform _target;
        internal Transform Target
        {
            get
            {
                if (_target == null && FpsSoloCharacter.localPlayerCharacter != null)
                    _target = FpsSoloCharacter.localPlayerCharacter.transform;
                return _target;
            }
        }

        Vector3 spawnPosition = Vector3.zero;
        Vector3 lastKnownPosition = Vector3.zero;
        Vector3 wanderDestination = Vector3.zero;
        float timeOfNextWanderPositionChange = 0;

        private void Start()
        {
            spawnPosition = transform.position;
        }

        protected virtual void Update()
        {
            if (Target == null)
            {
                Wander();
                return;
            }

            Vector3 targetPosition = Target.position;

            bool hasLineOfSight = false;
            if (requireLineOfSight && Vector3.Distance(targetPosition, transform.position) <= viewDistance)
            {
                Vector3 rayTargetPosition = targetPosition;
                rayTargetPosition.y = targetPosition.y + 0.8f; // TODO: Should use the seek targets

                Vector3 targetVector = rayTargetPosition - transform.position;

                Ray ray = new Ray(sensor.position, targetVector);
                Debug.DrawRay(sensor.position, targetVector, Color.red);
                RaycastHit hit;

                if (Physics.Raycast(ray, out hit, viewDistance, sensorMask) && hit.transform == Target)
                {
                    lastKnownPosition = targetPosition;
                    timeOfNextWanderPositionChange = Time.timeSinceLevelLoad + seekDuration;
                    hasLineOfSight = true;
                }
            } else if (!requireLineOfSight)
            {
                lastKnownPosition = targetPosition;
            }

            if (requireLineOfSight && !hasLineOfSight)
            {
                if (Time.timeSinceLevelLoad > timeOfNextWanderPositionChange)
                {
                    Wander();
                }
                else
                {
                    MoveTo(lastKnownPosition);
                }

                return;
            }
            else
            {
                MoveTo(lastKnownPosition);
            }
        }

        internal virtual void MoveTo(Vector3 destination, float speedMultiplier = 1)
        {
            if (Vector3.Distance(transform.position, destination) < 1f)
            {
                return;
            }

            float turnAngle = 50f;
            float distanceToObstacle = 0;
            bool rotateRight = true;
            Ray ray = new Ray(sensor.position, transform.forward);
            //ray.direction = transform.forward;
            
            if (Physics.Raycast(ray, out RaycastHit forwardHit, obstacleAvoidanceDistance, sensorMask))
            {
                if (forwardHit.collider.transform.root != Target)
                {
                    distanceToObstacle = forwardHit.distance;
                }
            }

            ray.direction = Quaternion.AngleAxis(turnAngle, transform.transform.up) * transform.forward;
            if (Physics.Raycast(ray, out RaycastHit rightForwardHit, obstacleAvoidanceDistance, sensorMask))
            {
                if (rightForwardHit.collider.transform.root != Target)
                {
                    distanceToObstacle = rightForwardHit.distance;
                    rotateRight = false;
                }
            }

            ray.direction = Quaternion.AngleAxis(-turnAngle, transform.transform.up) * transform.forward;
            if (Physics.Raycast(ray, out RaycastHit leftForwardHit, obstacleAvoidanceDistance, sensorMask))
            {
                if (leftForwardHit.collider.transform.root != Target)
                {
                    distanceToObstacle = leftForwardHit.distance;
                }
            }

            Quaternion targetRotation = Quaternion.identity;
            if (distanceToObstacle > 0) // turn to avoid obstacle
            {
                if (rotateRight)
                {
                    targetRotation = transform.rotation * Quaternion.Euler(0, turnAngle * (distanceToObstacle / obstacleAvoidanceDistance), 0);
                }
                else
                {
                    targetRotation = transform.rotation * Quaternion.Euler(0, -turnAngle * (distanceToObstacle / obstacleAvoidanceDistance), 0);
                }
                Debug.Log($"Rotating to avoid obstacle F {forwardHit.collider}, L {leftForwardHit.collider}, R {rightForwardHit.collider}");
            }
            else // no obstacle so rotate towards target
            {
                Vector3 directionToTarget = destination - transform.position;
                directionToTarget.y = 0;
                if (directionToTarget.magnitude > 0.5f)
                {
                    targetRotation = Quaternion.LookRotation(directionToTarget.normalized);
                }
            }

            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            
            transform.position += transform.forward * speed * speedMultiplier * Time.deltaTime;
            if (!Mathf.Approximately(destination.y, transform.position.y))
            {
                float rate = speed * speedMultiplier * Time.deltaTime;
                if (destination.y > transform.position.y)
                {
                    transform.position += Vector3.up * rate;
                }
                else
                {
                    transform.position -= Vector3.up * rate;
                }
                
            }
        }

        private void Wander()
        {
            if (Time.timeSinceLevelLoad > timeOfNextWanderPositionChange)
            {
                timeOfNextWanderPositionChange = Time.timeSinceLevelLoad + Random.Range(10f, maxWanderRange);
                wanderDestination = spawnPosition + Random.insideUnitSphere * wanderRange;
                wanderDestination.y = Mathf.Clamp(wanderDestination.y, 1, wanderRange);
                lastKnownPosition = wanderDestination;
            }

            MoveTo(wanderDestination);
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
