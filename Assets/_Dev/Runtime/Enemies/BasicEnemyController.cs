using NaughtyAttributes;
using NeoFPS;
using NeoFPS.SinglePlayer;
using System;
using UnityEngine;
using UnityEngine.Events;
using RogueWave.GameStats;
using Random = UnityEngine.Random;
using UnityEngine.Serialization;

namespace RogueWave
{
    public class BasicEnemyController : MonoBehaviour
    {
        internal enum SquadRole { None, Fodder, Leader, /* Heavy, Sniper, Medic, Scout*/ }

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
        internal Transform sensor;

        [Header("Animation")]
        [SerializeField, Tooltip("If true the enemy will rotate their head to face the player.")]
        internal bool headLook = true;
        [SerializeField, Tooltip("The head of the enemy. If set then this object will be rotated to face the player."), ShowIf("headLook")]
        Transform head;
        [SerializeField, Tooltip("The speed at which the head will rotate to face the plaeer."), Range(0, 10), ShowIf("headLook")]
        float headRotationSpeed = 2;
        [SerializeField, Tooltip("The maximum rotation of the head either side of forward."), Range(0, 180), ShowIf("headLook")]
        float maxHeadRotation = 75;

        [Header("Seek Behaviour")]
        [SerializeField, Tooltip("If true the enemy will return to their spawn point when they go beyond their seek distance.")]
        internal bool returnToSpawner = false;
        [SerializeField, Tooltip("If chasing an player and the player gets this far away from the enemy then the enemy will return to their spawn point and resume their normal behaviour.")]
        internal float seekDistance = 30;
        [SerializeField, Tooltip("How close to the player will this enemy try to get?")]
        internal float optimalDistanceFromPlayer = 0.2f;
        [SerializeField, Tooltip("How often the destination will be updated.")]
        private float destinationUpdateFrequency = 2f;

        [Header("Defensive Behaviour")]
        [SerializeField, Tooltip("If true then this enemy will spawn defensive units when it takes damage.")]
        internal bool spawnOnDamageEnabled = false;
        [SerializeField, Tooltip("If true defensive units will be spawned around the attacking unit. If false they will be spawned around this unit."), ShowIf("spawnOnDamageEnabled")]
        internal bool spawnOnDamageAroundAttacker = false;
        [SerializeField, Tooltip("The distance from the spawn point (damage source or this unit) that defensive units will be spawned. they will always spawn between the damage source and this enemy."), ShowIf("spawnOnDamageEnabled")]
        internal float spawnOnDamageDistance = 10;
        [SerializeField, Tooltip("Prototype to use to spawn defensive units when this enemy takes damage. This might be, for example, new enemies that will attack the thing doing damage."), ShowIf("spawnOnDamageEnabled")]
        internal PooledObject[] spawnOnDamagePrototypes;
        [SerializeField, Tooltip("The amount of damage the enemy must take before spawning defensive units."), ShowIf("spawnOnDamageEnabled")]
        internal float spawnOnDamageThreshold = 10;
        [SerializeField, Tooltip("The number of defensive units to spawn when this enemy takes damage."), ShowIf("spawnOnDamageEnabled")]
        internal int spawnOnDamageCount = 3;

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
        [SerializeField, Tooltip("The GameStat to increment when an enemy is spawned."), Foldout("Game Stats"), Required]
        internal GameStat enemySpawnedStat;
        [SerializeField, Tooltip("The GameStat to increment when an enemy is killed."), Foldout("Game Stats"), Required]
        internal GameStat enemyKillsStat;

        [SerializeField, Tooltip("Enable debuggging for this enemy."), Foldout("Editor Only")]
        bool isDebug;
        [SerializeField, Tooltip("Include this enemy in the showcase video generation."), Foldout("Editor Only")]
        public bool includeInShowcase = true;

        private AIDirector aiDirector;
        internal BasicEnemyController squadLeader;
        internal float timeOfNextDestinationChange = 0;
        internal Vector3 goalDestination = Vector3.zero;
        private float sqrSeekDistance;
        Vector3 wanderDestination = Vector3.zero;

        protected BasicMovementController movementController;

        Transform _target;
        internal Transform Target
        {
            get
            {
                if (_target == null && FpsSoloCharacter.localPlayerCharacter != null)
                    _target = FpsSoloCharacter.localPlayerCharacter.transform;
                return _target;
            }
            private set
            {
                _target = value;
            }
        }

