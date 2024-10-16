using NaughtyAttributes;
using NeoFPS;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

namespace WizardsCode.RogueWave
{
    [DefaultExecutionOrder(80)]
    public class AudioManager : MonoBehaviour
    {   
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

        static float mutedVolume = -80;
        Dictionary<AudioMixerGroup, float> currentVolumes = new Dictionary<AudioMixerGroup, float>();

        private static AudioManager instance;
        public static AudioManager Instance
        {
            get
            {
                if (instance == null)
                {
                    GameObject settingsObject = FpsSettings.runtimeSettingsObject; // ensure the settings object is created
                    instance = FindObjectOfType<AudioManager>();
                    DontDestroyOnLoad(instance);
                }
                return instance;
            }
            private set { instance = value; }
        }

        private void Start()
        {
            float startingVolume;

            currentVolumes[master] = FpsSettings.audio.masterVolume;
            currentVolumes[music] = FpsSettings.audio.musicVolume;
            currentVolumes[ambience] = FpsSettings.audio.ambienceVolume;
            currentVolumes[effectsMaster] = FpsSettings.audio.effectsVolume;

            ui.audioMixer.GetFloat(ui.name + "Volume", out startingVolume); 
            currentVolumes[ui] = startingVolume;
           
            nanobots.audioMixer.GetFloat(nanobots.name + "Volume", out startingVolume);
            currentVolumes[nanobots] = startingVolume;
            
            spatial.audioMixer.GetFloat(spatial.name + "Volume", out startingVolume);
            currentVolumes[spatial] = startingVolume;

            twoDimensional.audioMixer.GetFloat(twoDimensional.name + "Volume", out startingVolume);
            currentVolumes[twoDimensional] = startingVolume;
        }

        public static void FadeGroup(AudioMixerGroup group, float targetVolume, float duration)
        {
            Instance.StartCoroutine(FadeGroupCoroutine(group, targetVolume, duration));
        }

        public static void ResetGroup(AudioMixerGroup group, float duration)
        {
            Instance.StartCoroutine(FadeGroupCoroutine(group, Instance.currentVolumes[group], duration));
        }

