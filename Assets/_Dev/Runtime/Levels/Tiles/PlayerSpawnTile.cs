using NeoFPS;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RogueWave
{
    public class PlayerSpawnTile : BaseTile
    {
        internal override void GenerateTileContent(int x, int y, BaseTile[,] tiles)
        {
            SpawnPoint spawnPoint = gameObject.AddComponent<SpawnPoint>();

            base.GenerateTileContent(x, y, tiles);
        }
    }
}