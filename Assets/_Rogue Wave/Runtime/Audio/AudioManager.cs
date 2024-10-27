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

        static float mutedVolumeDb = -80;
        Dictionary<AudioMixerGroup, float> currentVolumes = new Dictionary<AudioMixerGroup, float>();
        FpsAudioSettings neoAudioSettings;

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

        public float MusicVolume {
            get => currentVolumes[music];
            internal set => currentVolumes[music] = value; 
        }

        private void Start()
        {
            CaptureCurrentMixerLevels();
        }

        private void OnEnable()
        {
            neoAudioSettings = FpsAudioSettings.GetInstance("FpsSettings_Audio");
            
            neoAudioSettings.onMasterVolumeChanged += OnMasterVolumeChanged;
            neoAudioSettings.onMusicVolumeChanged += OnMusicVolumeChanged;
            neoAudioSettings.onAmbienceVolumeChanged += OnAmbienceVolumeChanged;
            neoAudioSettings.onEffectsVolumeChanged += OnEffectsVolumeChanged;
        }

        private void OnEffectsVolumeChanged(float arg0)
        {
            currentVolumes[effectsMaster] = ConvertNormalizedToDb(FpsSettings.audio.effectsVolume);
        }

        private void OnAmbienceVolumeChanged(float arg0)
        {
            currentVolumes[ambience] = ConvertNormalizedToDb(FpsSettings.audio.ambienceVolume);
        }

        private void OnMusicVolumeChanged(float arg0)
        {
            MusicVolume = ConvertNormalizedToDb(FpsSettings.audio.musicVolume);
        }

        private void OnMasterVolumeChanged(float arg0)
        {
            currentVolumes[master] = ConvertNormalizedToDb(FpsSettings.audio.masterVolume);
        }

        private void OnDisable()
        {
            neoAudioSettings.onMasterVolumeChanged -= OnMasterVolumeChanged;
            neoAudioSettings.onMusicVolumeChanged -= OnMusicVolumeChanged;
            neoAudioSettings.onAmbienceVolumeChanged -= OnAmbienceVolumeChanged;
            neoAudioSettings.onEffectsVolumeChanged -= OnEffectsVolumeChanged;
        }

        private void CaptureCurrentMixerLevels()
        {
            float startingVolume;

            currentVolumes[master] = ConvertNormalizedToDb(FpsSettings.audio.masterVolume);
            MusicVolume = ConvertNormalizedToDb(FpsSettings.audio.musicVolume);
            currentVolumes[ambience] = ConvertNormalizedToDb(FpsSettings.audio.ambienceVolume);
            currentVolumes[effectsMaster] = ConvertNormalizedToDb(FpsSettings.audio.effectsVolume);

            ui.audioMixer.GetFloat(ui.name + "Volume", out startingVolume);
            currentVolumes[ui] = startingVolume;

            nanobots.audioMixer.GetFloat(nanobots.name + "Volume", out startingVolume);
            currentVolumes[nanobots] = startingVolume;

            spatial.audioMixer.GetFloat(spatial.name + "Volume", out startingVolume);
            currentVolumes[spatial] = startingVolume;

            twoDimensional.audioMixer.GetFloat(twoDimensional.name + "Volume", out startingVolume);
            currentVolumes[twoDimensional] = startingVolume;
        }

        public static void FadeGroup(AudioMixerGroup group, float targetVolumeDb, float duration, Action callback = null)
        {
            Instance.StartCoroutine(Instance.FadeGroupCoroutine(group, targetVolumeDb, duration, callback));
        }

        public static void ResetGroup(AudioMixerGroup group, float duration)
        {
            Instance.StartCoroutine(Instance.FadeGroupCoroutine(group, Instance.currentVolumes[group], duration));
        }

        public static void ResetAll(float duration)
        {
            Instance.StartCoroutine(Instance.FadeGroupCoroutine(Instance.music, Instance.currentVolumes[instance.music], duration));
            Instance.StartCoroutine(Instance.FadeGroupCoroutine(Instance.ui, Instance.currentVolumes[instance.ui], duration));
            Instance.StartCoroutine(Instance.FadeGroupCoroutine(Instance.effectsMaster, Instance.currentVolumes[instance.effectsMaster], duration));
            Instance.StartCoroutine(Instance.FadeGroupCoroutine(Instance.nanobots, Instance.currentVolumes[instance.nanobots], duration));
        }

        protected IEnumerator FadeGroupCoroutine(AudioMixerGroup group, float targetVolumeDb, float duration, Action callback = null)
        {
            if (duration <= 0)
            {
                group.audioMixer.SetFloat(group.name + "Volume", targetVolumeDb);
                callback?.Invoke();
                yield break;
            }

            float startVolume;
            group.audioMixer.GetFloat(group.name + "Volume", out startVolume);
            float time = 0;
            while (time < duration)
            {
                time += Time.deltaTime;
                group.audioMixer.SetFloat(group.name + "Volume", Mathf.Lerp(startVolume, targetVolumeDb, time / duration));
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

        private static IEnumerator FadeOutCo(AudioSource source, float duration, bool stopWhenFaded)
        {
            float startVolume = source.volume;
            float time = 0;
            while (time < duration && source != null)
            {
                time += Time.deltaTime;
                source.volume = Mathf.Lerp(startVolume, 0, time / duration);
                yield return null;
            }

            if (stopWhenFaded && source)
            {
                source.Stop();
            }
        }

        internal static void MuteAllExceptNanobots(float fadeDuration = 0, Action callback = null)
        {
            float targetVolumeDb = -80;

            Instance.StartCoroutine(Instance.FadeGroupCoroutine(Instance.music, targetVolumeDb, fadeDuration, callback));
            Instance.StartCoroutine(Instance.FadeGroupCoroutine(Instance.ui, targetVolumeDb, fadeDuration, callback));
            Instance.StartCoroutine(Instance.FadeGroupCoroutine(Instance.effectsMaster, targetVolumeDb, fadeDuration, callback));
        }

        public static float ConvertNormalizedToDb(float targetValueNormalized) {
            if (targetValueNormalized < 0.001)
            {
                return mutedVolumeDb;
            }
            else
            {
                return Mathf.Log10(targetValueNormalized) * 20f;
            }
        }

        /// <summary>
        /// Mute the music group.
        /// </summary>
        /// ,param name="fadeDuration">The duration of the fade in seconds.Set to 0 for instant, defaults to 2.</param>
        internal static IEnumerator MuteMusic(float fadeDuration = 2)
        {
            yield return Instance.FadeGroupCoroutine(Instance.music, mutedVolumeDb, fadeDuration);
        }

        /// <summary>
        /// Mute the music group for the duration of the session. It will only be unmuted if the music volume setting is changed.
        /// </summary>
        public void MuteMusicForSession()
        {
            currentVolumes[music] = mutedVolumeDb;
            MuteMusic();
        }


        #region Play SFX

        /// <summary>
        /// Play a one shot sound effect a provided AudioSource.
        /// </summary>
        /// <param name="source">The audio source with which to play the sound.</param>
        /// <param name="clip">The clip to play.</param>
        /// <seealso cref="Play3DOneShot(AudioClip, Vector3)"/>
        internal static void PlayOneShot(AudioSource source, AudioClip clip, float volume = 0.8f)
        {
            source.volume = volume;
            source.transform.SetParent(null);
            source.loop = false;
            source.PlayOneShot(clip);
        }

        /// <summary>
        /// Play a one shot sound effect in 3D space using a pooled audio source.
        /// </summary>
        /// <param name="clip">The clip to play.</param>
        /// <param name="position">The position at which to play the clip.</param>
        /// <returns>The audio source that is playing the sound.</returns>
        /// <seealso cref="Play3DOneShot(AudioSource, AudioClip, Vector3)"/>"/>
        internal static AudioSource Play3DOneShot(AudioClip clip, Vector3 position, float volume = 0.8f)
        {
            var source = NeoFpsAudioManager.Get3DAudioSource();
            if (source == null)
            {
                return null;
            }

            source.transform.position = position;
            PlayOneShot(source, clip, volume);
            
            return source;
        }

        /// <summary>
        /// Play a looping sound effect in 3D space using a provided audio source.
        /// </summary>
        /// <param name="source">The audio source with which to play the sound.</param>
        /// <param name="clip">The clip to play.</param>
        /// <param name="parent">The parent transform the audio source should follow.</param>
        /// <seealso cref="Play3DLooping(AudioClip, Transform)"/>
        internal static void PlayLooping(AudioSource source, AudioClip clip, float volume = 0.8f)
        {
            source.volume = volume;
            source.loop = true;
            source.clip = clip;
            source.Play();
        }

        internal static void StopLooping(AudioSource source)
        {
            FadeOut(source, 1f, true);
        }

        /// <summary>
        /// Play a looping sound effect in 3D space using a pooled audio source.
        /// </summary>
        /// <param name="clip">The clip to play.</param>
        /// <param name="parent">The parent transform the audio source should follow.</param>
        /// <returns>The audio source used.</returns>
        /// <seealso cref="Play3DLooping(AudioSource, AudioClip, Transform)"/>
        internal static AudioSource Play3DLooping(AudioClip clip, Transform parent, float volume = 0.8f)
        {
            var source = NeoFpsAudioManager.Get3DAudioSource();
            if (source == null)
                return null;

            source.transform.SetParent(parent);
            PlayLooping(source, clip, volume);

            return source;
        }

        /// <summary>
        /// Play a one shot sound effect in 2D space using a provided AudioSource.
        /// </summary>
        /// <param name="source">The audio source with which to play the sound.</param>
        /// <param name="clip">The clip to play.</param>
        /// <seealso cref="Play2DOneShot(AudioClip)"/>
        internal static void Play2DOneShot(AudioSource source, AudioClip clip, float volume = 0.8f)
        {
            source.PlayOneShot(clip, volume);
        }

        /// <summary>
        /// Play a one shot sound effect in 2D space using a pooled audio source.
        /// </summary>
        /// <param name="clip">The clip to play.</param>
        /// <returns>The audio source that is playing the sound.</returns>
        /// <seealso cref="Play2DOneShot(AudioSource source, AudioClip clip)"/>
        internal static AudioSource Play2DOneShot(AudioClip clip, float volume = 0.8f)
        {
            var source = NeoFpsAudioManager.Get2DAudioSource();
            if (source == null)
                return null;

            Play2DOneShot(source, clip, volume);

            return source;
        }

        internal static AudioSource PlayNanobotOneShot(AudioClip clip, float volume = 0.8f)
        {
            var source = NeoFpsAudioManager.Get2DAudioSource();
            if (source == null)
                return null;

            source.PlayOneShot(clip, volume);

            return source;
        }

        internal static AudioSource PlayAmbience(AudioClip audioClip, Vector3 position, float volume = 0.8f)
        {
            var source = NeoFpsAudioManager.Get3DAudioSource();
            if (source == null)
                return null;

            source.transform.position = position;
            source.PlayOneShot(audioClip, volume);

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