        internal static IEnumerator FadeGroupCoroutine(AudioMixerGroup group, float targetValue, float duration, Action callback = null)
        {
            float targetVolume = ConvertToVolume(targetValue);

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

        /// <summary>
        /// If the audio source is playing then fade out over 0.1s and then stop it.
        /// </summary>
        /// <param name="source">The source to stop</param>
        public static void Stop(AudioSource source)
        {
            if (source.isPlaying)
            {
                Instance.StartCoroutine(FadeOutCo(source, 0.1f, true));
            }
        }

        public static void FadeOut(AudioSource audioSource, float duration, bool stopWhenFaded = true)
        {
            Instance.StartCoroutine(FadeOutCo(audioSource, duration, stopWhenFaded));
        }

        private static IEnumerator FadeOutCo(AudioSource audioSource, float duration, bool stopWhenFaded)
        {
            float startVolume = audioSource.volume;
            float time = 0;
            while (time < duration)
            {
                time += Time.deltaTime;
                audioSource.volume = Mathf.Lerp(startVolume, 0, time / duration);
                yield return null;
            }

            if (stopWhenFaded)
            {
                audioSource.Stop();
            }
        }

        internal static void FadeAllExceptNanobots(float targetValue, float fadeDuration, Action callback = null)
        {
            float targetVolume = ConvertToVolume(targetValue);

            Instance.StartCoroutine(FadeGroupCoroutine(Instance.music, targetVolume, fadeDuration, callback));
            Instance.StartCoroutine(FadeGroupCoroutine(Instance.ui, targetVolume, fadeDuration, callback));
            Instance.StartCoroutine(FadeGroupCoroutine(Instance.effectsMaster, targetVolume, fadeDuration, callback));
        }

        static float ConvertToVolume(float targetValue) {
            if (targetValue < 0.001)
            {
                return mutedVolume;
            }
            else
            {
                return Mathf.Log10(targetValue) * 20f;
            }
        }

        /// <summary>
        /// Mute the music group.
        /// </summary>
        /// ,param name="fadeDuration">The duration of the fade in seconds.Set to 0 for instant, defaults to 2.</param>
        internal static IEnumerator MuteMusic(float fadeDuration = 2)
        {
            yield return FadeGroupCoroutine(Instance.music, mutedVolume, fadeDuration);
        }

        /// <summary>
        /// Mute the music group for the duration of the session. It will only be unmuted if the music volume setting is changed.
        /// </summary>
        public void MuteMusicForSession()
        {
            currentVolumes[music] = mutedVolume;
            MuteMusic();
        }


        #region Play SFX

        /// <summary>
        /// Play a one shot sound effect in 3D space using a provided AudioSource.
        /// </summary>
        /// <param name="source">The audio source with which to play the sound.</param>
        /// <param name="clip">The clip to play.</param>
        /// <param name="position">The position at which to play the clip.</param>
        /// <seealso cref="Play3DOneShot(AudioClip, Vector3)"/>
        internal static void Play3DOneShot(AudioSource source, AudioClip clip, Vector3 position)
        {
            source.transform.SetParent(null);
            source.loop = false;
            source.transform.position = position;
            source.PlayOneShot(clip);
        }

        /// <summary>
        /// Play a one shot sound effect in 3D space using a pooled audio source.
        /// </summary>
        /// <param name="clip">The clip to play.</param>
        /// <param name="position">The position at which to play the clip.</param>
        /// <returns>The audio source that is playing the sound.</returns>
        /// <seealso cref="Play3DOneShot(AudioSource, AudioClip, Vector3)"/>"/>
        internal static AudioSource Play3DEnemyOneShot(AudioClip clip, Vector3 position)
        {
            var source = NeoFpsAudioManager.Get3DAudioSource();
            if (source == null)
                return null;

            Play3DOneShot(source, clip, position);
            
            return source;
        }

        /// <summary>
        /// Play a looping sound effect in 3D space using a provided audio source.
        /// </summary>
        /// <param name="source">The audio source with which to play the sound.</param>
        /// <param name="clip">The clip to play.</param>
        /// <param name="parent">The parent transform the audio source should follow.</param>
        /// <seealso cref="Play3DLooping(AudioClip, Transform)"/>
        internal static void Play3DLooping(AudioSource source, AudioClip clip, Transform parent)
        {
            source.transform.SetParent(parent);
            source.loop = true;
            source.PlayOneShot(clip);
        }

        /// <summary>
        /// Play a looping sound effect in 3D space using a pooled audio source.
        /// </summary>
        /// <param name="clip">The clip to play.</param>
        /// <param name="parent">The parent transform the audio source should follow.</param>
        /// <returns>The audio source used.</returns>
        /// <seealso cref="Play3DLooping(AudioSource, AudioClip, Transform)"/>
        internal static AudioSource Play3DLooping(AudioClip clip, Transform parent)
        {
            var source = NeoFpsAudioManager.Get3DAudioSource();
            if (source == null)
                return null;

            Play3DLooping(source, clip, parent);

            return source;
        }

        /// <summary>
        /// Play a one shot sound effect in 2D space using a provided AudioSource.
        /// </summary>
        /// <param name="source">The audio source with which to play the sound.</param>
        /// <param name="clip">The clip to play.</param>
        /// <seealso cref="Play2DOneShot(AudioClip)"/>
        internal static void Play2DOneShot(AudioSource source, AudioClip clip)
        {
            source.PlayOneShot(clip);
        }

        /// <summary>
        /// Play a one shot sound effect in 2D space using a pooled audio source.
        /// </summary>
        /// <param name="clip">The clip to play.</param>
        /// <returns>The audio source that is playing the sound.</returns>
        /// <seealso cref="Play2DOneShot(AudioSource source, AudioClip clip)"/>
        internal static AudioSource Play2DEnemyOneShot(AudioClip clip)
        {
            var source = NeoFpsAudioManager.Get2DAudioSource();
            if (source == null)
                return null;

            Play2DOneShot(source, clip);

            return source;
        }

        internal static AudioSource PlayNanobotOneShot(AudioClip clip)
        {
            var source = NeoFpsAudioManager.Get2DAudioSource();
            if (source == null)
                return null;

            source.PlayOneShot(clip);

            return source;
        }

        internal static AudioSource PlayAmbience(AudioClip audioClip, Vector3 position)
        {
            var source = NeoFpsAudioManager.Get3DAudioSource();
            if (source == null)
                return null;

            source.transform.position = position;
            source.PlayOneShot(audioClip);

            return source;
        }
        #endregion

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
