using NaughtyAttributes;
using NeoFPS;
using NeoFPS.SinglePlayer;
using NeoSaveGames;
using System;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using WizardsCode.GameStats;
using Random = UnityEngine.Random;

namespace RogueWave
{
    public class BasicEnemyController : MonoBehaviour
    {
        [SerializeField, Tooltip("The name of this enemy as displayed in the UI.")]
        public string displayName = "TBD";
        [SerializeField, TextArea, Tooltip("The description of this enemy as displayed in the UI.")]
        public string description = "TBD";
        [SerializeField, Tooltip("The level of this enemy. Higher level enemies will be more difficult to defeat.")]
        public int challengeRating = 1;

        [Header("Senses")]
        [SerializeField, Tooltip("If true, the enemy will only move towards or attack the player if they have line of sight. If false they will always seek out the player.")]
        internal bool requireLineOfSight = true;
        [SerializeField, Tooltip("The maximum distance the character can see"), ShowIf("requireLineOfSight")]
        internal float viewDistance = 30f;
        [SerializeField, Tooltip("The layers the character can see"), ShowIf("requireLineOfSight")]
        internal LayerMask sensorMask = 0;
        [SerializeField, Tooltip("The source of the sensor array for this enemy. Note this must be inside the enemies collider."), ShowIf("requireLineOfSight")]
        Transform sensor;

        [Header("Animation")]
        [SerializeField, Tooltip("The head of the enemy. If set then this object will be rotated to face the player.")]
        Transform head;
        [SerializeField, Tooltip("The speed at which the head will rotate to face the plaeer."), Range(0, 10)]
        float headRotationSpeed = 2;
        [SerializeField, Tooltip("The maximum rotation of the head either side of forward."), Range(0, 180)]
        float maxHeadRotation = 75;

        [Header("Movement")]
        [SerializeField, Tooltip("Is this enemy mobile?")]
        public bool isMobile = true;
        [SerializeField, Tooltip("The minimum speed at which the enemy moves."), ShowIf("isMobile")]
        internal float minSpeed = 4f;
        [SerializeField, Tooltip("The maximum speed at which the enemy moves."), ShowIf("isMobile")]
        internal float maxSpeed = 6f;
        [SerializeField, Tooltip("How fast the enemy rotates."), ShowIf("isMobile")]
        internal float rotationSpeed = 1f;
        [SerializeField, Tooltip("The minimum height the enemy will move to."), ShowIf("isMobile")]
        internal float minimumHeight = 0.5f;
        [SerializeField, Tooltip("The maximum height the enemy will move to."), ShowIf("isMobile")]
        internal float maximumHeight = 75f;
        [SerializeField, Tooltip("How close to the player will this enemy try to get?"), ShowIf("isMobile")]
        internal float optimalDistanceFromPlayer = 0.2f;

        [Header("Navigation")]
        [SerializeField, Tooltip("The distance the enemy will try to avoid obstacles by."), ShowIf("isMobile")]
        internal float obstacleAvoidanceDistance = 2f;

        [Header("Seek Behaviour")]
        [SerializeField, Tooltip("How long the enemy will seek out the player for after losing sight of them."), ShowIf("isMobile")]
        internal float seekDuration = 7;
        [SerializeField, Tooltip("How far the enemy will go from their spawn point when attacking the player. If the enemy goes further than this then they will return to their spawn point to 'recharge'. Then they will resume their normal behaviour."), ShowIf("isMobile")]
        internal float seekDistance = 30;

        [Header("Juice")]
        [SerializeField, Tooltip("Set to true to generate a damaging and/or knock back explosion when the enemy is killed.")]
        internal bool shouldExplodeOnDeath = false;
        [SerializeField, ShowIf("shouldExplodeOnDeath"), Tooltip("The radius of the explosion when the enemy dies.")]
        internal float deathExplosionRadius = 5f;
        [SerializeField, ShowIf("shouldExplodeOnDeath"), Tooltip("The amount of damage the enemy does when it explodes on death.")]
        internal float explosionDamageOnDeath = 20;
        [SerializeField, ShowIf("shouldExplodeOnDeath"), Tooltip("The force of the explosion when the enemy dies.")]
        internal float explosionForceOnDeath = 15;

