using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WizardsCode.RogueWave;

namespace RogueWave
{
    [RequireComponent(typeof(AudioSource))]
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

        AudioSource source;
        int combatTrackIndex = 0;
        int menuTrackIndex = 0;
        float originalVolume = 1f;
        private bool isStopping = false;
        internal static MusicManager Instance;
        private MusicType currentType = MusicType.None;
        private MusicType nextType = MusicType.Menu;
        private Coroutine musicCoroutine;

        private void Awake()
        {
            if (Instance == null)
            {
                source = GetComponent<AudioSource>();
                originalVolume = source.volume;
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
            musicCoroutine = StartCoroutine(PlayMusicCo());
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
                if (currentType == nextType) {
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

                        AudioManager.ResetGroup(source.outputAudioMixerGroup, 0);
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
                            // mute current music
                            yield return AudioManager.FadeGroupCoroutine(source.outputAudioMixerGroup, AudioManager.Instance.mutedVolume, fadeDuration);
                            // start next track
                            AudioManager.ResetGroup(source.outputAudioMixerGroup, 0);
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
            isStopping = true;

            yield return AudioManager.FadeGroupCoroutine(source.outputAudioMixerGroup, AudioManager.Instance.mutedVolume, fadeDuration, 
                () => {
                    source.Stop();
                    isStopping = false;
                });
        }

        internal void PlayEscapeMusic()
        {
            AudioManager.FadeAllExceptNanobots(AudioManager.Instance.mutedVolume, 1);
            PlayMenuMusic();
        }

        internal void PlayDeathMusic()
        {
            PlayMenuMusic();
        }
    }
}