using NeoFPS;
using UnityEngine;

namespace RogueWave
{
    internal class SpawnerTile : BaseTile
    {
        [SerializeField, Tooltip("The spawner to use for this tile.")]
        internal Spawner spawnerPrefab;

        internal override void GenerateTileContent(int x, int y, BaseTile[,] tiles, LevelGenerator levelGenerator)
        {
            Vector3 position = new Vector3(0, 5, 0) + contentOffset;
            Spawner spawner = Instantiate<Spawner>(spawnerPrefab, position, Quaternion.identity);
            spawner.transform.SetParent(transform);
            spawner.transform.localPosition = position;
            spawner.name = "Spawner";

            spawner.Initialize(levelGenerator.levelDefinition);
        }
    }
}
