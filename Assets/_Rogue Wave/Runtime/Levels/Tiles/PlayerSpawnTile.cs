using NeoFPS;
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
            
            // rotate the spawn point to face the center of the map
            int xCenter = Mathf.RoundToInt(levelGenerator.levelDefinition.mapSize.x / 2);
            int yCenter = Mathf.RoundToInt(levelGenerator.levelDefinition.mapSize.y / 2);
            Vector3 center = new Vector3(xCenter * tileWidth, 0, yCenter * tileDepth);
            spawnPoint.transform.LookAt(center);

            SpawnManager.AddSpawnPoint(spawnPoint);

            base.GenerateTileContent(x, y, tiles, levelGenerator);
        }

        protected override void GenerateEnemies(int x, int y, BaseTile[,] tiles, LevelGenerator levelGenerator)
        {
            // do not put enemies in the same tile as the player spawn
        }
    }
}