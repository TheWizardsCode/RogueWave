using NaughtyAttributes;
using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

namespace RogueWave.Tutorial
{
    [CreateAssetMenu(fileName = "TutorialStep", menuName = "Rogue Wave/Tutorial Step", order = 1)]
    public class TutorialStep : ScriptableObject
    {
        [Header("Meta Data")]
        [SerializeField, Tooltip("The name of this tutorial step. This is for your reference only and is not used by the system.")]
        internal String displayName;
        [SerializeField, Tooltip("The description of this tutorial step. This is for your reference only and is not used by the system."), TextArea]
        internal String description;

        [Header("Triggers")]
        [SerializeField, Tooltip("The name of the scene that will trigger this tutorial content on load."), Scene]
        internal String sceneName;
        [SerializeField, Tooltip("The count of loads for sceneName that are required to trigger this tutorial step. That is if this is set to 3 then this loading screen will be used on the 3rd scene load.")]
        internal int sceneLoadCount = 1;
        [SerializeField, Tooltip("The duration that the loading screen will be visible ahead of the sceneName being made active. This will be used to ensure that the essential parts of the loading screen components of the tutorial are completed before the player can progress the game. For example all UI elements that have the RogueWaveUIElement.disableDuringTutorial flag set to true will be disabled until this amount of time has passed. If this is set to 0 then the length of the audio clip being played will be used.")]
        internal float loadingScreenDuration = 0;

        [Header("Visuals")]
        [SerializeField, Tooltip("The hero image for the loading screen when this tutorial step is started.")]
        internal Sprite loadingScreenHeroImage;
        [SerializeField, Tooltip("The script that will be displayed when delivering this part of the tutorial."), TextArea(5, 20)]
        internal string script;

        [Header("Audio")]
        [SerializeField, Tooltip("An audio clip from this collection will be played during the loading screen as the defined scene is loading.")]
        internal AudioClip[] loadingScreenClips;
        [SerializeField, Tooltip("An audio clip from this collection will be played when the identified scene becomes active.")]
        internal AudioClip[] sceneClips;

#if UNITY_EDITOR
        [HorizontalLine]
        [SerializeField]
        bool showDebug = false;

        [Button, ShowIf("showDebug")]
        public void PlayClip()
        {
            AudioClip loadingScreenClip = loadingScreenClips[UnityEngine.Random.Range(0, loadingScreenClips.Length)];

            if (loadingScreenDuration == 0)
            {
                loadingScreenDuration = loadingScreenClip.length + 1;
            }

            if (loadingScreenClip != null)
            {
                AudioSource audioSource = FindObjectOfType<AudioSource>();
                audioSource.clip = loadingScreenClip;
                audioSource.Play();
            }
        }
#endif
    }
}