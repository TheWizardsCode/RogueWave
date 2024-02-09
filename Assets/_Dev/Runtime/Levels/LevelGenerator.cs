using NeoFPS;
using NeoSaveGames;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;
using UnityEngine.UIElements;
using Random = UnityEngine.Random;

namespace Playground
{
    public class LevelGenerator : MonoBehaviour
    {
        [Header("Events")]
        [SerializeField, Tooltip("The event to trigger when the level generator creates a spawner.")]
        public UnityEvent<Spawner> onSpawnerCreated;

        private GameObject levelRoot;
        internal static LevelDefinition levelDefinition;

        BaseTile[,] tiles;

        /// <summary>
        /// Generate a level.
        /// </summary>
        /// <param name="gameMode"></param>
        /// <param name="seed">The seed to use, if it is set to -1 (the default) then a random seed will be generated.</param>
        internal void Generate(RogueWaveGameMode gameMode, int seed = -1)
        {
            levelDefinition = gameMode.currentLevelDefinition;

            if (levelRoot != null)
                Clear();

            if (seed == -1)
            {
                seed = Environment.TickCount;
            }
            UnityEngine.Random.InitState(seed);

            int xLots = Mathf.RoundToInt(levelDefinition.size.x / levelDefinition.lotSize.x);
            int yLots = Mathf.RoundToInt(levelDefinition.size.y / levelDefinition.lotSize.y);
            tiles = new BaseTile[xLots, yLots];

            levelRoot = new GameObject("Level");

            int x, y, spawnersPlaced = 0;
            for (int i = 0; i < levelDefinition.numberOfEnemySpawners; i++)
            {
                try {
                    spawnersPlaced++;   
                    PlaceEnemySpawner(xLots, yLots);
                    spawnersPlaced++;
                } catch (Exception e)
                {
                    Debug.LogError(e);
                }
            }

            PlacePlayerSpawn(xLots, yLots);

            PlaceTiles(xLots, yLots);
            GenerateTileContent(xLots, yLots);
        }

        private void PlacePlayerSpawn(int xLots, int yLots)
        {
            RogueWaveGameMode spawn = FindObjectOfType<RogueWaveGameMode>();
            int x, y;

            if (((RogueWaveGameMode)FpsGameMode.current).randomizePlayerSpawn)
            {
                x = Random.Range(1, xLots - 1);
                y = Random.Range(1, yLots - 1);

                if (tiles[x, y] != null)
                {
                    PlacePlayerSpawn(xLots, yLots);
                }
            }
            else
            {
                x = WorldPositionToTileCoordinates(spawn.transform.position).x;
                y = WorldPositionToTileCoordinates(spawn.transform.position).y;
            }

            tiles[x, y] = InstantiateTile(levelDefinition.emptyTileDefinition, x, y);
            spawn.transform.position = TileCoordinatesToWorldPosition(x, y);
        }

        private static Vector3 TileCoordinatesToWorldPosition(int x, int y)
        {
            return new Vector3((x * levelDefinition.lotSize.x) - (levelDefinition.size.x / 2), 0, (y * levelDefinition.lotSize.y) - (levelDefinition.size.y / 2));
        }

        private static Vector2Int WorldPositionToTileCoordinates(Vector3 position)
        {
            return new Vector2Int(Mathf.RoundToInt((position.x + (levelDefinition.size.x / 2)) / levelDefinition.lotSize.x), Mathf.RoundToInt((position.z + (levelDefinition.size.y / 2)) / levelDefinition.lotSize.y));
        }

