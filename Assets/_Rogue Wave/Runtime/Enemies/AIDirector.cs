using NaughtyAttributes;
using NeoFPS;
using NeoFPS.SinglePlayer;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static RogueWave.BasicEnemyController;
using Random = UnityEngine.Random;

namespace RogueWave
{
    /// <summary>
    /// The AI director is responsible for coordinating AI attacks.
    /// It manages the cycle of the game, ensuring the player gets some moments of respite, as well as some moments of intense action.
    /// </summary>
    public class AIDirector : MonoBehaviour
    {
        [Required, SerializeField, Tooltip("The level generator which will be used to create levels for the game.")]
        LevelGenerator levelGenerator;
        [SerializeField, Tooltip("The maximum time that the AI director will wait between reports of the players location. If no reports are made within this time the AI director will instruct a spawner to create a scannet.")]
        float maximumTimeBetweenReports = 30;
        [Required, SerializeField, Tooltip("The Scanner prefab that will be spawned when the AI director detects that the player has not been reported for a while.")]
        ScannerController scannerPrefab;
        [SerializeField, Tooltip("The length of a time slice. The AI director will evaluate the current state of play every timeSlice seconds. Based on this evaluation the AI will issue orders to the Enemies.")]
        float timeSlice = 50f;

        [Header("Difficulty")]
        [SerializeField, Tooltip("The target kill score, which is the total challenge rating of all the enemies killed in the last `timeSlice`, divided by the `timeslice`. " +
            "When the AI director detects that the current kill rate is below this value it will send more enemies to the player in order to pressure player."), CurveRange(0, 0.3f, 99, 10, EColor.Red)]
        private AnimationCurve targetSkillScoreByLevel;
        [SerializeField, Tooltip("A multiplier to the challenge rating of enemeies that will be sent when the player is below the target kill score. The game difficulty setting will be used to read from this curve.")]
        private AnimationCurve challengeRatingMultiplierByDifficulty = AnimationCurve.Linear(0,0,1f,1f);

        List<Spawner> spawners = new List<Spawner>();
        internal List<BasicEnemyController> enemies = new List<BasicEnemyController>();
        Dictionary<BasicEnemyController, List<BasicEnemyController>> squads = new Dictionary<BasicEnemyController, List<BasicEnemyController>>();
        List<Vector3> reportedLocations = new List<Vector3>();
        List<KillReport> killReports = new List<KillReport>();

        float timeOfLastPlayerLocationReport = 0;
        float timeOfNextTimeSlice = 0;
        float currentKillscore = 0; // the current value of the total challenge rating of enemies killed in the last timeSlice
        private BasicEnemyController enemyController;
        private RogueWaveGameMode gameMode;

        private static AIDirector _instance;
        public static AIDirector Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<AIDirector>();

