using NeoFPS;
using System;
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

        [SerializeField, Tooltip("The enemy prefabs to spawn.")]
        internal EnemySpawnConfiguration[] enemies;
        [SerializeField, Range(1f, 99f), Tooltip("The time between spawn events."), FormerlySerializedAs("spawnRate")]
        private float spawnEventFrequency = 5f;
        [SerializeField, Range(1, 99), Tooltip("The number of enemies to spawn during a spawn event."), FormerlySerializedAs("spawnAmount")]
        private int numberToSpawn = 3;
        [SerializeField, Tooltip("The duration of this wave in seconds. The spawner will continue to spawn enemies for this many seconds.")]
        private float waveDuration = 10f;
        [SerializeField, Tooltip("The order in which to spawn enemies.")] 
        private SpawnOrder spawnOrder = SpawnOrder.WeightedRandom;

        public EnemySpawnConfiguration[] Enemies => enemies;
        public float SpawnEventFrequency => spawnEventFrequency;
        public int SpawnAmount => numberToSpawn;
        public float WaveDuration => waveDuration;
        public SpawnOrder Order => spawnOrder;

        /// <summary>
        /// Get the Challenge Rating for this level. The higher this value
        /// the harder the wave will be to complete.
        /// </summary>
        public int CR { 
            get
            {
                int cr = 0;
                foreach (EnemySpawnConfiguration spawnConfig in enemies)
                {
                    BasicEnemyController enemy = spawnConfig.pooledEnemyPrefab.GetComponent<BasicEnemyController>();
                    cr += enemy.challengeRating;
                }
                cr /= enemies.Length;

                cr *= Mathf.RoundToInt(numberToSpawn * (waveDuration / spawnEventFrequency));

                return cr;
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