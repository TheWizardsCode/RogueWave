using NaughtyAttributes;
using NeoFPS;
using Palmmedia.ReportGenerator.Core;
using ProceduralToolkit.Buildings;
using UnityEngine;
using UnityEngine.Serialization;

namespace RogueWave.Procedural
{
    public class BuildingGeneratorComponent : MonoBehaviour
    {
        [Header("Building Configurations")]
        [SerializeField, Tooltip("If true then a new building will be generated on Awake.")]
        private bool generateOnAwake = true;
        [SerializeField, Expandable, Tooltip("The polygons that can be used as the first layer of two layer buildings.")]
        [FormerlySerializedAs("layerOnePolygons")]
        private PolygonAsset[] groundLayerPolygons = null;
        [SerializeField, Expandable, Tooltip("The polygons that can be used as the second layer of two layer buildings.")]
        private PolygonAsset[] upperLayerPolygons = null;
        [SerializeField, Tooltip("The number of minimum and maximum number of floors the building will have."), MinMaxSlider(2, 50)]
        internal Vector2Int floors = new Vector2Int(2, 50);

        [Header("Planners and Constructors")]
        [SerializeField, FormerlySerializedAs("facadePlanningStrategy"), Expandable]
        private FacadePlanner facadePlanner = null;
        [SerializeField, FormerlySerializedAs("facadeConstructionStrategy"), Expandable]
        private ProceduralFacadeConstructor facadeConstructor = null;
        [SerializeField, FormerlySerializedAs("roofPlanningStrategy"), Expandable]
        private RoofPlanner roofPlanner = null;
        [SerializeField, FormerlySerializedAs("roofConstructionStrategy"), Expandable]
        private RoofConstructor roofConstructor = null;

        private void Awake()
        {
            if (generateOnAwake)
            {
                Generate(groundLayerPolygons[Random.Range(0, groundLayerPolygons.Length)], Random.Range(floors.x, floors.y), new GameObject().transform, false);
            }
        }

        /// <summary>
        /// Generate a building with a random polygon from the first layer.
        /// </summary>
        /// <param name="size">The largest floorplan size this building can be. This is in meters and represent an approximate maximum length of a sqaure area the building may cover.</param>
        /// <returns></returns>
        public GameObject Generate(Vector2Int size, int layers)
        {
            GameObject building = new GameObject("Procedural Building");
            building.transform.SetParent(transform);

            PolygonAsset floorplan;
            float lengthMultiplier;
            int numOfFloors;

            for (int i = 0; i < layers; i++)
            {
                GameObject layer = new GameObject("Layer " + i);
                if (i == 0)
                {
                    floorplan = upperLayerPolygons[Random.Range(0, upperLayerPolygons.Length)];
                    lengthMultiplier = floorplan.sides > 6 ? Random.Range(0.6f, 0.7f) : Random.Range(0.7f, 0.9f);
                    floorplan.facadeLength = new Vector2Int(Mathf.RoundToInt(size.x * lengthMultiplier), Mathf.RoundToInt(size.y * lengthMultiplier));

                    if (layers > 0)
                    {
                        numOfFloors = Mathf.RoundToInt(Mathf.Max(Random.Range(floors.x, floors.y / 1.5f), 1));
                    }
                    else
                    {
                        numOfFloors = Random.Range(floors.x, floors.y);
                    }
                }
                else
                {
                    floorplan = upperLayerPolygons[Random.Range(0, upperLayerPolygons.Length)];
                    lengthMultiplier = floorplan.sides > 6 ? Random.Range(0.4f, 0.5f) : Random.Range(0.5f, 0.6f);
                    if (Random.value < 0.5f)
                    {
                        floorplan.facadeLength = new Vector2Int(Mathf.RoundToInt(size.x * lengthMultiplier), Mathf.RoundToInt(size.y * lengthMultiplier));
                    }
                    numOfFloors = Mathf.Max(Random.Range(floors.x / 2, floors.y / 2), 1);
                }
                if (i > 0)
                {
                    Generate(floorplan, numOfFloors, layer.transform, true);
                    Transform lowerLayer = building.transform.Find("Layer " + (i - 1)).Find("Facades");
                    layer.transform.localPosition = new Vector3(0, lowerLayer.GetComponent<Collider>().bounds.size.y, 0);
                } else
                {
                    Generate(floorplan, numOfFloors, layer.transform, false);
                }
                layer.transform.SetParent(building.transform);
            }

            return building;
        }

