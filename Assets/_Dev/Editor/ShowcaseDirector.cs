using NeoFPS;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using static Codice.Client.BaseCommands.BranchExplorer.ExplorerData.BrExTreeBuilder.BrExFilter;

namespace RogueWave.Editor
{
    public class ShowcaseDirector : MonoBehaviour
    {
        [SerializeField, Tooltip("The parent transforms to spawn the enemies under.")]
        BasicEnemyShowcase[] enemyShowcaseSpots;

        private void Start()
        {
            List<BasicEnemyController> enemies = GetAllObjects<BasicEnemyController>();
            enemies = enemies.OrderBy(e => e.challengeRating).ToList();
            StartCoroutine(ShowcaseEnemies(enemies));
        }

        private List<T> GetAllObjects<T>() where T : MonoBehaviour
        {
            string[] guids = AssetDatabase.FindAssets("t:Prefab");
            List<T> results = new List<T>();

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);

                T enemy = prefab.GetComponent<T>();
                if (enemy != null)
                {
                    results.Add(enemy);
                }
            }

            return results;
        }

        private IEnumerator ShowcaseEnemies(List<BasicEnemyController> enemies)
        {
            foreach (BasicEnemyController enemy in enemies)
            {
                if (enemy.includeInShowcase == false)
                {
                    continue;
                }

                CleanupScene();

                List<Coroutine> coroutines = new List<Coroutine>();
                foreach (BasicEnemyShowcase showcase in enemyShowcaseSpots)
                {
                    BasicEnemyController newEnemy = Instantiate(enemy, showcase.transform);
                    newEnemy.transform.localPosition = Vector3.zero;
                    newEnemy.transform.localRotation = Quaternion.identity;

                    coroutines.Add(StartCoroutine(showcase.StartShowcase()));
                }

                foreach (Coroutine coroutine in coroutines)
                {
                    yield return coroutine;
                }

                yield return new WaitForSeconds(2.5f);
            }
        }

        private void CleanupScene()
        {
            foreach (BasicEnemyShowcase showcase in enemyShowcaseSpots)
            {
                PickupTriggerZone[] pickups = FindObjectsOfType<PickupTriggerZone>();
                foreach (PickupTriggerZone pickup in pickups)
                {
                    Destroy(pickup.gameObject);
                }

                foreach (Transform child in showcase.transform)
                {
                    Destroy(child.gameObject);
                }
            }
        }
    }
}