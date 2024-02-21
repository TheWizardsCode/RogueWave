using NaughtyAttributes;
using NeoFPS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.Events;
using Random = UnityEngine.Random;

namespace RogueWave
{
    public class LevelGenerator : MonoBehaviour
    {
        [Header("Events")]
        [SerializeField, Tooltip("The event to trigger when the level generator creates a spawner.")]
        public UnityEvent<Spawner> onSpawnerCreated;

        private GameObject levelRoot;
        internal static LevelDefinition levelDefinition;
        BaseTile[,] tiles;
        private int xSize;
        private int ySize;

        /// <summary>
        /// Generate a level.
        /// </summary>
        /// <param name="gameMode"></param>
        internal void Generate(RogueWaveGameMode gameMode)
        {
            levelDefinition = gameMode.currentLevelDefinition;

            if (levelRoot != null)
                Clear();

            if (levelDefinition.seed <= 0)
            {
                Random.InitState(Environment.TickCount);
            } else
            {
                Random.InitState(levelDefinition.seed);
            }

            xSize = Mathf.RoundToInt(levelDefinition.size.x / levelDefinition.lotSize.x);
            ySize = Mathf.RoundToInt(levelDefinition.size.y / levelDefinition.lotSize.y);
            tiles = new BaseTile[xSize, ySize];

            levelRoot = new GameObject("Level");
            
            PlaceContainingWalls();

            PlaceFixedTiles();

            PlacePlayerSpawn();

            WaveFunctionCollapse();
            
            GenerateTileContent();
        }

        private void PlaceFixedTiles()
        {
            foreach (TileDefinition tile in levelDefinition.prePlacedTiles)
            {
                bool isPlaced = false;
                int tries = 0;
                while (isPlaced == false && tries < 50)
                {
                    tries++;

                    int x = Random.Range(Mathf.RoundToInt(tile.bottomLeftBoundary.x * xSize), Mathf.RoundToInt(tile.topRightBoundary.x * xSize));
                    int z = Random.Range(Mathf.RoundToInt(tile.bottomLeftBoundary.z * ySize), Mathf.RoundToInt(tile.topRightBoundary.z * ySize));

                    if (tiles[x,z] == null)
                    {
                        isPlaced = true;
                        InstantiateTile(tile, x, z);
                    }
                }

                if (isPlaced == false)
                {
                    Debug.LogError($"Failed to instantiate pre placed tile {tile.name} after {tries} tries.");
                }
            }
        }

        private void PlaceContainingWalls() {
            for (int x = 0; x < xSize; x++)
            {
                for (int y = 0; y < ySize; y++)
                {
                    if (x == 0 || x == xSize - 1 || y == 0 || y == ySize - 1)
                    {
                        InstantiateTile(levelDefinition.wallTileDefinition, x, y);
                    }
                }
            }
        }

        private void PlacePlayerSpawn()
        {
            RogueWaveGameMode spawn = FindObjectOfType<RogueWaveGameMode>();
            int x, y;

            if (((RogueWaveGameMode)FpsGameMode.current).randomizePlayerSpawn)
            {
                x = Random.Range(1, xSize - 1);
                y = Random.Range(1, ySize - 1);

                if (tiles[x, y] != null)
                {
                    PlacePlayerSpawn();
                }
            }
            else
            {
                x = WorldPositionToTileCoordinates(spawn.transform.position).x;
                y = WorldPositionToTileCoordinates(spawn.transform.position).y;
            }

            InstantiateTile(levelDefinition.emptyTileDefinition, x, y);
            spawn.transform.position = TileCoordinatesToWorldPosition(x, y);
        }

        private static Vector3 TileCoordinatesToWorldPosition(int x, int y)
        {

            return new Vector3(x * levelDefinition.lotSize.x, 0, y * levelDefinition.lotSize.y);
        }

        private static Vector2Int WorldPositionToTileCoordinates(Vector3 position)
        {
            return new Vector2Int(Mathf.RoundToInt(position.x / levelDefinition.lotSize.x), Mathf.RoundToInt(position.z / levelDefinition.lotSize.y));
        }

        private void GenerateTileContent()
        {
            for (int x = 0; x < xSize; x++)
            {
                for (int y = 0; y < ySize; y++)
                {
                    if (tiles[x, y] != null)
                    {
                        tiles[x, y].Generate(x, y, tiles);

                        if (tiles[x, y] is SpawnerTile)
                        {
                            onSpawnerCreated.Invoke(tiles[x, y].GetComponentInChildren<Spawner>());
                        }
                    }
                }
            }
        }

