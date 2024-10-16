using NaughtyAttributes;
using NeoFPS;
using NeoFPS.SinglePlayer;
using System;
using UnityEngine;
using UnityEngine.Events;
using RogueWave.GameStats;
using Random = UnityEngine.Random;
using UnityEngine.Serialization;
using System.Text;
using WizardsCode.RogueWave;

namespace RogueWave
{
    [RequireComponent(typeof(AudioSource))]
    public class BasicEnemyController : PooledObject
    {
        internal enum SquadRole { None, Fodder, Leader, /* Heavy, Sniper, Medic, Scout*/ }

        // Metadata
        [ValidateInput("Validate", "This enemy is not valid, run the validator in the Dev Management Window for details.")]

        [SerializeField, Tooltip("The name of this enemy as displayed in the UI."), BoxGroup("Metadata")]
        public string displayName = "TBD";
        [SerializeField, Tooltip("The icon that represents this enemy in the UI."), BoxGroup("Metadata")]
        public Sprite[] icon;
        [SerializeField, TextArea, Tooltip("The description of this enemy as displayed in the UI."), FormerlySerializedAs("description"), BoxGroup("Metadata")]
        private string m_description = "TBD";
        [SerializeField, Tooltip("The strengths of this enemy as displayed in the UI."), BoxGroup("Metadata")]
        private string strengths = string.Empty;
        [SerializeField, Tooltip("The weaknesses of this enemy as displayed in the UI."), BoxGroup("Metadata")]
        private string weaknesses = string.Empty;
        [SerializeField, Tooltip("The attacks of this enemy as displayed in the UI."), BoxGroup("Metadata")]
        private string attacks = string.Empty;
        [SerializeField, Tooltip("The level of this enemy. Higher level enemies will be more difficult to defeat."), BoxGroup("Metadata")]
        public int challengeRating = 1;

        // Senses
        [SerializeField, Tooltip("If true, the enemy will only move towards the player if they have line of sight OR if they are a part of a squad in which at least one squad member has line of sight. If true will only attack if this enemy has lince of sight. If false they will always seek and attack out the player."), BoxGroup("Senses")]
        internal bool requireLineOfSight = true;
        [SerializeField, Tooltip("The maximum distance the character can see"), ShowIf("requireLineOfSight"), BoxGroup("Senses")]
        internal float viewDistance = 30f;
        [SerializeField, Tooltip("The layers the character can see"), ShowIf("requireLineOfSight"), BoxGroup("Senses")]
        internal LayerMask sensorMask = 0;
        [SerializeField, Tooltip("The source of the sensor array for this enemy. Note this must be inside the enemies collider."), ShowIf("requireLineOfSight"), BoxGroup("Senses")]
        internal Transform sensor;

        // Animation
        [SerializeField, Tooltip("If true the enemy will rotate their head to face the player."), BoxGroup("Animation")]
        internal bool headLook = true;
        [SerializeField, Tooltip("The head of the enemy. If set then this object will be rotated to face the player."), ShowIf("headLook"), BoxGroup("Animation")]
        Transform head;
        [SerializeField, Tooltip("The maximum rotation of the head either side of forward."), Range(0, 180), ShowIf("headLook"), BoxGroup("Animation")]
        float maxHeadRotation = 75;

        // Seek Behaviour
        [SerializeField, Tooltip("If true the enemy will return to their spawn point when they go beyond their seek distance."), BoxGroup("Seek Behaviour")]
        internal bool returnToSpawner = false;
        [SerializeField, Tooltip("If chasing a player and the player gets this far away from the enemy then the enemy will return to their spawn point and resume their normal behaviour."), BoxGroup("Seek Behaviour")]
        internal float seekDistance = 30;
        [SerializeField, Tooltip("How close to the player will this enemy try to get?"), BoxGroup("Seek Behaviour")]
        internal float optimalDistanceFromPlayer = 0.2f;
        [SerializeField, Tooltip("How often the destination will be updated."), BoxGroup("Seek Behaviour")]
        private float destinationUpdateFrequency = 2f;

