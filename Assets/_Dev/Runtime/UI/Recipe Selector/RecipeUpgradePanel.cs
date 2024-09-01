using NeoFPS.SinglePlayer;
using NeoFPS;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NeoSaveGames.SceneManagement;
using NaughtyAttributes;
using System;
using RogueWave;
using System.Linq;
using RogueWave.GameStats;

namespace RogueWave.UI
{
    public class RecipeUpgradePanel : MonoBehaviour
    {
        [Header("Offer Configuration")]
        [SerializeField, Tooltip("The number of offers that should be shown to the plauer. This could be modified by the game situation.")]
        int m_NumberOfOffers = 3;

        [Header("UI")]
        [SerializeField, Tooltip("Message to display if there are no upgrade offers available.")]
        RectTransform noOffersMessage;
        [SerializeField, Tooltip("The recipe card protoype for displaying an upgrade offer.")]
        RecipeCard recipeCardPrototype;

        HubController hubController;
        private List<IRecipe> offers = new List<IRecipe>();
        private bool isDirty;

        private NanobotManager nanobotManager
        {
            get
            {
                if (FpsSoloCharacter.localPlayerCharacter != null)
                {
                    return FpsSoloCharacter.localPlayerCharacter.GetComponent<NanobotManager>();
                }
                else
                {
                    return null;
                }
            }
        }

        void Start()
        {
            int weapons = 0;
            if (RogueLiteManager.persistentData.runNumber == 0)
            {
                weapons = 1;
            }

            offers = RecipeManager.GetOffers(m_NumberOfOffers, weapons);
            isDirty = true;

            if (FpsSoloCharacter.localPlayerCharacter == null)
            {
                NeoFpsInputManager.captureMouseCursor = false;
            }

            hubController = GetComponentInParent<HubController>();
        }

        private void OnGUI()
        {
            if (offers == null || offers.Count == 0)
            {
                foreach (Transform child in transform)
                {
                    Destroy(child.gameObject);
                }

                noOffersMessage.gameObject.SetActive(true);
                return;
            }
            else
            {
                noOffersMessage.gameObject.SetActive(false);
            }

            if (isDirty)
            {
                foreach (Transform child in transform)
                {
                    Destroy(child.gameObject);
                }

                foreach (IRecipe recipe in offers)
                {
                    RecipeCard card = Instantiate(recipeCardPrototype, transform);
                    card.recipe = recipe;

                    if (recipe.IsStackable)
                    {
                        card.stackSize = HubController.permanentRecipes.Count(r => r.UniqueID == recipe.UniqueID);
                        card.stackSize += HubController.temporaryRecipes.Count(r => r.UniqueID == recipe.UniqueID);
                        card.stackSize++;
                    }

                    card.selectionButton.onClick.AddListener(() => Select(card.recipe));

                    card.gameObject.SetActive(true);
                }

                isDirty = false;
            }
        }

        private void Select(IRecipe offer)
        {
            if (nanobotManager != null)
            {
                RogueLiteManager.runData.Add(offer);
                nanobotManager.Add(offer);
            }

            RogueLiteManager.persistentData.Add(offer);
            HubController.AddPermanentRecipe(offer);
            GameStatsManager.Instance.GetIntStat("RESOURCES").Subtract(offer.BuyCost);
            RogueLiteManager.persistentData.isDirty = true;

            offers.RemoveAll(o => o == offer);
            isDirty = true;

            GameLog.Info($"Bought {offer} in hub scene.");
        }
    }
}
