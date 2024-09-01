using Codice.Client.BaseCommands;
using ModelShark;
using NeoFPS.Samples;
using RogueWave;
using RogueWave.GameStats;
using System.Collections;
using System.Collections.Generic;
using System.Text;
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
        [SerializeField, Tooltip("The UI element for displaying the name and description of this recipe.")]
        MultiInputLabel details;
        [SerializeField, Tooltip("The UI element for displaying the current stack size of this recipe.")]
        MultiInputLabel stackText;
        [SerializeField, Tooltip("The button that will be clicked to select this recipe.")]
        internal MultiInputButton selectionButton;

        internal int stackSize = 1;
        TooltipTrigger tooltip;

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

        private void Awake()
        {
            tooltip = GetComponent<TooltipTrigger>();
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
            tooltip.SetText("Description", details.description);

            selectionButton.label = $"Buy for {_recipe.BuyCost}";
            if (GameStatsManager.Instance.GetIntStat("RESOURCES").value >= _recipe.BuyCost)
            {
                selectionButton.interactable = true;
            }
            else
            {
                selectionButton.interactable = false;
                selectionButton.GetComponent<Image>().color = Color.red;
                selectionButton.label = $"Insufficient Funds ({_recipe.BuyCost})";
            }

            AddDependenciesToTooltip();
            AddUnlocksToTooltip();

            SetUpCommonElements();
        }

        private void AddUnlocksToTooltip()
        {
            // OPTIMIZATION: configure this at build time and cache in the recipe object
            StringBuilder sb = new StringBuilder();
            foreach (IRecipe candidate in RecipeManager.allRecipes.Values)
            {
                if (candidate.Dependencies.Length > 0)
                {
                    foreach (var dep in candidate.Dependencies)
                    {
                        if (dep == _recipe)
                        {
                            if (candidate.Dependencies.Length > 1)
                            {
                                sb.AppendLine($"{candidate.DisplayName} (partial)");
                            }
                            else
                            {
                                sb.AppendLine(candidate.DisplayName);
                            }

                            break;
                        }
                    }
                }
            }
            if (sb.Length > 0)
            {
                tooltip.SetText("Unlocks", sb.ToString());
            }
            else
            {
                tooltip.SetText("Unlocks", "None");
            }
        }

        private void AddDependenciesToTooltip()
        {
            StringBuilder sb = new StringBuilder();
            if (_recipe.Dependencies.Length > 0)
            {
                foreach (var dep in _recipe.Dependencies)
                {
                    sb.AppendLine(dep.DisplayName);
                }
                tooltip.SetText("Dependencies", sb.ToString());
            }
            else
            {
                tooltip.SetText("Dependencies", "None");
            }
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

            tooltip.SetText("Description", _recipe.Description);
            AddDependenciesToTooltip();
            AddUnlocksToTooltip();

            SetUpCommonElements();
        }

        private void SetUpCommonElements()
        {
            details.label = recipe.DisplayName;
            tooltip.SetText("Title", details.label);

            if (recipe.IsStackable)
            {
                stackText.label = $"{stackSize}/{recipe.MaxStack}";
                tooltip.SetText("Stack", $"({stackSize} of {recipe.MaxStack})");
            } else
            {
                stackText.label = string.Empty;
                tooltip.SetText("Stack", " ");              
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