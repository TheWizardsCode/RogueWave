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
                challengeRating = 0;
                foreach (EnemySpawnConfiguration spawnConfig in enemies)
                {
                    BasicEnemyController enemy = spawnConfig.pooledEnemyPrefab.GetComponent<BasicEnemyController>();
                    challengeRating += Mathf.Clamp(Mathf.RoundToInt(enemy.challengeRating * spawnConfig.baseWeight), 1, int.MaxValue);
                }
                challengeRating /= enemies.Length;

                challengeRating *= Mathf.RoundToInt(numberToSpawn * (waveDuration / spawnEventFrequency));
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
        }

        private void UpdateCR()
        {
            challengeRating = ChallengeRating;
        }

        [HorizontalLine(color: EColor.Blue)]

        [SerializeField, Tooltip("The targetchallenge rating for this wave. This value is used to randomize the enemies in the wave when the button below is clicked.")]
        int targetChallengeRating = 0;

        [Button()]
        private void RandomizeWave()
        {
            if (targetChallengeRating <= 0)
            {
                EditorUtility.DisplayDialog("Error", "Target Challenge Rating, entered above, must be greater than 0.", "OK");
                return;
            }

            // get all the BaseEnemyController prefabs in the project
            List<PooledObject> enemyCandidates = new List<PooledObject>();
            string[] guids = AssetDatabase.FindAssets("t:Prefab");
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                GameObject obj = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                BasicEnemyController controller = obj.GetComponent<BasicEnemyController>();
                if (controller != null && controller.isAvailableToWaveDefinitions)
                {
                    enemyCandidates.Add(obj.GetComponent<PooledObject>());
                }
            }

            // Randomize the enemies
            for(int i = 0; i < enemies.Length; i++)
            {
                int candidateIndex = Random.Range(0, enemyCandidates.Count);
                enemies[i].pooledEnemyPrefab = enemyCandidates[candidateIndex];
                enemies[i].baseWeight = Random.Range(0.01f, 1f);

                enemyCandidates.RemoveAt(candidateIndex);
            }

            // adjust the spawn rate, numbers and duration to hit the desired challenge rating
            int currentChallengeRating = ChallengeRating;
            while (currentChallengeRating != targetChallengeRating)
            {
                int parameterToChange = Random.Range(0, 3);
                switch (parameterToChange)
                {
                    case 0:
                        if (currentChallengeRating < targetChallengeRating)
                        {
                            spawnEventFrequency -= 0.25f;
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
                            numberToSpawn -= 1;
                        }
                        break;
                    case 2:
                        if (currentChallengeRating < targetChallengeRating)
                        {
                            waveDuration += 0.25f;
                        }
                        else
                        {
                            waveDuration -= 0.25f;
                        }
                        break;
                }

                currentChallengeRating = ChallengeRating;
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