        private void GenerateTileContent(int xLots, int yLots)
        {
            for (int x = 0; x < xLots; x++)
            {
                for (int y = 0; y < yLots; y++)
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

        private void PlaceTiles(int xLots, int yLots)
        {
            List<TileDefinition> candidates = new List<TileDefinition>();
            bool hasChanged = false;
            int x, y = 0;

            for (x = 0; x < xLots; x++)
            {
                for (y = 0; y < yLots; y++)
                {
                    if (tiles[x, y] != null)
                    {
                        continue;
                    }

                    // edges will always be walls
                    if (x == 0 || x == xLots - 1 || y == 0 || y == yLots - 1)
                    {
                        tiles[x, y] = InstantiateTile(levelDefinition.wallTileDefinition, x, y);
                        hasChanged = true;
                        continue;
                    }

                    // is xPositive a valid neighbour?
                    if (x > 0 && tiles[x - 1, y] != null)
                    {
                        TileDefinition[] xNegativeCandidates = tiles[x - 1, y].TileDefinition.GetTileCandidates(TileDefinition.Direction.XNegative);
                        // if there is only one candidate then we can place that tile
                        if (xNegativeCandidates.Length == 1)
                        {
                            tiles[x, y] = InstantiateTile(xNegativeCandidates[0], x, y);
                            hasChanged = true;
                            continue;
                        }
                        // if there are more than one candidate then we need to check the other neighbours, so store the candidates
                        candidates = xNegativeCandidates.ToList();
                    }

                    // is xNegative a valid neighbour?
                    if (x < xLots - 1 && tiles[x + 1, y] != null)
                    {
                        TileDefinition[] xPositiveCandidates = tiles[x + 1, y].TileDefinition.GetTileCandidates(TileDefinition.Direction.XPositive);
                        // if there is only one candidate then we can place that tile
                        if (xPositiveCandidates.Length == 1)
                        {
                            tiles[x, y] = InstantiateTile(xPositiveCandidates[0], x, y);
                            hasChanged = true;
                            continue;
                        }
                        // if there are more than one candidate then we need to check the other neighbours, so store the candidates that are valid for this neighbour && the previous neighbours

                        foreach (TileDefinition candidate in xPositiveCandidates)
                        {
                            if (!candidates.Contains(candidate))
                            {
                                candidates.Remove(candidate);
                            }
                        }
                    }
                    
                    // is yPositive a valid neighbour?
                    if (y > 0 && tiles[x, y - 1] != null)
                    {
                        TileDefinition[] yPositiveCandidates = tiles[x, y - 1].TileDefinition.GetTileCandidates(TileDefinition.Direction.YPositive);
                        // if there is only one candidate then we can place that tile
                        if (yPositiveCandidates.Length == 1)
                        {
                            tiles[x, y] = InstantiateTile(yPositiveCandidates[0], x, y);
                            hasChanged = true;
                            continue;
                        }
                        // if there are more than one candidate then we need to check the other neighbours, so store the candidates that are valid for this neighbour && the previous neighbours
                        foreach (TileDefinition candidate in yPositiveCandidates)
                        {
                            if (!candidates.Contains(candidate))
                            {
                                candidates.Remove(candidate);
                            }
                        }
                    }

                    // is yNegative a valid neighbour?
                    if (y < yLots - 1 && tiles[x, y - 1] != null)
                    {
                        TileDefinition[] yNegativeCandidates = tiles[x, y - 1].TileDefinition.GetTileCandidates(TileDefinition.Direction.YNegative);
                        // if there is only one candidate then we can place that tile
                        if (yNegativeCandidates.Length == 1)
                        {
                            tiles[x, y] = InstantiateTile(yNegativeCandidates[0], x, y);
                            hasChanged = true;
                            continue;
                        }
                        // if there are more than one candidate then we need to check the other neighbours, so store the candidates that are valid for this neighbour && the previous neighbours
                        foreach (TileDefinition candidate in yNegativeCandidates)
                        {
                            if (!candidates.Contains(candidate))
                            {
                                candidates.Remove(candidate);
                            }
                        }
                    }
                    
                    // if there are no candidates then we need to place a wall
                    if (candidates.Count == 0)
                    {
                        Debug.LogWarning($"No valid tile for {x}, {y}. Defaulting to a wall.");
                        tiles[x, y] = InstantiateTile(levelDefinition.wallTileDefinition, x, y);
                        hasChanged = true;
                    }
                    // if there is only one candidate then we can place that tile
                    else if (candidates.Count == 1)
                    {
                        tiles[x, y] = InstantiateTile(candidates[0], x, y);
                        hasChanged = true;
                    }
                }
            }

            bool isComplete = true;
            for (x = 0; x < xLots; x++)
            {
                for (y = 0; y < yLots; y++)
                {
                    if (tiles[x, y] == null)
                    {
                        isComplete = false;
                        break;
                    }
                }

                if (!isComplete)
                {
                    break;
                }
            }

            if (!isComplete && !hasChanged)
            {
                float totalChance = 0;
                foreach (TileDefinition candidate in candidates)
                {
                    totalChance += candidate.relativeChance;
                }

                float random = Random.Range(0, totalChance);
                foreach (TileDefinition candidate in candidates)
                {
                    totalChance -= candidate.relativeChance;
                    if (random >= totalChance)
                    {
                        tiles[x, y] = InstantiateTile(candidate, x, y);
                        break;
                    }
                }
            }

            if (!isComplete)
            {
                PlaceTiles(xLots, yLots);
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

            BaseTile spawnerTile = InstantiateTile(levelDefinition.spawnerTileDefinition, x, y);
            tiles[x, y] = spawnerTile;
        }

        private BaseTile InstantiateTile(TileDefinition tileDefinition, int x, int y)
        {
            BaseTile tile = tileDefinition.GetTileObject(levelRoot.transform);
            tile.transform.position = TileCoordinatesToWorldPosition(x, y);
            return tile;
        }

        internal void Clear()
        {
            if (levelRoot == null)
                return;
            Destroy(levelRoot);
        }
    }
}
