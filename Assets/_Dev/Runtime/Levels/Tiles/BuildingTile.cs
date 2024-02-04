using NeoFPS;
using ProceduralToolkit;
using UnityEngine;

namespace Playground
{
    internal class BuildingTile : BaseTile
    {
        [SerializeField, Tooltip("The building prefabs that can be placed on this tile.")]
        internal GameObject[] buildingPrefabs;

        internal override void GenerateTileContent(int x, int y, BaseTile[,] tiles)
        {
            GameObject buildingPrefab = buildingPrefabs[Random.Range(0, buildingPrefabs.Length)];
            Instantiate(buildingPrefab, transform);
        }
    }
}
