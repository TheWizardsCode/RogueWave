using NaughtyAttributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;
using UnityEngine.Serialization;

namespace RogueWave
{
	[CreateAssetMenu(fileName = "NewTileDefinition", menuName = "Rogue Wave/Tile Definition", order = 250)]
	public class TileDefinition : ScriptableObject
    {
        public enum Direction
        {
            XPositive,
            XNegative,
            YPositive,
            YNegative
        }

        [SerializeField, Tooltip("The tile prefab to spawn for this tile type.")]
        BaseTile tilePrefab;

        [Header("Tile Ground")]
        [SerializeField, Tooltip("Should this tile be flat or not? If true it will be flat.")]
        internal bool isFlat = true;
        [SerializeField, HideIf("isFlat"), Tooltip("The height of the terrain."), Range(0f, 10f)]
        internal float tileHeight = 5f;
        [SerializeField, HideIf("isFlat"), Tooltip("The size of the noise cells in the terrain. The smaller this the more rugged the tile will be."), Range(0.1f, 1f)]
        internal float noiseCellSize = 0.75f;
        [SerializeField, HideIf("isFlat"), Tooltip("The frequency of the noise. The higher this is the more rugged the terrain will be."), Range(1f, 8f)]
        internal float noiseFrequency = 3f;
        [SerializeField, Tooltip("The material to use for the ground.")]
        internal Material groundMaterial = null;

        [Header("Enemies")]
        [SerializeField, Range(0f, 1f), Tooltip("The chance of an enemy spawning in any given tile. These are only spawned on level creation. They are not spawned while the level is being played. For that you need spawners.")]
        internal float enemySpawnChance = 0f;

        [SerializeField, Tooltip("The constraints that define the placement of this tile.")]
        internal TileConstraint constraints;

        [Header("Connections")]
        [SerializeField, Tooltip("The constraints that define neighbours to the x positive edge.")]
        internal List<TileNeighbour> xPositiveConstraints = new List<TileNeighbour>();
        [SerializeField, Tooltip("The constraints that define neighbours to the x negative edge.")]
        internal List<TileNeighbour> xNegativeConstraints = new List<TileNeighbour>();
        [SerializeField, Tooltip("The constraints that define neighbours to the y positive edge.")]
        internal List<TileNeighbour> zPositiveConstraints = new List<TileNeighbour>();
        [SerializeField, Tooltip("The constraints that define neighbours to the x negative edge.")]
        internal List<TileNeighbour> zNegativeConstraints = new List<TileNeighbour>();

        internal BaseTile GetTileObject(Transform root)
        {
            GameObject go = Instantiate(tilePrefab.gameObject, root);

            BaseTile tile = go.GetComponent<BaseTile>();
            tile.tileDefinition = this;
            
            return tile;
        }

        /// <summary>
        /// Gets all the possible types for a neighbour of this tile in a given direction.
        /// </summary>
        /// <param name="direction">The direction we are testing.</param>
        /// <returns>An array of allowable TileDefinitions.</returns>
        internal TileDefinition[] GetTileCandidates(Direction direction)
        {
            switch (direction)
            {
                case Direction.XPositive:
                    return xPositiveConstraints.Select(c => c.tileDefinition).ToArray();
                case Direction.XNegative:
                    return xNegativeConstraints.Select(c => c.tileDefinition).ToArray();
                case Direction.YPositive:
                    return zPositiveConstraints.Select(c => c.tileDefinition).ToArray();
                case Direction.YNegative:
                    return zNegativeConstraints.Select(c => c.tileDefinition).ToArray();
                default:
                    return null;
            }
        }

