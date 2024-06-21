using NaughtyAttributes;
using NeoFPS.SinglePlayer;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace RogueWave
{
    [CreateAssetMenu(fileName = "Passive Item Stat Recipe", menuName = "Rogue Wave/Recipe/Passive Item Stat")]
    public class PassiveWeaponStatRecipe : GenericStatRecipe<PassiveWeapon>
    {
        // REFACTOR: remove the need for this class and use the BaseStatRecipe instead - following should be named parameter modifiers
        [Header("Weapon Modifiers")]
        [SerializeField, Tooltip("If true, this recipe will set a range modifier for the weapon.")]
        bool isRangeModifier = false;
        [SerializeField, Tooltip("The range multiplier to add to the weapon."), ShowIf("isRangeModifier")]
        float rangeMultiplier = 1.10f;
        [SerializeField, Tooltip("If true, this recipe will set a damage modifier for the weapon.")]
        bool isDamageModifier = false;
        [SerializeField, Tooltip("A multiplier for the weapons damage."), ShowIf("isDamageModifier")]
        float damageMultiplier = 1.10f;
        
        public override string Category => "Passive Item";

        internal override void Apply()
        {
            PassiveWeapon[] passiveWeapons = FpsSoloCharacter.localPlayerCharacter.GetComponentsInChildren<PassiveWeapon>();
            NanobotPawnController _nanobotPawnController = FindFirstObjectByType<NanobotPawnController>();
            if (_nanobotPawnController != null)
            {
                passiveWeapons = passiveWeapons.Concat(_nanobotPawnController.GetComponentsInChildren<PassiveWeapon>()).ToArray();
            }

            foreach (PassiveWeapon weapon in passiveWeapons)
            {
                if (weapon != null && weapon.name.StartsWith(targetPrototype.name) )
                {
                    base.Apply(weapon as MonoBehaviour);
                    
                    if (isRangeModifier)
                        weapon.range *= rangeMultiplier;

                    if (isDamageModifier)
                        weapon.damage *= damageMultiplier;
                }
            }
        }
    }
}