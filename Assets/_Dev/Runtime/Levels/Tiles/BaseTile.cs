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
        [SerializeField, ShowIf("spawnFurniture"), Tooltip("The base chance for furniture to be present on this tile.")]
        private float furnitureChance = 0.25f;
        [SerializeField, ShowIf("spawnFurniture"), Tooltip("Prefabs that may be spawned on this tile. Only one of these, selected at random, will be generated.")]
        private GameObject[] furniturePrefabs = null;

        protected float tileWidth = 25f;
        protected float tileHeight = 25f;

        public TileDefinition tileDefinition { get; internal set; }

        internal virtual void GenerateTileContent(int x, int y, BaseTile[,] tiles)
        {
            if (spawnFurniture && furniturePrefabs.Length > 0 && Random.value < furnitureChance)
            {
                GameObject tileContentPrefab = furniturePrefabs[Random.Range(0, furniturePrefabs.Length)];
                Instantiate(tileContentPrefab, transform);
            }
        }

        protected virtual void GenerateGround(int x, int y, BaseTile[,] tiles)
        {
            GameObject ground = new GameObject("Ground");
            ground.transform.SetParent(transform);
            ground.transform.localPosition = new Vector3(0, 0, 0);

            MeshDraft draft = MeshDraft.Plane(tileWidth, tileHeight);
            draft.name = "Ground";
            draft.Move(new Vector3(-tileWidth / 2, 0, -tileHeight / 2));

            ground.AddComponent<MeshFilter>().mesh = draft.ToMesh();
            ground.AddComponent<MeshRenderer>().material = groundMaterial;
            ground.AddComponent<MeshCollider>();
        }

        public void Generate(int x, int y, BaseTile[,] tiles)
        {
            GenerateGround(x, y, tiles);
            GenerateTileContent(x, y, tiles);
        }
    }
}