        private void WaveFunctionCollapse()
        {
            int x, y = 0;
            int uncollapsedTiles = 0;
            int lowestEntropy = int.MaxValue;
            List<Vector2Int> lowEntropyCoords = new List<Vector2Int>();

            // Create a list of all possible tile types in each undefined tile
            List<TileDefinition>[,] candidatesForTilesYetToCollapse = new List<TileDefinition>[xSize, ySize];
            for (x = 0; x < xSize; x++)
            {
                for (y = 0; y < ySize; y++)
                {
                    if (tiles[x, y] == null) {
                        List<TileDefinition> candidates = new List<TileDefinition>();

                        foreach (TileDefinition candidate in levelDefinition.tileDefinitions)
                        {
                            if (IsValidTileFor(x, y, candidate))
                            {
                                candidates.Add(CreateTileDefinition(candidate));

                                if (candidates.Count > lowestEntropy)
                                {
                                    break;
                                }
                            }
                        }

                        candidatesForTilesYetToCollapse[x, y] = candidates;
                        uncollapsedTiles++;

                        if (candidatesForTilesYetToCollapse[x, y].Count == 0)
                        {
                            //Debug.LogWarning($"Tile at ({x}, {y}) has no valid tile type. Note this may happen because weights are too low. Neighbours are:\n\t" +
                            //    $"({x + 1}, {y}) : {tiles[x + 1, y]?.tileDefinition}\n\t" +
                            //    $"({x}, {y + 1}) : {tiles[x, y + 1]?.tileDefinition}\n\t" +
                            //    $"({x - 1}, {y}) : {tiles[x - 1, y]?.tileDefinition}\n\t" +
                            //    $"({x}, {y - 1}) : {tiles[x, y - 1]?.tileDefinition}\n\t");
                        }
                        else if (candidatesForTilesYetToCollapse[x, y].Count < lowestEntropy)
                        {
                            lowestEntropy = candidatesForTilesYetToCollapse[x, y].Count;
                            lowEntropyCoords.Clear();
                            lowEntropyCoords.Add(new Vector2Int(x, y));
                        } else if (candidatesForTilesYetToCollapse[x, y].Count == lowestEntropy)
                        {
                            lowEntropyCoords.Add(new Vector2Int(x, y));
                        }
                    }
                }
            }
            
            // Collapse the lowest entropy tile (selecting one at random if there are more than 1 at the lowest entropy)
            if (lowestEntropy == 1)
            {
                foreach (Vector2Int coords in lowEntropyCoords)
                {
                    CollapseTile(candidatesForTilesYetToCollapse[coords.x, coords.y], coords.x, coords.y, xSize, ySize);
                    uncollapsedTiles--;
                }
            }
            else if (lowestEntropy < int.MaxValue)
            {
                int idx = Random.Range(0, lowEntropyCoords.Count - 1);
                x = lowEntropyCoords[idx].x;
                y = lowEntropyCoords[idx].y;
                CollapseTile(candidatesForTilesYetToCollapse[x, y], x, y, xSize, ySize);
                uncollapsedTiles--;
            }


            if (uncollapsedTiles > 0 && lowestEntropy < int.MaxValue)
            {
                WaveFunctionCollapse();
            }

            // we only have tiles with no options left
            for (x = 0; x < xSize; x++)
            {
                for (y = 0; y < ySize; y++)
                {
                    if (tiles[x, y] == null)
                    {
                        InstantiateTile(levelDefinition.emptyTileDefinition, x, y);
                    }
                }
            }
        }

