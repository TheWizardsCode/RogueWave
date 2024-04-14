using Codice.Client.BaseCommands;
using NaughtyAttributes;
using NeoFPS;
using NeoFPS.SinglePlayer;
using NeoSaveGames;
using System;
using System.Linq;
using System.Net;
using UnityEngine;
using UnityEngine.Events;
using RogueWave.GameStats;
using static Codice.Client.Commands.WkTree.WorkspaceTreeNode;
using Random = UnityEngine.Random;

namespace RogueWave
{
    public class BasicEnemyController : MonoBehaviour
    {
        internal enum SquadRole { Fodder, /* Heavy, Sniper, Medic, Scout, Leader,*/ None }

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
        [SerializeField, Tooltip("The rate at which the enemy accelerates to its maximum speed."), ShowIf("isMobile")]
        internal float acceleration = 10f;
        [SerializeField, Tooltip("How fast the enemy rotates."), ShowIf("isMobile")]
        internal float rotationSpeed = 1f;
        [SerializeField, Tooltip("The minimum height the enemy will move to."), ShowIf("isMobile")]
        internal float minimumHeight = 0.5f;
        [SerializeField, Tooltip("The maximum height the enemy will move to."), ShowIf("isMobile")]
        internal float maximumHeight = 75f;
        [SerializeField, Tooltip("How close to the player will this enemy try to get?"), ShowIf("isMobile")]
        internal float optimalDistanceFromPlayer = 0.2f;
        [SerializeField, Tooltip("How often the destination will be updated."), ShowIf("isMobile")]
        private float destinationUpdateFrequency = 2f;

        [Header("Navigation")]
        [SerializeField, Tooltip("The distance the enemy will try to avoid obstacles by."), ShowIf("isMobile")]
        internal float obstacleAvoidanceDistance = 2f;
        [SerializeField, Tooltip("The distance the enemy needs to be from a target destination for it to be considered as arrived. This is important as large enemies, or ones with a slow trun speed might have difficulty getting to the precise target location. This can result in circular motion around the destination."), ShowIf("isMobile")]
        internal float arrivalDistance = 1.5f;

        [Header("Seek Behaviour")]
        [SerializeField, Tooltip("How far the enemy will go from their spawn point when attacking the player. If the enemy goes further than this then they will return to their spawn point to 'recharge'. Then they will resume their normal behaviour."), ShowIf("isMobile")]
        internal float seekDistance = 30;

        [Header("SquadBehaviour")]
        [SerializeField, Tooltip("The role this enemy plays in a squad. This is used by the AI Director to determine how to deploy the enemy.")]
        internal SquadRole squadRole = SquadRole.Fodder;

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
        BasicEnemyController[] flockingGroup;
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

        internal bool shouldUpdateDestination
        {
            get { 
                return seekDistance > 0 && Time.timeSinceLevelLoad > timeOfNextDestinationChange; 
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
            if (timeOfNextDestinationChange > Time.timeSinceLevelLoad)
            {
                return goalDestination;
            }

            Vector3 newPosition = new Vector3();
            int tries = 0;
            do
            {
                tries++;
                newPosition = Random.onUnitSphere * optimalDistanceFromPlayer;
                newPosition += targetPosition;
                newPosition.y = Mathf.Max(newPosition.y, minimumHeight);
            } while (Physics.CheckSphere(newPosition, 1f) && tries < 50);

            if (tries == 50)
            {
                newPosition = targetPosition;
            }

            timeOfNextDestinationChange = Time.timeSinceLevelLoad + destinationUpdateFrequency;

            return newPosition;
        }

        Vector3 spawnPosition = Vector3.zero;
        internal Vector3 goalDestination = Vector3.zero;
        Vector3 wanderDestination = Vector3.zero;
        float timeOfNextDestinationChange = 0;
        private bool underOrders;
        internal BasicHealthManager healthManager;
        private float sqrSeekDistance;
        private float sqrArrivalDistance;
        private float sqrOptimalDistanceFromPlayer;
        private PooledObject pooledObject;
        private bool isRecharging;
        private bool fromPool;
        internal RogueWaveGameMode gameMode;

        protected virtual void Awake()
        {
            sqrSeekDistance = seekDistance * seekDistance;
            sqrArrivalDistance = arrivalDistance * arrivalDistance;
            sqrOptimalDistanceFromPlayer = optimalDistanceFromPlayer * optimalDistanceFromPlayer;
            pooledObject = GetComponent<PooledObject>();

            gameMode = FindObjectOfType<RogueWaveGameMode>();

#if ! UNITY_EDITOR
            isDebug = false;
#endif
        }

        private void Start()
        {
            spawnPosition = transform.position;
            currentSpeed = Random.Range(minSpeed, maxSpeed);
            flockingGroup = new BasicEnemyController[maxFlockSize];
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
                healthManager.AddHealth(healthManager.healthMax);
                healthManager.onIsAliveChanged += OnAliveIsChanged;
            }
        }