        // Defensive Behaviour
        [SerializeField, Tooltip("If true then this enemy will spawn defensive units when it takes damage."), BoxGroup("Defensive Behaviour")]
        internal bool spawnOnDamageEnabled = false;
        [SerializeField, Tooltip("If true defensive units will be spawned around the attacking unit. If false they will be spawned around this unit."), ShowIf("spawnOnDamageEnabled"), BoxGroup("Defensive Behaviour")]
        internal bool spawnOnDamageAroundAttacker = false;
        [SerializeField, Tooltip("The distance from the spawn point (damage source or this unit) that defensive units will be spawned. they will always spawn between the damage source and this enemy."), ShowIf("spawnOnDamageEnabled"), BoxGroup("Defensive Behaviour")]
        internal float spawnOnDamageDistance = 10;
        [SerializeField, Tooltip("Prototype to use to spawn defensive units when this enemy takes damage. This might be, for example, new enemies that will attack the thing doing damage."), ShowIf("spawnOnDamageEnabled"), BoxGroup("Defensive Behaviour")]
        internal PooledObject[] spawnOnDamagePrototypes;
        [SerializeField, Tooltip("The amount of damage the enemy must take before spawning defensive units."), ShowIf("spawnOnDamageEnabled"), BoxGroup("Defensive Behaviour")]
        internal float spawnOnDamageThreshold = 10;
        [SerializeField, Tooltip("The number of defensive units to spawn when this enemy takes damage."), ShowIf("spawnOnDamageEnabled"), BoxGroup("Defensive Behaviour")]
        internal int spawnOnDamageCount = 3;

        // SquadBehaviour
        [SerializeField, Tooltip("If true then this enemy will register with the AI director and be available to recieve orders. If false the AI director will not give this enemy orders."), BoxGroup("SquadBehaviour")]
        internal bool registerWithAIDirector = true;
        [SerializeField, Tooltip("The role this enemy plays in a squad. This is used by the AI Director to determine how to deploy the enemy."), ShowIf("registerWithAIDirector"), BoxGroup("SquadBehaviour")]
        internal SquadRole squadRole = SquadRole.Fodder;

        // Juice
        [SerializeField, Tooltip("A looping sound to play continuously while the enemy is alive."), BoxGroup("Juice")]
        internal AudioClip droneClip;
        [SerializeField, Tooltip("The sound to play when the enemy is killed."), BoxGroup("Juice")]
        internal AudioClip[] deathClips;
        [SerializeField, Tooltip("The Game object which has the juice to add when the enemy is killed, for example any particles, sounds or explosions."), Required, BoxGroup("Juice")]
        internal PooledExplosion deathJuicePrefab;
        [SerializeField, Tooltip("The offset from the enemy's position to spawn the juice."), BoxGroup("Juice")]
        internal Vector3 juiceOffset = Vector3.zero;
        [SerializeField, Tooltip("Set to true to generate a damaging and/or knock back explosion when the enemy is killed."), BoxGroup("Juice")]
        internal bool causeDamageOnDeath = false;
        [SerializeField, Tooltip("The radius of the explosion when the enemy dies."), ShowIf("causeDamageOnDeath"), BoxGroup("Juice")]
        internal float deathExplosionRadius = 5f;
        [SerializeField, Tooltip("The amount of damage the enemy does when it explodes on death."), ShowIf("causeDamageOnDeath"), BoxGroup("Juice")]
        internal float explosionDamageOnDeath = 20;
        [SerializeField, Tooltip("The force of the explosion when the enemy dies."), ShowIf("causeDamageOnDeath"), BoxGroup("Juice")]
        internal float explosionForceOnDeath = 15;

        // Rewards
        [SerializeField, Tooltip("The chance of dropping a reward when killed."), Range(0, 1), BoxGroup("Loot")]
        internal float resourcesDropChance = 0.5f;
        [SerializeField, Tooltip("The resources this enemy drops when killed."), BoxGroup("Loot")]
        internal ResourcesPickup resourcesPrefab;

        // Core Events
        [SerializeField, Tooltip("The event to trigger when this enemy dies."), Foldout("Events")]
        public UnityEvent<BasicEnemyController> onDeath;
        [SerializeField, Tooltip("The event to trigger when this enemy is destroyed."), Foldout("Events")]
        public UnityEvent onDestroyed;

        // Game Stats
        [SerializeField, Tooltip("The GameStat to increment when an enemy is spawned."), Foldout("Game Stats"), Required]
        internal IntGameStat enemySpawnedStat;
        [SerializeField, Tooltip("The GameStat to increment when an enemy is killed."), Foldout("Game Stats"), Required]
        internal IntGameStat enemyKillsStat;

