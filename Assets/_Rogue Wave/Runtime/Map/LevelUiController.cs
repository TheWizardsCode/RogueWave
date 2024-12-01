using ModelShark;
using NeoFPS;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RogueWave
{
    [RequireComponent(typeof(TooltipTrigger))]
    public class LevelUiController : MonoBehaviour
    {
        [Header("Level Selection")]
        [SerializeField, Tooltip("The button to launch into the selected level.")]
        Button launchButton;
        [SerializeField, Tooltip("The image element for the enemies icon of the level.")]
        Image iconPrototype;
        [SerializeField, Tooltip("The sprite to use for a level with a low Challenge Rating.")]
        Sprite lowCRSprite;
        [SerializeField, Tooltip("The sprite to use for a level with a medium Challenge Rating.")]
        Sprite highCRSprite;

        [Header("Level Information")]
        [SerializeField, Tooltip("The text element to display the level description.")]
        TMP_Text descriptionText;

        [Header("Enemies Information")]
        [SerializeField, RequiredObjectProperty, Tooltip("The list of enemies in this level.")]
        ScrollRect enemiesScrollRect = null;
        [SerializeField, Tooltip("The UI element to use to represent an enemy. This will be cloned for each enemy.")]
        EnemyDetailsUIController enemyDetailsPrototype;

        // An event that will be fired when the level is clicked on.
        public event System.Action<LevelUiController> OnLevelClicked;

        internal WfcDefinition levelDefinition;
        private HashSet<BasicEnemyController> activeEnemies = new HashSet<BasicEnemyController>();

        public void Init(WfcDefinition definition)
        {
            gameObject.SetActive(true);
            this.levelDefinition = definition;

            iconPrototype.name = "Enemy Icon";
            if (levelDefinition.challengeRating < 100)
            {
                iconPrototype.sprite = lowCRSprite;
            } else
            {
                iconPrototype.sprite = highCRSprite;
            }

            StringBuilder specialTileDescription = new StringBuilder();
            foreach (TileDefinition tile in levelDefinition.prePlacedTiles)
            {
                if (!tile.DisplayName.Contains("Player Spawn"))
                {
                    specialTileDescription.AppendLine($"  - {tile.DisplayName}");
                }

                if (tile.icon == null)
                {
                    continue;
                }

                Image icon = Instantiate(iconPrototype, iconPrototype.transform.parent);
                icon.sprite = tile.icon;
                icon.name = tile.name;
            }

            TooltipTrigger tooltip = GetComponent<TooltipTrigger>();
            tooltip.SetText("Size", levelDefinition.mapSize.ToString());
            tooltip.SetText("ChallengeRating", levelDefinition.challengeRating.ToString());
            if (specialTileDescription.Length > 0)
            {
                tooltip.SetText("SpecialTiles", specialTileDescription.ToString());
            } 
            else
            {
                tooltip.SetText("SpecialTiles", " ");
            }

            StringBuilder sb = new StringBuilder();
            if (levelDefinition.waves.Length == 0)
            {
                sb.AppendLine("  - No enemy spawner detected");
            }
            else
            {
                int number = 1;
                foreach (WaveDefinition wave in levelDefinition.waves)
                {
                    sb.AppendLine($"  - {number}: {string.Join(", ", wave.enemies.Select(e => e.pooledEnemyPrefab.name))} (CR: {wave.ChallengeRating})");
                    number++;
                }
            }
            tooltip.SetText("Waves", sb.ToString());

            SetLaunchButtonStatus();
        }

        public void OnClick()
        {
            UpdateLevelInformationPanel();
            UpdateEnemyInformationPanel();
            ConfigureLaunchButtons();

            OnLevelClicked?.Invoke(this);
        }

        private void ConfigureLaunchButtons()
        {
            SetLaunchButtonStatus();

            foreach (Transform sibling in transform.parent)
            {
                LevelUiController siblingLevel = sibling.GetComponent<LevelUiController>();
                if (siblingLevel != null && siblingLevel != this)
                {
                    siblingLevel.launchButton.interactable = false;
                }
            }
        }

        private void SetLaunchButtonStatus()
        {
            if (levelDefinition.IsUnlocked)
            {
                if (levelDefinition.Completed)
                {
                    launchButton.GetComponentInChildren<TMP_Text>().text = "Revisit";
                    launchButton.interactable = true;
                }
                else
                {
                    launchButton.GetComponentInChildren<TMP_Text>().text = "Go!";
                    launchButton.interactable = true;
                }
            }
            else
            {
                launchButton.GetComponentInChildren<TMP_Text>().text = "Locked";
                launchButton.interactable = false;
            }
        }

        private void UpdateLevelInformationPanel()
        {
            if (descriptionText != null)
            {
                descriptionText.text = levelDefinition.Description;
            }
        }

        private void UpdateEnemyInformationPanel()
        {
            foreach (RectTransform child in enemiesScrollRect.content)
            {
                Destroy(child.gameObject);
            }
            activeEnemies.Clear();
            var sortedEnemies = new List<BasicEnemyController>();

            foreach (WaveDefinition wave in levelDefinition.Waves)
            {
                foreach (EnemySpawnConfiguration enemySpawn in wave.enemies)
                {
                    BasicEnemyController enemy = enemySpawn.pooledEnemyPrefab.GetComponent<BasicEnemyController>();
                    if (!activeEnemies.Contains(enemy))
                    {
                        sortedEnemies.Add(enemy);
                        activeEnemies.Add(enemy);
                    }
                }
            }

            if (sortedEnemies.Count > 0)
            {
                // Sort enemies by their name
                sortedEnemies = sortedEnemies.OrderBy(enemy => enemy.name).ToList();

                foreach (var enemy in sortedEnemies)
                {
                    EnemyDetailsUIController element = Instantiate(enemyDetailsPrototype, enemiesScrollRect.content);
                    element.enemy = enemy;
                    element.gameObject.SetActive(true);
                }
            }
        }
    }
}