using ProceduralToolkit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Playground
{
    internal class WallTile : BaseTile
    {
        [SerializeField, Tooltip("The material to use for the wall.")]
        Material wallMaterial;
        [SerializeField, Tooltip("The height of the wall.")]
        float wallHeight = 40;

        GameObject contentObject;
        BaseTile xPositive, xNegative, yPositive, yNegative;

        internal override void GenerateTileContent(int x, int y, BaseTile[,] tiles)
        {
            MeshFilter meshFilter;
            if (contentObject == null)
            {
                contentObject = new GameObject("Wall Segment");
                contentObject.transform.SetParent(transform);
                contentObject.transform.localPosition = new Vector3(0, wallHeight / 2, 0);

                meshFilter = contentObject.AddComponent<MeshFilter>();
                contentObject.AddComponent<MeshRenderer>().material = wallMaterial;
            } else
            {
                meshFilter = contentObject.GetComponent<MeshFilter>();
            }

            GetNeighbours(x, y, tiles);

            CompoundMeshDraft compoundDraft = new CompoundMeshDraft();
            
            if (xPositive is WallTile)
            {
                MeshDraft draft = MeshDraft.Hexahedron(tileWidth / 2, tileHeight / 3, wallHeight);
                draft.name = "Wall";
                draft.Move(new Vector3(tileWidth / 4, 0, 0));
                compoundDraft.Add(draft);
            }

            if (xNegative is WallTile)
            {
                MeshDraft draft = MeshDraft.Hexahedron(tileWidth / 2, tileHeight / 3, wallHeight);
                draft.Move(new Vector3(-tileWidth / 4, 0, 0));
                draft.name = "Wall";
                compoundDraft.Add(draft);
            }
            
            if (yPositive is WallTile)
            {
                MeshDraft draft = MeshDraft.Hexahedron(tileWidth / 3, tileHeight, wallHeight);
                draft.name = "Wall";
                draft.Move(new Vector3(0, 0, tileHeight / 4));
                compoundDraft.Add(draft);
            }
            if (yNegative is WallTile)
            {
                MeshDraft draft = MeshDraft.Hexahedron(tileWidth / 3, tileHeight, wallHeight);
                draft.Move(new Vector3(0, 0, -tileHeight / 4));
                draft.name = "Wall";
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
