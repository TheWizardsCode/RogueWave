using ModelShark;
using NaughtyAttributes;
using NeoFPS;
using NeoSaveGames.SceneManagement;
using RosgueWave.UI;
using System;
using System.Collections;
using System.Runtime.Remoting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using WizardsCode.Common;
using WizardsCode.Tutorial;

namespace RogueWave.Story
{
    [RequireComponent(typeof(AudioSource))]
    public class StoryManager : MonoBehaviour
    {
        internal const string SCENE_PROGRESS_KEY_PREFIX = "SceneLoadCount_";

        [SerializeField, Tooltip("The scene to play if no profiles exist. This is the start of the story."), Scene, BoxGroup("Scenes")]
        private string introScene;
        [SerializeField, Tooltip("The loading scene that will be used to transition between scenes. When this scene is loaded some of the story content will be displayed."), Scene, BoxGroup("Scenes")]
        private string loadingScreen;

        [SerializeField, Tooltip("The tooltip style for story messages."), BoxGroup("Story Tooltips"), ShowIf("showWelcomeTooltip"), FormerlySerializedAs("tutorialTooltipStyle")]
        internal TooltipStyle tutorialTooltipStyle;
        [SerializeField, Tooltip("The minimum width for the story tooltips."), BoxGroup("Story Tooltips"), ShowIf("showWelcomeTooltip"), FormerlySerializedAs("tutorialTooltipMinWidth")]
        internal int tutorialTooltipMinWidth = 300;
        [SerializeField, Tooltip("The maximum width for the story tooltips."), BoxGroup("Story Tooltips"), ShowIf("showWelcomeTooltip"), FormerlySerializedAs("tutorialTooltipMaxWidth")]
        internal int tutorialTooltipMaxWidth = 500;
        [SerializeField, Tooltip("If true the story manager will show the welcome tooltop on first load."), BoxGroup("Welcome Tooltip"), FormerlySerializedAs("showWelcomeTooltip")]
        private bool showWelcomeBeat = true;
        [SerializeField, Tooltip("The story step to use as the welcome message."), BoxGroup("Welcome Tooltip"), ShowIf("showWelcomeTooltip"), FormerlySerializedAs("welcomeTooltip")]
        private PopupStoryBeat welcomeBeat;

        [SerializeField, Tooltip("Show the debug tooling for the story system."), BoxGroup("Debug")]
        private bool showDebug = false;
        [SerializeField, Tooltip("Should the story be reset when the game starts?"), BoxGroup("Debug"), ShowIf("showDebug"), FormerlySerializedAs("resetTutorial")]
        private bool resetStory = false;
        
        StoryBeat[] storyBeats;

        AudioSource audioSource;
        internal StoryBeat currentlyActiveBeat;

