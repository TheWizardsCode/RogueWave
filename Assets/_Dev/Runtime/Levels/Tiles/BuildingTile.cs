using NeoFPS;
using ProceduralToolkit;
using UnityEngine;

namespace RogueWave
{
    internal class BuildingTile : FlowTile
    {
        [SerializeField, Tooltip("The building prefabs that can be placed on this tile.")]
        internal GameObject[] buildingPrefabs;

        internal override void GenerateTileContent(int x, int y, BaseTile[,] tiles)
        {
            base.GenerateTileContent(x, y, tiles);

            GameObject buildingPrefab = buildingPrefabs[Random.Range(0, buildingPrefabs.Length)];
            Instantiate(buildingPrefab, transform);
        }
    }
}
