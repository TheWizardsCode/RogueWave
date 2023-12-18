using NeoFPS;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;
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

        void Start()
        {
            // Create a plane to represent the ground. setting the material as appropriate
            GameObject plane = GameObject.CreatePrimitive(PrimitiveType.Plane);
            plane.name = "Ground";
            plane.transform.localScale = new Vector3(size.x * 0.1f, 1, size.y * 0.1f); // Plane's default size is 10x10
            Renderer planeRenderer = plane.GetComponent<Renderer>();
            planeRenderer.material = groundMaterial;

            // iterate over a grid of positions buildingSize appart and place a building at each position
            Vector3 position;
            List<Vector2> possibleEnemySpawnPositions = new List<Vector2>();
            for (float x = -size.x / 2; x < size.x / 2; x += buildingLotSize.x)
            {
                for (float z = -size.y / 2; z < size.y / 2; z += buildingLotSize.y)
                {
                    if (Random.value > buildingDensity) 
                    { 
                        possibleEnemySpawnPositions.Add(new Vector2(x, z));
                        continue;
                    }
                    
                    GameObject buildingPrefab = buildingPrefabs[Random.Range(0, buildingPrefabs.Length)];
                    position = new Vector3(x, 0, z);
                    Instantiate(buildingPrefab, position, Quaternion.identity);
                }
            }

            // Place a number of spawners in a random lot that does not have a building in it
            for (int i = 0; i < numberOfEnemySpawners; i++)
            {
                if (possibleEnemySpawnPositions.Count == 0)
                    break;
                Vector2 spawnerPosition = possibleEnemySpawnPositions[Random.Range(0, possibleEnemySpawnPositions.Count)];
                position = new Vector3(spawnerPosition.x, 0, spawnerPosition.y);
                Spawner spawner = Instantiate(spawnerPrefab, position, Quaternion.identity);
                spawner.GetComponent<IHealthManager>().onIsAliveChanged += spawner.OnAliveIsChanged;
            }
        }
    }
}
