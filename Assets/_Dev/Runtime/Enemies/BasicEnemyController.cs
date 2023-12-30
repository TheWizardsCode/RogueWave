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
        [SerializeField, Tooltip("The minimum height the enemy will move to.")]
        float minimumHeight = 0.5f;
        [SerializeField, Tooltip("The maximum height the enemy will move to.")]
        float maximumHeight = 75f;
        [SerializeField, Tooltip("How close to the player will this enemy try to get?")]
        float optimalDistanceFromPlayer = 0.2f;

        [Header("Behaviour")]
        [SerializeField, Tooltip("How long the enemy will seek out the player for after losing sight of them.")]
        float seekDuration = 7;
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

        [Header("Debug")]
        [SerializeField, Tooltip("Enable debuggging for this enemy.")]
        bool isDebug;

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
        internal virtual bool shouldAttack
        {
            get
            { 
                if (requireLineOfSight && CanSeeTarget == false)
                {
                    return false;
                }

                return true;
            }
        }

        internal bool CanSeeTarget
        {
            get
            {
                if (Target == null)
                    return false;

                if (Vector3.Distance(Target.position, transform.position) <= viewDistance)
                {
                    Vector3 rayTargetPosition = Target.position;
                    rayTargetPosition.y = Target.position.y + 0.8f; // TODO: Should use the seek targets

                    Vector3 targetVector = rayTargetPosition - sensor.position;

                    Ray ray = new Ray(sensor.position, targetVector);
#if UNITY_EDITOR
                    if (isDebug)
                    {
                        Debug.DrawRay(sensor.position, targetVector, Color.red);
                    }
#endif
                    RaycastHit hit;
                    if (Physics.Raycast(ray, out hit, viewDistance, sensorMask) && hit.transform == Target)
                    {
                        lastKnownPosition = GetDestination(Target.position);
                        timeOfNextWanderPositionChange = Time.timeSinceLevelLoad + seekDuration;
                        return true;
                    }
                }

                return false;
            }
        }

        /// <summary>
        /// Enemies should not go to exactly where the player is but rather somewhere that places them at an
        /// optimal position. This method will return such a position.
        /// </summary>
        /// <param name="targetPosition">The current position of the target.</param>
        /// <returns>A position near the player that places the enemy at an optimal position to attack from.</returns>
        public Vector3 GetDestination(Vector3 targetPosition)
        {
            Vector3 newPosition = new Vector3();
            for (int i = 0; i < 4; i++)
            {
                newPosition = UnityEngine.Random.onUnitSphere * optimalDistanceFromPlayer;
                newPosition += targetPosition;
                if (newPosition.y >= minimumHeight)
                {
                    return newPosition;
                }
            }
            newPosition.y = Mathf.Max(newPosition.y, minimumHeight);
            return newPosition;
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

            if (requireLineOfSight == false)
            {
                lastKnownPosition = GetDestination(Target.position);
            }

            if (requireLineOfSight && CanSeeTarget == false)
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
            Vector3 directionToDestination = (destination - transform.position).normalized;
            float currentDistance = Vector3.Distance(transform.position, destination);

            if (currentDistance < optimalDistanceFromPlayer)
            {
                float distanceToMoveAway = optimalDistanceFromPlayer - currentDistance;
                destination = transform.position - (directionToDestination * distanceToMoveAway);
            }


            if (destination.y < minimumHeight)
            {
                destination.y = minimumHeight;
            } else if (destination.y > maximumHeight)
            {
                destination.y = maximumHeight;
            }

            float turnAngle = Vector3.Angle(transform.forward, directionToDestination);

            float distanceToObstacle = 0;
            bool rotateRight = true;

            Ray ray = new Ray(sensor.position, transform.forward);
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
                //Debug.Log($"Rotating to avoid obstacle F {forwardHit.collider}, L {leftForwardHit.collider}, R {rightForwardHit.collider}");
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
            AdjustHeight(destination, speedMultiplier);
        }

        private void AdjustHeight(Vector3 destination, float speedMultiplier)
        {
            float heightDifference = transform.position.y - destination.y;
            if ( heightDifference > 0.2f || heightDifference < -0.2f)
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
                wanderDestination = spawnPosition + Random.insideUnitSphere * maxWanderRange;
                wanderDestination.y = Mathf.Clamp(wanderDestination.y, 1, maxWanderRange);
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