                    if (_instance == null)
                    {
                        GameObject singletonObject = new GameObject();
                        _instance = singletonObject.AddComponent<AIDirector>();
                        singletonObject.name = typeof(AIDirector).ToString() + " (Created Singleton)";
                    }
                }
                return _instance;
            }
        }

        public int enemyCount => enemies.Count;

        /// <summary>
        /// Returns the suspected location of the player based on the reports made by the enemies.
        /// </summary>
        public Vector3 suspectedTargetLocation
        {
            get
            {
                if (reportedLocations.Count == 0)
                {
                    return Vector3.zero;
                }

                float x = 0f;
                float y = 0f;
                float z = 0f;

                foreach (Vector3 location in reportedLocations)
                {
                    x += location.x;
                    y += location.y;
                    z += location.z;
                }

                return new Vector3(x / reportedLocations.Count, y / reportedLocations.Count, z / reportedLocations.Count);
            }
        }

        protected void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
            }
            else if (_instance != this)
            {
                Destroy(gameObject);
            }

            gameMode = FindObjectOfType<RogueWaveGameMode>();
            gameMode.onSpawnerCreated.AddListener(OnSpawnerCreated);
            gameMode.onEnemySpawned.AddListener(OnEnemySpawned);
        }

        private void Update()
        {
            // If we have not recieves a scanner report assume all scanners are dead and spawn a new one
            if (spawners.Count > 0 && FpsSoloCharacter.localPlayerCharacter != null && Time.timeSinceLevelLoad - timeOfLastPlayerLocationReport > maximumTimeBetweenReports)
            {
                Spawner spawner = spawners[Random.Range(0, spawners.Count)];
                spawner.RequestSpawn(scannerPrefab.transform.root.GetComponent<BasicEnemyController>());
            }

            // Check the player is under pressure, if not send some enemies to attack
            if (Time.timeSinceLevelLoad > timeOfNextTimeSlice)
            {
                timeOfNextTimeSlice = Time.timeSinceLevelLoad + timeSlice;
                float totalChallengeRatingKilled = 0.0f;
                for (int i = killReports.Count - 1; i >= 0; i--)
                {
                    if (killReports[i].time < Time.timeSinceLevelLoad - timeSlice)
                    {
                        break;
                    }
                    totalChallengeRatingKilled += killReports[i].challengeRating;
                }
                currentKillscore = totalChallengeRatingKilled / timeSlice;

                float targetKillScore = targetSkillScoreByLevel.Evaluate(RogueLiteManager.persistentData.currentNanobotLevel);

                int challengeRatingToSend = Mathf.RoundToInt((RogueLiteManager.persistentData.currentNanobotLevel + 1) * targetKillScore * challengeRatingMultiplierByDifficulty.Evaluate(FpsSettings.playstyle.difficulty)) + 1;
                int challengeRatingSent = 0;
                if (currentKillscore < targetKillScore)
                {
                    // Send squads before individual enemies
                    foreach (BasicEnemyController leader in squads.Keys)
                    {
                        if (challengeRatingSent < challengeRatingToSend)
                        {
                            leader.RequestAttack(suspectedTargetLocation);
                            challengeRatingSent += leader.challengeRating;

                            // TODO: Consider moving this into the enemy controller where the individual leader can own the orders for the squad members, thus allowing more varied squad behaviour
                            foreach (BasicEnemyController member in squads[leader])
                            {
                                member.RequestAttack(suspectedTargetLocation);
                                challengeRatingSent += member.challengeRating;
                            }
                        } else
                        {
                            break;
                        }
                    }

                    // if there's still challenge rating to send, send individual enemies
                    if (challengeRatingSent < challengeRatingToSend)
                    {
                        foreach (BasicEnemyController enemy in enemies)
                        {
                            if (enemy != null && enemy.squadLeader != enemy)
                            {
                                enemy.RequestAttack(suspectedTargetLocation);
                                challengeRatingSent += enemy.challengeRating;
                            }

                            if (challengeRatingSent >= challengeRatingToSend) 
                            {
                                break;
                            }
                        }
                    }
                } 
                else
                {
                    GameLog.Info($"AIDirector: The current Kill Score is {currentKillscore} (above targetKillScore of {targetKillScore}).");
                    return;
                }

                GameLog.Info($"AIDirector: Sending existing enemies with a total challenge rating of {challengeRatingSent} to the player as the currentKillScore is {currentKillscore} (targetKillScore is {targetKillScore}).");
                if (challengeRatingSent >= challengeRatingToSend)
                {   
                    return;
                }

                SpawnEnemies(challengeRatingToSend - challengeRatingSent);
            }
        }

        /// <summary>
        /// Spawn enemies to attack the player. They will spawned from the nearest three spawners to the player.
        /// </summary>
        /// <param name="challengeRatingToSpawn">The total challenge rating of enemies to be spawned.</param>
        public void SpawnEnemies(float challengeRatingToSpawn)
        {
            Spawner[] nearestSpawners = GetNearbySpawners(3);
            if (nearestSpawners.Length == 0)
            {
                return;
            }
            
            // Spawn enemies from the nearest spawners until the challenge rating is reached
            int challengeRatingSpawned = 0;
            while (challengeRatingSpawned <= challengeRatingToSpawn)
            {
                BasicEnemyController randomEnemy = spawners[Random.Range(0, spawners.Count)].SpawnEnemy(true);
                if (randomEnemy != null)
                {
                    challengeRatingSpawned += randomEnemy.challengeRating;
                }
                else
                {
                    break;
                }
            }

            GameLog.Info($"AIDirector: Spawned additional enemies with total challenge rating of {challengeRatingSpawned}.\nCurrent currentKillScore is {currentKillscore}.");
        }

        private Spawner[] GetNearbySpawners(int spawnerCount)
        {
            // OPTIMIZATION: cache the results and only recalculate when the player has moved more than 10 units.
            List<Spawner> sortedSpawners = new List<Spawner>(spawners);
            if (FpsSoloCharacter.localPlayerCharacter != null && spawners.Count > spawnerCount)
            {
                sortedSpawners.Sort((a, b) => Vector3.Distance(a.transform.position, FpsSoloCharacter.localPlayerCharacter.transform.position).CompareTo(Vector3.Distance(b.transform.position, FpsSoloCharacter.localPlayerCharacter.transform.position)));
                // remove the spawners that are too far away by discarding all after the spawnerCount
                sortedSpawners.RemoveRange(spawnerCount, sortedSpawners.Count - spawnerCount);
            }

            return sortedSpawners.ToArray();
        }

        private void OnDestroy()
        {
            gameMode.onSpawnerCreated.RemoveListener(OnSpawnerCreated);
            gameMode.onEnemySpawned.RemoveListener(OnEnemySpawned);

            foreach (Spawner spawner in spawners)
            {
                spawner.onSpawnerDestroyed.RemoveListener(OnSpawnerDestroyed);
            }

            foreach (BasicEnemyController enemy in enemies)
            {
                enemy.onDeath.RemoveListener(OnEnemyDeath);
            }
        }

        private void OnSpawnerCreated(Spawner spawner)
        {
#if UNITY_EDITOR
            //Debug.Log($"{spawner.name} created with id {spawner.GetInstanceID()}.");
#endif
            spawners.Add(spawner);
            spawner.onSpawnerDestroyed.AddListener(OnSpawnerDestroyed);
        }

        private void OnSpawnerDestroyed(Spawner spawner)
        {
#if UNITY_EDITOR
            //Debug.Log($"{spawner.name} with id {spawner.GetInstanceID()} destroyed.");
#endif
            spawners.Remove(spawner);
            killReports.Add(new KillReport() { time = Time.timeSinceLevelLoad, challengeRating = spawner.challengeRating, enemyName = spawner.name, location = spawner.transform.position });
        }

        private void OnEnemySpawned(BasicEnemyController enemy)
        {
            if (!enemy.registerWithAIDirector)
            {
                return;
            }
            JoinOrCreateSquad(enemy);

            enemies.Add(enemy);
            enemy.onDeath.AddListener(OnEnemyDeath);
        }

        private void JoinOrCreateSquad(BasicEnemyController enemy)
        {
            if (enemy.squadRole == SquadRole.Leader)
            {
                enemy.squadLeader = enemy;
                squads.Add(enemy, new List<BasicEnemyController>());

                Collider[] colliders = Physics.OverlapSphere(enemy.transform.position, 10, LayerMask.GetMask("Enemy"));
                foreach (Collider collider in colliders)
                {
                    BasicEnemyController otherEnemy = collider.GetComponentInParent<BasicEnemyController>();
                    if (otherEnemy != null && otherEnemy.squadLeader == null && otherEnemy.squadRole != SquadRole.Leader && otherEnemy != enemy)
                    {
                        squads[enemy].Add(otherEnemy);
                        otherEnemy.squadLeader = enemy;
                    }
                }
            }
            else if (enemy.squadRole == SquadRole.Fodder)
            {
                foreach (BasicEnemyController leader in squads.Keys)
                {
                    if (leader.squadRole == SquadRole.Leader && Vector3.Distance(leader.transform.position, enemy.transform.position) < 10)
                    {
                        squads[leader].Add(enemy);
                        enemy.squadLeader = leader;
                        break;
                    }
                }
            }
        }

        private void OnEnemyDeath(BasicEnemyController enemy)
        {
            killReports.Add(new KillReport() { time = Time.timeSinceLevelLoad, challengeRating = enemy.challengeRating, enemyName = enemy.name, location = enemy.transform.position });
            enemy.onDeath.RemoveListener(OnEnemyDeath);

            foreach (BasicEnemyController leader in squads.Keys)
            {
                // OPTIMIZATION: Consider only allowing one leader in a squad, this will allow us to only check for leaders or members as opposed to both
                if (leader == enemy)
                {
                    List<BasicEnemyController> members = new List<BasicEnemyController>();
                    members.AddRange(squads[leader]);
                    squads.Remove(leader);
                    leader.squadLeader = null;
                    foreach (BasicEnemyController member in members)
                    {
                        member.squadLeader = null;
                        JoinOrCreateSquad(member);
                    }
                    break;
                } else if (squads[leader].Contains(enemy))
                {
                    squads[leader].Remove(enemy);
                    enemy.squadLeader = null;
                    break;
                }
            }
            enemies.Remove(enemy);
        }

        internal void ReportPlayerLocation(Vector3 position)
        {
            timeOfLastPlayerLocationReport = Time.timeSinceLevelLoad;

            if (reportedLocations.Count >= 3)
            {
                reportedLocations.RemoveAt(0);
            }
            reportedLocations.Add(position);
        }

        internal BasicEnemyController[] GetSquadMembers(BasicEnemyController squadLeader)
        {
            if (squadLeader != null && squads.TryGetValue(squadLeader, out List<BasicEnemyController> members))
            {
                return members.ToArray();
            } else
            {
                return new BasicEnemyController[0];
            }
        }

        /// <summary>
        /// Immediately kill a percentage of all enemies in the level.
        /// </summary>
        /// <param name="percentage">The percentage, between 0 and 1, of enemies to kill.</param>
        public void Kill(float percentage)
        {
            if (percentage < 0 || percentage > 1)
            {
                GameLog.LogError($"Invalid percentage {percentage} for Kill command. Must be between 0 and 1.");
                return;
            }

            int enemiesToKill = Mathf.RoundToInt(enemies.Count * percentage);
            for (int i = enemies.Count - 1; enemiesToKill > 0; i--)
            {
                enemies[i].healthManager.SetHealth(0, false, null);
                enemiesToKill--;
            }
        }

        /// <summary>
        /// Enable spawning of enemies by turning on the spawners.
        /// </summary>
        public void EnableSpawning()
        {
            foreach (Spawner spawner in spawners)
            {
                spawner.spawningEnabled = true;
            }
        }

        /// <summary>
        /// Enable spawning of enemies by turning on the spawners.
        /// </summary>
        public void DisableSpawning()
        {
            foreach (Spawner spawner in spawners)
            {
                spawner.spawningEnabled = false;
            }
        }

        /// <summary>
        /// Get a list of enemies that can be spawned by the spawners near the player.
        /// </summary>
        public BasicEnemyController[] GetSpawnerAvailableEnemies()
        {
            Spawner[] nearbySpawners = GetNearbySpawners(3);
            if (nearbySpawners.Length == 0)
            {
                return new BasicEnemyController[0];
            }

            List<BasicEnemyController> result = new List<BasicEnemyController>();
            foreach (Spawner spawner in nearbySpawners)
            {
                foreach (EnemySpawnConfiguration spawnConfig in spawner.currentWave.enemies)
                {
                    if (!result.Contains(spawnConfig.pooledEnemyPrefab.GetComponent<BasicEnemyController>()))
                    {
                        result.Add(spawnConfig.pooledEnemyPrefab.GetComponent<BasicEnemyController>());
                    }
                }
            }
            return result.ToArray();
        }

        /// <summary>
        /// Spawn a specific enemy at a spawner near the player.
        /// </summary>
        /// <param name="enemy"></param>
        /// <param name="count"></param>
        /// <exception cref="System.NotImplementedException"></exception>
        public void SpawnEnemiesNearPlayer(BasicEnemyController enemy, int count)
        {
            Spawner[] nearbySpawners = GetNearbySpawners(3);
            for (int i = 0; i < count; i++)
            {
                foreach (Spawner spawner in nearbySpawners)
                {
                    if (spawner.currentWave.enemies.Any(e => e.pooledEnemyPrefab.GetComponent<BasicEnemyController>() == enemy))
                    {
                        if (spawner.SpawnEnemy(enemy, true))
                        {
                            break;
                        }
                    }
                }
            }
        }
    }

    internal struct KillReport
    {
        public float time;
        public float challengeRating;
        public Vector3 location;
        public string enemyName;
    }
}