using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;

namespace Playground
{
	[CreateAssetMenu(fileName = "NewTileDefinition", menuName = "Playground/Tile Definition", order = 250)]
	public class TileDefinition : ScriptableObject
    {
        public enum Direction
        {
            XPositive,
            XNegative,
            YPositive,
            YNegative
        }

        [SerializeField, Range(0, 1), Tooltip("The likelyhood of this tile being selected when multiple tiles are viable. Note this is not an absolute probability it is relative. The higher the chance here the more likely it will be selected. So, if there are two candidates with a chance of 0.1 then each has an equabl probability of being selected, while if there is one at a chance of 1 and another at a chance of 0.5 the relative probabilities are 1/(1+0.5) and 0.5/(1+0.5).")]
        internal float weight = 0.5f;

        [SerializeField, Tooltip("The tile prefab to spawn for this tile type.")]
        BaseTile tilePrefab;

        [Header("Connections")]
        [SerializeField, Tooltip("The constraints that define neighbours to the x positive edge.")]
        internal List<TileConstraint> xPositiveConstraints = new List<TileConstraint>();
        [SerializeField, Tooltip("The constraints that define neighbours to the x negative edge.")]
        internal List<TileConstraint> xNegativeConstraints = new List<TileConstraint>();
        [SerializeField, Tooltip("The constraints that define neighbours to the y positive edge.")]
        internal List<TileConstraint> yPositiveConstraints = new List<TileConstraint>();
        [SerializeField, Tooltip("The constraints that define neighbours to the x negative edge.")]
        internal List<TileConstraint> yNegativeConstraints = new List<TileConstraint>();

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
                    return yPositiveConstraints.Select(c => c.tileDefinition).ToArray();
                case Direction.YNegative:
                    return yNegativeConstraints.Select(c => c.tileDefinition).ToArray();
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
    }

    [Serializable]
    internal class TileConstraint
    {
        [SerializeField, Tooltip("The definition of the tile described in this constraint.")]
        internal TileDefinition tileDefinition;
        [SerializeField, Range(0.01f, 1f), Tooltip("The liklihood of this tile definition being selected. If a random number is <= this value and other constraints match then this will be a candidate.")]
        internal float weight = 0.8f;
    }
}