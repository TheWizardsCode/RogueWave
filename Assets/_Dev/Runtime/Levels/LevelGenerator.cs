using NeoFPS;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

namespace Playground
{
    public class LevelGenerator : MonoBehaviour
    {
        [Header("Events")]
        [SerializeField, Tooltip("The event to trigger when the level generator creates a spawner.")]
        public UnityEvent<Spawner> onSpawnerCreated;

        private GameObject level;
        private LevelDefinition levelDefinition;

        /// <summary>
        /// Generate a level.
        /// </summary>
        /// <param name="gameMode"></param>
        /// <param name="seed">The seed to use, if it is set to -1 (the default) then a random seed will be generated.</param>
        /// <returns></returns>
        internal int Generate(RogueWaveGameMode gameMode, int seed = -1)
        {
            levelDefinition = gameMode.currentLevelDefinition;

            if (level != null)
                Clear();

            if (seed == -1)
            {
                seed = Environment.TickCount;
            }
            UnityEngine.Random.InitState(seed);

            level = new GameObject("Level");

            CreatePlane();
            CreateWalls();

            List<Vector2> possibleEnemySpawnPositions = PlaceBuildings();

            try
            {
                PlaceSpawners(possibleEnemySpawnPositions, gameMode);
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

            return levelDefinition.numberOfEnemySpawners;
        }

        /// <summary>
        /// Place buildings in the map.
        /// </summary>
        /// <returns>List of positions that are not occupied by a building.</returns>
        private List<Vector2> PlaceBuildings()
        {
            Vector3 position;
            List<Vector2> possibleEnemySpawnPositions = new List<Vector2>();
            for (float x = -levelDefinition.size.x / 2; x < levelDefinition.size.x / 2; x += levelDefinition.buildingLotSize.x)
            {
                for (float z = -levelDefinition.size.y / 2; z < levelDefinition.size.y / 2; z += levelDefinition.buildingLotSize.y)
                {
                    if ((x == 0 && z == 0) || Random.value > levelDefinition.buildingDensity)
                    {
                        possibleEnemySpawnPositions.Add(new Vector2(x, z));
                        continue;
                    }

                    bool hasBuildingSpawner = levelDefinition.buildingProximitySpawner != null && Random.value <= levelDefinition.buildingSpawnerDensity;

                    GameObject buildingPrefab = null;
                    if (hasBuildingSpawner)
                    {
                        buildingPrefab = levelDefinition.buildingWithSpawnerPrefabs[Random.Range(0, levelDefinition.buildingWithSpawnerPrefabs.Length - 1)];
                    }
                    else
                    {
                        buildingPrefab = levelDefinition.buildingWithoutSpawnerPrefabs[Random.Range(0, levelDefinition.buildingWithoutSpawnerPrefabs.Length)];
                    }
                    position = new Vector3(x + (levelDefinition.buildingLotSize.x / 2), 0, z + (levelDefinition.buildingLotSize.x / 2));
                    Transform building = Instantiate(buildingPrefab, position, Quaternion.identity, level.transform).transform;

                    if (hasBuildingSpawner)
                    {
                        Spawner spawner = Instantiate(levelDefinition.buildingProximitySpawner, building);
                        spawner.spawnRadius = 3;
                        spawner.transform.localPosition = new Vector3(0, 1.5f, 0);
                        spawner.GetComponent<IHealthManager>().onIsAliveChanged += spawner.OnAliveIsChanged;
                    }
                }
            }

            return possibleEnemySpawnPositions;
        }

        private void CreatePlane()
        {
            GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.transform.SetParent(level.transform);
            ground.name = "Ground";
            ground.transform.localScale = new Vector3(levelDefinition.size.x * 0.1f, 1, levelDefinition.size.y * 0.1f); // Plane's default size is 10x10, but we want the plane to be much larger than the play area to minimize the chances of the player falling off
            
            Renderer planeRenderer = ground.GetComponent<Renderer>();
            planeRenderer.material = levelDefinition.groundMaterial;
        }

          private void CreateWalls()
        {
            float wallThickness = 5.0f;
            float wallHeight = 20.0f;
            float xOffset = levelDefinition.size.x / 2;
            float zOffset = levelDefinition.size.y / 2;

            CreateWall(new Vector3(0, 0, zOffset + wallThickness / 2), new Vector3(levelDefinition.size.x, wallHeight, wallThickness), "South Wall");
            CreateWall(new Vector3(0, 0, -zOffset), new Vector3(levelDefinition.size.x, wallHeight, wallThickness), "North Wall");
            CreateWall(new Vector3(xOffset + wallThickness / 2, 0, wallThickness / 2), new Vector3(wallThickness, wallHeight, levelDefinition.size.y + wallThickness), "West Wall");
            CreateWall(new Vector3(-xOffset, 0, wallThickness / 2), new Vector3(wallThickness, wallHeight, levelDefinition.size.y + wallThickness), "East Wall");
        }

        void CreateWall(Vector3 position, Vector3 size, string name)
        {
            GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wall.name = name;
            wall.transform.position = position;
            wall.transform.localScale = size;
            Renderer renderer = wall.GetComponent<Renderer>();
            renderer.material = levelDefinition.wallMaterial;
        }

        private void PlaceSpawners(List<Vector2> possibleEnemySpawnPositions, RogueWaveGameMode gameMode)
        {
            Vector3 position;

            for (int i = 0; i < levelDefinition.numberOfEnemySpawners; i++)
            {
                if (possibleEnemySpawnPositions.Count == 0)
                    throw new System.Exception("Not enough space to place all the spawners.");

                Vector2 spawnerPosition = GetValidSpawnerPosition(possibleEnemySpawnPositions);
                position = new Vector3(spawnerPosition.x, levelDefinition.mainSpawnerPrefab.spawnRadius, spawnerPosition.y);
                possibleEnemySpawnPositions.Remove(spawnerPosition);

                Spawner spawner = Instantiate(levelDefinition.mainSpawnerPrefab, position, Quaternion.identity, level.transform);
                spawner.GetComponent<IHealthManager>().onIsAliveChanged += spawner.OnAliveIsChanged;
                
                spawner.Initialize(gameMode.currentLevelDefinition);

                onSpawnerCreated.Invoke(spawner);
            }
        }

        /// <summary>
        /// Finds a valid position for a spawning in the map. if it can't find one after 100 tries, it throws an exception.
        /// </summary>
        /// <param name="possibleEnemySpawnPositions">An array of positions that are considered valid.</param>
        /// <returns></returns>
        /// <exception cref="System.Exception">Unable to find a valid spawn position.</exception>
        private Vector2 GetValidSpawnerPosition(List<Vector2> possibleEnemySpawnPositions)
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
                } else if (position.x == -levelDefinition.size.x / 2 || position.x == levelDefinition.size.x /2
                    || position.y == -levelDefinition.size.y / 2 || position.y == levelDefinition.size.y / 2) // in the walls
                {
                    continue;
                }
                else 
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
