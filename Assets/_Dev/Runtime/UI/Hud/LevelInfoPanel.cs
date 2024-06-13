using NeoFPS;
using NeoFPS.Samples;
using NeoFPS.SinglePlayer;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace RogueWave
{
    public class LevelInfoPanel : PreSpawnPopup
    {
        [SerializeField, Tooltip("The audio to play when this panel is shown.")]
        AudioClip audio;

        private void OnEnable()
        {
            NeoFpsInputManager.captureMouseCursor = false;
        }

        private void OnDisable()
        {
            NeoFpsInputManager.captureMouseCursor = true;
        }

        public override void Initialise(FpsSoloGameCustomisable g, UnityAction onComplete)
        {
            GetComponent<AudioSource>().PlayOneShot(audio);
            base.Initialise(g, onComplete);
        }
    }
}
