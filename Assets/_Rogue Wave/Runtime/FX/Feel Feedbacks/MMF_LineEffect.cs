using UnityEngine;
using NeoFPS.SinglePlayer;
using MoreMountains.Feedbacks;

namespace WizardsCode.RogueWave
{
    [AddComponentMenu("")]
    [FeedbackHelp("This feedback will prepare a line based weapon effect by copying essential information from the Rogue Wave WeaponController into the effect.")]
    [FeedbackPath("Rogue Wave/Weapon Line Effect")]
    public class MMF_LineEffect : MMF_Feedback
    {
        /// a static bool used to disable all feedbacks of this type at once
        public static bool FeedbackTypeAuthorized = true;

        public override float FeedbackDuration { get { return 0f; } }
        /// pick a color here for your feedback's inspector
#if UNITY_EDITOR
        public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.GameObjectColor; } }
        public override bool EvaluateRequiresSetup() { return (WeaponEffects == null || EffectLine == null); }
        public override string RequiredTargetText { get { return EffectLine != null ? EffectLine.gameObject.name : ""; } }
        public override string RequiresSetupText { get { return $"This feedback has some missing required fields."; } }
#endif

        [MMFInspectorGroup("Rogue Wave", true, 12, true)]
        [Tooltip("The WeaponEffectController that will provide the settings needed for the FX contrroller (below).")]
        public BasicWeaponController WeaponEffects;
        [Tooltip("The motor that will drive the effect motion.")]
        public LineRenderer EffectLine;

        protected override void CustomInitialization(MMF_Player owner)
        {
            base.CustomInitialization(owner);
        }

        protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
        {
            if (!Active || !FeedbackTypeAuthorized)
            {
                return;
            }

            EffectLine.SetPosition(0, WeaponEffects.transform.position);
            EffectLine.SetPosition(1, WeaponEffects.targetPosition);
        }

        protected override void CustomStopFeedback(Vector3 position, float feedbacksIntensity = 1)
        {
            if (!FeedbackTypeAuthorized)
            {
                return;
            }
        }
    }
}