        [SerializeField, Tooltip("Enable debuggging for this enemy."), Foldout("Editor Only")]
        bool isDebug;
        [SerializeField, Tooltip("Include this enemy in the showcase video generation."), Foldout("Editor Only")]
        public bool includeInShowcase = true;

        private AIDirector aiDirector;
        internal BasicEnemyController squadLeader;
        internal float timeOfNextDestinationChange = 0;
        internal Vector3 goalDestination = Vector3.zero;
        private float sqrSeekDistance;
        private PooledObject _deathExplosionPrototype;

        internal BasicMovementController movementController;

        public float ResourceDropChange { get => resourcesDropChance; set => resourcesDropChance = value; }

        public string description { 
            get {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine(m_description);
                if (!string.IsNullOrEmpty(strengths))
                {
                    sb.AppendLine();
                    //sb.Append("Strengths: ");
                    sb.AppendLine(strengths);
                }
                if (!string.IsNullOrEmpty(weaknesses))
                {
                    sb.AppendLine();
                    //sb.Append("Weaknesses: ");
                    sb.AppendLine(weaknesses);
                }
                if (!string.IsNullOrEmpty(attacks))
                {
                    sb.AppendLine();
                    //sb.Append("Attacks: ");
                    sb.AppendLine(attacks);
                }
                return sb.ToString(); 
            } 
        }

        Transform _target;
        internal Transform Target
        {
            get
            {
                if (_target == null && FpsSoloCharacter.localPlayerCharacter != null)
                {
                    _target = FpsSoloCharacter.localPlayerCharacter.transform;
                }
                return _target;
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
                if (requireLineOfSight)
                {
                    return CanSeeTarget;
                }

                return true;
            }
        }

        int frameOfNextSightTest = 0;
        bool lastSightTestResult = false;
        /// <summary>
        /// Test to see if this enemy can see the target. Note that this will only return true if the target is within the view distance of this enemy and there is a clear line of sight to the target.
        /// 
        /// If you want to test for whether a squad member is aware of the targets position then use the SquadCanSeeTarget property.
        /// </summary>
        internal bool CanSeeTarget
        {
            get
            {
                if (Target == null)
                {
                    return false;
                }

                if (frameOfNextSightTest >= Time.frameCount)
                {
                    return lastSightTestResult;
                }

                frameOfNextSightTest = Time.frameCount + Random.Range(7, 18);

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
                            if (squadLeader != null && squadLeader != this)
                            {
                                squadLeader.lastKnownTargetPosition = Target.position;
                                squadLeader.lastSightTestResult = true;
                                squadLeader.frameOfNextSightTest = frameOfNextSightTest;
                            }

                            lastSightTestResult = true;
                            return lastSightTestResult;
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
                return lastSightTestResult;
            }
        }

