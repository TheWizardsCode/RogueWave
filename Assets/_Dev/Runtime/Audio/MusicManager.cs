using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Playground
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

        private void Awake()
        {
            source = GetComponent<AudioSource>();
            originalVolume = source.volume;
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            StartCoroutine(PlayTrack());
        }

        IEnumerator PlayTrack() {
            while (true)
            {
                source.clip = tracks[trackIndex];
                source.Play();
                
                yield return new WaitForSeconds(source.clip.length - fadeDuration);

                while (source.volume > 0)
                {
                    source.volume -= Time.deltaTime / fadeDuration;
                    yield return null;
                }

                trackIndex++;
                if (trackIndex >= tracks.Length)
                {
                    trackIndex = 0;
                }
                
                yield return new WaitForSeconds(pauseDuration);

                source.volume = originalVolume;
            }
        }
    }
}