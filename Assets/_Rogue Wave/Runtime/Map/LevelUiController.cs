using ModelShark;
using NeoFPS;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using WizardsCode.RogueWave;

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
            launchButton.gameObject.SetActive(false);

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
                if (tile.icon == null)
                {
                    continue;
                }

                Image icon = Instantiate(iconPrototype, iconPrototype.transform.parent);
                icon.sprite = tile.icon;
                icon.name = tile.name;

                specialTileDescription.AppendLine($"  - {tile.DisplayName}");
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
            int number = 1;
            foreach (WaveDefinition wave in levelDefinition.waves)
            {
                sb.AppendLine($"  - {number}: {string.Join(", ", wave.enemies.Select(e => e.pooledEnemyPrefab.name))} (CR: {wave.ChallengeRating})");
                number++;
            }
            tooltip.SetText("Waves", sb.ToString());
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
            launchButton.gameObject.SetActive(true);
            foreach (Transform sibling in transform.parent)
            {
                LevelUiController siblingLevel = sibling.GetComponent<LevelUiController>();
                if (siblingLevel != null && siblingLevel != this)
                {
                    siblingLevel.launchButton.gameObject.SetActive(false);
                }
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