        protected virtual void OnDisable()
        {
            if (healthManager != null)
            {
                healthManager.onIsAliveChanged -= OnAliveIsChanged;
            }

            onDestroyed?.Invoke();
            onDestroyed.RemoveAllListeners();
        }

        protected virtual void Update()
        {
            if (isMobile == false)
            {
                return;
            }

            bool timeToUpdate = shouldUpdateDestination;

            if (Target == null)
            {
                if (timeToUpdate)
                {
                    Wander();
                }
                return;
            }

            // First update the destination if required
            if (Time.frameCount % 5 == 0) {
                // if line of sight is not required then update the destination at the appropriate time
                if (!requireLineOfSight && timeToUpdate)
                {
                    goalDestination = GetDestination(Target.position);
                }
                // else if the enemy is under orders then move to the ordered destination
                else if (underOrders)
                {
                    // Just follow the orders
                }
                // else if the enemy is recharging then move to the spawn position
                else if (isRecharging)
                {
                    if (timeToUpdate)
                    {
                        isRecharging = false;
                    }
                }
                // else if the enemy can see the player and the current destination is > 2x the optimal distance to the player then update the destination at the appropriate time
                else if (timeToUpdate && CanSeeTarget)
                {
                    if (Vector3.SqrMagnitude(goalDestination - Target.position) > sqrOptimalDistanceFromPlayer * 2)
                    {
                        goalDestination = GetDestination(Target.position);
                    }
                }
                // time for a wander
                else if (timeToUpdate)
                {
                    if (Vector3.SqrMagnitude(goalDestination - Target.position) > sqrSeekDistance)
                    {
                        isRecharging = true;
                        goalDestination = spawnPosition;
                    } else
                    {
                        Wander();
                    }
                }

                currentSpeed = Mathf.Min(maxSpeed, currentSpeed + Time.deltaTime * acceleration);

                RotateHead();
            }
            
            // Second move towards the destination if it is still appropriate
            // if the distance to the goal destination is within the seek distance and outside the arrive distance then move towards the goal destination
            float distanceToGoal = Vector3.SqrMagnitude(goalDestination - transform.position); 
            if (distanceToGoal < sqrSeekDistance && distanceToGoal > sqrArrivalDistance)
            {
                if (underOrders)
                {
                    MoveTowards(goalDestination, 1.5f);
                }
                else
                {
                    MoveTowards(goalDestination);
                }
            }
            // else if the distance to the goalDestination is < arrive distance then slow down, eventually stopping
            else if (distanceToGoal < sqrArrivalDistance)
            {
                underOrders = false;
                currentSpeed = 0;
            }
            // else move towards the spawn position
            else
            {
                goalDestination = GetDestination(spawnPosition);
                MoveTowards(goalDestination);
                isRecharging = true;
            }
        }

