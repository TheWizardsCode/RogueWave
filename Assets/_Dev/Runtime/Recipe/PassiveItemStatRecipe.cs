using NeoFPS;
using NeoFPS.SinglePlayer;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RogueWave
{
    [CreateAssetMenu(fileName = "Passive Item Stat Recipe", menuName = "Rogue Wave/Recipe/Passive Item Stat", order = 109)]
    public class PassiveItemStatRecipe : BaseStatRecipe
    {
        [Header("Stat Modifier")]
        [SerializeField, Tooltip("The passive weapon to apply the stat changes to.")]
        PassiveWeapon passiveWeapon;
        [SerializeField, Tooltip("The range multiplier to add to the weapon.")]
        float rangeMultiplier = 1.00f;
        [SerializeField, Tooltip("A multiplier for the weapons damage.")]
        float damageMultiplier = 1.00f;

        public override string Category => "Passive Item";

        internal override void Apply()
        {
            PassiveWeapon[] passiveWeapons = FpsSoloCharacter.localPlayerCharacter.GetComponentsInChildren<PassiveWeapon>();
            foreach (PassiveWeapon weapon in passiveWeapons)
            {
                if (weapon != null && weapon.name.StartsWith(passiveWeapon.name) )
                {
                    if (rangeMultiplier > 0)
                        weapon.range *= rangeMultiplier;

                    if (damageMultiplier != 0)
                        weapon.damage *= damageMultiplier;
                }
            }
        }
    }
}