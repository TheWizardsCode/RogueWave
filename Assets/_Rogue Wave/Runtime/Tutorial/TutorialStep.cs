using NaughtyAttributes;
using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using WizardsCode.RogueWave;

namespace RogueWave.Tutorial
{
    [CreateAssetMenu(fileName = "TutorialStep", menuName = "Rogue Wave/Tutorial Step", order = 1)]
    public class TutorialStep : ScriptableObject, ITutorialStep
    {
        internal const string PLAYERPREFS_PREFIX = "TutorialStep_";

        [Header("Meta Data")]
        [SerializeField, Tooltip("The name of this tutorial step. This is for your reference only and is not used by the system.")]
        public String displayName;
        [SerializeField, Tooltip("The description of this tutorial step. This is for your reference only and is not used by the system."), TextArea]
        internal String description;
        [SerializeField, Tooltip("The duration that the scene will be frozen before giving player conrol again,  This will be used to ensure that the essential parts of the scene components of the tutorial are completed before the player can progress the game. For example all UI elements that have the RogueWaveUIElement.disableDuringTutorial flag set to true will be disabled until this amount of time has passed. If this is set to 0 then the length of the audio clip being played will be used."), ShowIf("isLoadingScene")]
        [FormerlySerializedAs("loadingScreenDuration")]
        internal float duration = 0;

        [Header("Triggers")]
        [SerializeField, Tooltip("If this tutorial step is triggered by a scene load then set this to true, if it is set to false then the step will be triggered by a Game Event.")]
        bool triggerBySceneLoad = true;
        [SerializeField, Tooltip("The Game Event that will trigger this tutorial content."), HideIf("triggerBySceneLoad")]
        GameEvent triggeringEvent;
        [SerializeField, Tooltip("If the trigger is set to isLoadingScreen then this tutorial step will be triggered during the loading scene leading to the loading of the indicated scene, otherwise it will be triggered upon the scene load itself."), ShowIf("triggerBySceneLoad")]
        internal bool isLoadingScene = true;
        [SerializeField, Tooltip("The name of the scene that will trigger this tutorial content on load."), Scene, ShowIf("triggerBySceneLoad")]
        internal String sceneName;
        [SerializeField, Tooltip("The count of loads for sceneName that are required to trigger this tutorial step. That is if this is set to 3 then this loading screen will be used on the 3rd scene load."), ShowIf("triggerBySceneLoad")]
        internal int sceneLoadCount = 1;
        
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

        public TutorialManager TutorialManager { get; set; }
        public bool TriggerBySceneLoad => triggerBySceneLoad;
        public GameEvent TriggeringEvent => triggeringEvent;

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
        public virtual void Execute()
        {
            PlayClip();
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
        }
    }
}