        /// <summary>
        /// Tests to see if any member of the squad can see the target. This is useful for determining if the squad should move towards the target.
        /// </summary>
        internal bool SquadCanSeeTarget
        {
            get
            {
                if (squadLeader != null && squadLeader != this && squadLeader.CanSeeTarget)
                {
                    return true; // doesn't matter if this enemy can see the target, the squad leader can and will tell this unit where to go.
                }

                if (Target == null)
                {
                    return false;
                }

                return CanSeeTarget;
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

        protected override void Awake()
        {
            base.Awake();

            pooledObject = this;
            _deathExplosionPrototype = deathJuicePrefab.GetComponent<PooledObject>(); ;

            gameMode = FindObjectOfType<RogueWaveGameMode>();

            movementController = GetComponent<BasicMovementController>();

            audioSource = GetComponent<AudioSource>();

#if ! UNITY_EDITOR
            isDebug = false;
#endif
        }

        private void Start()
        {
            spawnPosition = transform.position;
            sqrSeekDistance = seekDistance * seekDistance;
            aiDirector = AIDirector.Instance;
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
                enemySpawnedStat.Add(1);
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

            destinationMinX = gameMode.currentLevelDefinition.lotSize.x;
            destinationMinY = gameMode.currentLevelDefinition.lotSize.y;
            destinationMaxX = (gameMode.currentLevelDefinition.mapSize.x - 1) * gameMode.currentLevelDefinition.lotSize.x;
            destinationMaxY = (gameMode.currentLevelDefinition.mapSize.y - 1) * gameMode.currentLevelDefinition.lotSize.y;

            StartDroneAudio();
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

            AudioManager.Stop(audioSource);
        }
        void OnDeath()
        {
            if (deathJuicePrefab != null)
            {
                DeathVFX();
            }

            if (deathClips.Length == 0)
            {
                return;
            }

            AudioManager.Play3DOneShot(audioSource, deathClips[Random.Range(0, deathClips.Length)], transform.position);
        }
        private void StartDroneAudio()
        {
            if (droneClip == null)
            {
                return;
            }

            AudioManager.Play3DLooping(audioSource, droneClip, transform);
        }


        private void DeathVFX()
        {
            Vector3 pos = transform.position + juiceOffset;
            RWPooledExplosion explosion = PoolManager.GetPooledObject<RWPooledExplosion>(_deathExplosionPrototype, pos, Quaternion.identity);
            explosion.ParticleMaterial = parentRenderer.material;

            if (causeDamageOnDeath)
            {
                explosion.radius = deathExplosionRadius;
                explosion.Explode(explosionDamageOnDeath, explosionForceOnDeath, null);
            }
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
                    goalDestination = GetWanderDestination();
                }
                movementController.SetMovementGoals(goalDestination, 1, squadLeader);
                return;
            }

            if (timeToUpdate || SquadCanSeeTarget || movementController.hasArrived)
            {
                UpdateDestination();
            }

            if (underOrders && movementController.hasArrived)
            {
                underOrders = false;
            }

            if (underOrders)
            {
                movementController.SetMovementGoals(goalDestination, 1.5f, squadLeader);
            }
            else
            {
                movementController.SetMovementGoals(goalDestination, 1, squadLeader);
            }
        }

        private void UpdateDestination()
        {
            float sqrDistance = Vector3.SqrMagnitude(goalDestination - Target.position);

            if (shouldAttack)
            {
                goalDestination = GetDestination(Target.position);
            }
            else if (!underOrders)
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
                // else current destination is < the seek distance (how far the enemy is willing to move from its "base") then we need a new destination
                else if (sqrDistance < sqrSeekDistance)
                {
                    if (SquadCanSeeTarget)
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
                            goalDestination = GetWanderDestination();
                        }
                    }
                    else
                    {
                        goalDestination = GetWanderDestination();
                    }

                    movementController.SetMovementGoals(goalDestination, 1, squadLeader);
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
            if (!shouldAttack && timeOfNextDestinationChange < Time.timeSinceLevelLoad)
            {
                if (goalDestination != Vector3.zero)
                {
                    return goalDestination;
                }
            }

            Vector3 newPosition = targetPosition;
            int tries = 0;
            do
            {
                tries++;
                newPosition = Random.onUnitSphere * optimalDistanceFromPlayer;
                newPosition += targetPosition;
            } while (!IsValidDestination(newPosition, optimalDistanceFromPlayer * 0.9f) && tries < 50);

            if (tries == 50)
            {
                newPosition = targetPosition;
            }

            timeOfNextDestinationChange = Time.timeSinceLevelLoad + destinationUpdateFrequency;

            return newPosition;
        }

        internal Vector3 GetWanderDestination()
        {
            Vector3 wanderDestination = Vector3.positiveInfinity;
            if (Time.timeSinceLevelLoad > timeOfNextDestinationChange)
            {
                isRecharging = false;
                timeOfNextDestinationChange = Time.timeSinceLevelLoad + destinationUpdateFrequency;

                int tries = 0;
                while (!IsValidDestination(wanderDestination, 1f) && tries < 50)
                {
                    tries++;
                    wanderDestination.x = Random.Range(destinationMinX, destinationMaxX);
                    wanderDestination.y = Random.Range(movementController.minimumHeight, movementController.maximumHeight);
                    wanderDestination.z = Random.Range(destinationMinY, destinationMaxY);
                }


                if (tries == 50)
                {
                    wanderDestination = spawnPosition;
#if UNITY_EDITOR
                    Debug.LogWarning($"{name} unable to find a wander destination returning to spawn position.");
#endif
                }
            }
            return wanderDestination;
        }

        private float destinationMinX;
        private float destinationMinY;
        private float destinationMaxX;
        private float destinationMaxY;
        private AudioSource audioSource;

