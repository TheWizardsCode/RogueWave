using NeoFPS;
using NeoFPS.SinglePlayer;
using UnityEngine;

namespace RogueWave { 
    [CreateAssetMenu(fileName = "Shield Damage Mitigation Recipe", menuName = "Rogue Wave/Recipe/Shield Damage Mitigation Recipe", order = 1)]
    public class ShieldDamageMitigationRecipe : BaseStatRecipe
    {
        [SerializeField, Tooltip("The amount to add to the current damage mitigation of the shields.")]
        float additionalDamageMitigation = 0.1f;

        public override string Category => "Shield";

        public override string TechnicalSummary
        {
            get
            {
                if (base.TechnicalSummary != string.Empty)
                    return base.TechnicalSummary + ", " + "Damage Mitigation + " + additionalDamageMitigation;
                else
                    return "Damage Mitigation + " + additionalDamageMitigation;
            }
        }

        internal override void Apply()
        {
            var shield = FpsSoloCharacter.localPlayerCharacter.GetComponent<ShieldManager>();
            if (shield != null)
            {
                shield.AddDamageMitigation(additionalDamageMitigation);
            }
        }
    }
}