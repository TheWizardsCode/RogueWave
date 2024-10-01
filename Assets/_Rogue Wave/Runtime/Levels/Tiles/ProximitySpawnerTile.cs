using UnityEngine;
using Random = UnityEngine.Random;

namespace RogueWave
{
    internal class ProximitySpawnerTile : BuildingTile
    {
        [SerializeField, Tooltip("The spawner prefab to use when placing spawners in buildings.")]
        internal Spawner spawnerPrefab;

        internal override void GenerateTileContent(int x, int y, BaseTile[,] tiles, LevelGenerator levelGenerator)
        {
            GameObject buildingPrefab = buildingPrefabs[Random.Range(0, buildingPrefabs.Length - 1)];
            Transform building = Instantiate(buildingPrefab, transform).transform;

            Spawner spawner = Instantiate(spawnerPrefab, building);
            spawner.spawnRadius = 2;
            spawner.transform.localPosition = new Vector3(0, 1.5f, 0);
        }
    }
}
