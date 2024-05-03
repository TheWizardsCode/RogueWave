using NaughtyAttributes;
using NeoFPS.SinglePlayer;
using RogueWave;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using Debug = UnityEngine.Debug;
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
        [SerializeField, Tooltip("The target kill score, which is the total challenge rating of all the enemies killed in the last `timeSlice`, divided by the `timeslice`. " +
            "When the AI director detects that the current kill rate is below this value it will send more enemies to the player in order to pressure player."), CurveRange(0, 0.3f, 99, 10, EColor.Red)]
        private AnimationCurve targetSkillScoreByLevel;
        [SerializeField, Tooltip("The size of the attack squad. This is the number of enemies that will be sent from the main spawners to attack the player when the AI director detects that the player is hiding. Note that additional Enemies will be spawned from proximity spawners.")]
        private int sizeOfAttackSquad = 5;
        [SerializeField, Tooltip("The difficulty multiplier. This is used to increase the difficulty of the game as the player. It impacts things like the total challenge rating of squads sent to attack a hiding player.")]
        float difficultyMultiplier = 4f;


        [SerializeField, Tooltip("Turn on debug features for the AI Director"), Foldout("Debug")]
        bool isDebug = false;

        List<Spawner> spawners = new List<Spawner>();
        List<BasicEnemyController> enemies = new List<BasicEnemyController>();
        List<Vector3> reportedLocations = new List<Vector3>();
        List<KillReport> killReports = new List<KillReport>();

        float timeOfLastPlayerLocationReport = 0;
        float timeOfNextTimeSlice = 0;
        float currentKillscore = 0; // the current value of the total challenge rating of enemies killed in the last timeSlice
        private BasicEnemyController enemyController;
        private RogueWaveGameMode gameMode;

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
            gameMode = FindObjectOfType<RogueWaveGameMode>();
            gameMode.onSpawnerCreated.AddListener(OnSpawnerCreated);
            gameMode.onEnemySpawned.AddListener(OnEnemySpawned);
        }

        private void Update()
        {
            if (spawners.Count > 0 && FpsSoloCharacter.localPlayerCharacter != null && Time.timeSinceLevelLoad - timeOfLastPlayerLocationReport > maximumTimeBetweenReports)
            {
                Spawner spawner = spawners[Random.Range(0, spawners.Count)];
                spawner.RequestSpawn(scannerPrefab.transform.root.GetComponent<BasicEnemyController>());
            }

            if (Time.timeSinceLevelLoad > timeOfNextTimeSlice)
            {
                timeOfNextTimeSlice = Time.timeSinceLevelLoad + timeSlice;
                float totalChallengeRating = 0.0f;
                for (int i = killReports.Count - 1; i >= 0; i--)
                {
                    if (killReports[i].time < Time.timeSinceLevelLoad - timeSlice)
                    {
                        break;
                    }
                    totalChallengeRating += killReports[i].challengeRating;
                }
                currentKillscore = totalChallengeRating / timeSlice;

                float targetKillScore = targetSkillScoreByLevel.Evaluate(RogueLiteManager.persistentData.currentNanobotLevel);

                int challengeRatingToSend = Mathf.RoundToInt((RogueLiteManager.persistentData.currentNanobotLevel + 1) * targetKillScore * difficultyMultiplier);
                int challengeRatingSent = 0;
                int orderedEnemies = 0;
                if (enemies.Count > 0 && currentKillscore < targetKillScore)
                {
                    while (orderedEnemies <= enemies.Count 
                        && orderedEnemies <= sizeOfAttackSquad
                        && enemies.Count <= levelGenerator.levelDefinition.maxAlive 
                        && challengeRatingSent < challengeRatingToSend)
                    {
                        orderedEnemies++;
                        BasicEnemyController randomEnemy = enemies[Random.Range(0, enemies.Count)];
                        challengeRatingSent += randomEnemy.challengeRating;
                        randomEnemy.RequestAttack(suspectedTargetLocation);
                    }

                    GameLog.Info($"AIDirector: Sending enemies with a total challenge rating of {challengeRatingSent} to the player as the currentKillScore is {currentKillscore} (targetKillScore is {targetKillScore}).");
                } 
                else
                {
                    GameLog.Info($"AIDirector: The current Kill Score is {currentKillscore} (targetKillScore is {targetKillScore}).");
                    return;
                }

                if (challengeRatingSent <= 0)
                {
                    return;
                }

                // Select the nearest 3 spawners to the player
                Spawner[] nearestSpawners = null;
                if (spawners.Count > 3) {
                    nearestSpawners = new Spawner[3];
                    List<Spawner> sortedSpawners = new List<Spawner>(spawners);
                    sortedSpawners.Sort((a, b) => Vector3.Distance(a.transform.position, FpsSoloCharacter.localPlayerCharacter.transform.position).CompareTo(Vector3.Distance(b.transform.position, FpsSoloCharacter.localPlayerCharacter.transform.position)));
                    for (int i = 0; i < 3; i++)
                    {
                        nearestSpawners[i] = sortedSpawners[i];
                    }
                }
                else if (spawners.Count > 0)
                {
                    nearestSpawners = spawners.ToArray();
                } 
                else
                {
                    return;
                }

                // Send remaining enemies from the nearest spawners to the player
                int challengeRatingSpawned = 0;
                while (challengeRatingSpawned + challengeRatingSent < challengeRatingToSend
                        && enemies.Count <= levelGenerator.levelDefinition.maxAlive + difficultyMultiplier)
                {
                    BasicEnemyController randomEnemy = spawners[Random.Range(0, spawners.Count)].SpawnEnemy();
                    if (randomEnemy != null)
                    {
                        challengeRatingSpawned += randomEnemy.challengeRating;
                    } else
                    {
                        break;
                    }
                }

                GameLog.Info($"AIDirector: Spawned additional enemies with total challenge rating of {challengeRatingSpawned} as the currentKillScore is {currentKillscore} (targetKillScore is {targetKillScore}).");
            }
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
            spawners.Add(spawner);
            spawner.onSpawnerDestroyed.AddListener(OnSpawnerDestroyed);
        }

        private void OnSpawnerDestroyed(Spawner spawner)
        {
            spawners.Remove(spawner);
            killReports.Add(new KillReport() { time = Time.timeSinceLevelLoad, challengeRating = spawner.challengeRating, enemyName = spawner.name, location = spawner.transform.position });
        }

        private void OnEnemySpawned(BasicEnemyController enemy)
        {
            enemies.Add(enemy);
            enemy.onDeath.AddListener(OnEnemyDeath);
        }

        private void OnEnemyDeath(BasicEnemyController enemy)
        {
            killReports.Add(new KillReport() { time = Time.timeSinceLevelLoad, challengeRating = enemy.challengeRating, enemyName = enemy.name, location = enemy.transform.position });
            enemy.onDeath.RemoveListener(OnEnemyDeath);
            enemies.Remove(enemy);
        }

        internal void ReportPlayerLocation(Vector3 position, float precision)
        {
            timeOfLastPlayerLocationReport = Time.timeSinceLevelLoad;

            if (precision == 0)
            {
                reportedLocations.Clear();
                reportedLocations.Add(position);
            }
            else
            {
                if (reportedLocations.Count >= 3)
                {
                    reportedLocations.RemoveAt(0);
                }
                reportedLocations.Add(position);
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