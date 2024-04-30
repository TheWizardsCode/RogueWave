using NaughtyAttributes;
using NeoSaveGames.SceneManagement;
using RosgueWave.UI;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using WizardsCode.Common;
using Random = UnityEngine.Random;

namespace RogueWave.Tutorial
{
    public class TutorialManager : MonoBehaviour
    {
        [SerializeField, Tooltip("The loading scene that will be used to transition between scenes. When this scene is loaded some of the tutorial content will be displayed."), Scene]
        private string loadingScreen;
        [SerializeField, Tooltip("The tutorial steps that will be used to deliver tutorial content during scene loads and transitions."), FormerlySerializedAs("loadingScreenConfigurations"), Expandable]
        TutorialStep[] tutorialSteps;

        int[] sceneLoadCounts;
        AudioSource audioSource;
        internal TutorialStep currentlyActiveStep;

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
            audioSource = gameObject.GetComponent<AudioSource>();

            sceneLoadCounts = new int[SceneManager.sceneCountInBuildSettings];
        }

        private void OnEnable()
        {
            NeoSceneManager.onSceneLoadRequested += OnSceneLoadRequested;
            NeoSceneManager.onSceneLoaded += OnSceneLoaded;
        }

        private void OnDisable()
        {
            NeoSceneManager.onSceneLoadRequested -= OnSceneLoadRequested;
            NeoSceneManager.onSceneLoaded -= OnSceneLoaded;
        }

        private void OnSceneLoaded(int sceneIndex)
        {
            if (currentlyActiveStep == null)
            {
                return;
            }

            StartCoroutine(ExecuteSceneLoadedStep());
        }

        private void OnSceneLoadRequested(int sceneIndex, string sceneName)
        {
            if (RogueLiteManager.persistentData.tutorialNextStep >= tutorialSteps.Length)
            {
                currentlyActiveStep = null;
                return;
            }

            if (sceneIndex < 0) {
                sceneIndex = SceneManagement.SceneBuildIndexFromName(sceneName);
            }
            sceneLoadCounts[sceneIndex]++;

            TutorialStep step = tutorialSteps[RogueLiteManager.persistentData.tutorialNextStep];
            if (sceneName == step.sceneName && step.sceneLoadCount <= sceneLoadCounts[sceneIndex])
            {
                currentlyActiveStep = step;
                RogueLiteManager.persistentData.tutorialNextStep++;

                StartCoroutine(ExecuteLoadingStep());
            }
        }

        private IEnumerator ExecuteSceneLoadedStep()
        {
            float endTime = Time.time;
            AudioClip sceneClip = null;
            if (currentlyActiveStep.sceneClips.Length > 0)
            {
                sceneClip = currentlyActiveStep.sceneClips[Random.Range(0, currentlyActiveStep.sceneClips.Length)];
            }

            if (sceneClip != null)
            {
                endTime += sceneClip.length + 0.75f;
            }

            if (sceneClip == null)
            {
                this.currentlyActiveStep = null;
                yield break;
            }

            RogueWaveUIElement[] managedUIElements = null;
            Canvas canvas = FindObjectOfType<Canvas>();

            if (canvas != null)
            {
                managedUIElements = canvas.GetComponentsInChildren<RogueWaveUIElement>();
                if (managedUIElements != null)
                {
                    SetUIState(false, managedUIElements);
                }
            }

            yield return new WaitForSeconds(0.75f);

            audioSource.clip = sceneClip;
            audioSource.Play();

            while (Time.time < endTime)
            {
                yield return null;
            }

            if (managedUIElements != null)
            {
                SetUIState(true, managedUIElements);
            }

            this.currentlyActiveStep = null;
        }

        private IEnumerator ExecuteLoadingStep()
        {
            float oldDuration = NeoSceneManager.instance.minLoadScreenTime;
            NeoSceneManager.instance.minLoadScreenTime = currentlyActiveStep.loadingScreenDuration;
            
            audioSource.clip = currentlyActiveStep.loadingScreenClips[Random.Range(0, currentlyActiveStep.loadingScreenClips.Length)];
            audioSource.Play();

            float endTime = 0;
            if (currentlyActiveStep.loadingScreenDuration <= 0) {
                currentlyActiveStep.loadingScreenDuration = audioSource.clip.length + 1;
            }
            endTime = Time.time + currentlyActiveStep.loadingScreenDuration;

            while (Time.time < endTime)
            {
                yield return null;
            }

            NeoSceneManager.instance.minLoadScreenTime = oldDuration;
        }

        private void SetUIState(bool isActive, RogueWaveUIElement[] managedUIElements)
        {
            foreach (RogueWaveUIElement uiElement in managedUIElements)
            {
                if (uiElement.disableDuringTutorial)
                {
                    uiElement.gameObject.SetActive(isActive);
                }
            }
        }
    }
}