        /// <summary>
        /// Get the neighbours of a tile.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="tiles"></param>
        /// <returns>An array of the neighbours that indexed by (int)Direction.*</returns>
        private TileDefinition[] GetNeighbours(int x, int y, BaseTile[,] tiles)
        {
            TileDefinition[] neighbours = new TileDefinition[4];
            if (x > 0 && tiles[x - 1, y] != null)
            {
                neighbours[(int)Direction.XNegative] = tiles[x - 1, y].GetComponent<TileDefinition>();
            }

            if (x < tiles.GetLength(0) - 1 && tiles[x + 1, y] != null)
            {
                neighbours[(int)Direction.XPositive] = tiles[x + 1, y].GetComponent<TileDefinition>();
            }

            if (y > 0 && tiles[x, y - 1] != null)
            {
                neighbours[(int)Direction.YNegative] = tiles[x, y - 1].GetComponent<TileDefinition>();
            }

            if (y < tiles.GetLength(1) - 1 && tiles[x, y + 1] != null)
            {
                neighbours[(int)Direction.YPositive] = tiles[x, y + 1].GetComponent<TileDefinition>();
            }

            return neighbours;
        }

#if UNITY_EDITOR
        [Button]
        private void ClearAllExceptXDirection()
        {
            xNegativeConstraints.Clear();
            zPositiveConstraints.Clear();
            zNegativeConstraints.Clear();
        }
        [Button]
        private void CopyXPositiveToEmptyConstraints()
        {
            if (xNegativeConstraints.Count == 0)
            {
                xNegativeConstraints = xPositiveConstraints.Select(c => new TileNeighbour { tileDefinition = c.tileDefinition, constraints = c.constraints }).ToList();
            }
            if (zPositiveConstraints.Count == 0)
            {
                zPositiveConstraints = xPositiveConstraints.Select(c => new TileNeighbour { tileDefinition = c.tileDefinition, constraints = c.constraints }).ToList();
            }
            if (zNegativeConstraints.Count == 0)
            {
                zNegativeConstraints = xPositiveConstraints.Select(c => new TileNeighbour { tileDefinition = c.tileDefinition, constraints = c.constraints }).ToList();
            }
        }
#endif
    }

    [Serializable]
    internal class TileNeighbour
    {
        [SerializeField, Tooltip("The definition of the tile described.")]
        internal TileDefinition tileDefinition;
        [SerializeField, Tooltip("The constraints that define the placement of this tile.")]
        internal TileConstraint constraints;
    }

    [Serializable]
    internal class TileConstraint
    {
        [Header("Constraints")]
        [SerializeField, Tooltip("The bounds of the tile. This is used to determine the area that the tile can be placed in. This is expressed as a % of the map area from the bottom left of the total area. " +
            "For example, if this value is (0.5, 0, 0.5) and the level is 20x20x5 tiles then the bottome left of the allowed areas for this tile will be at (10, 0, 10).")]
        internal Vector3 bottomLeftBoundary = Vector3.zero;
        [SerializeField, Tooltip("The bounds of the tile. This is used to determine the area that the tile can be placed in. This is expressed as a % of the map area from the bottom left of the total area. " +
            "For example, if this value is (0.5, 0, 0.5) and the level is 20x20x5 tiles then the top right of the allowed areas for this tile will be at (10, 0, 10).")]
        internal Vector3 topRightBoundary = Vector3.one;
        [SerializeField, Range(0.01f, 1f), Tooltip("The liklihood of this tile definition being selected. If a random number is <= this value and other constraints match then this will be a candidate.")]
        internal float weight = 0.5f;

        internal TileConstraint()
        {
            bottomLeftBoundary = Vector3.zero;
            topRightBoundary = Vector3.one;
            weight = 0.5f;
        }

        internal bool IsValidLocatoin(Vector3Int tileCoords, int xSize, int ySize)
        {
            // if the tile is outside the bounds of the level then it cannot be placed.
            if (tileCoords.x < bottomLeftBoundary.x * xSize || tileCoords.x > topRightBoundary.x * xSize
                || tileCoords.y < bottomLeftBoundary.y * ySize || tileCoords.y > topRightBoundary.y * ySize
                || tileCoords.z < bottomLeftBoundary.z * ySize || tileCoords.z > topRightBoundary.z * ySize)
            {
                return false;
            }

            return true;
        }
    }
}