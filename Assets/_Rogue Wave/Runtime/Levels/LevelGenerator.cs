using NaughtyAttributes;
using NeoFPS;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

namespace RogueWave
{
    public class LevelGenerator : MonoBehaviour
    {
        [Header("Debugging")]
        [SerializeField, Tooltip("The seed used to generate this level. If set to -1 then the value fed in from the Game Mode will be used. This value has no effect outside the editor.")]
        private int seed = -1;
        [SerializeField, Tooltip("Should the generation fast fail when a problem is detected with the generation. In normal gameplay such levels will be regenerated. This has no effect outside the editor.")]
        private bool fastFail = true;

        internal Transform root;
        internal WfcDefinition levelDefinition;
        private int currentSeed;
        BaseTile[,] tiles;
        internal LevelGenerator parentGenerator;

        private int xSize => levelDefinition.mapSize.x;
        private int ySize => levelDefinition.mapSize.y;

        /// <summary>
        /// Generate a level.
        /// </summary>
        /// <param name="levelDefinition">The definition of the level to generate using the Wave Function Collapse algorithm</param>
        internal void Generate(WfcDefinition levelDefinition, int seed)
        {
            attemptCount = 0; 
            Generate(levelDefinition, Vector3.zero, seed);
        }

        /// <summary>
        /// Generate a level.
        /// </summary>
        /// <param name="levelDefinition">The definition of the level to generate using the Wave Function Collapse algorithm</param>
        /// <param name="baseCoords">The base coordinates for the level. This is the bottom left corner of the level in local coordinates.</param>
        internal void Generate(WfcDefinition levelDefinition, Vector3 baseCoords, int seed)
        {
            attemptCount = 0;
            if (root != null)
                Clear();

            root = new GameObject($"Environment {baseCoords}").transform;
            root.localPosition = baseCoords;
            GenerateLevel(levelDefinition, baseCoords, root.transform, seed);
        }

        /// <summary>
        /// Generate a level.
        /// </summary>
        /// <param name="levelDefinition">The definition of the level to generate using the Wave Function Collapse algorithm</param>
        /// <param name="baseCoords">The base coordinates for the level. This is the bottom left corner of the level in local coordinates.</param>
        /// <param name="root">The transform that this level will be parented to.</param>
        internal void Generate(WfcDefinition levelDefinition, Vector3 baseCoords, Transform root, int seed)
        {
            attemptCount = 0;
            GenerateLevel(levelDefinition, baseCoords, root, seed);
        }

        int attemptCount = 0;
        private string generationFailureReport;

        /// <summary>
        /// Hide all visible aspects of the level geometery
        /// </summary>
        [Button("Hide Level Geometry")]
        internal void HideLevelGeometry()
        {
            if (root == null)
            {
                return;
            }

            root.gameObject.SetActive(false);
        }

        [Button("Show Level Geometry")]
        internal void ShowLevelGeometry()
        {
            if (root == null)
            {
                return;
            }

            root.gameObject.SetActive(true);
        }

        /// <summary>
        /// Generate a level.
        /// </summary>
        /// <param name="levelDefinition">The definition of the level to generate using the Wave Function Collapse algorithm</param>
        /// <param name="baseCoords">The base coordinates for the level. This is the bottom left corner of the level in local coordinates.</param>
        /// <param name="root">The transform that this level will be parented to.</param>
        private void GenerateLevel(WfcDefinition levelDefinition, Vector3 baseCoords, Transform root, int seed)
        {
            this.levelDefinition = levelDefinition;
            this.root = root;

#if UNITY_EDITOR
            if (this.seed >= 0)
            {
                seed = this.seed;
            } else
#endif
            if (levelDefinition.seed <= 0)
            {
                seed = Environment.TickCount;
                Random.InitState(seed);
            }
            else
            {
                seed = levelDefinition.seed;
                Random.InitState(levelDefinition.seed);
            }

            currentSeed = seed;

            tiles = new BaseTile[levelDefinition.mapSize.x, levelDefinition.mapSize.y];

            PlaceContainingWalls();

            PlaceFixedTiles();

            WaveFunctionCollapse();

            GenerateTileContent();

            attemptCount++;
            if (attemptCount <= 3 && !isValidateLevel())
            {
#if UNITY_EDITOR
                if (fastFail)
                {
                    GameLog.LogError($"Level with seed {seed} using {levelDefinition} is not valid. {generationFailureReport} In normal gameplay this would be regenerated. In the editor we fast fail to avoid endless loops during level development.");
                }
#else
                GameLog.LogError($"Level with seed {seed} using {levelDefinition} is not valid. {generationFailureReport} Regenerating.");
                foreach (Transform child in root)
                {
                    Destroy(child.gameObject);
                }
                GenerateLevel(levelDefinition, baseCoords, root, -1);
#endif
            }

            GameLog.Log($"Level generated with seed {seed} using {levelDefinition}.");

            PositionSceneCamera();
        }

