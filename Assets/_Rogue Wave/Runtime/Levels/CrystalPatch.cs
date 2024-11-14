using NaughtyAttributes;
using RogueWave;
using System;
using UnityEngine;
using Random = UnityEngine.Random;

namespace rogueWave.procedural
{
    /// <summary>
    /// Crystal patch creates a patch of crystals on the ground.
    /// This component will destroy itself once the patch is generated.
    /// </summary>
    public class CrystalPatch : MonoBehaviour
    {
        [SerializeField, Tooltip("The minimum number of crystals to generate."), MinMaxSlider(minValue: 1, maxValue: 100)]
        public Vector2Int numberOfCrystals;
        [SerializeField, Tooltip("The details of this item data to spawn.")]
        CrystalItemData[] crystalItemData;

        private void Start()
        {
            GeneratePatch();

            Destroy(this);
        }

        private void GeneratePatch()
        {
            WeightedRandom<CrystalItemData> weightedItems = new WeightedRandom<CrystalItemData>();
            for (int i = 0; i < crystalItemData.Length; i++)
            {
                weightedItems.Add(crystalItemData[i], crystalItemData[i].weight);
            }

            int numberOfCrystals = Random.Range(this.numberOfCrystals.x, this.numberOfCrystals.y);
            for (int i = 0; i < numberOfCrystals; i++)
            {
                CrystalItemData item = weightedItems.GetRandom();

                Vector3 position = transform.position + Random.insideUnitSphere * item.radius;
                position.y = Random.Range(item.yOffset.x, item.yOffset.y);

                Quaternion rotation = Quaternion.Euler(0, Random.Range(0, 360), 0);

                float baseScale = Random.Range(item.scaleRange.x, item.scaleRange.y);
                Vector3 scale = new Vector3(baseScale * Random.Range(0.85f, 1.15f), baseScale * Random.Range(0.85f, 1.15f), baseScale * Random.Range(0.85f, 1.15f));

                GameObject crystal = Instantiate(item.crystalPrefab, position, rotation);
                crystal.transform.localScale = scale;
                crystal.transform.SetParent(transform);
            }
        }

        private void OnValidate()
        {
            if (crystalItemData.Length == 0) 
            { 
                return;
            }

            for (int i = 0; i < crystalItemData.Length; i++)
            {
                if (crystalItemData[i].scaleRange == Vector2.zero)
                {
                    crystalItemData[i].scaleRange = new Vector2(0.75f, 1.25f);
                }
                if (crystalItemData[i].radius == 0)
                {
                    crystalItemData[i].radius = 10;
                }
                if (crystalItemData[i].weight == 0)
                {
                    crystalItemData[i].weight = 0.5f;
                }
            }
        }
    }

    [Serializable]
    public struct CrystalItemData
    {
        [SerializeField, Tooltip("The crystal prefaba to use to generate the patch."), Required]
        public GameObject crystalPrefab;
        [SerializeField, Tooltip("The y offset for the crystals."), MinMaxSlider(minValue: -0.5f, maxValue: 0)]
        public Vector2 yOffset;
        [SerializeField, Tooltip("The minimum and maximum scale of the crystals."), MinMaxSlider(minValue: 0.5f, maxValue: 1.5f)]
        public Vector2 scaleRange;
        [SerializeField, Tooltip("The radius of the patch.")]
        public float radius;
        [SerializeField, Tooltip("The liklihood that this item will be chosen relative to any other items defined for this patch."), Range(0.01f, 1)]
        public float weight;
    }
}
