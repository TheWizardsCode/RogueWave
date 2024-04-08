using NeoFPS;
using ProceduralToolkit;
using RogueWave.Procedural;
using UnityEngine;

namespace RogueWave
{
    internal class BuildingTile : ConnectedTile
    {
        [SerializeField, Tooltip("If set to true and the building prefab used is for a procedural building then the number of floors will be adjusted based on the distance from the center of the level.")]
        internal bool adjustFloorsBasedOnDistance = true;
        [SerializeField, Tooltip("The building prefabs that can be placed on this tile.")]
        internal GameObject[] buildingPrefabs;

        internal override void GenerateTileContent(int x, int y, BaseTile[,] tiles, LevelGenerator levelGenerator)
        {
            base.GenerateTileContent(x, y, tiles, levelGenerator);

            GameObject buildingPrefab = buildingPrefabs[Random.Range(0, buildingPrefabs.Length)];
            GameObject building = Instantiate(buildingPrefab, transform);
            building.transform.localPosition = contentOffset;
            building.transform.localRotation = Quaternion.Euler(0, Random.Range(0, 4) * 90, 0);
            building.isStatic = true;

            BuildingGeneratorComponent generator = building.GetComponentInChildren<BuildingGeneratorComponent>();
            if (generator != null)
            {
                if (!adjustFloorsBasedOnDistance)
                {
                    float widthDistancePercentage = 1 - (Mathf.Abs(x - (tiles.GetLength(0) / 2.0f)) / tiles.GetLength(0));
                    float heightDistancePercentage = 1 - (Mathf.Abs(y - (tiles.GetLength(1) / 2.0f)) / tiles.GetLength(1));

                    int minFloors = Mathf.RoundToInt(generator.floors.y * widthDistancePercentage * heightDistancePercentage);
                    int maxFloors = Mathf.RoundToInt(generator.floors.y * widthDistancePercentage * heightDistancePercentage);

                    generator.floors = new Vector2Int(Mathf.Max(minFloors, generator.floors.x), Mathf.Min(minFloors, generator.floors.y));
                    // Debug.Log($"Building at {x} ({widthDistancePercentage}%), {y} ({heightDistancePercentage}%) had floors range of {generator.floors}");
                }

                generator.Generate();
            }
        }
    }
}
