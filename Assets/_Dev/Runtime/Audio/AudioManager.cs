using NaughtyAttributes;
using NeoFPS;
using RogueWave;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

namespace WizardsCode.RogueWave
{
    public class AudioManager : MonoBehaviour
    {
        [Header("Volume Settings")]
        [SerializeField, Range(-100, -60), Tooltip("The volume to set when a group is faded out.")]
        internal float mutedVolume = -80;

        [Header("Audio Mixer Groups")]
        [SerializeField, Tooltip("Master group is the final group in the chain. All other groups ulitmately route this this group.")]
        internal AudioMixerGroup master;
        [SerializeField, Tooltip("Music group plays all music and stings.")]
        internal AudioMixerGroup music;
        [SerializeField, Tooltip("The Nanobots group is used for nanobot voice communications.")]
        internal AudioMixerGroup nanobots;
        [SerializeField, Tooltip("Effects master group is a group used to control all effects groups, below. It provides a way to control all effects that are responsive to gameplay and routes through the master.")]
        AudioMixerGroup effectsMaster;
        [SerializeField, Tooltip("UI effects are two dimensional sounds that provide user feedback for the UI.")]
        internal AudioMixerGroup ui;
        [SerializeField, Tooltip("Ambience is background audio. The audio in this group is is not fully responsive to the gameplay, beyond changing with location.")]
        internal AudioMixerGroup ambience;
        [SerializeField, Tooltip("Spatial effects are 3D and as such are used for raising awareness about what is happening around the player.")]
        internal AudioMixerGroup spatial;
        [SerializeField, Tooltip("2D effects are computationally cheaper than spatial effects but are less effective in creating spatial awareness.")]
        internal AudioMixerGroup twoDimensional;

        Dictionary<AudioMixerGroup, float> startingVolumes = new Dictionary<AudioMixerGroup, float>();

        public static AudioManager Instance { get; private set; }

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            float startingVolume;

            startingVolumes[master] = FpsSettings.audio.masterVolume;
            startingVolumes[music] = FpsSettings.audio.musicVolume;
            startingVolumes[ambience] = FpsSettings.audio.ambienceVolume;
            startingVolumes[effectsMaster] = FpsSettings.audio.effectsVolume;

            ui.audioMixer.GetFloat(ui.name + "Volume", out startingVolume); 
            startingVolumes[ui] = startingVolume;
           
            nanobots.audioMixer.GetFloat(nanobots.name + "Volume", out startingVolume);
            startingVolumes[nanobots] = startingVolume;
            
            spatial.audioMixer.GetFloat(spatial.name + "Volume", out startingVolume);
            startingVolumes[spatial] = startingVolume;

            twoDimensional.audioMixer.GetFloat(twoDimensional.name + "Volume", out startingVolume);
            startingVolumes[twoDimensional] = startingVolume;
        }

        public static void FadeGroup(AudioMixerGroup group, float targetVolume, float duration)
        {
            Instance.StartCoroutine(FadeGroupCoroutine(group, targetVolume, duration));
        }

        public static void ResetGroup(AudioMixerGroup group, float duration)
        {
            Instance.StartCoroutine(FadeGroupCoroutine(group, Instance.startingVolumes[group], duration));
        }

        internal static IEnumerator FadeGroupCoroutine(AudioMixerGroup group, float targetVolume, float duration, Action callback = null)
        {
            if (duration <= 0)
            {
                group.audioMixer.SetFloat(group.name + "Volume", targetVolume);
                callback?.Invoke();
                yield break;
            }

            float startVolume;
            group.audioMixer.GetFloat(group.name + "Volume", out startVolume);
            float time = 0;
            while (time < duration)
            {
                time += Time.deltaTime;
                group.audioMixer.SetFloat(group.name + "Volume", Mathf.Lerp(startVolume, targetVolume, time / duration));
                yield return null;
            }
            callback?.Invoke();
        }

        internal static void FadeAllExceptNanobots(float targetVolume, float fadeDuration, Action callback = null)
        {
            Instance.StartCoroutine(FadeGroupCoroutine(Instance.music, targetVolume, fadeDuration, callback));
            Instance.StartCoroutine(FadeGroupCoroutine(Instance.ui, targetVolume, fadeDuration, callback));
            Instance.StartCoroutine(FadeGroupCoroutine(Instance.effectsMaster, targetVolume, fadeDuration, callback));
        }

#if UNITY_EDITOR
        [Button("Fade Music")]
        static void TestFade()
        {
            FadeGroup(Instance.music, -80, 2);
        }
        [Button("Reset Music")]
        static void TestReset()
        {
            ResetGroup(Instance.music, 2);
        }
#endif
    }
}
