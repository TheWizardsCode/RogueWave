using NaughtyAttributes;
using NeoFPS;
using NeoFPS.Constants;
using ProceduralToolkit;
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
        [SerializeField, Expandable]
        private PolygonAsset[] foundationPolygon = null;
        [SerializeField, Tooltip("The number of floors the building will have."), MinMaxSlider(2, 50)]
        private Vector2Int floors = new Vector2Int(2, 50);
        [SerializeField, Tooltip("The parent for the building generated.")]
        private Transform parent = null;

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
            Generate();
        }

        public Transform Generate()
        {
            PolygonAsset foundations = foundationPolygon[Random.Range(0, foundationPolygon.Length)];
            if (foundations.autoGenerate)
            {
                foundations.Randomize();
            }

            BuildingGenerator.Config config = new BuildingGenerator.Config();
            config.floors = Random.Range(floors.x, floors.y);
            
            var generator = new BuildingGenerator();
            generator.SetFacadePlanner(facadePlanner);
            generator.SetFacadeConstructor(facadeConstructor);
            generator.SetRoofPlanner(roofPlanner);
            generator.SetRoofConstructor(roofConstructor);
            config.palette = facadeConstructor.palette;
            Transform building = generator.Generate(foundations.vertices, config, parent);

            for (int i = parent.childCount - 1; i >= 0; i--)
            {
                parent.GetChild(i).gameObject.isStatic = true;
                CreateColliders(parent.GetChild(i));
                AddDamageHandlers(parent.GetChild(i));
            }

            return building;
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
        [Button("Generate Building")]
        void GenerateBuilding()
        {
            ClearBuildings();
            Generate();
        }

        [Button("Clear Buildings")]
        void ClearBuildings()
        {
            for (int i = parent.childCount - 1; i >= 0; i--)
            {
                GameObject.DestroyImmediate(parent.GetChild(i).gameObject);
            }
        }
#endif
    }
}
