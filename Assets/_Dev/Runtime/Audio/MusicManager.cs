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
        private bool isPlaying = false;
        private bool isStopping = false;
        internal static MusicManager Instance;

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

        public void PlayMenuMusic()
        {
            StartCoroutine(PlayMenuMusicCo());
        }

        public void PlayCombatMusic()
        {
            StartCoroutine(PlayCombatMusicCo());
        }

        IEnumerator PlayMenuMusicCo()
        {
            if (isPlaying)
            {
                yield return StopMusicCo();
            }

            while (isStopping)
            {
                yield return null;
            }

            isPlaying = true;
            isStopping = false;

            while (isPlaying)
            {
                AudioManager.ResetGroup(source.outputAudioMixerGroup, 0);

                menuTrackIndex++;
                if (menuTrackIndex >= menuTracks.Length)
                {
                    menuTrackIndex = 0;
                }

                source.clip = menuTracks[combatTrackIndex];
                source.Play();

                yield return new WaitForSeconds(source.clip.length - fadeDuration);

                yield return AudioManager.FadeGroupCoroutine(source.outputAudioMixerGroup, AudioManager.Instance.mutedVolume, fadeDuration);

                yield return new WaitForSeconds(pauseDuration);
            }
        }

        IEnumerator PlayCombatMusicCo()
        {
            if (isPlaying)
            {
                yield return StopMusicCo();
            }

            while (isStopping)
            {
                yield return null;
            }

            isPlaying = true;
            isStopping = false;

            while (isPlaying)
            {
                AudioManager.FadeGroupCoroutine(source.outputAudioMixerGroup, originalVolume, fadeDuration);

                combatTrackIndex++;
                if (combatTrackIndex >= combatTracks.Length)
                {
                    combatTrackIndex = 0;
                }

                source.clip = combatTracks[combatTrackIndex];
                source.Play();

                yield return new WaitForSeconds(source.clip.length - fadeDuration);
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
                    isPlaying = false; 
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