        [Header("Rewards")]
        [SerializeField, Tooltip("The chance of dropping a reward when killed.")]
        internal float resourcesDropChance = 0.5f;
        [SerializeField, Tooltip("The resources this enemy drops when killed.")]
        internal ResourcesPickup resourcesPrefab;

        [Header("Core Events")]
        [SerializeField, Tooltip("The event to trigger when this enemy dies."), Foldout("Events")]
        public UnityEvent<BasicEnemyController> onDeath;
        [SerializeField, Tooltip("The event to trigger when this enemy is destroyed."), Foldout("Events")]
        public UnityEvent onDestroyed;

        // Game Stats
        [SerializeField, Tooltip("The GameStat to increment when an enemy is spawned."), Foldout("Game Stats")]
        internal GameStat enemySpawnedStat;
        [SerializeField, Tooltip("The GameStat to increment when an enemy is killed."), Foldout("Game Stats")]
        internal GameStat enemyKillsStat;

        [SerializeField, Tooltip("Enable debuggging for this enemy."), Foldout("Editor Only")]
        bool isDebug;
        [SerializeField, Tooltip("Include this enemy in the showcase video generation."), Foldout("Editor Only")]
        public bool includeInShowcase = true;

        internal float currentSpeed;

        int maxFlockSize = 8;
        Transform[] flockingGroup;
        Collider[] flockingColliders;
        float flockAvoidanceDistance = 10f;
        float flockRadius = 40f;

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

        internal bool shouldWander
        {
            get { 
                return seekDistance > 0 && Time.timeSinceLevelLoad > timeOfNextWanderPositionChange; 
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
                    if (Physics.Raycast(ray, out hit, viewDistance, sensorMask))
                    {
                        if (hit.transform == Target)
                        {
                            goalDestination = GetDestination(Target.position);
                            return true;
                        }
#if UNITY_EDITOR
                        else if (isDebug)
                        {
                            Debug.Log($"{name} couldn't see the player, sightline blocked by {hit.collider} on {hit.transform.root} at {hit.point}.");
                        }
#endif
                    }
#if UNITY_EDITOR
                    else if (isDebug)
                    {
                        Debug.Log($"{name} couldn't see the player, the raycast hit nothing.");
                    }
#endif
                }
#if UNITY_EDITOR
                else if (isDebug)
                {
                    Debug.Log($"{name} cannot see the player as they are further than {viewDistance}m away.");
                }
#endif

                return false;
            }
        }

