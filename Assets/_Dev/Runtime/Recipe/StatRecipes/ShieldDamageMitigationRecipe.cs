using NeoFPS;
using NeoFPS.SinglePlayer;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Playground { 
    [CreateAssetMenu(fileName = "Shield Damage Mitigation Recipe", menuName = "Playground/Recipe/Shield Damage Mitigation Recipe", order = 1)]
    public class ShieldDamageMitigationRecipe : BaseStatRecipe
    {
        [SerializeField, Tooltip("The amount to add to the current damage mitigation of the shiels.")]
        float additionalDamageMitigation = 0.1f;

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