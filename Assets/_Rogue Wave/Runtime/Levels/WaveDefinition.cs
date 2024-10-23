using NaughtyAttributes;
using NeoFPS;
using NUnit.Framework.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

namespace RogueWave
{
    /// <summary>
    /// Defines a single wave of enemies to spawn. Each level is madde of one or more waves.
    /// </summary>
    /// <seealso cref="WfcDefinition"/>
    /// <see cref="Spawner"/>
    [CreateAssetMenu(fileName = "WaveDefinition", menuName = "Rogue Wave/Wave Definition", order = 201)]
    public class WaveDefinition : ScriptableObject
    {
        public enum SpawnOrder
        {
            WeightedRandom,
            Sequential
        }

        [SerializeField, Tooltip("The enemy prefabs to spawn."), OnValueChanged("UpdateCR")]
        internal EnemySpawnConfiguration[] enemies;
        [SerializeField, Range(1f, 99f), Tooltip("The time between spawn events."), FormerlySerializedAs("spawnRate"), OnValueChanged("UpdateCR")]
        private float spawnEventFrequency = 5f;
        [SerializeField, Range(1, 99), Tooltip("The number of enemies to spawn during a spawn event."), FormerlySerializedAs("spawnAmount"), OnValueChanged("UpdateCR")]
        private int numberToSpawn = 3;
        [SerializeField, Tooltip("The duration of this wave in seconds. The spawner will continue to spawn enemies for this many seconds."), OnValueChanged("UpdateCR")]
        private float waveDuration = 10f;
        [SerializeField, Tooltip("The order in which to spawn enemies.")] 
        private SpawnOrder spawnOrder = SpawnOrder.WeightedRandom;

        [ReadOnly, SerializeField, Tooltip("The Challenge Rating for this wave. The higher this value the harder the wave will be to complete.")]
        private int challengeRating = 0;

        public EnemySpawnConfiguration[] Enemies => enemies;
        public float SpawnEventFrequency => spawnEventFrequency;
        public int SpawnAmount => numberToSpawn;
        public float WaveDuration => waveDuration;
        public SpawnOrder Order => spawnOrder;

        /// <summary>
        /// Get the Challenge Rating for this level. The higher this value
        /// the harder the wave will be to complete.
        /// </summary>
        public int ChallengeRating { 
            get
            {
#if UNITY_EDITOR
                float totalWeight = 0;
                foreach (EnemySpawnConfiguration spawnConfig in enemies)
                {
                    totalWeight += spawnConfig.baseWeight;
                }

                float floatCR = 0;
                foreach (EnemySpawnConfiguration spawnConfig in enemies)
                {
                    BasicEnemyController enemy = spawnConfig.pooledEnemyPrefab.GetComponent<BasicEnemyController>();
                    floatCR += enemy.challengeRating * (spawnConfig.baseWeight / totalWeight);
                }
                floatCR /= enemies.Length;

                challengeRating = Mathf.RoundToInt(floatCR * numberToSpawn * (waveDuration / spawnEventFrequency));
#endif

                return challengeRating;
            } 
        }

        private int currentEnemyIndex = 0;
        private WeightedRandom<EnemySpawnConfiguration> weightedEnemies;

        public void Init()
        {
            currentEnemyIndex = 0;
            weightedEnemies = new WeightedRandom<EnemySpawnConfiguration>();
            foreach (EnemySpawnConfiguration enemy in enemies)
            {
                weightedEnemies.Add(enemy, enemy.baseWeight);
            }
        }

        public void Init(EnemySpawnConfiguration[] enemies, float spawnRate, float waveDuration, SpawnOrder spawnOrder)
        {
            this.enemies = enemies;
            this.spawnEventFrequency = spawnRate;
            this.waveDuration = waveDuration;
            this.spawnOrder = spawnOrder;
            Init();
        }

        public PooledObject GetNextEnemy()
        {
            if (weightedEnemies == null)
            {
                Init();
            }

            if (spawnOrder == SpawnOrder.WeightedRandom)
            {
                return weightedEnemies.GetRandom().pooledEnemyPrefab;
            }
            else
            {
                return enemies[currentEnemyIndex++ % enemies.Length].pooledEnemyPrefab;
            }
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            foreach (var enemy in enemies)
            {
                if (enemy.baseWeight == 0)
                {
                    enemy.baseWeight = 0.5f;
                }
                if (enemy.baseWeight < 0.01f)
                {
                    enemy.baseWeight = 0.01f;
                }
            }

            if (targetChallengeRating <= 0)
            {
                if (challengeRating == 0)
                {
                    targetChallengeRating = 15;
                } else
                {
                    targetChallengeRating = challengeRating;
                }
            }
        }

        [Button]
        private void UpdateCR()
        {
            challengeRating = ChallengeRating; // force a recalculation of the challenge rating
        }

