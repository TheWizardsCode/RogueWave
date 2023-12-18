using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Playground
{
    public class Spawner : MonoBehaviour
    {
        [SerializeField, Tooltip("The enemy prefabs to spawn.")]
        private GameObject[] enemyPrefabs;
        [SerializeField, Tooltip("The number of enemies to spawn to spawn each second.")]
        private float spawnRate = 1f;
        [SerializeField, Tooltip("The radius around the spawner to spawn enemies.")]
        private float spawnRadius = 10f;
        [SerializeField, Tooltip("The duration of each spawn wave in seconds.")]
        private float waveDuration = 10f;
        [SerializeField, Tooltip("The duration of the wait between each spawn wave in seconds.")]
        private float waveWait = 5f;

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
            Instantiate(enemyPrefabs[Random.Range(0, enemyPrefabs.Length)], spawnPosition, Quaternion.identity);
        }
    }
}