        private bool isValidateLevel()
        {
            generationFailureReport = string.Empty;

            // Check that there is at least one spawn point
            if (SpawnManager.GetNextSpawnPoint(false) == null)
            {
                Debug.LogError($"No valid spawn points found in generatored level.");
                return false;
            }

            // Check that every tile, except the outer walls, has at least one non barrier neighbor.
            for (int x = 1; x < xSize - 1; x++)
            {
                for (int y = 1; y < ySize - 1; y++)
                {
                    if (tiles[x, y] != null)
                    {
                        int barrierNeighbours = 0;
                        if (x > 0 && tiles[x - 1, y] is BarrierTile)
                        {
                            barrierNeighbours++;
                        }
                        else if (x < xSize - 1 && tiles[x + 1, y] is BarrierTile)
                        {
                            barrierNeighbours++;
                        }
                        else if (y > 0 && tiles[x, y - 1] is BarrierTile)
                        {
                            barrierNeighbours++;
                        }
                        else if (y < ySize - 1 && tiles[x, y + 1] is BarrierTile)
                        {
                            barrierNeighbours++;
                        }

                        if (barrierNeighbours >= 4)
                        {
                            generationFailureReport = $"Tile {x},{y} does has too many barrier neighbour.";
                            return false;
                        }
                    }
                }
            }

            return true;
        }

        private void PositionSceneCamera()
        {
            Camera sceneCamera = Camera.main;
            float height = Mathf.Max(levelDefinition.mapSize.x * levelDefinition.lotSize.x, levelDefinition.mapSize.y * levelDefinition.lotSize.y) ;
            if (sceneCamera != null)
            {
                sceneCamera.transform.position = new Vector3(levelDefinition.mapSize.x * levelDefinition.lotSize.x / 2, height, levelDefinition.mapSize.y * levelDefinition.lotSize.y / 2);
                sceneCamera.transform.rotation = Quaternion.Euler(90, 0, 0);
            }
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

                    int bottomLeftX = Mathf.RoundToInt(tile.constraints.bottomLeftBoundary.x * xSize);
                    int topRightX = Mathf.RoundToInt(tile.constraints.topRightBoundary.x * xSize);
                    int bottomLeftZ = Mathf.RoundToInt(tile.constraints.bottomLeftBoundary.z * ySize);
                    int topRightZ = Mathf.RoundToInt(tile.constraints.topRightBoundary.z * ySize);

                    // Ensure it is in the bounds of the map, accounting for enclosing tiles if required
                    if (levelDefinition.encloseLevel)
                    {
                        bottomLeftX = Mathf.Clamp(bottomLeftX, 1, xSize - 2);
                        topRightX = Mathf.Clamp(topRightX, 1, xSize - 2);
                        bottomLeftZ = Mathf.Clamp(bottomLeftZ, 1, ySize - 2);
                        topRightZ = Mathf.Clamp(topRightZ, 1, ySize - 2);
                    } 
                    else
                    {
                        bottomLeftX = Mathf.Clamp(bottomLeftX, 0, xSize - 1);
                        topRightX = Mathf.Clamp(topRightX, 0, xSize - 1);
                        bottomLeftZ = Mathf.Clamp(bottomLeftZ, 0, ySize - 1);
                        topRightZ = Mathf.Clamp(topRightZ, 0, ySize - 1);
                    }

                    int x = Random.Range(bottomLeftX, topRightX);
                    int z = Random.Range(bottomLeftZ, topRightZ);

                    // TODO: we should be checking that this tile is valid here as we may have already placed a tile
                    if (tiles[x, z] == null && IsValidTileFor(x, z, tile))
                    {
                        isPlaced = true;
                        InstantiateTile(tile, x, z);
                        // Debug.Log($"Placed fixed tile {tile.name} at ({x}, {z})");
                    }
                }