        internal bool shouldUpdateDestination
        {
            get {
                return Time.timeSinceLevelLoad > timeOfNextDestinationChange; 
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

        int frameOfNextSightTest = 0;
        bool lastSightTestResult = false;
        internal bool CanSeeTarget
        {
            get
            {
                if (squadLeader != null && squadLeader != this)
                {
                    Target = squadLeader.Target;
                    return squadLeader.CanSeeTarget;
                }

                if (Target == null)
                {
                    return false;
                }

                if (frameOfNextSightTest >= Time.frameCount)
                {
                    return lastSightTestResult;
                }

                frameOfNextSightTest = Time.frameCount + 15;

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
                            lastSightTestResult = true;
                            if (squadLeader != null && squadLeader != this)
                            {
                                squadLeader.lastKnownTargetPosition = Target.position;
                                squadLeader.Target = Target;
                                squadLeader.frameOfNextSightTest = frameOfNextSightTest;

                            }
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

                lastSightTestResult = false;
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

        public Vector3 lastKnownTargetPosition { get; private set; }

        Vector3 spawnPosition = Vector3.zero;
        private bool underOrders;
        internal BasicHealthManager healthManager;
        private PooledObject pooledObject;
        private bool isRecharging;
        // TODO: Are both fromPool and isPooled needed?
        private bool fromPool;
        private bool isPooled = false;
        internal RogueWaveGameMode gameMode;

        protected virtual void Awake()
        {
            pooledObject = GetComponent<PooledObject>();

            gameMode = FindObjectOfType<RogueWaveGameMode>();

            movementController = GetComponent<BasicMovementController>();

#if ! UNITY_EDITOR
            isDebug = false;
#endif
        }

        private void Start()
        {
            spawnPosition = transform.position;
            sqrSeekDistance = seekDistance * seekDistance;
            aiDirector = FindObjectOfType<AIDirector>();
        }


        protected virtual void OnEnable()
        {
            PooledObject pooledObject = GetComponent<PooledObject>();
            if (pooledObject != null)
            {
                isPooled = true;
            }

            if (enemySpawnedStat != null && (!isPooled || fromPool)) // note that if the enemy is not pooled this means it is not counted. Handy for Spawners, but beware if you add other non-pooled enemies.
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
                healthManager.onHealthChanged += OnHealthChanged;
            }
        }

        protected virtual void OnDisable()
        {
            if (healthManager != null)
            {
                healthManager.onIsAliveChanged -= OnAliveIsChanged;
                healthManager.onHealthChanged -= OnHealthChanged;
            }

            onDestroyed?.Invoke();
            onDestroyed.RemoveAllListeners();
        }

        protected virtual void Update()
        {
            if (movementController == null)
            {
                return;
            }

            bool timeToUpdate = shouldUpdateDestination;

            if (Target == null)
            {
                if (timeToUpdate)
                {
                    goalDestination = WanderDestination();
                }
                movementController.MoveUpdateMovement(goalDestination, 1, squadLeader);
                return;
            }

            if (timeToUpdate)
            {
                UpdateDestination();
            }

            if (underOrders && movementController.hasArrived)
            {
                underOrders = false;
            }

            if (underOrders)
            {
                movementController.MoveUpdateMovement(goalDestination, 1.5f, squadLeader);
            }
            else
            {
                movementController.MoveUpdateMovement(goalDestination, 1, squadLeader);
            }
        }

        private void UpdateDestination()
        {
            float sqrDistance = Vector3.SqrMagnitude(goalDestination - Target.position);

            if (shouldAttack)
            {
                goalDestination = GetDestination(Target.position);
            }
            else if (!underOrders && Time.frameCount % 5 == 0)
            {
                // if line of sight is not required then update the destination at the appropriate time
                if (!requireLineOfSight)
                {
                    goalDestination = GetDestination(Target.position);
                }
                // else if the enemy is recharging and time is up then stop recharging
                else if (isRecharging)
                {
                    isRecharging = false;
                }
                // else if the enemy can see the player and the current destination is > 2x the optimal distance to the player then update the destination
                else if (sqrDistance < sqrSeekDistance)
                {
                    if (CanSeeTarget)
                    {
                        goalDestination = GetDestination(Target.position);
                        lastKnownTargetPosition = Target.position;
                    }
                    else
                    {
                        goalDestination = GetDestination(lastKnownTargetPosition);
                    }
                }
                // time for a wander
                else
                {
                    if (Vector3.SqrMagnitude(goalDestination - Target.position) > sqrSeekDistance)
                    {
                        if (returnToSpawner)
                        {
                            isRecharging = true;
                            goalDestination = spawnPosition;
                        }
                        else
                        {
                            goalDestination = WanderDestination();
                        }
                    }
                    else
                    {
                        goalDestination = WanderDestination();
                    }

                    movementController.MoveUpdateMovement(goalDestination, 1, squadLeader);
                }

                RotateHead();
            }
        }

        /// <summary>
        /// Enemies should not go to exactly where the player is but rather somewhere that places them at an
        /// optimal position. This method will return such a position.
        /// </summary>
        /// <param name="targetPosition">The current position of the target.</param>
        /// <returns>A position near the player that places the enemy at an optimal position to attack from.</returns>
        internal Vector3 GetDestination(Vector3 targetPosition)
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
            } while (Physics.CheckSphere(newPosition, 1f) && tries < 50);

            if (tries == 50)
            {
                newPosition = targetPosition;
            }

            timeOfNextDestinationChange = Time.timeSinceLevelLoad + destinationUpdateFrequency;

            return newPosition;
        }

        internal Vector3 WanderDestination()
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
                    wanderDestination.y = Mathf.Clamp(wanderDestination.y, movementController.minimumHeight, movementController.maximumHeight);
                } while (Physics.CheckSphere(wanderDestination, 1f) && tries < 50);

