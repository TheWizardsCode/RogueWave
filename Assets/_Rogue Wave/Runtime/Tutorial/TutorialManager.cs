using NaughtyAttributes;
using NeoSaveGames.SceneManagement;
using RosgueWave.UI;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using WizardsCode.CommandTerminal;
using WizardsCode.Common;
using Random = UnityEngine.Random;

namespace RogueWave.Tutorial
{
    public class TutorialManager : MonoBehaviour
    {
        internal const string SCENE_PROGRESS_KEY_PREFIX = "SceneLoadCount_";
        [SerializeField, Tooltip("The loading scene that will be used to transition between scenes. When this scene is loaded some of the tutorial content will be displayed."), Scene]
        private string loadingScreen;
        [SerializeField, Tooltip("The scene to play if no profiles exist. This is the start of the tutorial."), Scene]
        private string introScene;

        TutorialStep[] tutorialSteps;

        int[] sceneLoadCounts;
        AudioSource audioSource;
        internal TutorialStep currentlyActiveStep;

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
            audioSource = gameObject.GetComponent<AudioSource>();

            sceneLoadCounts = new int[SceneManager.sceneCountInBuildSettings];
            for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
            {
                sceneLoadCounts[i] = PlayerPrefs.GetInt(SCENE_PROGRESS_KEY_PREFIX + i.ToString(), 0);
                // Debug.LogError($"Scene Load Count for SceneIndex {i} is {sceneLoadCounts[i]}");
            }

            tutorialSteps = Resources.LoadAll<TutorialStep>("Tutorial");
        }

        private void Start()
        {
            NeoSceneManager.onSceneLoadRequested += OnSceneLoadRequested;
            NeoSceneManager.onSceneLoaded += OnSceneLoaded;

            RogueLiteManager.UpdateAvailableProfiles();
            if (!RogueLiteManager.hasProfile)
            {
                NeoSceneManager.LoadScene(introScene);
            }
        }

        private void OnDisable()
        {
            NeoSceneManager.onSceneLoadRequested -= OnSceneLoadRequested;
            NeoSceneManager.onSceneLoaded -= OnSceneLoaded;
        }

        private void OnSceneLoaded(int sceneIndex)
        {
            GameLog.Log($"Scene loaded (index: {sceneIndex}");

            foreach (TutorialStep step in tutorialSteps)
            {
                if (!step.isLoadingScene 
                    && sceneIndex == SceneManagement.SceneBuildIndexFromName(step.sceneName) 
                    && step.sceneLoadCount == sceneLoadCounts[sceneIndex])
                {
                    currentlyActiveStep = step;

                    StartCoroutine(ExecuteSceneLoadedStep());
                }
            }
        }

        private void OnSceneLoadRequested(int sceneIndex, string sceneName)
        {
            if (sceneIndex < 0) {
                sceneIndex = SceneManagement.SceneBuildIndexFromName(sceneName);
            }

            GameLog.Log($"Scene load requested {sceneName} (index: {sceneIndex}");

            sceneLoadCounts[sceneIndex]++;
            PlayerPrefs.SetInt(SCENE_PROGRESS_KEY_PREFIX + sceneIndex.ToString(), sceneLoadCounts[sceneIndex]);
            currentlyActiveStep = null;

            foreach (TutorialStep step in tutorialSteps)
            {
                if (step.isLoadingScene && sceneName == step.sceneName && step.sceneLoadCount == sceneLoadCounts[sceneIndex])
                {
                    currentlyActiveStep = step;

                    StartCoroutine(ExecuteSceneLoadingStep());
                    return;
                }
            }

            NeoSceneManager.instance.minLoadScreenTime = 3;
        }

        private IEnumerator ExecuteSceneLoadedStep()
        {
            GameLog.Log($"Executing scene loaded tutorial step in {currentlyActiveStep.sceneName}");

            float endTime = Time.time;
            AudioClip sceneClip = null;
            if (currentlyActiveStep.audioClips.Length > 0)
            {
                sceneClip = currentlyActiveStep.audioClips[Random.Range(0, currentlyActiveStep.audioClips.Length)];
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

        private IEnumerator ExecuteSceneLoadingStep()
        {
            GameLog.Log($"Executing scene loading tutorial step in {currentlyActiveStep.sceneName}");

            float oldDuration = NeoSceneManager.instance.minLoadScreenTime;
            NeoSceneManager.instance.minLoadScreenTime = currentlyActiveStep.duration;
            
            audioSource.clip = currentlyActiveStep.audioClips[Random.Range(0, currentlyActiveStep.audioClips.Length)];
            audioSource.Play();

            float endTime = 0;
            if (currentlyActiveStep.duration <= 0) {
                currentlyActiveStep.duration = audioSource.clip.length + 1;
            }
            endTime = Time.time + currentlyActiveStep.duration;

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

#if UNITY_EDITOR
        [UnityEditor.MenuItem("Tools/Rogue Wave/Profiles/Reset Tutorial Progress", priority = 1)]
#endif
        public static void ClearTutorialProgress()
        {
            for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
            {
                PlayerPrefs.DeleteKey(SCENE_PROGRESS_KEY_PREFIX + i.ToString());
            }
        }
    }
}