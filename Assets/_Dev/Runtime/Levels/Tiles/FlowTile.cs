using ProceduralToolkit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Serialization;

namespace Playground
{
    /// <summary>
    /// A Flow Tile is one that generates a wall or path to influence the flow of traffic through that tile.
    /// </summary>
    internal class FlowTile : BaseTile
    {
        [SerializeField, Tooltip("The material to use for the wall.")]
        [FormerlySerializedAs("wallMaterial")] // Changed 2/17/24
        Material structureMaterial;
        [SerializeField, Tooltip("The height of the wall.")]
        [FormerlySerializedAs("wallHeight")] // changed 2/17/24
        float structureHeight = 40;

        GameObject contentObject;
        BaseTile xPositive, xNegative, yPositive, yNegative;

        internal override void GenerateTileContent(int x, int y, BaseTile[,] tiles)
        {
            MeshFilter meshFilter;
            if (contentObject == null)
            {
                contentObject = new GameObject("StruStructurement");
                contentObject.transform.SetParent(transform);
                contentObject.transform.localPosition = new Vector3(0, structureHeight / 2, 0);

                meshFilter = contentObject.AddComponent<MeshFilter>();
                contentObject.AddComponent<MeshRenderer>().material = structureMaterial;
            } else
            {
                meshFilter = contentObject.GetComponent<MeshFilter>();
            }

            GetNeighbours(x, y, tiles);

            CompoundMeshDraft compoundDraft = new CompoundMeshDraft();
            
            if (xPositive is FlowTile)
            {
                MeshDraft draft = MeshDraft.Hexahedron(tileWidth / 2, tileHeight / 3, structureHeight);
                draft.name = "Structure";
                draft.Move(new Vector3(tileWidth / 4, 0, 0));
                compoundDraft.Add(draft);
            }

            if (xNegative is FlowTile)
            {
                MeshDraft draft = MeshDraft.Hexahedron(tileWidth / 2, tileHeight / 3, structureHeight);
                draft.Move(new Vector3(-tileWidth / 4, 0, 0));
                draft.name = "Structure";
                compoundDraft.Add(draft);
            }
            
            if (yPositive is FlowTile)
            {
                MeshDraft draft = MeshDraft.Hexahedron(tileWidth / 3, tileHeight, structureHeight);
                draft.name = "Structure";
                draft.Move(new Vector3(0, 0, tileHeight / 4));
                compoundDraft.Add(draft);
            }
            if (yNegative is FlowTile)
            {
                MeshDraft draft = MeshDraft.Hexahedron(tileWidth / 3, tileHeight, structureHeight);
                draft.Move(new Vector3(0, 0, -tileHeight / 4));
                draft.name = "Structure";
                compoundDraft.Add(draft);
            }

            compoundDraft.MergeDraftsWithTheSameName();
            meshFilter.mesh = compoundDraft.ToMeshDraft().ToMesh();

            contentObject.AddComponent<MeshCollider>().sharedMesh = meshFilter.mesh;

            base.GenerateTileContent(x, y, tiles);
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