        public void Generate(PolygonAsset floorplan, int floors, Transform parent, bool generateFloor)
        {
            if (floorplan.autoGenerate)
            {
                floorplan.Randomize();
            }

            BuildingGenerator.Config config = new BuildingGenerator.Config();
            config.floors = floors;
            
            var generator = new BuildingGenerator();
            generator.SetFacadePlanner(facadePlanner);
            generator.SetFacadeConstructor(facadeConstructor);
            generator.SetRoofPlanner(roofPlanner);
            generator.SetRoofConstructor(roofConstructor);
            config.palette = facadeConstructor.palette;
            Transform building = generator.Generate(floorplan.vertices, config, parent);

            for (int i = parent.childCount - 1; i >= 0; i--)
            {
                parent.GetChild(i).gameObject.isStatic = true;
                CreateColliders(parent.GetChild(i));
                AddDamageHandlers(parent.GetChild(i));
            }

            if (generateFloor)
            {
                GameObject floor = Instantiate(building.root.Find("Roof").gameObject, parent);
                floor.transform.localPosition = Vector3.zero;
                floor.transform.localRotation = Quaternion.Euler(180, 0, 0);
            }

            // Set UVs and colours
            MeshFilter[] meshFilters = building.GetComponentsInChildren<MeshFilter>();
            foreach (MeshFilter meshFilter in meshFilters)
            {
                Mesh mesh = meshFilter.sharedMesh;
                if (mesh == null)
                {
                    Debug.LogWarning($"Mesh filter in building generator has no mesh. The Building generator prefab has probably been saved with some mesh data in it. Stripping now, but you should remove it from the prefab to avoid this warning.");
                    Destroy(meshFilter.gameObject);
                    continue;
                }
                Vector3[] vertices = mesh.vertices;
                Vector2[] uvs = new Vector2[vertices.Length];

                for (int i = 0; i < uvs.Length; i++)
                {
                    uvs[i] = new Vector2(0, vertices[i].y / mesh.bounds.size.y);
                }

                mesh.uv = uvs;
            }
        }

        void AddDamageHandlers(Transform building)
        {
            building.gameObject.AddComponent<BasicDamageHandler>();
            BuildingSurface surface = building.gameObject.AddComponent<BuildingSurface>();
            surface.Surface = facadeConstructor.surface;
        }

        void CreateColliders(Transform model)
        {
            MeshCollider meshCollider = model.GetComponent<MeshCollider>();

            if (meshCollider == null)
            {
                meshCollider = model.gameObject.AddComponent<MeshCollider>();
            }

            Mesh mesh = model.GetComponent<MeshFilter>().sharedMesh;
            meshCollider.sharedMesh = null;
            meshCollider.sharedMesh = mesh;
        }

#if UNITY_EDITOR
        [Button]
        void GenerateOneLevelBuilding()
        {
            DestroyBuilding();
            Generate(new Vector2Int(25, 25), 1);
        }

        [Button]
        void GenerateTwoLevelBuilding()
        {
            DestroyBuilding();
            Generate(new Vector2Int(25,25), 2);
        }

        [Button("Destroy Building")]
        void DestroyBuilding()
        {
            if (transform.childCount == 0)
            {
                return;
            }

            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                Transform building = transform.GetChild(i);
                if (Application.isPlaying)
                    building.GetComponentInParent<BasicDamageHandler>().AddDamage(10000);
                else
                    DestroyImmediate(building.gameObject);
            }
        }
#endif
    }
}
