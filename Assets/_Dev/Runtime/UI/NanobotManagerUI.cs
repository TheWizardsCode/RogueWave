using NeoFPS;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using static RogueWave.NanobotManager;

namespace RogueWave
{
    public class NanobotManagerUI : PlayerCharacterHudBase
    {
        [SerializeField, Tooltip("Image elements for displaying the icon of the recipe currently being offered.")]
        [FormerlySerializedAs("recipeIcons")]
        private Image[] offerIcon = null;
        [SerializeField, Tooltip("Text boxes for displaying the name of the recipe currently being offered.")]
        private Text[] offerText = null;
        [SerializeField, Tooltip("Text boxes for displaying the current status of the nanobots.")]
        private Text statusText = null;

        private NanobotManager nanobotManager;

        protected override void Start()
        {
            OnStatusChanged(Status.Collecting);
            base.Start();
        }

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
                OnOfferChanged(nanobotManager.currentOfferRecipes);

                nanobotManager.onStatusChanged += OnStatusChanged;
                OnStatusChanged(nanobotManager.status);
                gameObject.SetActive(true);
            }
            else
            {
                gameObject.SetActive(false);
            }
        }

        private void OnOfferChanged(IRecipe[] currentOffers)
        {
            for (int i = 0; i < offerIcon.Length; i++)
            {
                if (currentOffers == null)
                {
                    offerIcon[i].sprite = null;
                } 
                else if (i < currentOffers.Length && currentOffers[i] != null)
                {
                    if (currentOffers[i].Icon != null)
                    {
                        offerIcon[i].sprite = currentOffers[i].Icon;
                    } else
                    {
                        if (currentOffers[i].HeroImage == null)
                        {
                            Debug.LogWarning("No icon or hero image for recipe " + currentOffers[i].DisplayName);
                            offerIcon[i].sprite = null;
                            continue;
                        }
                        else
                        {
                            offerIcon[i].sprite = Sprite.Create(currentOffers[i].HeroImage,
                                new Rect(0.0f, 0.0f, currentOffers[i].HeroImage.width,
                                currentOffers[i].HeroImage.height),
                                new Vector2(0.5f, 0.5f),
                                currentOffers[i].HeroImage.width / 50.0f);
                        }
                    }
                    offerText[i].text = currentOffers[i].DisplayName;
                }
                else
                {
                    offerIcon[i].sprite = null;
                    offerText[i].text = string.Empty;
                }
            }
        }

        private void OnStatusChanged(Status status)
        {
            bool showStatus = true;
            switch (status)
            {
                case Status.Collecting:
                    statusText.text = "Collect Resources";
                    break;
                case Status.OfferingRecipe:
                    statusText.text = "Offering";
                    showStatus = false;
                    break;
                case Status.Requesting:
                    statusText.text = "Requesting";
                    break;
                case Status.RequestRecieved:
                    statusText.text = "Recieved";
                    break;
                case Status.Building:
                    break;
            }

            statusText.enabled = showStatus;
            for (int i = 0; i < offerText.Length; i++)
            {
                if (string.IsNullOrEmpty(offerText[i].text))
                {
                    offerText[i].gameObject.SetActive(false);
                    offerIcon[i].gameObject.SetActive(false);
                } else
                {
                    offerText[i].gameObject.SetActive(!showStatus);
                    offerIcon[i].gameObject.SetActive(!showStatus);
                }
                
            }
        }
    }
}