using NaughtyAttributes;
using UnityEngine;

namespace rogueWave.procedural
{
    /// <summary>
    /// Crystal patch creates a patch of crystals on the ground.
    /// This component will destroy itself once the patch is generated.
    /// </summary>
    public class CrystalPatch : MonoBehaviour
    {
        [SerializeField, Tooltip("The crystal prefaba to use to generate the patch.")]
        private GameObject[] crystalPrefab;
        [SerializeField, Tooltip("The y offset for the crystals."), MinMaxSlider(minValue: -0.5f, maxValue: 0)]
        private Vector2 yOffSet = new Vector2(-0.25f, 0);
        [SerializeField, Tooltip("The minimum and maximum scale of the crystals."), MinMaxSlider(minValue: 0.5f, maxValue: 1.5f)]
        private Vector2 scaleRange = new Vector2(0.75f, 1.25f);
        [SerializeField, Tooltip("The minimum number of crystals to generate."), MinMaxSlider(minValue: 1, maxValue:100)]
        private Vector2Int numberOfCrystals = new Vector2Int(3, 10);
        [SerializeField, Tooltip("The radius of the patch.")]
        private float radius = 5;

        private void Start()
        {
            GeneratePatch();

            Destroy(this);
        }

        private void GeneratePatch()
        {
            int numberOfCrystals = Random.Range(this.numberOfCrystals.x, this.numberOfCrystals.y);
            for (int i = 0; i < numberOfCrystals; i++)
            {
                Vector3 position = transform.position + Random.insideUnitSphere * radius;
                position.y = Random.Range(yOffSet.x, yOffSet.y);

                Quaternion rotation = Quaternion.Euler(0, Random.Range(0, 360), 0);

                float baseScale = Random.Range(scaleRange.x, scaleRange.y);
                Vector3 scale = new Vector3(baseScale * Random.Range(0.85f, 1.15f), baseScale * Random.Range(0.85f, 1.15f), baseScale * Random.Range(0.85f, 1.15f));

                GameObject crystal = Instantiate(crystalPrefab[Random.Range(0, crystalPrefab.Length)], position, rotation);
                crystal.transform.localScale = scale;
                crystal.transform.SetParent(transform);
            }
        }
    }
}
