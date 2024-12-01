using NaughtyAttributes;
using NeoFPS;
using RogueWave.GameStats;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using WizardsCode.RogueWave;

namespace RogueWave.Story
{
    [CreateAssetMenu(fileName = "TutorialStep", menuName = "Rogue Wave/Tutorial Step", order = 1)]
    public class StoryBeat : ScriptableObject, IStoryBeat
    {
        internal const string PLAYERPREFS_PREFIX = "TutorialStep_";

        [Header("Meta Data")]
        [SerializeField, Tooltip("The name of this tutorial step. This is for your reference only and is not used by the system.")]
        public string displayName;
        [SerializeField, Tooltip("The description of this tutorial step. This is for your reference only and is not used by the system."), TextArea]
        internal string description;
        [SerializeField, Tooltip("The duration that the scene will be frozen before giving player conrol again,  This will be used to ensure that the essential parts of the scene components of the tutorial are completed before the player can progress the game. For example all UI elements that have the RogueWaveUIElement.disableDuringTutorial flag set to true will be disabled until this amount of time has passed. If this is set to 0 then the length of the audio clip being played will be used.")]
        internal float duration = 0;

        [Header("Triggers")]
        [SerializeField, Tooltip("A Game Event that must have been raised for this tutorial content to be valid. If a scene is also specified then this step will only be executed when the scene is loaded next time (regardless of currently loaded scene) and the event has previously fired. If no scene is specified then the step is executed when the event is raised."), HideIf("isLoadingScene")]
        [FormerlySerializedAs("triggeringEvent")]
        GameEvent requiredEvent;
        [SerializeField, Tooltip("An achievement that must be unlocked before this tutorial content is valid. If a scene is also specified then this step will only be executed when the scene is loaded next time (regardless of currently loaded scene) and the achievement has previously been unlocked. If no scene is specified then the step is executed when the achievement is unlocked."), HideIf("isLoadingScene")]
        Achievement requiredAchievement;
        [SerializeField, Tooltip("If the tutorial step is only to be shown in a particular scene or when a particular scene is loading check this box. If left unchecked the tutorial step will be executed immediately upon all other considitions have been met.")]
        internal bool hasSceneTrigger = false;
        [SerializeField, Tooltip("The name of the scene that will trigger this tutorial content on load."), Scene, ShowIf("hasSceneTrigger")]
        internal string sceneName;
        [SerializeField, Tooltip("If the tutorial step is to be shown while the specified scene is loading then set this to true. If left false then the step will be executed when the scene is loaded."), ShowIf("hasSceneTrigger")]
        internal bool isLoadingScene = false;

        [Header("Visuals")]
        [SerializeField, Tooltip("The hero image for the scene when this tutorial step is started."), ShowIf("isLoadingScene")]
        [FormerlySerializedAs("loadingScreenHeroImage")]
        internal Sprite heroImage;
        [SerializeField, Tooltip("The script that will be displayed when delivering the audo part of this tutorial step."), TextArea(5, 20)]
        [FormerlySerializedAs("loadingSceneScript")]
        public string script;

        [Header("Audio")]
        [SerializeField, Tooltip("The preferred voice actor for this tutorial step. This will be used to select the voice actor for the tutorial content.")]
        public Constants.Actor actor;
        [SerializeField, Tooltip("An audio clip from this collection will be played at the start of this scene.")]
        public AudioClip[] audioClips;

        [HorizontalLine]
        [SerializeField, BoxGroup("Debug")]
#pragma warning disable CS0414 // used to show/hide buttons in the inspector
        bool showDebug = false;
#pragma warning restore CS0414

        public StoryManager StoryManager { get; set; }
        public GameEvent RequiredEvent => requiredEvent;
        public Achievement RequiredAchievement => requiredAchievement;
        public string SceneName => sceneName;
        public bool HasSceneTrigger => hasSceneTrigger;
        public bool IsComplete { get; set; }

        bool requiredEventRaised = false;

        float endTime = 0;

        public bool ReadyToExecute {
            get
            {
                bool isReady = !IsComplete;
                isReady &= RequiredEvent == null || requiredEventRaised;
                isReady &= RequiredAchievement == null || requiredAchievement.isUnlocked;
                return isReady;
            }
        }

        private void OnEnable()
        {
            RequiredEvent?.RegisterListener(OnRequiredEventRaised);
        }

        private void OnDestroy()
        {
            RequiredEvent?.UnregisterListener(OnRequiredEventRaised);
        }

        void OnRequiredEventRaised()
        {
            requiredEventRaised = true;
            RequiredEvent.UnregisterListener(OnRequiredEventRaised);
        }

        [Button, ShowIf("showDebug")]
        void PlayClip()
        {
            if (audioClips.Length == 0)
            {
                return;
            }

            AudioClip loadingScreenClip = audioClips[UnityEngine.Random.Range(0, audioClips.Length)];

            if (duration == 0)
            {
                duration = loadingScreenClip.length + 1;
            }

            if (loadingScreenClip != null)
            {
                AudioSource audioSource = FindObjectOfType<AudioSource>();
                audioSource.clip = loadingScreenClip;
                audioSource.Play();
            }
        }

        /// <summary>
        /// Execute this tutorial step. This method should be called by the TutorialManager when it is time to execute this step.
        /// </summary>
        public virtual IEnumerator Execute()
        {
            if (!ReadyToExecute)
            {
                yield break;
            }

            PlayClip();

            endTime = Time.time + duration;

            while (Time.time < endTime)
            {
                yield return null;
            }

            IsComplete = true;
        }

        public void Reset()
        {
            IsComplete = false;
            OnEnable();
        }

        private void OnValidate()
        {
            if (audioClips != null && audioClips.Length > 0)
            {
                if (duration < audioClips[0].length)
                {
                    foreach (AudioClip clip in audioClips)
                    {
                        if (clip.length > duration)
                        {
                            duration = clip.length + 0.5f;
                        }
                    }
                }
            }

            requiredEventRaised = RequiredEvent == null;
        }

        [Button, ShowIf("IsComplete")]
        void MarkIncomplete()
        {
            IsComplete = false;
        }

        [Button("Mark Complete"), HideIf("IsComplete")]
        /// <summary>
        /// Mark this beat as complete.
        /// </summary>
        /// <exception cref="NotImplementedException"></exception>
        internal void Complete()
        {
            endTime = Time.time;
            IsComplete = true;
        }
    }
}