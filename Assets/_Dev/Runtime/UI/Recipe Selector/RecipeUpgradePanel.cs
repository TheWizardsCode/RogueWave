using NeoFPS.SinglePlayer;
using NeoFPS;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NeoSaveGames.SceneManagement;
using NaughtyAttributes;
using System;

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

        private List<IRecipe> offers = new List<IRecipe>();

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

            if (FpsSoloCharacter.localPlayerCharacter == null)
            {
                NeoFpsInputManager.captureMouseCursor = false;
            }

        }

        private void OnGUI()
        {
            if (offers == null || offers.Count == 0)
            {
                noOffersMessage.gameObject.SetActive(true);
                return;
            }
            else
            {
                noOffersMessage.gameObject.SetActive(false);
            }

            if (offers.Count != transform.childCount)
            {
                foreach (Transform child in transform)
                {
                    Destroy(child.gameObject);
                }

                foreach (IRecipe recipe in offers)
                {
                    RecipeCard recipeCard = Instantiate(recipeCardPrototype, transform);
                    recipeCard.recipe = recipe;
                    recipeCard.gameObject.SetActive(true);
                    recipeCard.selectionButton.onClick.AddListener(() => Select(recipeCard.recipe));
                }
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
            RogueLiteManager.persistentData.currentResources -= offer.BuyCost;
            RogueLiteManager.SaveProfile();
            
            offers.RemoveAll(o => o == offer);
        }
    }
}