                goalDestination = wanderDestination;

#if UNITY_EDITOR
                if (isDebug)
                    Debug.LogWarning($"{name} selected wander destination of {wanderDestination}");
#endif
            }
            return wanderDestination;
        }

        private void RotateHead()
        {
            if (headLook && head != null)
            {
                Vector3 direction = Target.position - head.position;
                Quaternion targetRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
                float clampedRotation = Mathf.Clamp(head.rotation.eulerAngles.y, -maxHeadRotation, maxHeadRotation);
                head.rotation = Quaternion.Euler(head.rotation.eulerAngles.x, clampedRotation, head.rotation.eulerAngles.z);
            }
        }

        private void OnHealthChanged(float from, float to, bool critical, IDamageSource source)
        {
            if (spawnOnDamageEnabled == false)
            {
                return;
            }

            if (from - to >= spawnOnDamageThreshold)
            {
                for (int i = 0; i < spawnOnDamageCount; i++)
                {
                    PooledObject prototype = spawnOnDamagePrototypes[Random.Range(0, spawnOnDamagePrototypes.Length)];

                    Vector3 pos;
                    if (spawnOnDamageAroundAttacker)
                    {
                        pos = source.damageSourceTransform.position + (source.damageSourceTransform.forward * spawnOnDamageDistance);
                    }
                    else
                    {
                        pos = transform.position - (source.damageSourceTransform.forward * spawnOnDamageDistance);
                    }

                    pos += Random.insideUnitSphere;
                    pos.y = 1.5f;
                    BasicEnemyController enemy = PoolManager.GetPooledObject<BasicEnemyController>(prototype, pos, Quaternion.identity);
                    enemy.RequestAttack(Target.position);

                    // add a line renderer to indicate why the new enemies have spawned
                    LineRenderer line = enemy.GetComponent<LineRenderer>();
                    if (line == null)
                    {
                        line = enemy.gameObject.AddComponent<LineRenderer>();
                    }
                    line.startWidth = 0.03f;
                    line.endWidth = 0.05f;
                    line.material = new Material(Shader.Find("Unlit/Color"));
                    line.material.color = Color.blue;
                    line.SetPosition(0, transform.position);
                    line.SetPosition(1, enemy.transform.position);
                    Destroy(line, 0.2f);
                }
            }
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

        private void OnDrawGizmos()
        {
            if (underOrders)
            {
                Gizmos.color = Color.magenta;
                Gizmos.DrawLine(transform.position, goalDestination);
            }
        }

        private void OnDrawGizmosSelected()
        {
            if (goalDestination != Vector3.zero)
            {
                Gizmos.color = Color.blue;
                Gizmos.DrawLine(transform.position, goalDestination);
            }

            if (squadLeader == null)
            {
                return;
            }

            if (squadLeader == this)
            {
                foreach (BasicEnemyController enemy in aiDirector.GetSquadMembers(this))
                {
                    if (enemy != null && enemy != this)
                    {
                        Gizmos.color = Color.red;
                        Gizmos.DrawLine(transform.position, enemy.transform.position);
                    }
                }
            }
            else
            {
                Gizmos.color = Color.red;
                Gizmos.DrawLine(transform.position, squadLeader.transform.position);
            }
        }
    }
}
