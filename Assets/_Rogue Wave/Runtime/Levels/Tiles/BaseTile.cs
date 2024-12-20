using NaughtyAttributes;
using NeoFPS;
using NeoFPS.Constants;
using ProceduralToolkit;
using ProceduralToolkit.FastNoiseLib;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace RogueWave
{
    /// <summary>
    /// Responsible for the creation of the contents of a tile in the level.
    /// </summary>
    public class BaseTile : MonoBehaviour
    {
        [Header("Ground")]
        [SerializeField, Tooltip("The surface material for the ground of this tile.")]
        internal FpsSurfaceMaterial groundSurface = FpsSurfaceMaterial.Dust;

        [Header("Tile Furniture")]
        [SerializeField, Tooltip("Spawn furniture on the tile. If true, the tile will be populated with furniture.")]
        internal bool spawnFurniture = false;
        [SerializeField, Range(0, 1f), ShowIf("spawnFurniture"), Tooltip("The base chance for furniture to be present on this tile.")]
        internal float furnitureChance = 0.25f;
        [SerializeField, ShowIf("spawnFurniture"), Tooltip("Prefabs that may be spawned on this tile. Only one of these, selected at random, will be generated.")]
        internal GameObject[] furniturePrefabs = null;

        [Header("Difficulty")]
        [SerializeField, Tooltip("A multiplier for the chance of an enemiy spawning on this tile. The basd chance is set in the tile definition, this curve is used to adjust the chance based on the game difficulty.")]
        AnimationCurve enemySpawnMultiplierByDifficulty = AnimationCurve.Linear(0,0,1,5);

        protected LevelGenerator levelGenerator;
        protected float tileWidth = 25f;
        protected float tileDepth = 25f;

        public TileDefinition tileDefinition { get; internal set; }
        internal virtual Vector3 TileArea => Vector3.one;

        private AIDirector m_aiDirector;
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

        public virtual void Generate(int x, int y, BaseTile[,] tiles, LevelGenerator levelGenerator)
        {
            this.levelGenerator = levelGenerator;
            tileWidth = levelGenerator.levelDefinition.lotSize.x;
            tileDepth = levelGenerator.levelDefinition.lotSize.y;
            GenerateGround(x, y, tiles, levelGenerator);
            GenerateTileContent(x, y, tiles, levelGenerator);
            GenerateEnemies(x, y, tiles, levelGenerator);
        }

        internal virtual void GenerateTileContent(int x, int y, BaseTile[,] tiles, LevelGenerator levelGenerator)
        {
            if (spawnFurniture && furniturePrefabs.Length > 0 && Random.value < furnitureChance)
            {
                GameObject tileContentPrefab = furniturePrefabs[Random.Range(0, furniturePrefabs.Length)];
                PlaceItem(Instantiate(tileContentPrefab, transform).transform);
            }
        }

        /// <summary>
        /// Sets the position of an item on the tile.
        /// </summary>
        internal void PlaceItem(Transform item)
        {
            item.localPosition = new Vector3(Random.Range(-tileWidth / 2, tileWidth / 2), 0, Random.Range(-tileDepth / 2, tileDepth / 2)) + contentOffset;
            item.localRotation = Quaternion.Euler(0, Random.Range(0, 360), 0);
        }

        protected virtual void GenerateGround(int x, int y, BaseTile[,] tiles, LevelGenerator levelGenerator)
        {
            GameObject ground = new GameObject("Ground");
            ground.transform.SetParent(transform);
            ground.transform.localPosition = new Vector3(0, 0, 0);

            MeshDraft draft;
            if (tileDefinition.isFlat)
            {
                draft = MeshDraft.Plane(tileWidth * TileArea.x, tileDepth * TileArea.z);
            }
            else
            {
                draft = TerrainDraft();
            }
            draft.name = "Ground";

            ground.AddComponent<MeshFilter>().mesh = draft.ToMesh();
            // TODO: get the material from the surface
            ground.AddComponent<MeshRenderer>().material = tileDefinition.groundMaterial;
            ground.AddComponent<MeshCollider>();

            BuildingSurface surface = ground.gameObject.AddComponent<BuildingSurface>();
            surface.Surface = this.groundSurface;

            // TODO: Don't hard code the ground tag
            ground.tag = "Ground";
        }

        private MeshDraft TerrainDraft()
        {
            Gradient gradient = ColorE.Gradient(Color.black, Color.white);
            Vector2 noiseOffset = new Vector2(Random.Range(0f, 100f), Random.Range(0f, 100f));

            float width = tileWidth * tileDefinition.TileArea.x;
            float depth = tileDepth * tileDefinition.TileArea.z;

            int xSegments = Mathf.CeilToInt(width / tileDefinition.noiseCellSize);
            int zSegments = Mathf.CeilToInt(depth / tileDefinition.noiseCellSize);

            float xStep = width / (xSegments - 1);
            float zStep = depth / (zSegments - 1);
            int vertexCount = 6 * (xSegments - 1) * (zSegments - 1);
            var draft = new MeshDraft
            {
                name = "Ground",
                vertices = new List<Vector3>(vertexCount),
                triangles = new List<int>(vertexCount),
                normals = new List<Vector3>(vertexCount),
                colors = new List<Color>(vertexCount)
            };

            for (int i = 0; i < vertexCount; i++)
            {
                draft.vertices.Add(Vector3.zero);
                draft.triangles.Add(0);
                draft.normals.Add(Vector3.zero);
                draft.colors.Add(Color.black);
            }

            var noise = new FastNoise();
            noise.SetNoiseType(FastNoise.NoiseType.SimplexFractal);
            noise.SetFrequency(tileDefinition.noiseFrequency);

            for (int x = 0; x < xSegments - 1; x++)
            {
                for (int z = 0; z < zSegments - 1; z++)
                {
                    int index0 = 6 * (x + z * (xSegments - 1));
                    int index1 = index0 + 1;
                    int index2 = index0 + 2;
                    int index3 = index0 + 3;
                    int index4 = index0 + 4;
                    int index5 = index0 + 5;

                    float height00 = GetTerrainHeight(x + 0, z + 0, xSegments, zSegments, noiseOffset, noise);
                    float height01 = GetTerrainHeight(x + 0, z + 1, xSegments, zSegments, noiseOffset, noise);
                    float height10 = GetTerrainHeight(x + 1, z + 0, xSegments, zSegments, noiseOffset, noise);
                    float height11 = GetTerrainHeight(x + 1, z + 1, xSegments, zSegments, noiseOffset, noise);

                    var vertex00 = new Vector3((x + 0) * xStep, height00, (z + 0) * zStep);
                    var vertex01 = new Vector3((x + 0) * xStep, height01, (z + 1) * zStep);
                    var vertex10 = new Vector3((x + 1) * xStep, height10, (z + 0) * zStep);
                    var vertex11 = new Vector3((x + 1) * xStep, height11, (z + 1) * zStep);

                    draft.vertices[index0] = vertex00;
                    draft.vertices[index1] = vertex01;
                    draft.vertices[index2] = vertex11;
                    draft.vertices[index3] = vertex00;
                    draft.vertices[index4] = vertex11;
                    draft.vertices[index5] = vertex10;

                    draft.colors[index0] = gradient.Evaluate(height00);
                    draft.colors[index1] = gradient.Evaluate(height01);
                    draft.colors[index2] = gradient.Evaluate(height11);
                    draft.colors[index3] = gradient.Evaluate(height00);
                    draft.colors[index4] = gradient.Evaluate(height11);
                    draft.colors[index5] = gradient.Evaluate(height10);

                    Vector3 normal000111 = Vector3.Cross(vertex01 - vertex00, vertex11 - vertex00).normalized;
                    Vector3 normal001011 = Vector3.Cross(vertex11 - vertex00, vertex10 - vertex00).normalized;

                    draft.normals[index0] = normal000111;
                    draft.normals[index1] = normal000111;
                    draft.normals[index2] = normal000111;
                    draft.normals[index3] = normal001011;
                    draft.normals[index4] = normal001011;
                    draft.normals[index5] = normal001011;

                    draft.triangles[index0] = index0;
                    draft.triangles[index1] = index1;
                    draft.triangles[index2] = index2;
                    draft.triangles[index3] = index3;
                    draft.triangles[index4] = index4;
                    draft.triangles[index5] = index5;
                }
            }

            return draft;
        }
        
        float GetTerrainHeight(int x, int z, int xSegments, int zSegments, Vector2 noiseOffset, FastNoise noise)
        {
            if (x <= 1 || x >= xSegments - 2 || z <= 1 || z >= zSegments - 2)
            {
                return 0;
            }

            float noiseX = x / (float)xSegments + noiseOffset.x;
            float noiseZ = z / (float)zSegments + noiseOffset.y;
            float rawHeight = noise.GetNoise01(noiseX, noiseZ);

            // Calculate the distance from the center of the tile
            float centerX = 0.5f;
            float centerZ = 0.5f;
            float dx = centerX - (x / (float)xSegments);
            float dz = centerZ - (z / (float)zSegments);
            float distanceFromCenter = Mathf.Sqrt(dx * dx + dz * dz);

            // Calculate the edge factor based on the distance from the center
            float edgeFactor = Mathf.Clamp01(1 - (distanceFromCenter / 0.5f));

            return Mathf.Lerp(0, rawHeight, edgeFactor) * tileDefinition.tileHeight;
        }

        protected virtual void GenerateEnemies(int x, int y, BaseTile[,] tiles, LevelGenerator levelGenerator)
        {
            float enemyChance = tiles[x, y].tileDefinition.enemySpawnChance * enemySpawnMultiplierByDifficulty.Evaluate(FpsSettings.playstyle.difficulty);
            if (enemyChance > 0 && Random.value < enemyChance)
            {
                PooledObject prototype = levelGenerator.levelDefinition.GetRandomEnemy();
                if (prototype == null)
                {
                    return;
                }

                BasicEnemyController enemy = PoolManager.GetPooledObject<BasicEnemyController>(prototype);
                enemy.transform.position = levelGenerator.TileCoordinatesToWorldPosition(x, y) + new Vector3(Random.Range(-tileWidth / 2, tileWidth / 2), 0, Random.Range(-tileDepth / 2, tileDepth / 2)) + contentOffset;
            }
        }

        public Vector3 contentOffset => new Vector3(tileWidth / 2, 0, tileDepth / 2);
    }
}