        private void RotateHead()
        {
            if (head != null)
            {
                Vector3 direction = Target.position - head.position;
                Quaternion targetRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
                float clampedRotation = Mathf.Clamp(head.rotation.eulerAngles.y, -maxHeadRotation, maxHeadRotation);
                head.rotation = Quaternion.Euler(head.rotation.eulerAngles.x, clampedRotation, head.rotation.eulerAngles.z);
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
                BasicEnemyController enemy = flockingColliders[i].GetComponentInParent<BasicEnemyController>();
                if (enemy != null && enemy != this && enemy.squadRole != SquadRole.None && flockingGroup.Contains(enemy) == false)
                {
                    flockingGroup[flockSize] = enemy;
                    centerDirection += enemy.transform.position;

                    if (Vector3.Distance(enemy.transform.position, transform.position) < flockAvoidanceDistance)
                    {
                        avoidanceDirection += transform.position - enemy.transform.position;
                    }

                    flockSize++;
                }
            }

            if (flockSize > 0)
            {
                // TODO: centreDirection is never used!
                centerDirection /= flockSize;
                centerDirection = (centerDirection - transform.position).normalized;
            }

            if (destination.y < minimumHeight)
            {
                destination.y = minimumHeight;
            }
            else if (destination.y > maximumHeight)
            {
                destination.y = maximumHeight;
            }

            Vector3 directionToTarget = (destination - transform.position).normalized;
            float dotProduct = Vector3.Dot(transform.forward, directionToTarget);

            if (dotProduct > 0)
            {
                transform.rotation = Quaternion.Slerp(transform.rotation, ObstacleAvoidanceRotation(destination, avoidanceDirection), rotationSpeed * Time.deltaTime);
            }
            else
            {
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(directionToTarget), rotationSpeed * Time.deltaTime);
            }

            transform.position += transform.forward * currentSpeed * speedMultiplier * Time.deltaTime;
            
            AdjustHeight(destination, speedMultiplier);
        }

