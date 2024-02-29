using NaughtyAttributes;
using NeoFPS;
using UnityEngine;
using UnityEngine.Serialization;

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
            Random,
            Sequential
        }

        [SerializeField, Tooltip("The enemy prefabs to spawn.")]
        private PooledObject[] pooledEnemyPrefabs;
        public BasicEnemyController[] enemyPrefabs;
        [SerializeField, Range(1f, 99f), Tooltip("The number of seconds between enemy spawns. The number of enemies spawned each cycle is defined by spawnAmount.")]
        private float spawnRate = 5f;
        [SerializeField, Range(1, 99), Tooltip("The number of enemies to spawn every rate interval."), FormerlySerializedAs("spawnAmount")]
        private int numberToSpawn = 3;
        [SerializeField, Tooltip("The duration of each spawn wave in seconds. The spawner will continue to spawn enemies for this many seconds.")]
        private float waveDuration = 10f;
        [SerializeField, Tooltip("The order in which to spawn enemies.")] 
        private SpawnOrder spawnOrder = SpawnOrder.Random;

        public PooledObject[] EnemyPrefabs => pooledEnemyPrefabs;
        public float SpawnRate => spawnRate;
        public int SpawnAmount => numberToSpawn;
        public float WaveDuration => waveDuration;
        public SpawnOrder Order => spawnOrder;

        private int currentEnemyIndex = 0;

        public void Reset()
        {
            currentEnemyIndex = 0;
        }

        public void Init(PooledObject[] enemyPrefabs, float spawnRate, float waveDuration, SpawnOrder spawnOrder)
        {
            this.pooledEnemyPrefabs = enemyPrefabs;
            this.spawnRate = spawnRate;
            this.waveDuration = waveDuration;
            this.spawnOrder = spawnOrder;
            Reset();
        }

        public PooledObject GetNextEnemy()
        {
            if (pooledEnemyPrefabs.Length == 0)
                return null;

            if (spawnOrder == SpawnOrder.Random)
                return pooledEnemyPrefabs[Random.Range(0, pooledEnemyPrefabs.Length)];

            return pooledEnemyPrefabs[currentEnemyIndex++ % pooledEnemyPrefabs.Length];
        }
    }
}