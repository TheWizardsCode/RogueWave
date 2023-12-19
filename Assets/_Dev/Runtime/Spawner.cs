using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Playground
{
    public class Spawner : MonoBehaviour
    {
        [Header("Wave Definition")]
        [SerializeField, Tooltip("The enemy prefabs to spawn.")]
        private EnemyController[] enemyPrefabs;
        [SerializeField, Tooltip("The number of enemies to spawn to spawn each second.")]
        private float spawnRate = 1f;
        [SerializeField, Tooltip("The radius around the spawner to spawn enemies.")]
        internal float spawnRadius = 5f;
        [SerializeField, Tooltip("The duration of each spawn wave in seconds.")]
        private float waveDuration = 10f;
        [SerializeField, Tooltip("The duration of the wait between each spawn wave in seconds.")]
        private float waveWait = 5f;

        [Header("Events")]
        [SerializeField, Tooltip("The event to trigger when this spawner is destroyed.")]
        public UnityEvent onDestroyed;

        List<EnemyController> spawnedEnemies = new List<EnemyController>();

        private void Start()
        {
            StartCoroutine(SpawnEnemies());
        }

        private IEnumerator SpawnEnemies()
        {
            while (true)
            {
                for (int i = 0; i < spawnRate * waveDuration; i++)
                {
                    SpawnEnemy();
                    yield return new WaitForSeconds(1f / spawnRate);
                }
                yield return new WaitForSeconds(waveWait);
            }
        }

        private void SpawnEnemy()
        {
            Vector3 spawnPosition = transform.position + Random.insideUnitSphere * spawnRadius;
            EnemyController enemy = Instantiate(enemyPrefabs[Random.Range(0, enemyPrefabs.Length)], spawnPosition, Quaternion.identity);
            spawnedEnemies.Add(enemy);
        }

        public void OnAliveIsChanged(bool isAlive)
        {
            if (isAlive)
            {
                StartCoroutine(SpawnEnemies());
            }
            else
            {
                StopCoroutine(SpawnEnemies());
                onDestroyed?.Invoke();

                for (int i  = 0; i < spawnedEnemies.Count; i++)
                {
                    if (spawnedEnemies[i] != null) {
                        Destroy(spawnedEnemies[i].gameObject);
                    }
                }

                Destroy(gameObject);
            }
        }
    }
}