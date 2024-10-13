using NeoFPS;
using NeoFPS.SinglePlayer;
using UnityEngine.Events;

namespace RogueWave
{
    public class BuildOrderEditorUI : PreSpawnPopup
    {
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
            base.Initialise(g, onComplete);
        }
    }
}