        private void Awake()
        {
#if UNITY_EDITOR
            if (resetStory)
            {
                ClearStoryProgress();
            }
#endif

            DontDestroyOnLoad(gameObject);
            audioSource = gameObject.GetComponent<AudioSource>();

            storyBeats = Resources.LoadAll<StoryBeat>("Story");
            foreach (IStoryBeat beat in storyBeats)
            {
                beat.StoryManager = this;

                IStoryBeat thisBeat = beat;
                if (beat.RequiredEvent)
                {
                    beat.RequiredEvent.RegisterListener(() => OnRequiredEvent(thisBeat));
                }
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

            if (showWelcomeBeat)
            {
                if (welcomeBeat == null)
                {
                    Debug.LogError("No welcome tooltip has been set in the Story Manager, yet showWelcomeTooltip is set to true.");
                }
                else
                {
                    StartBeat(welcomeBeat);
                }
            }
        }

        private void StartBeat(PopupStoryBeat beat)
        {
#if UNITY_EDITOR
            if (currentlyActiveBeat != null)
            {
                Debug.LogError("A story beat is already active, cannot start another beat until the current one is complete. Either wait for it to complete or call `StoryManager.FinishCurrentBeat()");
                return;
            }
#endif
            currentlyActiveBeat = beat;
            StartCoroutine(beat.Execute());
        }

        private void OnDisable()
        {
            NeoSceneManager.onSceneLoadRequested -= OnSceneLoadRequested;
            NeoSceneManager.onSceneLoaded -= OnSceneLoaded;

            foreach (StoryBeat step in storyBeats)
            {
                if (step.RequiredEvent)
                {
                    IStoryBeat thisStep = step;
                    step.RequiredEvent.UnregisterListener(() => OnRequiredEvent(thisStep));
                }
            }
        }

        bool cursorEnabled = false;
        private void Update()
        {
            bool newCursorEnabled = false;

            if (TooltipManager.Instance != null && TooltipManager.Instance.VisibleTooltips().Count > 0)
            {
                foreach (TooltipStyle tip in TooltipManager.Instance.VisibleTooltips())
                {
                    if (tip.cursorEnabled)
                    {
                        newCursorEnabled = true;
                        break;
                    }
                }
            }
            
            if (newCursorEnabled)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
            else if (cursorEnabled)
            {
                if (NeoFpsInputManager.captureMouseCursor)
                {
                    Cursor.lockState = CursorLockMode.Locked;
                    Cursor.visible = false;
                }
                else
                {
                    Cursor.lockState = CursorLockMode.None;
                    Cursor.visible = true;
                }
            }

            cursorEnabled = newCursorEnabled;
        }

        /// <summary>
        /// Call this to advance the story ahead of the time elapsing.
        /// </summary>
        internal void FinishCurrentBeat()
        {
            currentlyActiveBeat.Complete();
            currentlyActiveBeat = null;
        }

        private void OnRequiredEvent(IStoryBeat beat)
        {
            if (!beat.HasSceneTrigger && !beat.IsComplete) // only execute if there is no scene defined, otherwise we will execute the next time the scene is loaded
            {
                StartBeat((PopupStoryBeat)beat);
            }
        }

        private void OnSceneLoaded(int sceneIndex)
        {
            foreach (StoryBeat step in storyBeats)
            {
                if (!step.HasSceneTrigger || step.IsComplete)
                {
                    continue;
                }

                if (step.hasSceneTrigger
                    && !step.isLoadingScene
                    && sceneIndex == SceneManagement.SceneBuildIndexFromName(step.sceneName))
                {
                    currentlyActiveBeat = step;

                    StartCoroutine(ExecuteSceneLoadedStep());
                    break;
                }
            }
        }

        private void OnSceneLoadRequested(int sceneIndex, string sceneName)
        {
            if (sceneIndex < 0) {
                sceneIndex = SceneManagement.SceneBuildIndexFromName(sceneName);
            }

            currentlyActiveBeat = null;

            foreach (StoryBeat step in storyBeats)
            {
                if (!step.HasSceneTrigger) 
                { 
                    continue; 
                }

                if (step.isLoadingScene 
                    && sceneName == step.sceneName)
                {
                    currentlyActiveBeat = step;

                    StartCoroutine(ExecuteSceneLoadingStep());
                    return;
                }
            }

            NeoSceneManager.instance.minLoadScreenTime = 3;
        }

        private IEnumerator ExecuteSceneLoadedStep()
        {
            if (currentlyActiveBeat.isLoadingScene 
                || !currentlyActiveBeat.ReadyToExecute) 
            { 
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

            yield return currentlyActiveBeat.Execute();

            if (managedUIElements != null)
            {
                SetUIState(true, managedUIElements);
            }
        }

        private IEnumerator ExecuteSceneLoadingStep()
        {
            if (!currentlyActiveBeat.isLoadingScene || currentlyActiveBeat.IsComplete) { yield break; }

            float oldDuration = NeoSceneManager.instance.minLoadScreenTime;
            NeoSceneManager.instance.minLoadScreenTime = currentlyActiveBeat.duration;

            yield return currentlyActiveBeat.Execute();

            NeoSceneManager.instance.minLoadScreenTime = oldDuration;
        }

        /// <summary>
        /// Enables and disables UI elements when a story step is started or stopped.
        /// This allows us to minimize the clutter on screen during a story step.
        /// </summary>
        /// <param name="isActive"></param>
        /// <param name="managedUIElements"></param>
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
        [UnityEditor.MenuItem("Tools/Rogue Wave/Profiles/Reset Story Progress", priority = 1)]
        [Button, ShowIf("showDebug")]
#endif
        public static void ClearStoryProgress()
        {
            foreach(StoryBeat step in Resources.LoadAll<StoryBeat>("Story"))
            {
                step.Reset();
            }
        }
    }
}