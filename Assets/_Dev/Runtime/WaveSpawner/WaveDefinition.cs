using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Playground
{
    [CreateAssetMenu(fileName = "WaveDefinition", menuName = "Playground/Wave Definition")]
    public class WaveDefinition : ScriptableObject
    {
        public enum SpawnOrder
        {
            Random,
            Sequential
        }

        [SerializeField, Tooltip("The enemy prefabs to spawn.")]
        private BasicEnemyController[] enemyPrefabs;
        [SerializeField, Tooltip("The rate at which to spawn enemies.")]
        private float spawnRate = 1f;
        [SerializeField, Tooltip("The number of enemies to spawn every rate interval.")]
        private int spawnAmount = 1;
        [SerializeField, Tooltip("The duration of each spawn wave in seconds.")]
        private float waveDuration = 10f;
        [SerializeField, Tooltip("The order in which to spawn enemies.")] 
        private SpawnOrder spawnOrder = SpawnOrder.Random;

        public BasicEnemyController[] EnemyPrefabs => enemyPrefabs;
        public float SpawnRate => spawnRate;
        public int SpawnAmount => spawnAmount;
        public float WaveDuration => waveDuration;
        public SpawnOrder Order => spawnOrder;

        private int currentEnemyIndex = 0;

        public void Reset()
        {
            currentEnemyIndex = 0;
        }

        public void Init(BasicEnemyController[] enemyPrefabs, float spawnRate, float waveDuration, SpawnOrder spawnOrder)
        {
            this.enemyPrefabs = enemyPrefabs;
            this.spawnRate = spawnRate;
            this.waveDuration = waveDuration;
            this.spawnOrder = spawnOrder;
            Reset();
        }

        public BasicEnemyController GetNextEnemy()
        {
            if (enemyPrefabs.Length == 0)
                return null;

            if (spawnOrder == SpawnOrder.Random)
                return enemyPrefabs[Random.Range(0, enemyPrefabs.Length)];

            return enemyPrefabs[currentEnemyIndex++ % enemyPrefabs.Length];
        }
    }
}