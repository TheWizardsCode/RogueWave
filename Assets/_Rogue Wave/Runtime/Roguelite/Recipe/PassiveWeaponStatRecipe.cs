using NaughtyAttributes;
using NeoFPS.SinglePlayer;
using System.Linq;
using UnityEngine;
using WizardsCode.RogueWave;

namespace RogueWave
{
    [CreateAssetMenu(fileName = "Weapon Stat Recipe", menuName = "Rogue Wave/Recipe/Passive Item Stat")]
    public class PassiveWeaponStatRecipe : BaseStatRecipe
    {
        [SerializeField, Tooltip("A prototype used to find the target to apply this stat modifier to.")]
        internal PassiveWeapon targetPrototype;
        [Header("Weapon Modifiers")]
        [SerializeField, Tooltip("If true, this recipe will set a range modifier for the weapon.")]
        bool isRangeModifier = false;
        [SerializeField, Tooltip("The range multiplier to add to the weapon."), ShowIf("isRangeModifier")]
        float rangeMultiplier = 1.10f;
        [SerializeField, Tooltip("If true, this recipe will set a damage modifier for the weapon.")]
        bool isDamageModifier = false;
        [SerializeField, Tooltip("A multiplier for the weapons damage."), ShowIf("isDamageModifier")]
        float damageMultiplier = 1.10f;
        
        public override string Category => "Passive Weapon Stat";

        public override string TechnicalSummary
        {
            get
            {
                if (base.TechnicalSummary != string.Empty)
                {
                    return $"{targetPrototype.name} {base.TechnicalSummary}";
                } else if (isRangeModifier)
                {
                    return $"{targetPrototype.name} Range * {rangeMultiplier}";
                } else if (isDamageModifier)
                {
                    return $"{targetPrototype.name} Damage * {damageMultiplier}";
                }

                return string.Empty;
            }
        }

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