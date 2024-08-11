using NaughtyAttributes;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace RogueWave
{
    [ExecuteAlways]
    public class LevelUiController : MonoBehaviour
    {
        [SerializeField, Tooltip("The image element for the enemies icon of the level.")]
        Image iconPrototype;
        [SerializeField, Tooltip("The sprite to use for a level with a low Challenge Rating.")]
        Sprite lowCRSprite;
        [SerializeField, Tooltip("The sprite to use for a level with a medium Challenge Rating.")]
        Sprite highCRSprite;

        WfcDefinition levelDefinition;
        TMP_Text descriptionText;

        public void Init(WfcDefinition definition, TMP_Text descriptionText)
        {
            this.levelDefinition = definition;
            this.descriptionText = descriptionText;

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
        }
    }
}