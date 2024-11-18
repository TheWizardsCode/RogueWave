using ModelShark;
using NaughtyAttributes;
using NeoSaveGames.SceneManagement;
using RosgueWave.UI;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using WizardsCode.Common;
using WizardsCode.Tutorial;
using Random = UnityEngine.Random;

namespace RogueWave.Tutorial
{
    [RequireComponent(typeof(AudioSource))]
    public class TutorialManager : MonoBehaviour
    {
        internal const string SCENE_PROGRESS_KEY_PREFIX = "SceneLoadCount_";

        [SerializeField, Tooltip("The scene to play if no profiles exist. This is the start of the tutorial."), Scene, BoxGroup("Scenes")]
        private string introScene;
        [SerializeField, Tooltip("The loading scene that will be used to transition between scenes. When this scene is loaded some of the tutorial content will be displayed."), Scene, BoxGroup("Scenes")]
        private string loadingScreen;


        [SerializeField, Tooltip("The tooltip style for tutorial messages."), BoxGroup("Tutorial Tooltips"), ShowIf("showWelcomeTooltip")]
        internal TooltipStyle tutorialTooltipStyle;
        [SerializeField, Tooltip("The minimum width for the tutorial tooltips."), BoxGroup("Tutorial Tooltips"), ShowIf("showWelcomeTooltip")]
        internal int tutorialTooltipMinWidth = 300;
        [SerializeField, Tooltip("The maximum width for the tutorial tooltips."), BoxGroup("Tutorial Tooltips"), ShowIf("showWelcomeTooltip")]
        internal int tutorialTooltipMaxWidth = 500;
        [SerializeField, Tooltip("If true the tutorial manager will show the welcome tooltop on first load."), BoxGroup("Welcome Tooltip")]
        private bool showWelcomeTooltip = true;
        [SerializeField, Tooltip("The tutorial step to use as the welcome message."), BoxGroup("Welcome Tooltip"), ShowIf("showWelcomeTooltip")]
        private PopupTutorialStep welcomeTooltip;

        [SerializeField, Tooltip("Show the debug tooling for the tutorial system."), BoxGroup("Debug")]
        private bool showDebug = false;
        [SerializeField, Tooltip("Should the tutorial be reset when the game starts?"), BoxGroup("Debug"), ShowIf("showDebug")]
        private bool resetTutorial = false;
        
        TutorialStep[] tutorialSteps;

        int[] sceneLoadCounts;
        AudioSource audioSource;
        internal TutorialStep currentlyActiveStep;

        private void Awake()
        {
#if UNITY_EDITOR
            if (resetTutorial)
            {
                ClearTutorialProgress();
            }
#endif

            DontDestroyOnLoad(gameObject);
            audioSource = gameObject.GetComponent<AudioSource>();

            sceneLoadCounts = new int[SceneManager.sceneCountInBuildSettings];
            for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
            {
                sceneLoadCounts[i] = PlayerPrefs.GetInt(SCENE_PROGRESS_KEY_PREFIX + i.ToString(), 0);
                // Debug.LogError($"Scene Load Count for SceneIndex {i} is {sceneLoadCounts[i]}");
            }

            tutorialSteps = Resources.LoadAll<TutorialStep>("Tutorial");
            foreach (ITutorialStep step in tutorialSteps)
            {
                step.TutorialManager = this;

                if (!step.TriggerBySceneLoad)
                {
                    step.TriggeringEvent.RegisterListener(() => step.Execute());}
            }

            NeoSceneManager.onSceneLoadRequested += OnSceneLoadRequested;
            NeoSceneManager.onSceneLoaded += OnSceneLoaded;
        }

        private void Start()
        {
            RogueLiteManager.UpdateAvailableProfiles();
            if (!RogueLiteManager.hasProfile)
            {
                NeoSceneManager.LoadScene(introScene);
            }

            if (showWelcomeTooltip)
            {
                if (welcomeTooltip == null)
                {
                    Debug.LogError("No welcome tooltip has been set in the Tutorial Manager, yet showWelcomeTooltip is set to true.");
                }
                else
                {
                    welcomeTooltip.Execute();
                }
            }
        }

        private void OnDisable()
        {
            NeoSceneManager.onSceneLoadRequested -= OnSceneLoadRequested;
            NeoSceneManager.onSceneLoaded -= OnSceneLoaded;

            foreach (TutorialStep step in tutorialSteps)
            {
                if (!step.TriggerBySceneLoad)
                {
                    step.TriggeringEvent.UnregisterListener(step.Execute);
                }
            }
        }

        private void OnSceneLoaded(int sceneIndex)
        {
            //GameLog.Log($"Scene loaded (index: {sceneIndex}");

            foreach (TutorialStep step in tutorialSteps)
            {
                if (!step.TriggerBySceneLoad)
                {
                    continue;
                }

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
                if (!step.TriggerBySceneLoad) 
                { 
                    continue; 
                }

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

            endTime += currentlyActiveStep.duration;

            currentlyActiveStep.Execute();

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

            if (currentlyActiveStep.audioClips.Length > 0)
            {
                audioSource.clip = currentlyActiveStep.audioClips[Random.Range(0, currentlyActiveStep.audioClips.Length)];
                audioSource.Play();
            }

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
        [Button, ShowIf("showDebug")]
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