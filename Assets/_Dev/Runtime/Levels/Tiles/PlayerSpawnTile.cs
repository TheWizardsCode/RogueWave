using NeoFPS;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RogueWave
{
    public class PlayerSpawnTile : BaseTile
    {
        internal override void GenerateTileContent(int x, int y, BaseTile[,] tiles, LevelGenerator levelGenerator)
        {
            GameObject spawn = new GameObject("Player Spawn Point");
            spawn.transform.parent = this.transform;

            SpawnPoint spawnPoint = spawn.AddComponent<SpawnPoint>();
            spawnPoint.transform.localPosition = new Vector3(Random.Range(10, tileWidth - 10), 0, Random.Range(10, tileDepth - 10));

            SpawnManager.AddSpawnPoint(spawnPoint);

            base.GenerateTileContent(x, y, tiles, levelGenerator);
        }
    }
}