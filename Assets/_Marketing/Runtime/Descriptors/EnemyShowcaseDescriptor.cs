#if UNITY_EDITOR
using RogueWave;
using RogueWave.Editor;
using System;
using System.Collections;
using UnityEngine;

namespace WizardsCode.Marketing
{
    [CreateAssetMenu(fileName = "New Enemy Showcase Descriptor", menuName = "Wizards Code/Marketing/Enemy Showcase Descriptor")]
    public class EnemyShowcaseDescriptor : GifAssetDescriptor
    {
        [Header("Enemy Showcase Settings")]
        [SerializeField, Tooltip("The enemy prefab to showcase.")]
        private BasicEnemyController enemyPrefab;

        public override IEnumerator GenerateAsset(Action callback = null)
        {
            StartShowcase();
            yield return base.GenerateAsset(() => StopShowcase(callback));
        }

        private void StartShowcase()
        {
            // Setup the showcase slots
            BasicEnemyShowcase[] showcaseSlot = FindObjectsOfType<BasicEnemyShowcase>(true);
            foreach (BasicEnemyShowcase showcase in showcaseSlot)
            {
                foreach (Transform child in showcase.transform)
                {
                    Destroy(child.gameObject);
                }

                BasicEnemyController enemy = Instantiate(enemyPrefab, showcase.transform);
                enemy.ResourceDropChange = 0;

                BasicMovementController movementController = enemy.GetComponent<BasicMovementController>();
                if (movementController != null)
                {
                    movementController.enabled = false;
                }

                CoroutineHelper.Instance.StartCoroutine(showcase.StartShowcase());
            }
        }

        private void StopShowcase(Action callback = null)
        {
            BasicEnemyShowcase[] showcaseSlot = FindObjectsOfType<BasicEnemyShowcase>(true);
            foreach (BasicEnemyShowcase showcase in showcaseSlot)
            {
                showcase.StopShowcase();
            }

            callback?.Invoke();
        }
    }
}
#endif