using NeoFPS;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;

namespace Playground
{
    public class LevelGenerator : MonoBehaviour
    {
        [Header("Size and Layout")]
        [SerializeField, Tooltip("The size of the level in square meters.")]
        Vector2 size = new Vector2(500f, 500f);
        [SerializeField, Tooltip("The space to allocate for each building.")]
        Vector2 buildingLotSize = new Vector2(25f, 25f);
        [SerializeField, Range(0.1f, 1), Tooltip("How frequently buildings should be placed. Increase for a more dense level.")]
        float buildingDensity = 0.6f;

        [Header("Level Visuals")]
        [SerializeField, Tooltip("The material to apply to the ground.")]
        Material groundMaterial;
        [SerializeField, Tooltip("The prefabs to use for buildings.")]
        GameObject[] buildingPrefabs;

        [Header("Enemies")]
        [SerializeField, Tooltip("The spawner to use for this level. This will be placed in a random lot that does not have a building in it.")]
        Spawner spawnerPrefab;
        [SerializeField, Tooltip("The number of Enemy Spawners to create.")]
        int numberOfEnemySpawners = 2;
        private GameObject level;

        internal int Generate(PlaygroundDecember23GameMode gameMode)
        {
            if (level != null)
                Clear();

            level = new GameObject("Level");

            // Create a plane to represent the ground. setting the material as appropriate
            GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "Ground";
            ground.transform.localScale = new Vector3(size.x * 0.1f, 1, size.y * 0.1f); // Plane's default size is 10x10
            Renderer planeRenderer = ground.GetComponent<Renderer>();
            planeRenderer.material = groundMaterial;

            // iterate over a grid of positions buildingSize appart and place a building at each position
            Vector3 position;
            List<Vector2> possibleEnemySpawnPositions = new List<Vector2>();
            for (float x = -size.x / 2; x < size.x / 2; x += buildingLotSize.x)
            {
                for (float z = -size.y / 2; z < size.y / 2; z += buildingLotSize.y)
                {
                    if ((x == 0 && z == 0) || Random.value > buildingDensity)
                    {
                        possibleEnemySpawnPositions.Add(new Vector2(x, z));
                        continue;
                    }

                    GameObject buildingPrefab = buildingPrefabs[Random.Range(0, buildingPrefabs.Length)];
                    position = new Vector3(x, 0, z);
                    Instantiate(buildingPrefab, position, Quaternion.identity, level.transform);
                }
            }

            try
            {
                PlaceSpawners(possibleEnemySpawnPositions, gameMode);
            } catch
            {
                // this is an invalid level, so clear it and try again.
                Clear();
                return Generate(gameMode);
            }

            return numberOfEnemySpawners;
        }

        private void PlaceSpawners(List<Vector2> possibleEnemySpawnPositions, PlaygroundDecember23GameMode gameMode)
        {
            Vector3 position;

            for (int i = 0; i < numberOfEnemySpawners; i++)
            {
                if (possibleEnemySpawnPositions.Count == 0)
                    throw new System.Exception("Not enough space to place all the spawners.");

                Vector2 spawnerPosition = ValidSpawnerPosition(possibleEnemySpawnPositions);
                position = new Vector3(spawnerPosition.x, spawnerPrefab.spawnRadius, spawnerPosition.y);
                possibleEnemySpawnPositions.Remove(spawnerPosition);

                Spawner spawner = Instantiate(spawnerPrefab, position, Quaternion.identity, level.transform);
                spawner.GetComponentInChildren<MeshRenderer>().transform.localScale = new Vector3(spawnerPrefab.spawnRadius * 2, spawnerPrefab.spawnRadius * 2, spawnerPrefab.spawnRadius * 2);
                spawner.GetComponent<IHealthManager>().onIsAliveChanged += spawner.OnAliveIsChanged;
                spawner.onDestroyed.AddListener(() => gameMode.SpawnerDestroyed());

                spawner.Initialize(gameMode.currentLevelDefinition);
            }
        }

        /// <summary>
        /// Finds a valid position for a spawning in the map. if it can't find one after 100 tries, it throws an exception.
        /// </summary>
        /// <param name="possibleEnemySpawnPositions">An array of positions that are considered valid.</param>
        /// <returns></returns>
        /// <exception cref="System.Exception">Unable to find a valid spawn position.</exception>
        private Vector2 ValidSpawnerPosition(List<Vector2> possibleEnemySpawnPositions)
        {
            Vector2 position = Vector2.zero;
            bool validPosition = false;
            int tries = 100;
            while (!validPosition && tries > 0)
            {
                tries--;
                position = possibleEnemySpawnPositions[Random.Range(0, possibleEnemySpawnPositions.Count)];
                if (position.x == 0 && position.y == 0)
                {
                    continue;
                } else
                {
                    validPosition = true;
                }
            }

            if (validPosition)
            {
                return position;
            } 
            else
            {
                throw new System.Exception("Could not find a valid position for the spawner.");
            } 
        }

        internal void Clear()
        {
            if (level == null)
                return;
            Destroy(level);
        }
    }
}
