using System.Collections;
using UnityEngine;
using NeoFPS.SinglePlayer;
using NeoFPS;

namespace RogueWave
{
    [RequireComponent (typeof (CanvasGroup))]
	public class HudLevelCompletePopup : MonoBehaviour
    {
        [SerializeField, Tooltip("THe amount of time to delay before making nanobot announcements. This is to give time for explosions and similar loud noises to fade.")]
        private float nanobotAnnouncementDelay = 1.5f;
        [SerializeField, Tooltip("The time taken to fade in (or out) the victory screen.")]
        private float fadeDuration = 1.5f;
        [SerializeField, Tooltip("The audio clip options to play when the vicotry screen is shown. This can be overridden in the level definition, but if not set there then this will be used. One of these will be selected at random.")]
        AudioClip[] victoryPopupClip;

        private CanvasGroup canvasGroup = null;

        void Awake()
        {
            canvasGroup = GetComponent<CanvasGroup>();
            RogueWaveGameMode.onLevelComplete += OnLevelComplete;
            canvasGroup.alpha = 0f;
        }

        private void OnDestroy()
        {
            RogueWaveGameMode.onLevelComplete -= OnLevelComplete;
        }

        private void OnLevelComplete()
        {
            StartCoroutine(FadeCanvasGroup(canvasGroup.alpha, 1));
            StartCoroutine(NanobotVictoryRoutine());
        }

        private IEnumerator NanobotVictoryRoutine()
        {
            yield return new WaitForSeconds(nanobotAnnouncementDelay);

            AudioClip[] clips = FindAnyObjectByType<RogueWaveGameMode>().currentLevelDefinition.levelCompleteAudioClips;
            if (clips != null && clips.Length > 0)
            {
                NeoFpsAudioManager.PlayEffectAudioAtPosition(clips[Random.Range(0, clips.Length)], FpsSoloCharacter.localPlayerCharacter.transform.position);
            }
            else if (victoryPopupClip.Length > 0)
            {
                NeoFpsAudioManager.PlayEffectAudioAtPosition(victoryPopupClip[Random.Range(0, victoryPopupClip.Length)], FpsSoloCharacter.localPlayerCharacter.transform.position);
            }
        }

        private IEnumerator FadeCanvasGroup(float startAlpha, float endAlpha)
        {
            float startTime = Time.time;
            while (Time.time - startTime < fadeDuration)
            {
                canvasGroup.alpha = Mathf.Lerp(startAlpha, endAlpha, (Time.time - startTime) / fadeDuration);
                yield return null;
            }

            canvasGroup.alpha = endAlpha;
        }
    }
}