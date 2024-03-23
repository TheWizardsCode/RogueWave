using NeoFPS;
using UnityEngine;

namespace RogueWave
{
    internal class SpawnerTile : BaseTile
    {
        [SerializeField, Tooltip("The spawner to use for this tile.")]
        internal PooledObject spawnerPrefab;

        internal override void GenerateTileContent(int x, int y, BaseTile[,] tiles)
        {
            Vector3 position = new Vector3(0, 5, 0) + contentOffset;
            Spawner spawner = PoolManager.GetPooledObject<Spawner>(spawnerPrefab, position, Quaternion.identity);
            spawner.transform.SetParent(transform);
            spawner.transform.localPosition = position;
            spawner.GetComponent<IHealthManager>().onIsAliveChanged += spawner.OnAliveIsChanged;
            spawner.name = "Spawner";

            spawner.Initialize(levelGenerator.levelDefinition);
        }
    }
}
