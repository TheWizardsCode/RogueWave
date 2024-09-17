using NeoFPS;
using NeoFPS.SinglePlayer;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace RogueWave
{
    [Obsolete("This was used as part of the `RW_PreSpawnPopup` UI in an earlier version. It has been replaced by the `LevelManu` prefab at this point. We can probably get rid of this code now. (StO Aug 11th 2024))")]
    public class EnemyDetailsTab : InstantSwitchTabBase
    {
        [SerializeField, RequiredObjectProperty, Tooltip("The list of enemies in this level.")]
        ScrollRect enemiesScrollRect = null;
        [SerializeField, Tooltip("The UI element to use to represent an enemy. This will be cloned for each enemy.")]
        EnemyDetailsUIController enemyDetailsPrefab;

        private HashSet<BasicEnemyController> addedEnemies = new HashSet<BasicEnemyController>();

        public override string tabName => "Enemies in Area";

        void Start()
        {
            ConfigureUI();
        }

        private void ConfigureUI()
        {
            foreach (RectTransform child in enemiesScrollRect.content)
            {
                Destroy(child.gameObject);
            }
            addedEnemies.Clear();

            var sortedEnemies = new List<BasicEnemyController>();

            foreach (WaveDefinition wave in ((RogueWaveGameMode)gameMode).currentLevelDefinition.Waves)
            {
                foreach (EnemySpawnConfiguration enemySpawn in wave.enemies)
                {
                    BasicEnemyController enemy = enemySpawn.pooledEnemyPrefab.GetComponent<BasicEnemyController>();
                    if (!addedEnemies.Contains(enemy))
                    {
                        sortedEnemies.Add(enemy);
                        addedEnemies.Add(enemy);
                    }
                }
            }

            // Sort enemies by their name
            sortedEnemies = sortedEnemies.OrderBy(enemy => enemy.name).ToList();

            foreach (var enemy in sortedEnemies)
            {
                EnemyDetailsUIController element = Instantiate(enemyDetailsPrefab, enemiesScrollRect.content);
                element.enemy = enemy;
                element.gameObject.SetActive(true);
            }
        }
    }
}
