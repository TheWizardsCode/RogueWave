using NeoFPS;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static RogueWave.NanobotManager;

namespace RogueWave
{
    public class NanobotManagerUI : PlayerCharacterHudBase
    {
        [SerializeField, Tooltip("Image element for displaying the icon of the recipe currently being offered.")]
        private Image recipeIcon = null;
        [SerializeField, Tooltip("Text box for displaying the current status of the nanobots.")]
        private Text statusText = null;

        private NanobotManager nanobotManager;

        public override void OnPlayerCharacterChanged(ICharacter character)
        {
            if (nanobotManager != null)
            {
                nanobotManager.onStatusChanged -= OnStatusChanged;
                nanobotManager.onOfferChanged -= OnOfferChanged;
            }

            if (character as Component != null)
            {
                nanobotManager = character.GetComponent<NanobotManager>();
            }
            else
            {
                nanobotManager = null;
            }

            if (nanobotManager != null)
            {
                nanobotManager.onOfferChanged += OnOfferChanged;
                OnOfferChanged(nanobotManager.currentOffer);
                statusText.text = string.Empty;

                nanobotManager.onStatusChanged += OnStatusChanged;
                OnStatusChanged(nanobotManager.status);
                gameObject.SetActive(true);
            }
            else
            {
                gameObject.SetActive(false);
            }
        }

        private void OnOfferChanged(IRecipe currentOffer)
        {
            if (currentOffer != null)
            {
                recipeIcon.sprite = currentOffer.Icon;
            }
            else
            {
                recipeIcon.sprite = null;
            }
        }

        private void OnStatusChanged(Status status)
        {
            switch (status) {
                case Status.OfferingRecipe:
                    statusText.text = $"Offering {nanobotManager.currentOffer.DisplayName}";
                    break;
                case Status.RequestQueued:
                    statusText.text = $"Queued: {nanobotManager.currentOffer.DisplayName}";
                    break;
                case Status.Requesting:
                    statusText.text = $"Requesting {nanobotManager.currentOffer.DisplayName}";
                    break;
                case Status.RequestRecieved:
                    statusText.text = $"Waiting";
                    break;
            }
        }
    }
}