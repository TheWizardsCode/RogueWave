using NeoFPS;
using UnityEngine;

namespace Playground
{
    internal class SpawnerTile : BaseTile
    {
        [SerializeField, Tooltip("The spawner to use for this tile.")]
        internal Spawner spawnerPrefab;

        internal override void GenerateTileContent(int x, int y, BaseTile[,] tiles)
        {
            Vector3 position = new Vector3(0, 5, 0);
            Spawner spawner = Instantiate(spawnerPrefab, transform);
            spawner.transform.localPosition = position;
            spawner.GetComponent<IHealthManager>().onIsAliveChanged += spawner.OnAliveIsChanged;
            spawner.name = "Spawner";

            spawner.Initialize(LevelGenerator.levelDefinition);
        }
    }
}
