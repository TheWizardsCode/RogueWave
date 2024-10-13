using NaughtyAttributes;
using NeoFPS.SinglePlayer;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace RogueWave
{
    /// <summary>
    /// Modify a stat on the motion graph.
    /// </summary>
    [CreateAssetMenu(fileName = "Movement Stat Recipe", menuName = "Rogue Wave/Recipe/Movement Stat", order = 1)]
    public class MotionGraphRecipe : BaseStatRecipe
    {
        [SerializeField, Tooltip("The name of the stat to modify.")]
        internal string statName = string.Empty;
        [SerializeField, Tooltip("The amount to add to the current multiplier for the stat. For example, if this value is 0.1 and the current multiplier is 1.5 then the new multiplier will be 1.6.")]
        float additionalMultiplier = 0.1f;
        [SerializeField, Tooltip("The amount to add to the pre-multiplication additive value for the stat. For example if the current pre-multiplier additive is 0.5 and this value is 0.1 then the new value will be 0.6.")]
        float additionalPreMultiplyAdd = 0f;
        [SerializeField, Tooltip("The amount to add to the post-multiplication additive value for the stat. For example if the current post-multiplier additive is 0.5 and this value is 0.1 then the new value will be 0.6.")]
        float additionalPostMultiplyAdd = 0f;

        public override string Category => "Movement";

        MovementUpgradeManager _movementUpgradeManager;
        internal MovementUpgradeManager movementUpgradeManager
        {
            get
            {
                if (_movementUpgradeManager == null)
                {
                    _movementUpgradeManager = FpsSoloCharacter.localPlayerCharacter.GetComponent<MovementUpgradeManager>();
                }
                return _movementUpgradeManager;
            }
        }

        public override string TechnicalSummary
        {
            get
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(ConvertToReadableString(statName));
                if (additionalPreMultiplyAdd != 0)
                {
                    sb.Append("( + ");
                    sb.Append(additionalPreMultiplyAdd);
                    sb.Append(") ");
                }
                if (additionalMultiplier != 1)
                {
                    sb.Append(" * ");
                    sb.Append(additionalMultiplier);
                }
                if (additionalPostMultiplyAdd > 0)
                {
                    sb.Append(" + ");
                    sb.Append(additionalPostMultiplyAdd);
                }

                return sb.ToString();
            }
        }

        [Button("Apply Float Modifier (works in game only)")]
        internal override void Apply()
        {
            FloatValueModifier modifier = movementUpgradeManager.GetFloatModifier(statName);
            modifier.multiplier += additionalMultiplier;
            modifier.preMultiplyAdd += additionalPreMultiplyAdd;
            modifier.postMultiplyAdd += additionalPostMultiplyAdd;
        }
    }
}