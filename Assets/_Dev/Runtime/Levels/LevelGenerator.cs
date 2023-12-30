using NeoFPS;
using NeoSaveGames;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using Unity.VisualScripting.YamlDotNet.Core;
using UnityEngine;
using UnityEngine.Android;
using UnityEngine.Events;
using UnityEngine.Serialization;
using UnityEngine.UIElements;
using Random = UnityEngine.Random;

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

        [Header("Additional Items")]
        [SerializeField, Tooltip("Recipe drops enable the player to colelct recipes for use within the run. if this is not null than a number of drops, proportional to the map size and density, will be placed in empty areas.")]
        RecipeSelectorUI recipeDropPrefab;

        [Header("Enemies")]
        //TODO: Move spawner prefab into the Wave definition
        [SerializeField, Tooltip("The spawner to use for this level. This will be placed in a random lot that does not have a building in it."), FormerlySerializedAs("spawnerPrefab")]
        Spawner mainSpawnerPrefab;
        [SerializeField, Tooltip("The number of Enemy Spawners to create.")]
        int numberOfEnemySpawners = 2;
        [SerializeField, Tooltip("The density of buildings that will contain a proximity spawner. These buildings will generate enemies if the player is nearby.")]
        float buildingSpawnerDensity = 0.25f;
        [SerializeField, Tooltip("The prefab to use when generating proximity spawners in buildings.")]
        Spawner buildingProximitySpawner;

        private GameObject level;

        /// <summary>
        /// Generate a level.
        /// </summary>
        /// <param name="gameMode"></param>
        /// <param name="seed">The seed to use, if it is set to -1 (the default) then a random seed will be generated.</param>
        /// <returns></returns>
        internal int Generate(PlaygroundDecember23GameMode gameMode, int seed = -1)
        {
            if (level != null)
                Clear();

            if (seed == -1)
            {
                seed = Environment.TickCount;
            }
            UnityEngine.Random.InitState(seed);

            level = new GameObject("Level");

            CreatePlane();

            List<Vector2> possibleEnemySpawnPositions = PlaceBuildings(gameMode);

            try
            {
                PlaceSpawners(possibleEnemySpawnPositions, gameMode);
                PlaceRecipeDrops(possibleEnemySpawnPositions, gameMode);
            }
            catch (Exception e)
            {
                Debug.LogWarning($"Creating level using seed {seed} failed. See below for more details.\n\n{e.Message}\n\n{e.StackTrace}");
                Clear();
                return Generate(gameMode);
            }

            try
            {
            }
            catch
            {
                // this is an invalid level, so clear it and try again.
                Clear();
                return Generate(gameMode);
            }

            return numberOfEnemySpawners;
        }

        /// <summary>
        /// Place buildings in the map.
        /// </summary>
        /// <returns>List of positions that are not occupied by a building.</returns>
        private List<Vector2> PlaceBuildings(PlaygroundDecember23GameMode gameMode)
        {
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
                    position = new Vector3(x + (buildingLotSize.x / 2), 0, z + (buildingLotSize.x / 2));
                    Transform building = Instantiate(buildingPrefab, position, Quaternion.identity, level.transform).transform;

                    if (Random.value <= buildingSpawnerDensity)
                    {
                        Spawner spawner = Instantiate(buildingProximitySpawner, building);
                        spawner.transform.localPosition = new Vector3(0, 1.5f, 0);
                        spawner.GetComponent<IHealthManager>().onIsAliveChanged += spawner.OnAliveIsChanged;
                        spawner.onDestroyed.AddListener(() => gameMode.SpawnerDestroyed());
                    }
                }
            }

            return possibleEnemySpawnPositions;
        }

        private void CreatePlane()
        {
            GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "Ground";
            ground.transform.localScale = new Vector3(size.x * 0.1f, 1, size.y * 0.1f); // Plane's default size is 10x10
            Renderer planeRenderer = ground.GetComponent<Renderer>();
            planeRenderer.material = groundMaterial;
        }

        private void PlaceSpawners(List<Vector2> possibleEnemySpawnPositions, PlaygroundDecember23GameMode gameMode)
        {
            Vector3 position;

            for (int i = 0; i < numberOfEnemySpawners; i++)
            {
                if (possibleEnemySpawnPositions.Count == 0)
                    throw new System.Exception("Not enough space to place all the spawners.");

                Vector2 spawnerPosition = ValidSpawnerPosition(possibleEnemySpawnPositions);
                position = new Vector3(spawnerPosition.x, mainSpawnerPrefab.spawnRadius, spawnerPosition.y);
                possibleEnemySpawnPositions.Remove(spawnerPosition);

                Spawner spawner = Instantiate(mainSpawnerPrefab, position, Quaternion.identity, level.transform);
                spawner.GetComponent<IHealthManager>().onIsAliveChanged += spawner.OnAliveIsChanged;
                spawner.onDestroyed.AddListener(() => gameMode.SpawnerDestroyed());

                spawner.Initialize(gameMode.currentLevelDefinition);
            }
        }

        private void PlaceRecipeDrops(List<Vector2> possibleRecipeDropPositions, PlaygroundDecember23GameMode gameMode)
        {
            Vector3 position;

            int numberOfRecipeDrops = Mathf.RoundToInt(possibleRecipeDropPositions.Count * 0.05f);
            if (numberOfRecipeDrops < numberOfEnemySpawners)
            {
                numberOfRecipeDrops = numberOfEnemySpawners;
            }

            while (possibleRecipeDropPositions.Count < numberOfRecipeDrops)
            {
                numberOfRecipeDrops--;
            }
            
            for (int i = 0; i < numberOfRecipeDrops; i++)
            {
                Vector2 spawnerPosition = ValidSpawnerPosition(possibleRecipeDropPositions);
                position = new Vector3(spawnerPosition.x, mainSpawnerPrefab.spawnRadius, spawnerPosition.y);
                possibleRecipeDropPositions.Remove(spawnerPosition);

                Instantiate(recipeDropPrefab, position, Quaternion.identity, level.transform);
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
