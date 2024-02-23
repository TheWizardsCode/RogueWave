using ProceduralToolkit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Serialization;

namespace RogueWave
{
    /// <summary>
    /// A Flow Tile is one that generates a wall or path to influence the flow of traffic through that tile.
    /// </summary>
    internal class ConnectedTile : BaseTile
    {
        [SerializeField, Tooltip("The material to use for the structure.")]
        Material structureMaterial;
        [SerializeField, Tooltip("The height of the structure.")]
        float structureHeight = 40;
        [SerializeField, Tooltip("Define the tiles that this kind of tile will create a flow structure (wall, path, fence etc.) to if they are neighbours.")]
        BaseTile[] validConnections;

        GameObject contentObject;
        BaseTile xPositive, xNegative, yPositive, yNegative;

        /// <summary>
        /// Place and or generate the tile contents.
        /// </summary>
        /// <param name="x">The x coordinate of the location of this tile.</param>
        /// <param name="z">The z coordinate of the location of this tile.</param>
        /// <param name="tiles">The map of tiles.</param>
        internal override void GenerateTileContent(int x, int z, BaseTile[,] tiles)
        {
            MeshFilter meshFilter;
            if (contentObject == null)
            {
                contentObject = new GameObject("Flow Structure");
                contentObject.transform.SetParent(transform);
                contentObject.transform.localPosition = new Vector3(0, structureHeight / 2, 0);

                meshFilter = contentObject.AddComponent<MeshFilter>();
                contentObject.AddComponent<MeshRenderer>().material = structureMaterial;
            }
            else
            {
                meshFilter = contentObject.GetComponent<MeshFilter>();
            }

            GetNeighbours(x, z, tiles);

            CompoundMeshDraft compoundDraft = new CompoundMeshDraft();

            if (ShouldConnect(xPositive))
            {
                MeshDraft draft = MeshDraft.Hexahedron(tileWidth / 2, tileHeight / 3, structureHeight);
                draft.name = "Flow Structure";
                draft.Move(new Vector3(tileWidth / 4, 0, 0));
                compoundDraft.Add(draft);
            }

            if (ShouldConnect(xNegative))
            {
                MeshDraft draft = MeshDraft.Hexahedron(tileWidth / 2, tileHeight / 3, structureHeight);
                draft.Move(new Vector3(-tileWidth / 4, 0, 0));
                draft.name = "Flow Structure";
                compoundDraft.Add(draft);
            }

            if (ShouldConnect(yPositive))
            {
                MeshDraft draft = MeshDraft.Hexahedron(tileWidth / 3, tileHeight, structureHeight);
                draft.name = "Flow Structure";
                draft.Move(new Vector3(0, 0, tileHeight / 4));
                compoundDraft.Add(draft);
            }
            if (ShouldConnect(yNegative))
            {
                MeshDraft draft = MeshDraft.Hexahedron(tileWidth / 3, tileHeight, structureHeight);
                draft.Move(new Vector3(0, 0, -tileHeight / 4));
                draft.name = "Flow Structure";
                compoundDraft.Add(draft);
            }

            compoundDraft.MergeDraftsWithTheSameName();
            meshFilter.mesh = compoundDraft.ToMeshDraft().ToMesh();

            contentObject.AddComponent<MeshCollider>().sharedMesh = meshFilter.mesh;

            base.GenerateTileContent(x, z, tiles);
        }

        private bool ShouldConnect(BaseTile otherTile)
        {
            foreach (BaseTile connectionType in validConnections)
            {
                if (otherTile?.GetType() == connectionType.GetType())
                {
                    return true;
                }
            }

            return false;
        }

        private void GetNeighbours(int x, int y, BaseTile[,] tiles)
        {
            if (x > 0)
            {
                xNegative = tiles[x - 1, y];
            }
            if (x < tiles.GetLength(0) - 1)
            {
                xPositive = tiles[x + 1, y];
            }
            if (y > 0)
            {
                yNegative = tiles[x, y - 1];
            }
            if (y < tiles.GetLength(1) - 1)
            {
                yPositive = tiles[x, y + 1];
            }
        }
    }
}