        private bool IsValidDestination(Vector3 destination, float avoidanceDistance)
        {
            if (destination.x < destinationMinX || destination.x > destinationMaxX || destination.z < destinationMinY || destination.z > destinationMaxY)
            {
                return false;
            }

            // OPTIMIZATION: Check only essential layers
            RaycastHit hit;
            Physics.queriesHitBackfaces = true;

            bool hitCollider = Physics.Raycast(destination, Vector3.forward, out hit, avoidanceDistance);
            if (!hitCollider)
            {
                hitCollider = Physics.Raycast(destination, Vector3.back, out hit, avoidanceDistance);
            }
            if (!hitCollider)
            {
                hitCollider = Physics.Raycast(destination, Vector3.left, out hit, avoidanceDistance);
            }
            if (!hitCollider)
            {
                hitCollider = Physics.Raycast(destination, Vector3.right, out hit, avoidanceDistance);
            }

            Physics.queriesHitBackfaces = false;

            return !hitCollider;
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
                SpawnOnDamage(source);
            }
        }

        private void SpawnOnDamage(IDamageSource source)
        {
            for (int i = 0; i < spawnOnDamageCount; i++)
            {
                PooledObject prototype = spawnOnDamagePrototypes[Random.Range(0, spawnOnDamagePrototypes.Length)];

                Vector3 pos;
                if (spawnOnDamageAroundAttacker)
                {
                    pos = source.damageSourceTransform.position;
                    if (source != null)
                    {
                        pos += source.damageSourceTransform.forward * spawnOnDamageDistance;
                    }
                    pos += Random.insideUnitSphere * spawnOnDamageDistance;
                    pos.y = source.damageSourceTransform.position.y + 1f;
                }
                else
                {
                    pos = transform.position;
                    if (source != null)
                    {
                        pos += source.damageSourceTransform.forward * spawnOnDamageDistance;
                    }
                    pos += Random.insideUnitSphere * spawnOnDamageDistance;
                    pos.y = transform.position.y + 1f;
                }

                BasicEnemyController enemy = PoolManager.GetPooledObject<BasicEnemyController>(prototype, pos, Quaternion.identity);
                enemy.RequestAttack(Target.position);

                if (spawnOnDamageAroundAttacker)
                {
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

        public virtual void OnAliveIsChanged(bool isAlive)
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

            OnDeath();
            onDeath?.Invoke(this);

            if (enemyKillsStat != null)
            {
                enemyKillsStat.Add(1);
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

#if UNITY_EDITOR
        [Button]
        void UpdateIconsFromShowcase()
        {
            string[] guids = UnityEditor.AssetDatabase.FindAssets($"t:Texture {displayName}_");
            if (guids.Length == 0)
            {
                return;
            }

            icon = new Sprite[guids.Length];
            for (int i = 0; i < guids.Length; i++)
            {
                string assetPath = UnityEditor.AssetDatabase.GUIDToAssetPath(guids[i]);
                icon[i] = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
            }
        }

        public bool Validate()
        {
            return IsValid(out string message, out Component component);
        }

        public virtual bool IsValid(out string message, out Component component)
        {
            message = string.Empty;
            component = null;

            if (causeDamageOnDeath && deathExplosionRadius < 0.1f)
            {
                component = this;
                message = "Death explosion radius is too small.";
                return false;
            }
            if (causeDamageOnDeath && explosionDamageOnDeath < 1)
            {
                component = this;
                message = "Explosion damage on death is too small.";
                return false;
            }
            if (causeDamageOnDeath && explosionForceOnDeath < 1)
            {
                component = this;
                message = "Explosion force on death is too small.";
                return false;
            }
            if (deathJuicePrefab == null)
            {
                component = this;
                message = "No death juice prefab defined.";
                return false;
            }
            if (deathClips.Length == 0)
            {
                component = this;
                message = "No death audio clips defined.";
                return false;
            }
            if (spawnOnDamageEnabled && spawnOnDamagePrototypes.Length == 0)
            {
                component = this;
                message = "Spawn On Damage is enabled, but no spawn on damage prototypes defined.";
                return false;
            }

            BasicWeaponBehaviour[] weapons = GetComponentsInChildren<BasicWeaponBehaviour>();
            foreach (BasicWeaponBehaviour weapon in weapons)
            {
                if (!weapon.IsValid(out message))
                {
                    component = weapon;
                    return false;
                }
            }

            return true;
        }

#endif
    }
}
