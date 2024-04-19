using NeoSaveGames.SceneManagement;
using RogueWave.Tutorial;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace RogueWave.UI
{
    public class RW_LoadingScreen : MonoBehaviour
    {
        [Header("Visduals")]
        [SerializeField, Tooltip("The UI Element to display the hero image.")]
        private Image m_HeroImage = null;

        [Header("Hints")]
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
        [SerializeField, Tooltip("The audio listener for the loading screen (disable when activating the main scene)")]
        private AudioListener m_AudioListener = null;

        private void Start()
        {
            // Check if first run
            bool firstRun = (m_HintObject == null) || PlayerPrefs.GetInt("loading.first", 1) == 1;
            PlayerPrefs.SetInt("loading.first", 0);

            int loadingScreenDataIndex = Random.Range(0, m_LoadingScreenData.Length);

            if (firstRun)
            {
                ShowSaveWarning();
            }
            else
            {
                ShowHint(loadingScreenDataIndex);
                ShowHeroImage(loadingScreenDataIndex);
            }

            TutorialManager tutorialManager = GameObject.FindObjectOfType<TutorialManager>();
            if (tutorialManager.currentlyActiveStep.loadingScreenHeroImage != null)
            {
                ShowHeroImage(tutorialManager.currentlyActiveStep.loadingScreenHeroImage);
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
            if (m_HeroImage == null || m_LoadingScreenData.Length == 0)
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
            if (m_HeroImage == null || heroImage == null)
            {
                return;
            }
            else
            {
                float displayWidth = m_HeroImage.rectTransform.rect.width;
                float displayHeight = m_HeroImage.rectTransform.rect.height;
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
                m_HeroImage.sprite = heroImage;
                m_HeroImage.rectTransform.sizeDelta = new Vector2(imageWidth, imageHeight);
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