                // If we failed to place the tile then we should try every location to see if we can place it
                if (isPlaced == false)
                {
                    for (int x = 0; x < xSize; x++)
                    {
                        for (int y = 0; y < ySize; y++)
                        {
                            if (tiles[x, y] == null && IsValidTileFor(x, y, tile))
                            {
                                isPlaced = true;
                                InstantiateTile(tile, x, y);
                                // Debug.Log($"Placed fixed tile {tile.name} at ({x}, {y})");
                            }
                        }
                    }
                }

                if (isPlaced == false)
                {
                    Debug.LogError($"Failed to instantiate pre placed tile {tile.name}.");
                }
            }
        }

        private void PlaceContainingWalls()
        {
            if (!levelDefinition.encloseLevel)
            {
                return;
            }

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

        internal Vector3 TileCoordinatesToWorldPosition(int x, int y)
        {
            return new Vector3(x * levelDefinition.lotSize.x, 0, y * levelDefinition.lotSize.y);
        }

        internal Vector2Int WorldPositionToTileCoordinates(Vector3 position)
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
                        tiles[x, y].Generate(x, y, tiles, this);
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
                    if (tiles[x, y] == null)
                    {
                        HashSet<TileDefinition> candidates = new HashSet<TileDefinition>();
                        // Get possible candidates for x positive direction
                        if (x + 1 < xSize)
                        {
                            TileDefinition neighbour = tiles[x + 1, y]?.tileDefinition;
                            if (neighbour != null)
                            {
                                candidates.UnionWith(neighbour.GetTileCandidates(TileDefinition.Direction.XNegative));
                            }
                        }
                        // Get possible candidates for x negative direction
                        if (x - 1 >= 0)
                        {
                            TileDefinition neighbour = tiles[x - 1, y]?.tileDefinition;
                            if (neighbour != null)
                            {
                                candidates.UnionWith(neighbour.GetTileCandidates(TileDefinition.Direction.XPositive));
                            }
                        }
                        // Get possible candidates for y positive direction
                        if (y + 1 < ySize)
                        {
                            TileDefinition neighbour = tiles[x, y + 1]?.tileDefinition;
                            if (neighbour != null)
                            {
                                candidates.UnionWith(neighbour.GetTileCandidates(TileDefinition.Direction.YNegative));
                            }
                        }
                        // Get possible candidates for y negative direction
                        if (y - 1 >= 0)
                        {
                            TileDefinition neighbour = tiles[x, y - 1]?.tileDefinition;
                            if (neighbour != null)
                            {
                                candidates.UnionWith(neighbour.GetTileCandidates(TileDefinition.Direction.YPositive));
                            }
                        }

                        var itemsToRemove = new List<TileDefinition>();
                        foreach (var tile in candidates)
                        {
                            if (levelDefinition.excludedTileTypes.HasFlag(tile.tileType))
                            {
                                itemsToRemove.Add(tile);
                            }
                        }
                        foreach (var tile in itemsToRemove)
                        {
                            candidates.Remove(tile);
                        }

                        foreach (TileDefinition forbiddenTile in levelDefinition.forbiddenTiles)
                        {
                            if (candidates.Contains(forbiddenTile))
                            {
                                candidates.Remove(forbiddenTile);
                            }
                        }

                        // Debug.Log($"Tile at ({x}, {y}) has {candidates.Count} possible candidates.");

                        candidatesForTilesYetToCollapse[x, y] = candidates.ToList<TileDefinition>();
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
                        }
                        else if (candidatesForTilesYetToCollapse[x, y].Count == lowestEntropy)
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
                        InstantiateTile(levelDefinition.defaultTileDefinition, x, y);
                    }
                }
            }
        }

        /// <summary>
        /// Tests to see if a tile is valid at a defined position.
        /// </summary>
        /// <param name="x">The x coordinate for the location of the tile.</param>
        /// <param name="y">The y coordination for the location of the tile.</param>
        /// <param name="tile">The tile to test.</param>
        /// <returns></returns>
        private bool IsValidTileFor(int x, int y, TileDefinition tile)
        {
            bool isValid = tile.constraints.IsValidLocation(new Vector3Int(x, 0, y), xSize, ySize, levelDefinition.encloseLevel);

            //Check there is enough space for the tile
            if (tile.TileArea != Vector3.one)
            {
                for (int i = 0; i < tile.TileArea.x; i++)
                {
                    for (int j = 0; j < tile.TileArea.z; j++)
                    {
                        // TODO: should really be checking for adjacent tiles on each cell not just the spawn position
                        if (x + i >= xSize || y + j >= ySize || tiles[x + i, y + j] != null)
                        {
                            isValid = false;
                        }
                    }
                }
            }

            // Check X Positive direction
            if (isValid && x < xSize - 1)
            {
                TileDefinition otherTile = tiles[x + 1, y]?.tileDefinition;

                if (otherTile != null && otherTile != levelDefinition.wallTileDefinition)
                {
                    TileDefinition candidate = otherTile.GetTileCandidates(TileDefinition.Direction.XNegative).FirstOrDefault(t => t == tile);

                    if (candidate == null)
                    {
                        candidate = tile.GetTileCandidates(TileDefinition.Direction.XPositive).FirstOrDefault(t => t == otherTile);
                    }

                    if (candidate == null)
                    {
                        isValid = false;

#if UNITY_EDITOR
                        //if (!isValid)
                        //{
                        //    Debug.Log($"{tile} is not valid for ({x}, {y}) because the x positive tile is {otherTile}.");
                        //}
#endif
                    }
                }
            }

            // Check X Negative direction
            if (isValid && x > 0)
            {
                TileDefinition otherTile = tiles[x - 1, y]?.tileDefinition;

                if (otherTile != null && otherTile != levelDefinition.wallTileDefinition)
                {
                    TileDefinition candidate = otherTile.GetTileCandidates(TileDefinition.Direction.XPositive).FirstOrDefault(t => t == tile);

                    if (candidate == null)
                    {
                        candidate = tile.GetTileCandidates(TileDefinition.Direction.XNegative).FirstOrDefault(t => t == otherTile);
                    }

                    if (candidate == null)
                    {
                        isValid = false;

#if UNITY_EDITOR
                        //if (!isValid)
                        //{
                        //    Debug.Log($"{tile} is not valid for ({x}, {y}) because the x negative tile is {otherTile}.");
                        //}
#endif
                    }
                }
            }

            // Check Y Positive direction
            if (isValid && y < ySize - 1)
            {
                TileDefinition otherTile = tiles[x, y + 1]?.tileDefinition;

                if (otherTile != null && otherTile != levelDefinition.wallTileDefinition)
                {
                    TileDefinition candidate = otherTile.GetTileCandidates(TileDefinition.Direction.YNegative).FirstOrDefault(t => t == tile);

                    if (candidate == null)
                    {
                        candidate = tile.GetTileCandidates(TileDefinition.Direction.YPositive).FirstOrDefault(t => t == otherTile);
                    }

                    if (candidate == null)
                    {
                        isValid = false;

#if UNITY_EDITOR
                        //if (!isValid)
                        //{
                        //    Debug.Log($"{tile} is not valid for ({x}, {y}) because the y positive tile is {otherTile}.");
                        //}
#endif
                    }
                }
            }

            // Check Y Negative direction
            if (isValid && y > 0)
            {
                TileDefinition otherTile = tiles[x, y - 1]?.tileDefinition;

                if (otherTile != null && otherTile != levelDefinition.wallTileDefinition)
                {
                    TileDefinition candidate = otherTile.GetTileCandidates(TileDefinition.Direction.YPositive).FirstOrDefault(t => t == tile);

                    if (candidate == null)
                    {
                        candidate = tile.GetTileCandidates(TileDefinition.Direction.YNegative).FirstOrDefault(t => t == otherTile);
                    }

                    if (candidate == null)
                    {
                        isValid = false;

#if UNITY_EDITOR
                        //if (!isValid)
                        //{
                        //    Debug.Log($"{tile.name} is not valid for ({x}, {y}) because the y negative tile is {otherTile.name}.");
                        //}
#endif
                    }
                }
            }

            return isValid;
        }

        private void CollapseTile(List<TileDefinition> possibleTiles, int x, int y, int xSize, int ySize)
        {
            // if this tile is not null then we already collapsed this tile, shouldn't happen but worth checking.
            if (tiles[x, y] != null)
            {
                return;
            }

            // if there are no candidates then we need to place an Empty tile
            if (possibleTiles.Count == 0)
            {
                Debug.LogWarning($"No valid tile for {x}, {y}. Defaulting to a empty. We shouldn't get to this stage with no candidate.");
                InstantiateTile(levelDefinition.defaultTileDefinition, x, y);
            }
            // if there is only one candidate then we can place that tile
            else if (possibleTiles.Count == 1)
            {
                InstantiateTile(possibleTiles[0], x, y);
            }
            // We have more than one candidate, pick one using the weighted random method
            else
            {
                WeightedRandom<TileDefinition> weights = new WeightedRandom<TileDefinition>();

                foreach (TileDefinition candidate in possibleTiles)
                {
                    float weight = 0f;
                    if (x < xSize - 1)
                    {
                        TileDefinition neighbour = tiles[x + 1, y]?.tileDefinition;
                        if (neighbour != null)
                        {
                            TileNeighbour reciprocalNeighbour = neighbour.xNegativeConstraints.Find(neighbour => neighbour.tileDefinition == candidate);
                            if (reciprocalNeighbour != null)
                            {
                                weight += (float)reciprocalNeighbour.constraints.weight;
                            }
                        }
                    }
                    if (x > 0)
                    {
                        TileDefinition neighbour = tiles[x - 1, y]?.tileDefinition;
                        if (neighbour != null)
                        {
                            TileNeighbour reciprocalNeighbour = neighbour.xPositiveConstraints.Find(neighbour => neighbour.tileDefinition == candidate);
                            if (reciprocalNeighbour != null)
                            {
                                weight += (float)reciprocalNeighbour.constraints.weight;
                            }
                        }
                    }
                    if (y < ySize - 1)
                    {
                        TileDefinition neighbour = tiles[x, y + 1]?.tileDefinition;
                        if (neighbour != null)
                        {
                            TileNeighbour reciprocalNeighbour = neighbour.zNegativeConstraints.Find(neighbour => neighbour.tileDefinition == candidate);
                            if (reciprocalNeighbour != null)
                            {
                                weight += (float)reciprocalNeighbour.constraints.weight;
                            }
                        }
                    }
                    if (y > 0)
                    {
                        TileDefinition neighbour = tiles[x, y - 1]?.tileDefinition;
                        if (neighbour != null)
                        {
                            TileNeighbour reciprocalNeighbour = neighbour.zPositiveConstraints.Find(neighbour => neighbour.tileDefinition == candidate);
                            if (reciprocalNeighbour != null)
                            {
                                weight += (float)reciprocalNeighbour.constraints.weight;
                            }
                        }
                    }

                    weights.Add(candidate, weight);
                }

                InstantiateTile(weights.GetRandom(), x, y);
            }
        }

        private void InstantiateTile(TileDefinition tileDefinition, int x, int y)
        {
            BaseTile tile = tileDefinition.GetTileObject(root.transform, TileCoordinatesToWorldPosition(x, y));
            tile.name = $"{tileDefinition.name} ({x}. {y})";
            //tile.transform.localPosition = TileCoordinatesToWorldPosition(x, y);
            
            //Debug.Log($"Instantiating tile of type {tile.tileDefinition} at ({x}, {y}) of {root}");

            tiles[x, y] = tile;

            // if this tile covers more than one cell then we need to instantiate the other tiles too
            if (tileDefinition.TileArea != Vector3.one)
            {
                for (int i = 0; i < tileDefinition.TileArea.x; i++)
                {
                    for (int j = 0; j < tileDefinition.TileArea.z; j++)
                    {
                        if (i != 0 || j != 0)
                        {
                            tiles[x + i, y + j] = tile;
                        }
                    }
                }
            }
        }

        internal void Clear()
        {
            if (root == null)
                return;
            Destroy(root.gameObject);
        }

#if UNITY_EDITOR
        [Button("(Re)Generate")]
        void TestGenerate()
        {
            if (!Application.isPlaying)
            {
                Debug.LogError("Generating a level should only be called in play mode.");
            }

            Generate(FindObjectOfType<RogueWaveGameMode>().currentLevelDefinition, -1);
        }
#endif
    }
}
