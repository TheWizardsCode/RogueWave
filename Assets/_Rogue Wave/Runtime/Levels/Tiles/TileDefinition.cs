using NaughtyAttributes;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace RogueWave
{
	[CreateAssetMenu(fileName = "NewTileDefinition", menuName = "Rogue Wave/Tile Definition", order = 250)]
	public class TileDefinition : ScriptableObject
    {
        public enum TileType
        {
            Undefined = 0,
            Empty = 1 << 0,
            Wall = 1 << 1,
            Building = 1 << 2,
            Flora = 1 << 3,
            SocialSpace = 1 << 4,
            EnemySpawner = 1 << 5,
            PlayerSpawner = 1 << 6,
            DiscoverableItem = 1 << 7,
            SetAreas = 1 << 8,
            Infrastructure = 1 << 9
        }


        public enum Direction
        {
            XPositive,
            XNegative,
            YPositive,
            YNegative
        }


        [Header("UI")]
        [SerializeField, Tooltip("The name of the tile as displayed in the UI.")]
        string m_DisplayName = string.Empty;
        [SerializeField, Tooltip("The description of the tile as displayed in the UI."), TextArea(2,5)]
        string m_Description;
        [SerializeField, Tooltip("The sprite to use when representing this tile, or a level containing this tile, in the UI.")]
        internal Sprite icon;

        [SerializeField, Tooltip("The type of tile this is. This is used to determine how the tile is used in the level.")]
        internal TileType tileType = TileType.Undefined;
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
        public TileConstraint constraints;

        [Header("Connections")]
        [SerializeField, Tooltip("The constraints that define neighbours to the x positive edge.")]
        internal List<TileNeighbour> xPositiveConstraints = new List<TileNeighbour>();
        [SerializeField, Tooltip("The constraints that define neighbours to the x negative edge.")]
        internal List<TileNeighbour> xNegativeConstraints = new List<TileNeighbour>();
        [SerializeField, Tooltip("The constraints that define neighbours to the y positive edge.")]
        internal List<TileNeighbour> zPositiveConstraints = new List<TileNeighbour>();
        [SerializeField, Tooltip("The constraints that define neighbours to the x negative edge.")]
        internal List<TileNeighbour> zNegativeConstraints = new List<TileNeighbour>();


        internal Vector3 TileArea {
            get => tilePrefab is MultiCellTile multiCellTilePrefab ? multiCellTilePrefab.TileArea : Vector3.one;
        }

        public string DisplayName {
            get {
                if (string.IsNullOrEmpty(m_DisplayName))
                {
                    return name;
                }
                return m_DisplayName; 
            }
        }

        public string Description
        {
            get
            {
                if (string.IsNullOrEmpty(m_Description))
                {
                    return string.Empty;
                }
                return m_Description;
            }
        }

        internal BaseTile GetTileObject(Transform root, Vector3 position)
        {
            GameObject go = Instantiate(tilePrefab.gameObject, position, Quaternion.identity, root);

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
        private void ClearAllConstraintsExceptXPositiveDirection()
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

        void OnValidate()
        {
            if (constraints == null)
            {
                constraints = new TileConstraint();
            }
            if (constraints.bottomLeftBoundary.x < 0)
            {
                constraints.bottomLeftBoundary.x = 0;
            }
            if (constraints.bottomLeftBoundary.y < 0)
            {
                constraints.bottomLeftBoundary.y = 0;
            }
            if (constraints.bottomLeftBoundary.z < 0)
            {
                constraints.bottomLeftBoundary.z = 0;
            }
            if (constraints.topRightBoundary.x > 1)
            {
                constraints.topRightBoundary.x = 1;
            }
            if (constraints.topRightBoundary.y > 1)
            {
                constraints.topRightBoundary.y = 1;
            }
            if (constraints.topRightBoundary.z > 1)
            {
                constraints.topRightBoundary.z = 1;
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
    public class TileConstraint
    {
        [Header("Constraints")]
        [SerializeField, Tooltip("The bounds of the tile. This is used to determine the area that the tile can be placed in. This is expressed as a % of the map area from the bottom left of the total area. " +
            "For example, if this value is (0.5, 0, 0.5) and the level is 20x20x5 tiles then the bottome left of the allowed areas for this tile will be at (10, 0, 10).")]
        public Vector3 bottomLeftBoundary = Vector3.zero;
        [SerializeField, Tooltip("The bounds of the tile. This is used to determine the area that the tile can be placed in. This is expressed as a % of the map area from the bottom left of the total area. " +
            "For example, if this value is (0.5, 0, 0.5) and the level is 20x20x5 tiles then the top right of the allowed areas for this tile will be at (10, 0, 10).")]
        public Vector3 topRightBoundary = Vector3.one;
        [SerializeField, Range(0.01f, 1f), Tooltip("The liklihood of this tile definition being selected. If a random number is <= this value and other constraints match then this will be a candidate.")]
        public float weight = 0.5f;

        internal TileConstraint()
        {
            bottomLeftBoundary = Vector3.zero;
            topRightBoundary = Vector3.one;
            weight = 0.5f;
        }

        /// <summary>
        /// Check if a tile can be placed at a given location.
        /// </summary>
        /// <param name="tileCoords">The coordinates of the location to test.</param>
        /// <param name="xSize">The size of the map on the x axis</param>
        /// <param name="ySize">The size of the map on the y (aka z) axis</param>
        /// <param name="isEnclosed">If the map is enclosed on the outer tiles or not.</param>
        /// <returns></returns>
        internal bool IsValidLocation(Vector3Int tileCoords, int xSize, int ySize, bool isEnclosed)
        {

            int bottomLeftX = Mathf.RoundToInt(bottomLeftBoundary.x * xSize);
            int topRightX = Mathf.RoundToInt(topRightBoundary.x * xSize);
            int bottomLeftZ = Mathf.RoundToInt(bottomLeftBoundary.z * ySize);
            int topRightZ = Mathf.RoundToInt(topRightBoundary.z * ySize);

            // Ensure it is in the bounds of the map, accounting for enclosing tiles if required
            if (isEnclosed)
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

            // if the tile is outside the bounds of the level then it cannot be placed.
            if (tileCoords.x < bottomLeftX)
            {
                return false;
            }
            if (tileCoords.x > topRightX)
            {
                return false;
            }
            if (tileCoords.y < Mathf.RoundToInt(bottomLeftBoundary.y * ySize))
            {
                return false;
            }
            if (tileCoords.y > Mathf.RoundToInt(topRightBoundary.y * ySize))
            {
                return false;
            }
            if (tileCoords.z < bottomLeftZ)
            {
                return false;
            }
            if (tileCoords.z > topRightZ)
            {
                return false;
            }

            return true;
        }
    }
}