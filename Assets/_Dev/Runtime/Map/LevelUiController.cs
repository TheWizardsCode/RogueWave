using NaughtyAttributes;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace RogueWave
{
    public class LevelUiController : MonoBehaviour
    {
        [SerializeField, Tooltip("The text element to display the level description.")]
        TMP_Text descriptionText;
        [SerializeField, Tooltip("The button to launch into the selected level.")]
        Button launchButton;
        [SerializeField, Tooltip("The image element for the enemies icon of the level.")]
        Image iconPrototype;
        [SerializeField, Tooltip("The sprite to use for a level with a low Challenge Rating.")]
        Sprite lowCRSprite;
        [SerializeField, Tooltip("The sprite to use for a level with a medium Challenge Rating.")]
        Sprite highCRSprite;

        WfcDefinition levelDefinition;

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

            foreach (TileDefinition tile in levelDefinition.prePlacedTiles)
            {
                if (tile.icon == null)
                {
                    continue;
                }

                Image icon = Instantiate(iconPrototype, transform);
                icon.sprite = tile.icon;
                icon.name = tile.name;
            }
        }

        public void OnClick()
        {
            if (descriptionText != null)
            {
                descriptionText.text = levelDefinition.Description;
            }

            launchButton.gameObject.SetActive(true);
        }
    }
}