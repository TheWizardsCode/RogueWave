using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Playground
{
    public class Spawner : MonoBehaviour
    {
        [Header("Wave Definition")]
        [SerializeField, Tooltip("The list of defined waves used for spawning.")]
        private WaveDefinition[] waves;
        [SerializeField, Tooltip("The radius around the spawner to spawn enemies.")]
        internal float spawnRadius = 5f;
        [SerializeField, Tooltip("The duration of the wait between each spawn wave in seconds.")]
        private float waveWait = 5f;
        [SerializeField, Tooltip("When defined waves are exhausted, generate more?.")]
        private bool generateWaves = true;

        [Header("Events")]
        [SerializeField, Tooltip("The event to trigger when this spawner is destroyed.")]
        public UnityEvent onDestroyed;
        [SerializeField, Tooltip("The event to trigger when all waves are complete.")]
        public UnityEvent onAllWavesComplete;

        List<BasicEnemyController> spawnedEnemies = new List<BasicEnemyController>();

        private int currentWaveIndex = -1;

        private WaveDefinition currentWave;

        private void Start()
        {
            StartCoroutine(SpawnWaves());
        }

        private void NextWave()
        {
            currentWaveIndex++;
            if (currentWaveIndex >= waves.Length)
            {
                onAllWavesComplete?.Invoke();
                if (!generateWaves)
                {
                    Debug.LogWarning("No more waves to spawn.");
                    currentWave = null;
                    StopCoroutine(SpawnWaves());
                    return;
                }
                currentWave = GenerateNewWave();
                return;
            }
            Debug.Log($"Starting wave {currentWaveIndex + 1} of {waves.Length}...");
            currentWave = waves[currentWaveIndex];
            currentWave.Reset();
        }

        private WaveDefinition GenerateNewWave()
        {
            Debug.Log("Generating new wave...");
            WaveDefinition newWave = ScriptableObject.CreateInstance<WaveDefinition>();
            // populate newWave with random values based loosely on the previous wave
            var lastWave = currentWave;
            if (lastWave == null)
            {
                lastWave = waves[waves.Length - 1];
            }
            // collect all enemy prefabs from all waves and then randomly select from them
            List<BasicEnemyController> enemyPrefabs = new List<BasicEnemyController>();
            for (int i = 0; i < waves.Length; i++)
            {
                enemyPrefabs.AddRange(waves[i].EnemyPrefabs);
            }
            newWave.Init(
                enemyPrefabs.ToArray(),
                Mathf.Max(lastWave.SpawnRate - 0.1f, 0.1f), // faster!
                lastWave.WaveDuration + Random.Range(1f, 5f), // longer!
                WaveDefinition.SpawnOrder.Random
            );
            return newWave;
        }

        private IEnumerator SpawnWaves()
        {
            NextWave();
            while (currentWave != null)
            {
                float waveStart = Time.time;
                while (Time.time - waveStart < currentWave.WaveDuration)
                {
                    yield return new WaitForSeconds(currentWave.SpawnRate);
                    for (int i = 0; i < currentWave.SpawnAmount; i++)
                        SpawnEnemy();
                }
                yield return new WaitForSeconds(waveWait);
                NextWave();
            }
        }

        private void SpawnEnemy()
        {
            Vector3 spawnPosition = transform.position + Random.insideUnitSphere * spawnRadius;
            var prefab = currentWave.GetNextEnemy();
            if (prefab == null)
            {
                Debug.LogError("No enemy prefab found in wave definition.");
                return;
            }
            BasicEnemyController enemy = Instantiate(prefab, spawnPosition, Quaternion.identity);
            spawnedEnemies.Add(enemy);
        }

        public void OnAliveIsChanged(bool isAlive)
        {
            if (isAlive)
            {
                StartCoroutine(SpawnWaves());
            }
            else
            {
                StopCoroutine(SpawnWaves());
                onDestroyed?.Invoke();

                for (int i = 0; i < spawnedEnemies.Count; i++)
                {
                    if (spawnedEnemies[i] != null)
                    {
                        Destroy(spawnedEnemies[i].gameObject);
                    }
                }

                Destroy(gameObject);
            }
        }
    }
}