using NeoFPS;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WizardsCode.RogueWave;

namespace RogueWave
{
    [RequireComponent(typeof(AudioSource), typeof(AudioManager))]
    [DefaultExecutionOrder(100)]
    public class MusicManager : MonoBehaviour
    {
        public enum MusicType
        {
            None,
            Menu,
            Combat
        }

        [SerializeField, Tooltip("Music to play when in a menu screen.")]
        AudioClip[] menuTracks;

        [SerializeField, Tooltip("The music tracks to play in order.")]
        AudioClip[] combatTracks;
        [SerializeField, Tooltip("Track fade duration.")]
        float fadeDuration = 2f;
        [SerializeField, Tooltip("Time to pause between tracks.")]
        float pauseDuration = 3f;

        static AudioSource source;
        int combatTrackIndex = 0;
        int menuTrackIndex = 0;
        private MusicType currentType = MusicType.None;
        private MusicType nextType = MusicType.Menu;

        static MusicManager m_Instance;
        public static MusicManager Instance
        {
            get
            {
                if (m_Instance == null)
                {
                    m_Instance = FindAnyObjectByType<MusicManager>();
                    if (m_Instance == null)
                    {
                        Debug.LogError("MusicManager is being referenced, but there isn't one present in the scene. Add one to make this error go away.");
                    }
                    source = m_Instance.GetComponent<AudioSource>();
                    DontDestroyOnLoad(m_Instance);
                }

                return m_Instance;
            }
        }

        private void Start()
        {
            if (Instance == null)
            {
                Debug.LogError("MusicManager not found in scene. Please add one.");
                return;
            }
            StartCoroutine(PlayMusicCo());
        }

        public void PlayMenuMusic()
        {
            nextType = MusicType.Menu;
        }

        public void PlayCombatMusic()
        {
            nextType = MusicType.Combat;
        }

        IEnumerator PlayMusicCo()
        {
            while (true)
            {
                // OPTIMIZATION: Only play music when the volume is not muted
                if (currentType == nextType) 
                {
                    float timeRemaining = 0;
                    if (source.isPlaying)
                    {
                        timeRemaining = source.clip.length - source.time;
                    }

                    if (timeRemaining > 0)
                    {
                        yield return null;
                    }
                    else
                    {
                        if (currentType == MusicType.Menu)
                        {
                            yield return new WaitForSeconds(pauseDuration);
                        }

                        AudioManager.ResetGroup(source.outputAudioMixerGroup, FpsSettings.audio.musicVolume);
                        source.clip = SelectNextClip();
                        source.Play();

                        yield return null;
                    }
                } 
                else
                {
                    switch (nextType)
                    {
                        case MusicType.None:
                            yield return null;
                            break;
                        default:
                            // mute then stop current music
                            yield return AudioManager.MuteMusic();
                            source.Stop();

                            // start next track
                            AudioManager.ResetGroup(source.outputAudioMixerGroup, FpsSettings.audio.musicVolume);
                            source.clip = SelectNextClip();
                            source.Play();

                            currentType = nextType;

                            yield return null;

                            break;
                    }
                }
            }
        }

        private AudioClip SelectNextClip()
        {
            switch (nextType)
            {
                case MusicType.Menu:
                    menuTrackIndex++;
                    if (menuTrackIndex >= menuTracks.Length)
                    {
                        menuTrackIndex = 0;
                    }

                    return menuTracks[menuTrackIndex];
                case MusicType.Combat:
                    combatTrackIndex++;
                    if (combatTrackIndex >= combatTracks.Length)
                    {
                        combatTrackIndex = 0;
                    }

                    if (combatTracks.Length == 0)
                    {
                        return null;
                    }

                    return combatTracks[combatTrackIndex];
                default:
                    return null;
            }
        }

        public void StopMusic()
        {
            StartCoroutine(StopMusicCo());
        }

        private IEnumerator StopMusicCo()
        {
            yield return AudioManager.FadeGroupCoroutine(source.outputAudioMixerGroup, 0, fadeDuration, 
                () => {
                    source.Stop();
                });
        }

        internal void PlayEscapeMusic()
        {
            AudioManager.FadeAllExceptNanobots(0, 1);
            PlayMenuMusic();
        }

        internal void PlayDeathMusic()
        {
            PlayMenuMusic();
        }
    }
}