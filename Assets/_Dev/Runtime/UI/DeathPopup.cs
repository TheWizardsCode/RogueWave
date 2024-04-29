using NeoFPS;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace RogueWave.UI
{
    public class DeathPopup : PlayerCharacterHudBase
    {
        [SerializeField, Tooltip("The time taken to fade in (or out) the death screen.")] 
        private float fadeDuration = 1.5f;
        [SerializeField, Tooltip("The audio clip options to play when the death screen is shown. One of these will be selected at random.")]
        AudioClip[] deathPopupClip;


        private CanvasGroup canvasGroup = null;
        private ICharacter character = null;
        private float originalVolume;
        private bool inVictoryRoutine;

        protected override void Awake()
        {
            base.Awake();
            canvasGroup = GetComponent<CanvasGroup>();
            canvasGroup.alpha = 0f;
            inVictoryRoutine = false;
        }

        private void OnEnable()
        {
            RogueWaveGameMode.onLevelComplete += OnLevelComplete;
            RogueWaveGameMode.onPortalEntered += OnPortalEntered;
            inVictoryRoutine = false;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            RogueWaveGameMode.onLevelComplete -= OnLevelComplete;
            RogueWaveGameMode.onPortalEntered -= OnPortalEntered;

            if (character != null)
                character.onIsAliveChanged -= OnIsAliveChanged;

            NeoFpsAudioManager.masterGroup.audioMixer.SetFloat("MasterVolume", originalVolume);
            NeoFpsAudioManager.masterGroup.audioMixer.SetFloat("LowPassCutoff",22000f);
        }

        private void OnPortalEntered()
        {
            inVictoryRoutine = true;
        }

        private void OnLevelComplete()
        {
            inVictoryRoutine = true;
        }

        public override void OnPlayerCharacterChanged(ICharacter character)
        {
            if (this.character != null)
                this.character.onIsAliveChanged -= OnIsAliveChanged;

            this.character = character;

            if (this.character as Component != null)
            {
                this.character.onIsAliveChanged += OnIsAliveChanged;
                OnIsAliveChanged(this.character, this.character.isAlive);
            }
            else
                gameObject.SetActive(false);
        }

        void OnIsAliveChanged(ICharacter character, bool alive)
        {
            if (inVictoryRoutine || alive)
            {
                return;
            }

            if (deathPopupClip.Length > 0)
            {
                int randomClip = Random.Range(0, deathPopupClip.Length);
                NeoFpsAudioManager.PlayEffectAudioAtPosition(deathPopupClip[randomClip], character.transform.position);
            }
            FadeIn(fadeDuration);
        }

        public void FadeIn(float duration)
        {
            StartCoroutine(FadeCanvasGroup(canvasGroup, canvasGroup.alpha, 1, duration));
            StartCoroutine(FadeAudioOut(duration, duration * 3));
        }

        public void FadeOut(float duration)
        {
            StartCoroutine(FadeCanvasGroup(canvasGroup, canvasGroup.alpha, 0, duration));
        }

        private IEnumerator FadeAudioOut(float delay, float duration)
        {
            NeoFpsAudioManager.masterGroup.audioMixer.GetFloat("MasterVolume", out originalVolume);

            float startTime = Time.time;
            while (Time.time - startTime < delay)
            {
                NeoFpsAudioManager.masterGroup.audioMixer.SetFloat("LowPassCutoff", Mathf.Lerp(22000, 200, (Time.time - startTime) / delay));
                yield return null;
            }
            
            startTime = Time.time;
            while (Time.time - startTime < duration)
            {
                NeoFpsAudioManager.masterGroup.audioMixer.GetFloat("MasterVolume", out float volume);
                NeoFpsAudioManager.masterGroup.audioMixer.SetFloat("MasterVolume", Mathf.Lerp(volume, -80, (Time.time - startTime) / duration));
                yield return null;
            }
        }

        private IEnumerator FadeCanvasGroup(CanvasGroup cg, float start, float end, float duration)
        {
            float startTime = Time.time;
            while (Time.time - startTime < duration)
            {
                cg.alpha = Mathf.Lerp(start, end, (Time.time - startTime) / duration);
                yield return null;
            }

            cg.alpha = end;
        }
    }
}