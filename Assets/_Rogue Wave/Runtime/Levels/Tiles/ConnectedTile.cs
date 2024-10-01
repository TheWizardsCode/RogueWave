using NaughtyAttributes;
using NeoFPS;
using NeoFPS.Constants;
using ProceduralToolkit;
using ProceduralToolkit.Buildings;
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
        [SerializeField, Tooltip("The surface material for the connecting material on this tile.")]
        internal FpsSurfaceMaterial surface = FpsSurfaceMaterial.CrystalAggregate;
        // TODO: Lookup the surface material from the FpsSurfaceMaterial enum
        [SerializeField, Tooltip("The material to use for the structure.")]
        Material structureMaterial;
        [SerializeField, Tooltip("The height of the structure.")]
        float structureHeight = 40;
        [SerializeField, Tooltip("If true then this structure will be destructible")]
        bool destructible = false;
        [SerializeField, Tooltip("How much damage this structure can take, before being destroyed."), ShowIf("destructible")]
        float health = 100;
        [SerializeField, Tooltip("The scaled destruction particles object to use when this structure is destroyed."), ShowIf("destructible")]
        PooledObject destructionParticles;
        [SerializeField, Tooltip("The scaled smoke particles object to use when this structure is destroyed."), ShowIf("destructible")]
        PooledObject smokeParticles;
        [SerializeField, Tooltip("The pickup to drop when this structure is destroyed."), ShowIf("destructible")]
        List<AbstractRecipe> possibleLoot;
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
        internal override void GenerateTileContent(int x, int z, BaseTile[,] tiles, LevelGenerator levelGenerator)
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
                MeshDraft draft = MeshDraft.Hexahedron(tileWidth / 2, tileDepth / 3, structureHeight);
                draft.name = "Flow Structure";
                draft.Move(new Vector3(tileWidth / 4, 0, 0));
                compoundDraft.Add(draft);
            }

            if (ShouldConnect(xNegative))
            {
                MeshDraft draft = MeshDraft.Hexahedron(tileWidth / 2, tileDepth / 3, structureHeight);
                draft.Move(new Vector3(-tileWidth / 4, 0, 0));
                draft.name = "Flow Structure";
                compoundDraft.Add(draft);
            }

            if (ShouldConnect(yPositive))
            {
                MeshDraft draft = MeshDraft.Hexahedron(tileWidth / 3, tileDepth / 2, structureHeight);
                draft.name = "Flow Structure";
                draft.Move(new Vector3(0, 0, tileDepth / 4));
                compoundDraft.Add(draft);
            }

            if (ShouldConnect(yNegative))
            {
                MeshDraft draft = MeshDraft.Hexahedron(tileWidth / 3, tileDepth / 2, structureHeight);
                draft.Move(new Vector3(0, 0, -tileDepth / 4));
                draft.name = "Connected Structure";
                compoundDraft.Add(draft);
            }

            compoundDraft.MergeDraftsWithTheSameName();
            compoundDraft.Move(contentOffset);
            meshFilter.mesh = compoundDraft.ToMeshDraft().ToMesh();

            contentObject.AddComponent<MeshCollider>().sharedMesh = meshFilter.mesh;
            BuildingSurface surface = contentObject.gameObject.AddComponent<BuildingSurface>();
            surface.Surface = this.surface;

            if (destructible)
            {
                BasicHealthManager healthManager = contentObject.AddComponent<BasicHealthManager>();
                contentObject.AddComponent<BasicDamageHandler>();

                DestructibleController destructibleController = contentObject.AddComponent<DestructibleController>();
                destructibleController.m_PooledScaledDestructionParticles = new PooledObject[] { destructionParticles };
                destructibleController.m_PooledScaledFXParticles = new PooledObject[] { smokeParticles };
                destructibleController.possibleDrops = possibleLoot;
                destructibleController.resourcesDropChance = 1;

                healthManager.healthMax = health;
                healthManager.health = health;
            }

            BuildingSurface flowSurface = contentObject.gameObject.AddComponent<BuildingSurface>();
            flowSurface.Surface = this.surface;

            base.GenerateTileContent(x, z, tiles, levelGenerator);
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
