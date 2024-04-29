using NeoSaveGames.SceneManagement;
using RogueWave.Tutorial;
using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace RogueWave.UI
{
    public class RW_LoadingScreen : MonoBehaviour
    {
        [Header("Visuals")]
        [SerializeField, Tooltip("The tutorial container. This will be shown or hidden depending on whether there is a tutorial step to show.")]
        private RectTransform m_TutorialContainer = null;
        [SerializeField, Tooltip("The none tutorial container. This will be shown or hidden depending on whether there is a tutorial step to show.")]
        private RectTransform m_NoneTutorialContainer = null;

        [Header("Tutorial Components")]
        [SerializeField, Tooltip("The UI Element to display the hero image on tutorial loading screens.")]
        private Image m_TutorialHeroImage = null;
        [SerializeField, Tooltip("The UI Element to display the story text if any is available.")]
        private TextMeshProUGUI m_StoryText = null;

        [Header("None Tutorial Components")]
        [SerializeField, Tooltip("The UI Element to display the hero image on none tutorial loading scenes.")]
        private Image m_NoneTutorialHeroImage = null;
        [SerializeField, Tooltip("The UI text for the hints")]
        private Text m_HintText = null;
        [SerializeField, Tooltip("The object to enable if showing hints")]
        private GameObject m_HintObject = null;
        [SerializeField, Tooltip("Loading screen data that will be shown if no tutorial step is active.")]
        LoadingScreenData[] m_LoadingScreenData = default;

        [Header("Save Warning")]
        [SerializeField, Tooltip("The object to enable if showing the save warning")]
        private GameObject m_SaveWarningObject = null;

        [Header("Audio Listener")]
        [SerializeField, Tooltip("The audio listener for the loading screen (disabled when activating the main scene)")]
        private AudioListener m_AudioListener = null;
        private bool isTutorial;

        private void Start()
        {
            // Check if first run
            bool firstRun = (m_HintObject == null) || PlayerPrefs.GetInt("loading.first", 1) == 1;
            PlayerPrefs.SetInt("loading.first", 0);

            if (firstRun)
            {
                ShowSaveWarning();
            }

            TutorialManager tutorialManager = GameObject.FindObjectOfType<TutorialManager>();
            if (tutorialManager != null)
            {
                isTutorial = tutorialManager.currentlyActiveStep != null;
            } else
            {
                isTutorial = false;
            }

            if (isTutorial)
            {
                ShowHeroImage(tutorialManager.currentlyActiveStep.loadingScreenHeroImage);
                m_StoryText.text = tutorialManager.currentlyActiveStep.script;
                m_TutorialContainer.gameObject.SetActive(true);
                m_NoneTutorialContainer.gameObject.SetActive(false);
            } else
            {
                int loadingScreenDataIndex = Random.Range(0, m_LoadingScreenData.Length);
                ShowHint(loadingScreenDataIndex);
                ShowHeroImage(loadingScreenDataIndex);
                m_TutorialContainer.gameObject.SetActive(false);
                m_NoneTutorialContainer.gameObject.SetActive(true);
            }

            NeoSceneManager.preSceneActivation += PreSceneActivation;
            NeoSceneManager.onSceneLoadProgress += OnSceneLoadProgress;
        }

        protected virtual void OnSceneLoadProgress(float progress)
        {
        }

        protected void OnDestroy()
        {
            NeoSceneManager.preSceneActivation -= PreSceneActivation;
            NeoSceneManager.onSceneLoadProgress -= OnSceneLoadProgress;
        }

        void PreSceneActivation()
        {
            StartCoroutine(DelayedDisableAudioListener());
        }

        IEnumerator DelayedDisableAudioListener()
        {
            yield return new WaitForEndOfFrame();
            if (m_AudioListener != null)
                m_AudioListener.enabled = false;
        }

        void ShowHint(int loadingDataIndex)
        {
            if (m_HintObject == null || m_LoadingScreenData.Length == 0)
                ShowSaveWarning();
            else
            {
                // Hide save warning object
                if (m_SaveWarningObject != null)
                    m_SaveWarningObject.SetActive(false);

                // Show hint
                m_HintText.text = m_LoadingScreenData[loadingDataIndex].hint;
                m_HintObject.SetActive(true);
            }
        }

        void ShowHeroImage(int loadingDataIndex)
        {
            if (m_TutorialHeroImage == null || m_LoadingScreenData.Length == 0)
            {
                return;
            }
            else
            {
                ShowHeroImage(m_LoadingScreenData[loadingDataIndex].heroImage);
            }
        }

        void ShowHeroImage(Sprite heroImage)
        {
            if (m_TutorialHeroImage == null || heroImage == null)
            {
                return;
            }
            else
            {
                float displayWidth = 0;
                float displayHeight = 0;
                if (isTutorial)
                {
                    displayWidth = m_TutorialHeroImage.rectTransform.rect.width;
                    displayHeight = m_TutorialHeroImage.rectTransform.rect.height;
                }
                else
                {
                    displayWidth = m_NoneTutorialHeroImage.rectTransform.rect.width;
                    displayHeight = m_NoneTutorialHeroImage.rectTransform.rect.height;
                }

                float imageWidth = heroImage.rect.width;
                float imageHeight = heroImage.rect.height;
                if (imageWidth != displayWidth || imageHeight != displayHeight)
                {
                    float aspectRatio = imageWidth / imageHeight;
                    float scaleFactor;
                    if (displayWidth / displayHeight > aspectRatio)
                    {
                        scaleFactor = displayHeight / imageHeight;
                    }
                    else
                    {
                        scaleFactor = displayWidth / imageWidth;
                    }
                    imageWidth *= scaleFactor;
                    imageHeight *= scaleFactor;
                }

                if (isTutorial)
                {
                    m_TutorialHeroImage.sprite = heroImage;
                    m_TutorialHeroImage.rectTransform.sizeDelta = new Vector2(imageWidth, imageHeight);
                }
                else
                {
                    m_NoneTutorialHeroImage.sprite = heroImage;
                    m_NoneTutorialHeroImage.rectTransform.sizeDelta = new Vector2(imageWidth, imageHeight);
                }
            }
        }   

        void ShowSaveWarning()
        {
            // Show save warning & hide hints
            if (m_HintObject != null)
                m_HintObject.SetActive(false);
            if (m_SaveWarningObject != null)
                m_SaveWarningObject.SetActive(true);
        }
    }

    [Serializable]
    struct LoadingScreenData
    {
        public Sprite heroImage;
        public string hint;
    }
}