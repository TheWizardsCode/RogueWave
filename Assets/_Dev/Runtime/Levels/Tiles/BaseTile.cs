using NaughtyAttributes;
using NeoFPS;
using ProceduralToolkit;
using UnityEngine;
using Random = UnityEngine.Random;

namespace RogueWave
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
        [SerializeField, Range(0, 1f), ShowIf("spawnFurniture"), Tooltip("The base chance for furniture to be present on this tile.")]
        private float furnitureChance = 0.25f;
        [SerializeField, ShowIf("spawnFurniture"), Tooltip("Prefabs that may be spawned on this tile. Only one of these, selected at random, will be generated.")]
        private GameObject[] furniturePrefabs = null;

        protected LevelGenerator levelGenerator;
        protected float tileWidth = 25f;
        protected float tileHeight = 25f;
        private AIDirector m_aiDirector;

        public TileDefinition tileDefinition { get; internal set; }

        private AIDirector aiDirector
        {
            get
            {
                if (m_aiDirector == null)
                {
                    m_aiDirector = FindAnyObjectByType<AIDirector>();
                }
                return m_aiDirector;
            }
        }

        internal virtual void GenerateTileContent(int x, int y, BaseTile[,] tiles, LevelGenerator levelGenerator)
        {
            if (spawnFurniture && furniturePrefabs.Length > 0 && Random.value < furnitureChance)
            {
                GameObject tileContentPrefab = furniturePrefabs[Random.Range(0, furniturePrefabs.Length)];
                Transform furniture = Instantiate(tileContentPrefab, transform).transform;
                furniture.localPosition = new Vector3(Random.Range(-tileWidth / 2, tileWidth / 2), 0, Random.Range(-tileHeight / 2, tileHeight / 2)) + contentOffset;
                furniture.localRotation = Quaternion.Euler(0, Random.Range(0, 360), 0);
            }
        }

        protected virtual void GenerateGround(int x, int y, BaseTile[,] tiles, LevelGenerator levelGenerator)
        {
            GameObject ground = new GameObject("Ground");
            ground.transform.SetParent(transform);
            ground.transform.localPosition = new Vector3(0, 0, 0);

            MeshDraft draft = MeshDraft.Plane(tileWidth, tileHeight);
            draft.name = "Ground";

            ground.AddComponent<MeshFilter>().mesh = draft.ToMesh();
            ground.AddComponent<MeshRenderer>().material = groundMaterial;
            ground.AddComponent<MeshCollider>();

            // TODO: Don't hard code the ground tag
            ground.tag = "Ground";
        }

        protected virtual void GenerateEnemies(int x, int y, BaseTile[,] tiles, LevelGenerator levelGenerator)
        {
            float enemyChance = tiles[x, y].tileDefinition.enemySpawnChance * aiDirector.difficultyMultiplier;
            if (enemyChance > 0 && Random.value < enemyChance)
            {
                PooledObject prototype = levelGenerator.levelDefinition.GetRandomEnemy();
                if (prototype == null)
                {
                    return;
                }

                BasicEnemyController enemy = PoolManager.GetPooledObject<BasicEnemyController>(prototype);
                enemy.transform.position = levelGenerator.TileCoordinatesToWorldPosition(x, y) + new Vector3(Random.Range(-tileWidth / 2, tileWidth / 2), 0, Random.Range(-tileHeight / 2, tileHeight / 2)) + contentOffset;
            }
        }

        public void Generate(int x, int y, BaseTile[,] tiles, LevelGenerator levelGenerator)
        {
            this.levelGenerator = levelGenerator;
            tileWidth = levelGenerator.levelDefinition.lotSize.x;
            tileHeight = levelGenerator.levelDefinition.lotSize.y;
            GenerateGround(x, y, tiles, levelGenerator);
            GenerateTileContent(x, y, tiles, levelGenerator);
            GenerateEnemies(x, y, tiles, levelGenerator);
        }

        public Vector3 contentOffset => new Vector3(tileWidth / 2, 0, tileHeight / 2);
    }
}
