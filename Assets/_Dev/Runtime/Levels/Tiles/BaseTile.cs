using NaughtyAttributes;
using ProceduralToolkit;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Playground
{
    /// <summary>
    /// Responsible for the creation of the contents of a tile in the level.
    /// </summary>
    public class BaseTile : MonoBehaviour
    {
        [SerializeField, Tooltip("The material to use for the ground.")]
        private Material groundMaterial = null;

        [Header("Tile Content")]
        [SerializeField, Tooltip("Spawn furniture on the tile. If true, the tile will be populated with furniture.")]
        private bool spawnFurniture = false;
        [SerializeField, ShowIf("spawnFurniture"), Tooltip("Prefabs that may be spawned on this tile. Only one of these, selected at random, will be generated.")]
        private GameObject[] furniturePrefabs = null;

        protected float tileWidth = 25f;
        protected float tileHeight = 25f;

        public TileDefinition TileDefinition { get; internal set; }

        internal virtual void GenerateTileContent(int x, int y, BaseTile[,] tiles)
        {
            if (spawnFurniture && furniturePrefabs.Length > 0)
            {
                GameObject tileContentPrefab = furniturePrefabs[Random.Range(0, furniturePrefabs.Length)];
                Instantiate(tileContentPrefab, transform);
            }
        }

        protected virtual void GenerateGround(int x, int y, BaseTile[,] tiles)
        {
            GameObject go = new GameObject("Ground");
            go.transform.SetParent(transform);
            go.transform.localPosition = new Vector3(0, 0, 0);

            MeshDraft draft = MeshDraft.Plane(tileWidth, tileHeight);
            draft.name = "Ground";
            draft.Move(new Vector3(-tileWidth / 2, 0, -tileHeight / 2));

            go.AddComponent<MeshFilter>().mesh = draft.ToMesh();
            go.AddComponent<MeshRenderer>().material = groundMaterial;
            go.AddComponent<MeshCollider>();
        }

    public void Generate(int x, int y, BaseTile[,] tiles)
        {
            GenerateGround(x, y, tiles);
            GenerateTileContent(x, y, tiles);
        }
    }
}
