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
            WaitForSeconds standardWait = new WaitForSeconds(0.35f);

            while (true)
            {
                if (AudioManager.Instance.MusicVolume > -80)
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
                            yield return standardWait;
                        }
                        else
                        {
                            if (currentType == MusicType.Menu)
                            {
                                yield return new WaitForSeconds(pauseDuration);
                            }

                            source.clip = SelectNextClip();
                            if (source.clip != null)
                            {
                                source.Play();
                            }
                        }
                    }
                    else
                    {
                        switch (nextType)
                        {
                            case MusicType.None:
                                yield return standardWait;
                                break;
                            default:
                                // mute then stop current music
                                yield return AudioManager.MuteMusic();
                                source.Stop();

                                // start next track
                                AudioManager.ResetGroup(source.outputAudioMixerGroup, FpsSettings.audio.musicVolume);
                                source.clip = SelectNextClip();
                                if (source.clip != null)
                                {
                                    source.Stop();
                                }

                                currentType = nextType;

                                break;
                        }
                    }
                }

                yield return standardWait;
            }
        }

        private AudioClip SelectNextClip()
        {
            switch (nextType)
            {
                case MusicType.Menu:
                    if (menuTracks.Length == 0)
                    {
                        return null;
                    }

                    menuTrackIndex++;
                    if (menuTrackIndex >= menuTracks.Length)
                    {
                        menuTrackIndex = 0;
                    }

                    return menuTracks[menuTrackIndex];
                case MusicType.Combat:
                    if (combatTracks.Length == 0)
                    {
                        return null;
                    }

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
            AudioManager.FadeGroup(source.outputAudioMixerGroup, -80, fadeDuration, 
                () => {
                    source.Stop();
                });
        }

        internal void PlayEscapeMusic()
        {
            AudioManager.MuteAllExceptNanobots();
            PlayMenuMusic();
        }

        internal void PlayDeathMusic()
        {
            PlayMenuMusic();
        }
    }
}