        /// <summary>
        /// Tests to see is a tile is valid at a defined position.
        /// </summary>
        /// <param name="x">The x coordinate for the location of the tile.</param>
        /// <param name="y">The y coordination for the location of the tile.</param>
        /// <param name="tile">The tile to test.</param>
        /// <returns></returns>
        private bool IsValidTileFor(int x, int y, TileDefinition tile)
        {
            bool isValid = tile.CanPlace(new Vector3Int(x, 0, y), xSize, ySize);

            // Check X Positive direction
            if (isValid && x < xSize - 1)
            {
                TileDefinition otherTile = tiles[x + 1, y]?.tileDefinition;

                if (otherTile != null)
                {
                    TileDefinition candidate = otherTile.GetTileCandidates(TileDefinition.Direction.XNegative).FirstOrDefault(t => t == tile);
                    //TileDefinition candidate = tile.GetTileCandidates(TileDefinition.Direction.XPositive).FirstOrDefault(t => t == otherTile);

                    if (candidate == null)
                    {
                        isValid = false;

                        //if (!isValid) 
                        //{
                        //    Debug.Log($"{tile} is not valid for ({x}, {y}) because the x positive tile is {otherTile}.");
                        //}
                    }
                    else
                    {
                        isValid &= Random.value <= candidate.weight;
                    }
                }
            }

            // Check X Negative direction
            if (isValid && x > 0)
            {
                TileDefinition otherTile = tiles[x - 1, y]?.tileDefinition;

                if (otherTile != null)
                {
                    TileDefinition candidate = otherTile.GetTileCandidates(TileDefinition.Direction.XPositive).FirstOrDefault(t => t == tile);
                    //TileDefinition candidate = tile.GetTileCandidates(TileDefinition.Direction.XNegative).FirstOrDefault(t => t == otherTile);

                    if (candidate == null)
                    {
                        isValid = false;
                        //if (!isValid)
                        //{
                        //    Debug.Log($"{tile} is not valid for ({x}, {y}) because the x negative tile is {otherTile}.");
                        //}
                    }
                    else
                    {
                        isValid &= Random.value <= candidate.weight;
                    }
                }
            }

            // Check Y Positive direction
            if (isValid && y < ySize - 1)
            {
                TileDefinition otherTile = tiles[x, y + 1]?.tileDefinition;

                if (otherTile != null)
                {
                    TileDefinition candidate = otherTile.GetTileCandidates(TileDefinition.Direction.YNegative).FirstOrDefault(t => t == tile);
                    //TileDefinition candidate = tile.GetTileCandidates(TileDefinition.Direction.YPositive).FirstOrDefault(t => t == otherTile);

                    if (candidate == null)
                    {
                        isValid = false;
                        //if (!isValid)
                        //{
                        //    Debug.Log($"{tile} is not valid for ({x}, {y}) because the y positive tile is {otherTile}.");
                        //}
                    }
                    else
                    {
                        isValid &= Random.value <= candidate.weight;
                    }
                }
            }

            // Check Y Negative direction
            if (isValid && y > 0)
            {
                TileDefinition otherTile = tiles[x, y - 1]?.tileDefinition;

                if (otherTile != null)
                {
                    TileDefinition candidate = otherTile.GetTileCandidates(TileDefinition.Direction.YPositive).FirstOrDefault(t => t == tile);
                    //TileDefinition candidate = tile.GetTileCandidates(TileDefinition.Direction.YNegative).FirstOrDefault(t => t == otherTile);

                    if (candidate == null)
                    {
                        isValid = false;
                        //if (!isValid)
                        //{
                        //    Debug.Log($"{tile.name} is not valid for ({x}, {y}) because the y negative tile is {otherTile.name}.");
                        //}
                    }
                    else
                    {
                        isValid &= Random.value <= candidate.weight;
                    }
                }
            }

            return isValid;
        }

        private TileDefinition CreateTileDefinition(TileDefinition template)
        {
            TileDefinition instance = ScriptableObject.CreateInstance<TileDefinition>();
            foreach (var field in typeof(TileDefinition).GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
            {
                field.SetValue(instance, field.GetValue(template));
            }

            instance.name = template.name;
            return instance;
        }

        private void CollapseTile(List<TileDefinition> possibleTileDefinitions, int x, int y, int xSize, int ySize) {
            // if this tile is not null then we already collapsed this tile, shouldn't happen but worth checking.
            if (tiles[x, y] != null) 
            {
                return;
            }

            // if there are no candidates then we need to place an Empty tile
            if (possibleTileDefinitions.Count == 0)
            {
                Debug.LogWarning($"No valid tile for {x}, {y}. Defaulting to a empty. We shouldn't get to this stage with no candidate.");
                InstantiateTile(levelDefinition.emptyTileDefinition, x, y);
            }
            // if there is only one candidate then we can place that tile
            else if (possibleTileDefinitions.Count == 1)
            {
                InstantiateTile(possibleTileDefinitions[0], x, y);
            }
            // We have more than one candidate, pick one at random
            else
            {
                float totalChance = 0;
                foreach (TileDefinition candidate in possibleTileDefinitions)
                {
                    totalChance += candidate.weight;
                }

                float random = Random.Range(0, totalChance);
                foreach (TileDefinition candidate in possibleTileDefinitions)
                {
                    totalChance -= candidate.weight;
                    if (random >= totalChance)
                    {
                        InstantiateTile(candidate, x, y);
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Place enemy spawners in the level.
        /// </summary>
        /// <param name="xLots">The number of lots in the x axis.</param>
        /// <param name="yLots">The number of lots on the y axis.</param>
        private void PlaceEnemySpawner(int xLots, int yLots)
        {
            int x = Random.Range(1, xLots - 1);
            int y = Random.Range(1, yLots - 1);

            if (tiles[x, y] != null)
            {
                PlaceEnemySpawner(xLots, yLots);
            }

            InstantiateTile(levelDefinition.spawnerTileDefinition, x, y);
        }

        private void InstantiateTile(TileDefinition tileDefinition, int x, int y)
        {
            BaseTile tile = tileDefinition.GetTileObject(levelRoot.transform);
            tile.name = $"{tileDefinition.name} ({x}. {y})";
            tile.transform.position = TileCoordinatesToWorldPosition(x, y);

            // Debug.Log($"Instantiating tile of type {tile.tileDefinition} at ({x}, {y})");

            tiles[x, y] = tile;
        }

        internal void Clear()
        {
            if (levelRoot == null)
                return;
            Destroy(levelRoot);
        }

#if UNITY_EDITOR
        [Button("(Re)Generate")]
        void TestGenerate()
        {
            Generate(FindObjectOfType<RogueWaveGameMode>());
        }
#endif
    }
}