        [HorizontalLine(color: EColor.Blue)]

        [SerializeField, Tooltip("The targetchallenge rating for this wave. This value is used to randomize the enemies in the wave when the button below is clicked.")]
        int targetChallengeRating = 0;

        [SerializeField, Tooltip("When balancing the wave with the 'Balance Wave' button below, should the enemy types be randomized? If this is false only the rate of spawning and duration will be adjusted.")]
        bool randomizeEnemyTypes = true;

        private List<BasicEnemyController> availableEnemies = new List<BasicEnemyController>();

        public void GenerateWave(int targetChallengeRating, bool randomizeEnemyTypes, BasicEnemyController[] availableEnemies)
        {
            this.targetChallengeRating = targetChallengeRating;
            this.randomizeEnemyTypes = randomizeEnemyTypes;
            this.availableEnemies = availableEnemies.ToList<BasicEnemyController>();
            BalanceWave(true);
        }

        [Button()]
        private void BalanceWave(bool forceTargetUpdate = false)
        {
            if (targetChallengeRating <= 0)
            {
                EditorUtility.DisplayDialog("Error", "Target Challenge Rating, entered above, must be greater than 0.", "OK");
                return;
            }

            if (randomizeEnemyTypes)
            {
                if (availableEnemies.Count == 0)
                {
                    LoadAllEnemies();
                }

                // Randomize the enemies
                if (enemies == null || enemies.Length == 0 || enemies.Length > availableEnemies.Count)
                {
                    enemies = new EnemySpawnConfiguration[Random.Range(Mathf.Min(availableEnemies.Count, 3), Mathf.Min(availableEnemies.Count, 5))];
                }

                for (int i = 0; i < enemies.Length; i++)
                {
                    int candidateIndex = Random.Range(0, availableEnemies.Count);
                    if (enemies[i] == null)
                    {
                        enemies[i] = new EnemySpawnConfiguration();
                    }
                    enemies[i].pooledEnemyPrefab = availableEnemies[candidateIndex];
                    enemies[i].baseWeight = Random.Range(0.01f, 1f);

                    availableEnemies.RemoveAt(candidateIndex);
                }
            }

            // adjust the spawn rate, numbers and duration to hit the desired challenge rating
            spawnEventFrequency = Random.Range(1.0f, 4.0f);
            numberToSpawn = Random.Range(2, 15);
            int iterations = 5000;
            int currentChallengeRating = ChallengeRating;
            while (currentChallengeRating != targetChallengeRating && iterations > 0)
            {
                iterations--;

                int parameterToChange = Random.Range(0, 2);
                switch (parameterToChange)
                {
                    case 0:
                        if (currentChallengeRating < targetChallengeRating)
                        {
                            spawnEventFrequency = Mathf.Clamp(spawnEventFrequency - 0.25f, 1, float.MaxValue);
                        }
                        else
                        {
                            spawnEventFrequency += 0.25f;
                        }
                        break;
                    case 1:
                        if (currentChallengeRating < targetChallengeRating)
                        {
                            numberToSpawn += 1;
                        }
                        else
                        {
                            numberToSpawn = Mathf.Clamp(numberToSpawn - 1, 1, int.MaxValue);
                        }
                        break;
                    //case 2:
                    //    if (currentChallengeRating < targetChallengeRating)
                    //    {
                    //        waveDuration += 0.25f;
                    //    }
                    //    else
                    //    {
                    //        waveDuration -= 0.25f;
                    //    }
                    //    break;
                }

                if (iterations == 0)
                {
                    if (forceTargetUpdate)
                    {
                        targetChallengeRating = ChallengeRating;
                    } 
                    else if (EditorUtility.DisplayDialog("Error", "Failed to fully balance wave the wave. What would you like to do?", $"Make current CR of {ChallengeRating} the Target", "Adjust the target and try again"))
                    {
                        targetChallengeRating = ChallengeRating;
                    }
                }


                currentChallengeRating = ChallengeRating;
                availableEnemies.Clear();
            }
        }

        private void LoadAllEnemies()
        {
            string[] guids = AssetDatabase.FindAssets("t:Prefab");
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                GameObject obj = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                BasicEnemyController controller = obj.GetComponent<BasicEnemyController>();
                if (controller != null && controller.isAvailableToWaveDefinitions)
                {
                    availableEnemies.Add(obj.GetComponent<BasicEnemyController>());
                }
            }
        }
#endif
    }

    [Serializable]
    public class EnemySpawnConfiguration
    {
        [SerializeField, Tooltip("The enemy prefab to spawn.")]
        internal PooledObject pooledEnemyPrefab;
        [SerializeField, Range(0.01f, 1f), Tooltip("The weight of this enemy in the spawn pool. The higher the weight, the more likely it is to be spawned.")]
        internal float baseWeight = 0.5f;
    }
}