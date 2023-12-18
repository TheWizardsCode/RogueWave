using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Playground
{
    public class LevelGenerator : MonoBehaviour
    {
        [SerializeField, Tooltip("The size of the level in square meters.")]
        Vector2 size = new Vector2(500f, 500f);
        [SerializeField, Tooltip("The material to apply to the ground.")]
        Material groundMaterial;
        [SerializeField, Tooltip("The prefabs to use for buildings.")]
        GameObject[] buildingPrefabs;
        [SerializeField, Range(0.1f, 1), Tooltip("The number of buildings to generate per square meter.")]
        float buildingDensity = 0.1f;
        [SerializeField, Tooltip("The space to allocate for each building.")]
        Vector2 buildingLotSize = new Vector2(25f, 25f);

        void Start()
        {
            // Create a plane to represent the ground. setting the material as appropriate
            GameObject plane = GameObject.CreatePrimitive(PrimitiveType.Plane);
            plane.name = "Ground";
            plane.transform.localScale = new Vector3(size.x * 0.1f, 1, size.y * 0.1f); // Plane's default size is 10x10
            Renderer planeRenderer = plane.GetComponent<Renderer>();
            planeRenderer.material = groundMaterial;

            // iterate over a gride of positions buildingSize appart and place a building at each position
            for (float x = -size.x / 2; x < size.x / 2; x += buildingLotSize.x)
            {
                for (float z = -size.y / 2; z < size.y / 2; z += buildingLotSize.y)
                {
                    if (Random.value > buildingDensity)
                        continue;

                    GameObject buildingPrefab = buildingPrefabs[Random.Range(0, buildingPrefabs.Length)];
                    Vector3 position = new Vector3(x, 0, z);
                    GameObject building = Instantiate(buildingPrefab, position, Quaternion.identity);
                }
            }
        }
    }
}
