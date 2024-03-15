using NaughtyAttributes;
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
    /// <seealso cref="LevelDefinition"/>
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

        private int currentEnemyIndex = 0;
        private WeightedRandom<EnemySpawnConfiguration> weightedEnemies;

        public void Reset()
        {
            currentEnemyIndex = 0;
            weightedEnemies = new WeightedRandom<EnemySpawnConfiguration>();
            foreach (var enemy in enemies)
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
            Reset();
        }

        public PooledObject GetNextEnemy()
        {
            if (weightedEnemies == null)
            {
                Reset();
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