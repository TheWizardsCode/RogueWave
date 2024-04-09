using RogeWave;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

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

        [SerializeField, Tooltip("The type of card this is.")]
        internal RecipeCardType cardType;
        [SerializeField, Tooltip("The UI element for displaying the image for this recipe.")]
        Image image;
        [SerializeField, Tooltip("The UI element for displaying the name of this recipe.")]
        TMP_Text nameText;
        [SerializeField, Tooltip("The UI element for displaying the description of this recipe.")]
        TMP_Text descriptionText;
        [SerializeField, Tooltip("The UI element for displaying the current stack size of this recipe.")]
        TMP_Text stackText;
        [SerializeField, Tooltip("The button that will be clicked to select this recipe.")]
        internal Button selectionButton;

        internal int stackSize = 1;

        IRecipe _recipe;
        internal IRecipe recipe
        {
            get { return _recipe; }
            set
            {
                _recipe = value;

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
            descriptionText.text = recipe.Description;
            selectionButton.GetComponentInChildren<TMP_Text>().text = $"Buy for {_recipe.BuyCost}";
            if (RogueLiteManager.persistentData.currentResources >= _recipe.BuyCost)
            {
                selectionButton.interactable = true;
            }
            else
            {
                selectionButton.interactable = false;
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
            selectionButton.GetComponentInChildren<TMP_Text>().text = $"Permanent ({recipe.BuyCost})";
            if (RogueLiteManager.persistentData.currentResources < _recipe.BuyCost)
            {
                selectionButton.interactable = false;
            }
            SetUpCommonElements();
        }

        private void SetUpCommonElements()
        {
            nameText.text = recipe.DisplayName;

            if (stackText != null)
            {
                if (recipe.IsStackable)
                {
                    stackText.text = $"{stackSize}/{recipe.MaxStack}";
                } else
                {
                    stackText.text = string.Empty;
                }
            }
        }

        public void MakePermanent()
        {
            RogueLiteManager.runData.Remove(recipe);
            HubController.temporaryRecipes.Remove(recipe);

            RogueLiteManager.persistentData.Add(recipe);
            HubController.permanentRecipes.Add(recipe);

            RogueLiteManager.persistentData.currentResources -= recipe.BuyCost;

            GameLog.Info($"Made {recipe} permanent.");
        }
    }
}