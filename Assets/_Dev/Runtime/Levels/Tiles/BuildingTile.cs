using NeoFPS;
using ProceduralToolkit;
using UnityEngine;

namespace RogueWave
{
    internal class BuildingTile : ConnectedTile
    {
        [SerializeField, Tooltip("The building prefabs that can be placed on this tile.")]
        internal GameObject[] buildingPrefabs;

        internal override void GenerateTileContent(int x, int y, BaseTile[,] tiles, LevelGenerator levelGenerator)
        {
            base.GenerateTileContent(x, y, tiles, levelGenerator);

            GameObject buildingPrefab = buildingPrefabs[Random.Range(0, buildingPrefabs.Length)];
            buildingPrefab.transform.localPosition = contentOffset;
            buildingPrefab.transform.localRotation = Quaternion.Euler(0, Random.Range(0, 4) * 90, 0);
            Instantiate(buildingPrefab, transform);
        }
    }
}