        private Renderer _parentRenderer;
        internal Renderer parentRenderer
        {
            get
            {
                if (_parentRenderer == null)
                {
                    _parentRenderer = GetComponentInChildren<Renderer>();
                }
                return _parentRenderer;
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
            int tries = 0;
            for (tries = 0; tries < 4; tries++)
            {
                newPosition = UnityEngine.Random.onUnitSphere * optimalDistanceFromPlayer;
                newPosition += targetPosition;
                if (newPosition.y >= minimumHeight)
                {
                    break;
                }
            }

            newPosition.y = Mathf.Max(newPosition.y, minimumHeight);
            return newPosition;
        }

        Vector3 spawnPosition = Vector3.zero;
        internal Vector3 goalDestination = Vector3.zero;
        Vector3 wanderDestination = Vector3.zero;
        float timeOfNextWanderPositionChange = 0;
        private bool underOrders;
        internal BasicHealthManager healthManager;
        private float sqrSeekDistance;
        private PooledObject pooledObject;
        private bool isRecharging;
        private bool fromPool;
        private RogueWaveGameMode gameMode;

        private void Awake()
        {
            sqrSeekDistance = seekDistance * seekDistance;
            pooledObject = GetComponent<PooledObject>();

#if ! UNITY_EDITOR
            isDebug = false;
#endif
        }

        private void Start()
        {
            spawnPosition = transform.position;
            currentSpeed = Random.Range(minSpeed, maxSpeed);
            flockingGroup = new Transform[maxFlockSize];
            flockingColliders = new Collider[maxFlockSize * 3];
        }


        protected virtual void OnEnable()
        {
            if (enemySpawnedStat != null && fromPool) // note that if the enemy is not pooled this means it is not counted. Handy for Spawners, but beware if you add other non-pooled enemies.
            {
                enemySpawnedStat.Increment();
                gameMode.RegisterEnemy(this);
            } 
            else
            {
                fromPool = true;
            }

            healthManager = GetComponent<BasicHealthManager>();
            if (healthManager != null)
            {
                healthManager.onIsAliveChanged += OnAliveIsChanged;
            }

            gameMode = FindObjectOfType<RogueWaveGameMode>();
        }

        protected virtual void OnDisable()
        {
            if (healthManager != null)
            {
                healthManager.onIsAliveChanged -= OnAliveIsChanged;
            }

            healthManager.AddHealth(healthManager.healthMax);

            onDestroyed?.Invoke();
            onDestroyed.RemoveAllListeners();
        }

        protected virtual void LateUpdate()
        {
            if (isMobile == false)
            {
                return;
            }

            if (Target == null)
            {
                if (shouldWander)
                {
                    Wander();
                }
                return;
            }

            if (head != null)
            {
                Vector3 direction = Target.position - head.position;
                Quaternion targetRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
                head.rotation = Quaternion.Slerp(head.rotation, targetRotation, headRotationSpeed * Time.deltaTime);
            }


            if (requireLineOfSight == false)
            {
                goalDestination = GetDestination(Target.position);
            }

            if ((requireLineOfSight && CanSeeTarget == false) || underOrders)
            {
                if (shouldWander)
                {
                    Wander();
                }
                else if (underOrders || (isRecharging == false && Vector3.SqrMagnitude(goalDestination - transform.position) < sqrSeekDistance))
                {
                    MoveTowards(goalDestination);
                }
                else
                {
                    if (Vector3.SqrMagnitude(spawnPosition - transform.position) > 15f)
                    {
                        MoveTowards(spawnPosition);
                        isRecharging = true;
                    }
                    else
                    {
                        isRecharging = false;
                    }
                }

                return;
            }
            else if (isRecharging == false && Vector3.SqrMagnitude(goalDestination - transform.position) < sqrSeekDistance)
            {
                MoveTowards(goalDestination);
            }
            else
            {
                if (Vector3.SqrMagnitude(spawnPosition - transform.position) > 15f)
                {
                    MoveTowards(spawnPosition);
                    isRecharging = true;
                }
                else
                {
                    if (shouldWander)
                    {
                        Wander();
                    }
                    else
                    {
                        timeOfNextWanderPositionChange = 0;
                    }
                    isRecharging = false;
                }
            }
        }
        
        internal virtual void MoveTowards(Vector3 destination, float speedMultiplier = 1)
        {
            Vector3 centerDirection = destination - transform.position;
            Vector3 avoidanceDirection = Vector3.zero;
            int flockSize = 0;

            // OPTIMIZATON: AI Director should define flock groups, we shouldn't be doing overlap sphere checks every frame
            Array.Clear(flockingGroup, 0, flockingGroup.Length);
            Array.Clear(flockingColliders, 0, flockingColliders.Length);
            int colliderCount = Physics.OverlapSphereNonAlloc(transform.position, flockRadius, flockingColliders, LayerMask.GetMask("Enemy"));
            for (int i = 0; i < colliderCount && flockSize < maxFlockSize; i++)
            {
                if (flockingColliders[i].transform != transform && flockingGroup.Contains(flockingColliders[i].transform) == false)
                {
                    flockingGroup[flockSize] = flockingColliders[i].transform;
                    centerDirection += flockingGroup[flockSize].position;

                    if (Vector3.Distance(flockingGroup[flockSize].position, transform.position) < flockAvoidanceDistance)
                    {
                        avoidanceDirection += transform.position - flockingGroup[flockSize].position;
                    }

                    flockSize++;
                }
            }

            centerDirection /= flockSize;
            centerDirection = (centerDirection - transform.position).normalized;

            if (destination.y < minimumHeight)
            {
                destination.y = minimumHeight;
            }
            else if (destination.y > maximumHeight)
            {
                destination.y = maximumHeight;
            }

            transform.rotation = Quaternion.Slerp(transform.rotation, AvoidanceRotation(destination, avoidanceDirection), rotationSpeed * Time.deltaTime);

            float currentDistance = Mathf.Infinity;
            if (FpsSoloCharacter.localPlayerCharacter != null)
            {
                currentDistance = Vector3.Distance(transform.position, FpsSoloCharacter.localPlayerCharacter.transform.position);
            }

            if (currentDistance >= optimalDistanceFromPlayer)
            {
                transform.position += transform.forward * currentSpeed * speedMultiplier * Time.deltaTime;
            }

            AdjustHeight(destination, speedMultiplier);
        }

        /// <summary>
        /// Get a recommended rotation for the enemy to take in order to avoid the nearest obstacle.
        /// </summary>
        /// <param name="destination">The destination we are trying to reach.</param>
        /// <param name="avoidanceDirection">The optimal direction we are trying to avoid other flock members.</param>
        /// <returns></returns>
        private Quaternion AvoidanceRotation(Vector3 destination, Vector3 avoidanceDirection)
        {
            Vector3 directionToDestination = (destination - transform.position).normalized;
            float distanceToObstacle = 0;
            float turnAngle = 0;
            float turnRate = obstacleAvoidanceDistance * 10;

            // Check for obstacle dead ahead
            Ray ray = new Ray(sensor.position, transform.forward);
            if (Physics.Raycast(ray, out RaycastHit forwardHit, obstacleAvoidanceDistance, sensorMask))
            {
                if (forwardHit.collider.transform.root != Target)
                {
                    distanceToObstacle = forwardHit.distance;
                    turnAngle = 90;
#if UNITY_EDITOR
                    if (isDebug)
                    {
                        Debug.DrawRay(sensor.position, ray.direction * obstacleAvoidanceDistance, Color.red, 2);
                    }
#endif
                }
            }

            // check for obstacle to the left
            if (distanceToObstacle == 0)
            {
                ray.direction = Quaternion.AngleAxis(-turnRate, transform.transform.up) * transform.forward;
                if (Physics.Raycast(ray, out RaycastHit leftForwardHit, obstacleAvoidanceDistance, sensorMask))
                {
                    if (leftForwardHit.collider.transform.root != Target)
                    {
                        distanceToObstacle = leftForwardHit.distance;
                        turnAngle = 45;
#if UNITY_EDITOR
                        if (isDebug)
                        {
                            Debug.DrawRay(sensor.position, ray.direction * obstacleAvoidanceDistance, Color.red, 2);
                        }
#endif
                    }
                }
            }

            // Check for obstacle to the right
            if (distanceToObstacle == 0)
            {
                ray.direction = Quaternion.AngleAxis(turnRate, transform.transform.up) * transform.forward;
                if (Physics.Raycast(ray, out RaycastHit rightForwardHit, obstacleAvoidanceDistance, sensorMask))
                {
                    if (rightForwardHit.collider.transform.root != Target)
                    {
                        distanceToObstacle = rightForwardHit.distance;
                        turnAngle = -45;
#if UNITY_EDITOR
                        if (isDebug)
                        {
                            Debug.DrawRay(sensor.position, ray.direction * obstacleAvoidanceDistance, Color.red, 2);
                        }
#endif
                    }
                }
            }

            // Calculate avoidance rotation
            Quaternion targetRotation = Quaternion.identity;
            if (distanceToObstacle > 0) // turn to avoid obstacle
            {
                targetRotation = transform.rotation * Quaternion.Euler(0, turnAngle * (distanceToObstacle / obstacleAvoidanceDistance), 0);
                currentSpeed = Mathf.Max(minSpeed, currentSpeed * 0.5f * Time.deltaTime);
#if UNITY_EDITOR
                if (isDebug)
                {
                    Debug.Log($"Rotating {turnAngle * (distanceToObstacle / obstacleAvoidanceDistance)} degrees to avoid obstacle.");
                }
#endif
            }
            else // no obstacle so rotate towards target
            {
                Vector3 directionToTarget = destination - transform.position;
                directionToTarget.y = 0;
                avoidanceDirection.y = 0;
                Vector3 direction = (directionToTarget + avoidanceDirection).normalized;
                if (direction != Vector3.zero)
                {
                    targetRotation = Quaternion.LookRotation(direction);
                }
                currentSpeed = Mathf.Max(maxSpeed, currentSpeed * 2f * Time.deltaTime);
            }

            return targetRotation;
        }

        private void AdjustHeight(Vector3 destination, float speedMultiplier)
        {
            float heightDifference = transform.position.y - destination.y;
            if ( heightDifference > 0.2f || heightDifference < -0.2f)
            {
                float rate = currentSpeed * speedMultiplier * Time.deltaTime;
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

        protected void Wander()
        {
            if (Time.timeSinceLevelLoad > timeOfNextWanderPositionChange)
            {
                timeOfNextWanderPositionChange = Time.timeSinceLevelLoad + Random.Range(10f, seekDistance);

                do
                {
                    wanderDestination = spawnPosition + Random.insideUnitSphere * seekDistance;
                    wanderDestination.y = Mathf.Clamp(wanderDestination.y, 1, seekDistance);
                } while (Physics.CheckSphere(wanderDestination, 1f));

                goalDestination = wanderDestination;

#if UNITY_EDITOR
                if (isDebug)
                    Debug.LogWarning($"{name} updating wander position to {goalDestination}");
#endif

            }

            MoveTowards(wanderDestination);
        }

        public void OnAliveIsChanged(bool isAlive)
        {
            if (!isAlive)
                Die();
        }

        private void Die()
        {
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

            onDeath?.Invoke(this);

            if (enemyKillsStat != null)
            {
                enemyKillsStat.Increment();
            }

            // OPTIMIZATION: cache PooledObject reference
            if (pooledObject != null)
            {
                pooledObject.ReturnToPool();
            } else
            {
                Destroy(gameObject);
            }
            
        }

        /// <summary>
        /// The Enemy is requested to move to and attack the location provided. 
        /// The enemy will move to a point near the location and attack if it sees a target on the way.
        /// </summary>
        /// <param name="position"></param>
        internal void RequestAttack(Vector3 position)
        {
            goalDestination = GetDestination(position);
            timeOfNextWanderPositionChange = Time.timeSinceLevelLoad + seekDuration;
            underOrders = true;
            //Debug.Log($"{name} has been requested to attack {position}.");
        }

        private void OnDrawGizmosSelected()
        {
            if (flockingGroup != null && flockingGroup.Count() > 0)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawWireSphere(transform.position, flockRadius);
                for (int i = 0; i < maxFlockSize && flockingGroup[i] != null; i++)
                {
                    Gizmos.DrawLine(transform.position, flockingGroup[i].position);
                }
            }

            if (goalDestination != Vector3.zero)
            {
                Gizmos.color = Color.blue;
                Gizmos.DrawLine(transform.position, goalDestination);
            }
        }
    }
}