        /// <summary>
        /// Get a recommended rotation for the enemy to take in order to avoid the nearest obstacle.
        /// </summary>
        /// <param name="destination">The destination we are trying to reach.</param>
        /// <param name="avoidanceDirection">The optimal direction we are trying to avoid other flock members.</param>
        /// <returns></returns>
        private Quaternion ObstacleAvoidanceRotation(Vector3 destination, Vector3 avoidanceDirection)
        {
            float distanceToObstacle = 0;
            float turnAngle = 0;
            float verticalAngle = 0;
            float turnRate = obstacleAvoidanceDistance * 10;

            // Check for obstacle dead ahead
            Ray ray = new Ray(sensor.position, transform.forward);
            if (Physics.Raycast(ray, out RaycastHit forwardHit, obstacleAvoidanceDistance, sensorMask))
            {
                if (forwardHit.collider.transform.root != Target)
                {
                    distanceToObstacle = forwardHit.distance;
                    turnAngle = 90;
                }
            }

            // check for obstacle to the left
            if (turnAngle == 0)
            {
                ray.direction = Quaternion.AngleAxis(-turnRate, transform.transform.up) * transform.forward;
                if (Physics.Raycast(ray, out RaycastHit leftForwardHit, obstacleAvoidanceDistance, sensorMask))
                {
                    if (leftForwardHit.collider.transform.root != Target)
                    {
                        distanceToObstacle = leftForwardHit.distance;
                        turnAngle = 45;
                    }
                }
            }

            // Check for obstacle to the right
            if (turnAngle == 0)
            {
                ray.direction = Quaternion.AngleAxis(turnRate, transform.transform.up) * transform.forward;
                if (Physics.Raycast(ray, out RaycastHit rightForwardHit, obstacleAvoidanceDistance, sensorMask))
                {
                    if (rightForwardHit.collider.transform.root != Target)
                    {
                        distanceToObstacle = rightForwardHit.distance;
                        turnAngle = -45;
                    }
                }
            }

#if UNITY_EDITOR
            if (isDebug) { 
                if (distanceToObstacle > 0)
                {
                    Debug.DrawRay(sensor.position, ray.direction * obstacleAvoidanceDistance, Color.red, 2);
                }
            }
#endif

            // Calculate avoidance rotation
            Quaternion targetRotation = Quaternion.identity;
            if (distanceToObstacle > 0) // turn to avoid obstacle
            {
                targetRotation = transform.rotation * Quaternion.Euler(verticalAngle * (distanceToObstacle / obstacleAvoidanceDistance), turnAngle * (distanceToObstacle / obstacleAvoidanceDistance), 0);
                if (distanceToObstacle < 1f)
                {
                    currentSpeed = 0.1f;
                }
#if UNITY_EDITOR
                if (isDebug)
                {
                    if (turnAngle != 0)
                    {
                        Debug.Log($"Rotating {turnAngle * (distanceToObstacle / obstacleAvoidanceDistance)} degrees on the Y axis to avoid an obstacle.");
                    }
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
            }

            return targetRotation;
        }

        private void AdjustHeight(Vector3 destination, float speedMultiplier)
        {
            float distanceToObstacle = 0;
            float testingAngle = 12;
            float verticalAngle = 0;

            // check for obstacle to the above/in front
            Ray ray = new Ray(sensor.position, Quaternion.AngleAxis(-testingAngle, transform.transform.right) * transform.forward);
            if (Physics.Raycast(ray, out RaycastHit forwardUpHit, obstacleAvoidanceDistance, sensorMask))
            {
                if (forwardUpHit.collider.transform.root != Target)
                {
                    distanceToObstacle = forwardUpHit.distance;
                    verticalAngle = -45;

#if UNITY_EDITOR
                    if (isDebug) {
                        if (distanceToObstacle > 0)
                        {
                            Debug.DrawRay(sensor.position, ray.direction * obstacleAvoidanceDistance, Color.red, 2);
                        }
                    }
#endif
                }
#if UNITY_EDITOR
                else
                {
                    Debug.DrawRay(sensor.position, ray.direction * obstacleAvoidanceDistance, Color.green, 2);
                }
#endif
            }

            // check for obstacle to the below/in front
            ray.direction = Quaternion.AngleAxis(testingAngle, transform.transform.right) * transform.forward;
            if (Physics.Raycast(ray, out RaycastHit forwardDownHit, obstacleAvoidanceDistance, sensorMask))
            {
                // TODO: Don't hard code the ground tag
                if (forwardDownHit.collider.transform.root != Target && !forwardDownHit.collider.CompareTag("Ground"))
                {
                    distanceToObstacle = forwardDownHit.distance;
                    verticalAngle = 45;

#if UNITY_EDITOR
                    if (isDebug && distanceToObstacle > 0)
                    {
                        Debug.DrawRay(sensor.position, ray.direction * obstacleAvoidanceDistance, Color.red, 2);
                    }
#endif
                }
#if UNITY_EDITOR
                else
                {
                    Debug.DrawRay(sensor.position, ray.direction * obstacleAvoidanceDistance, Color.green, 2);
                }
#endif
            }

            float rate = maxSpeed * speedMultiplier * Time.deltaTime;
            if (distanceToObstacle > 0)
            {
                // Try to get over or under the obstacle
                if (verticalAngle == 0)
                {
                    transform.position += Vector3.up * rate;
#if UNITY_EDITOR
                    if (isDebug)
                    {
                        Debug.Log("Obstacle is above and below in front, moving up in an atttempt to get over it.");
                    }
#endif
                }
                if (verticalAngle < 0)
                {
                    transform.position -= Vector3.up * rate;
#if UNITY_EDITOR
                    if (isDebug)
                    {
                        Debug.Log($"Moving down to avoid an obstacle.");
                    }
#endif
                }
                else if (verticalAngle > 0)
                {
                    transform.position += Vector3.up * rate;
#if UNITY_EDITOR
                    if (isDebug)
                    {
                        Debug.Log($"Moving up to avoid an obstacle.");
                    }              
#endif
                }
            } else
            {
                // No obstacle so adjust height to match destination
                float heightDifference = transform.position.y - destination.y;
                if (heightDifference > 0.2f || heightDifference < -0.2f)
                {
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
        }

        protected void Wander()
        {
            if (Time.timeSinceLevelLoad > timeOfNextDestinationChange)
            {
                isRecharging = false;
                timeOfNextDestinationChange = Time.timeSinceLevelLoad + destinationUpdateFrequency;

                int tries = 0;
                do
                {
                    tries++;
                    wanderDestination = spawnPosition + Random.insideUnitSphere * seekDistance;
                    wanderDestination.y = Mathf.Clamp(wanderDestination.y, minimumHeight, maximumHeight);
                } while (Physics.CheckSphere(wanderDestination, 1f) && tries < 50);

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
            timeOfNextDestinationChange = Time.timeSinceLevelLoad + destinationUpdateFrequency;
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
                    Gizmos.DrawLine(transform.position, flockingGroup[i].transform.position);
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
