using NeoFPS;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Playground
{
    public class Spawner : MonoBehaviour
    {
        [Header("Spawn Behaviours")]
        [SerializeField, Tooltip("The radius around the spawner to spawn enemies.")]
        internal float spawnRadius = 5f;
        [SerializeField, Tooltip("If true, all enemies spawned by this spawner will be destroyed when this spawner is destroyed.")]
        internal bool destroySpawnsOnDeath = true;

        [Header("Juice")]
        [SerializeField, Tooltip("The particle system to play when the spawner is destroyed.")]
        internal ParticleSystem deathParticlePrefab;
        
        [Header("Events")]
        [SerializeField, Tooltip("The event to trigger when this spawner is destroyed.")]
        public UnityEvent onDestroyed;
        [SerializeField, Tooltip("The event to trigger when all waves are complete.")]
        public UnityEvent onAllWavesComplete;

        [Header("Shield")]
        [SerializeField, Tooltip("")]
        internal BasicHealthManager shieldGenerator;
        [SerializeField, Tooltip("The particle system to play when a shield generator is destroyed.")]
        internal ParticleSystem shieldGenParticlePrefab;
        [SerializeField, Tooltip("")]
        internal int numShieldGenerators = 3;
        [SerializeField, Tooltip("")]
        internal float shieldGeneratorRPM = 45f;
        [SerializeField, Tooltip("")]
        internal float shieldGenSpawnDelay = 10f;
        [SerializeField, Tooltip("")]
        internal Collider shieldCollider;

        List<BasicEnemyController> spawnedEnemies = new List<BasicEnemyController>();
        List<ShieldGenerator> shieldGenerators = new List<ShieldGenerator>();

        private class ShieldGenerator
        {
            public BasicHealthManager healthManager;
            public GameObject gameObject;
            public Transform transform;

            public ShieldGenerator (BasicHealthManager h)
            {
                healthManager = h;
                gameObject = h.gameObject;
                transform = h.transform;
                h.onIsAliveChanged += OnIsAliveChanged;
            }

            public void OnIsAliveChanged(bool alive)
            {
                gameObject.SetActive(alive);
            }

            public void Respawn(float health)
            {
                healthManager.healthMax = health;
                healthManager.health = health;
                // TODO: Shader
            }
        }

        private int currentWaveIndex = -1;
        private WaveDefinition currentWave;

        private WaveDefinition[] waves;
        private float waveWait = 5f;
        private bool generateWaves = true;

        private int livingShieldGenerators = 0;

        private void Update()
        {
            var rotation = new Vector3(0f, shieldGeneratorRPM * Time.deltaTime, 0f);
            for (int i = 0; i < numShieldGenerators; i++)
                shieldGenerators[i].transform.Rotate(rotation, Space.Self);
        }

        private void Start()
        {
            transform.localScale = new Vector3(spawnRadius * 2, spawnRadius * 2, spawnRadius * 2);

            StartCoroutine(SpawnWaves());

            if (shieldGenerator != null)
            {
                RegisterShieldGenerator(shieldGenerator);
                for (int i = 1; i < numShieldGenerators; ++i)
                {
                    var duplicate = Instantiate(shieldGenerator, shieldGenerator.transform.parent);
                    duplicate.transform.localRotation = Quaternion.Euler(0f, 360f * i / numShieldGenerators, 0f);
                    RegisterShieldGenerator(duplicate);

                    //var lineRenderer = duplicate.GetComponentInChildren<LineRenderer>();
                    //if (lineRenderer != null)
                    //    lineRenderer.widthMultiplier = spawnRadius * 2;
                }

                livingShieldGenerators = numShieldGenerators;
            }
        }

        void RegisterShieldGenerator(BasicHealthManager h)
        {
            var sg = new ShieldGenerator(h);
            shieldGenerators.Add(sg);
            h.onIsAliveChanged += OnShieldGeneratorIsAliveChanged;
        }

        private void OnShieldGeneratorIsAliveChanged(bool alive)
        {
            // TODO: Need to detect which shield gen and disable it, spawn particles, etc

            int oldLivingShieldGenerators = livingShieldGenerators;

            if (alive)
                ++livingShieldGenerators;
            else
                --livingShieldGenerators;

            if (shieldCollider != null)
            {
                if (livingShieldGenerators == 0 && oldLivingShieldGenerators != 0)
                {
                    // Disable the shield
                    shieldCollider.enabled = false;
                    shieldCollider.gameObject.SetActive(false); // TODO: Shader
                }
                else
                {
                    if (livingShieldGenerators != 0 && oldLivingShieldGenerators == 0)
                    {
                        // Enable the shield
                        shieldCollider.enabled = true;
                        shieldCollider.gameObject.SetActive(true); // TODO: Shader
                    }
                }
            }
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
            //Debug.Log($"Starting wave {currentWaveIndex + 1} of {waves.Length}...");
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

                Renderer parentRenderer = GetComponentInChildren<Renderer>();

                // TODO use pool for particles
                if (deathParticlePrefab != null)
                {
                    ParticleSystem deathParticle = Instantiate(deathParticlePrefab, transform.position, Quaternion.identity);
                    if (parentRenderer != null)
                    {
                        var particleSystemRenderer = deathParticle.GetComponent<ParticleSystemRenderer>();
                        if (particleSystemRenderer != null)
                        {
                            particleSystemRenderer.material = parentRenderer.material;
                        }
                    }
                    deathParticle.Play();
                }

                if (destroySpawnsOnDeath)
                {
                    for (int i = 0; i < spawnedEnemies.Count; i++)
                    {
                        if (spawnedEnemies[i] != null)
                        {
                            spawnedEnemies[i].OnAliveIsChanged(false);
                        }
                    }
                }

                Destroy(gameObject);
            }
        }

        /// <summary>
        /// Configure this spawner accoring to a level definition.
        /// </summary>
        /// <param name="currentLevelDefinition"></param>
        internal void Initialize(LevelDefinition currentLevelDefinition)
        {
            waves = currentLevelDefinition.Waves;
            waveWait  = currentLevelDefinition.WaveWait;
            generateWaves = currentLevelDefinition.GenerateNewWaves;
        }
    }
}