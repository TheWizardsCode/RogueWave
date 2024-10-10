using ModelShark;
using NeoFPS.Samples;
using RogueWave.GameStats;
using UnityEngine;
using UnityEngine.UI;
using WizardsCode.RogueWave;

namespace RogueWave.UI
{
    public class RecipeCard : MonoBehaviour
    {
        internal enum RecipeCardType
        {
            Offer,
            AcquiredPermanentMini,
            AcquiredTemporaryMini
        }

        [SerializeField, Tooltip("The UI style for this element.")]
        internal UiStyle style;
        [SerializeField, Tooltip("The type of card this is.")]
        internal RecipeCardType cardType;
        [SerializeField, Tooltip("The UI element for displaying the image for this recipe.")]
        Image image;
        [SerializeField, Tooltip("The UI element for displaying the name and description of this recipe.")]
        MultiInputLabel details;
        [SerializeField, Tooltip("The UI element for displaying the current stack size of this recipe.")]
        MultiInputLabel stackText;
        [SerializeField, Tooltip("The button that will be clicked to select this recipe.")]
        internal MultiInputButton selectionButton;

        internal int stackSize = 1;
        RecipeTooltipTrigger _tooltip;
        private RecipeTooltipTrigger tooltip
        {
            get
            {
                if (_tooltip == null)
                {
                    _tooltip = GetComponent<RecipeTooltipTrigger>();
                }
                return _tooltip;
            }
        }

        IRecipe _recipe;
        internal IRecipe recipe
        {
            get { return _recipe; }
            set
            {
                _recipe = value;
                if (cardType == RecipeCardType.Offer)
                {
                    tooltip.Initialize(_recipe, true);
                } else
                {
                    tooltip.Initialize(_recipe, false);
                }

                if (_recipe == null)
                {
                    gameObject.SetActive(false);
                } else
                {
                    gameObject.SetActive(true);
                }
            }
        }

        private void OnGUI()
        {
            if (recipe == null)
            {
                return;
            }

            switch (cardType)
            {
                case RecipeCardType.Offer:
                    SetupOfferCard();
                    break;
                case RecipeCardType.AcquiredPermanentMini:
                    SetupPermenantlyAcquiredCard();
                    break;
                case RecipeCardType.AcquiredTemporaryMini:
                    SetupAcquiredCard();
                    break;
            }
        }

        private void SetupOfferCard()
        {
            image.sprite = _recipe.HeroImage;

            details.description = recipe.Description;

            selectionButton.label = $"Encode with {_recipe.BuyCost} Resources";
            if (GameStatsManager.Instance.GetIntStat("RESOURCES").value >= _recipe.BuyCost)
            {
                selectionButton.interactable = true;
            }
            else
            {
                selectionButton.interactable = false;
                selectionButton.GetComponent<Image>().color = style.colours.disabled;
                selectionButton.label = $"Insufficient Resources ({_recipe.BuyCost})";
            }

            SetUpCommonElements();
        }

        private void SetupPermenantlyAcquiredCard()
        {
            selectionButton.gameObject.SetActive(false);
            SetupAcquiredCard();
        }

        private void SetupAcquiredCard()
        {
            image.sprite = _recipe.Icon;
            selectionButton.label = $"Permanent ({recipe.BuyCost})";
            if (GameStatsManager.Instance.GetIntStat("RESOURCES").value < _recipe.BuyCost)
            {
                selectionButton.interactable = false;
                selectionButton.GetComponent<Image>().color = Color.red;
            }

            SetUpCommonElements();
        }

        private void SetUpCommonElements()
        {
            details.label = recipe.DisplayName;

            if (recipe.IsStackable)
            {
                stackText.label = $"{stackSize}/{recipe.MaxStack}";
            } else
            {
                stackText.label = string.Empty;
            }
        }

        public void MakePermanent()
        {
            RogueLiteManager.runData.Remove(recipe);
            HubController.RemoveTemporaryRecipe(recipe);
            RogueLiteManager.persistentData.Add(recipe);
            HubController.AddPermanentRecipe(recipe);

            if (recipe is WeaponRecipe weapon)
            {
                if (weapon.ammoRecipe != null)
                {
                    RogueLiteManager.runData.Remove(weapon.ammoRecipe);
                    HubController.RemoveTemporaryRecipe(weapon.ammoRecipe);
                    RogueLiteManager.persistentData.Add(weapon.ammoRecipe);
                    HubController.AddPermanentRecipe(weapon.ammoRecipe);
                }
            }

            GameStatsManager.Instance.GetIntStat("RESOURCES").Subtract(recipe.BuyCost);

            GameLog.Info($"Made {recipe} permanent.");
        }
    }
}