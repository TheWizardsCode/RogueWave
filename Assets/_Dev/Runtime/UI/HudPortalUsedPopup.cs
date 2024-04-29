using System.Collections;
using UnityEngine;
using NeoFPS.SinglePlayer;
using NeoFPS;

namespace RogueWave
{
    [RequireComponent (typeof (CanvasGroup))]
	public class HudPortalUsedPopup : MonoBehaviour
    {
        [SerializeField, Tooltip("The amount of time to delay before making nanobot announcements. This is to give time for explosions and similar loud noises to fade.")]
        private float nanobotAnnouncementDelay = 1.5f;
        [SerializeField, Tooltip("The time taken to fade in (or out) the victory screen.")]
        private float fadeDuration = 1.5f;
        [SerializeField, Tooltip("The audio clip options to play when the vicotry screen is shown. One of these will be selected at random.")]
        AudioClip[] victoryPopupClip;

        private CanvasGroup canvasGroup = null;

        void Awake()
        {
            canvasGroup = GetComponent<CanvasGroup>();
            RogueWaveGameMode.onPortalEntered += OnPortalEntered;
            canvasGroup.alpha = 0f;
        }

        private void OnDestroy()
        {
            RogueWaveGameMode.onPortalEntered -= OnPortalEntered;
        }

        private void OnPortalEntered()
        {
            StartCoroutine(FadeCanvasGroup(canvasGroup.alpha, 1));
            StartCoroutine(NanobotVictoryRoutine());
        }

        private IEnumerator NanobotVictoryRoutine()
        {
            yield return new WaitForSeconds(nanobotAnnouncementDelay);


            if (victoryPopupClip.Length > 0)
            {
                int randomClip = Random.Range(0, victoryPopupClip.Length);
                NeoFpsAudioManager.PlayEffectAudioAtPosition(victoryPopupClip[randomClip], FpsSoloCharacter.localPlayerCharacter.transform.position);
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