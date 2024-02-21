using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RogueWave
{
    [RequireComponent(typeof(AudioSource))]
    public class MusicManager : MonoBehaviour
    {
        [SerializeField, Tooltip("The music tracks to play in order.")]
        AudioClip[] tracks;
        [SerializeField, Tooltip("Track fade duration.")]
        float fadeDuration = 2f;
        [SerializeField, Tooltip("Time to pause between tracks.")]
        float pauseDuration = 3f;

        AudioSource source;
        int trackIndex = 0;
        float originalVolume = 1f;
        private bool isPlaying = false;
        private static MusicManager instance;

        private void Awake()
        {
            if (instance == null)
            {
                source = GetComponent<AudioSource>();
                originalVolume = source.volume;
                instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            if (isPlaying == false)
            {
                StartCoroutine(PlayMusic());
            }
        }

        public IEnumerator PlayMusic()
        {
            isPlaying = true;

            while (isPlaying)
            {
                source.clip = tracks[trackIndex];
                source.Play();
                
                yield return new WaitForSeconds(source.clip.length - fadeDuration);

                yield return FadeMusic();

                trackIndex++;
                if (trackIndex >= tracks.Length)
                {
                    trackIndex = 0;
                }
                
                yield return new WaitForSeconds(pauseDuration);

                source.volume = originalVolume;
            }
        }

        private IEnumerator FadeMusic()
        {
            while (source.volume > 0)
            {
                source.volume -= Time.deltaTime / fadeDuration;
                yield return null;
            }
        }

        public void StopMusic()
        {
            StartCoroutine(StopMusicCo());
        }

        private IEnumerator StopMusicCo()
        {
            yield return FadeMusic();

            isPlaying = false;
        }
    }
}