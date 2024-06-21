using NaughtyAttributes;
using NeoFPS.SinglePlayer;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace RogueWave
{
    [CreateAssetMenu(fileName = "Passive Item Stat Recipe", menuName = "Rogue Wave/Recipe/Passive Item Stat", order = 109)]
    public class PassiveItemStatRecipe : BaseStatRecipe
    {
        [SerializeField, Tooltip("The passive weapon to apply the stat changes to.")]
        PassiveWeapon passiveWeapon;
        [SerializeField, Tooltip("If true, this recipe will set a range modifier for the weapon.")]
        bool isRangeModifier = false;
        [SerializeField, Tooltip("The range multiplier to add to the weapon."), ShowIf("isRangeModifier")]
        float rangeMultiplier = 1.10f;
        [SerializeField, Tooltip("If true, this recipe will set a damage modifier for the weapon.")]
        bool isDamageModifier = false;
        [SerializeField, Tooltip("A multiplier for the weapons damage."), ShowIf("isDamageModifier")]
        float damageMultiplier = 1.10f;
        [SerializeField, Tooltip("If true, this recipe will set a named parameter modifier for the weapon.")]
        bool isNamedParameterModifier = false;
        [SerializeField, Tooltip("The name of a parameter in the weapon to modify."), ShowIf("isNamedParameterModifier")]
        string parameterName;
        [SerializeField, Tooltip("The modifier to apply to the parameter. Note that while this is a float value, if the target parameter is an integer it will be rounded to int."), ShowIf("isNamedParameterModifier")]
        float parameterModifier = 1f;
        [SerializeField, Tooltip("The multiplier to apply to the parameter. This is applied after any modifier value. If the parameter is an int value the result will be rounded to an int."), ShowIf("isNamedParameterModifier")]
        float parameterMultiplier = 1.10f;

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
                if (weapon != null && weapon.name.StartsWith(passiveWeapon.name) )
                {
                    if (isRangeModifier)
                        weapon.range *= rangeMultiplier;

                    if (isDamageModifier)
                        weapon.damage *= damageMultiplier;

                    if (isNamedParameterModifier)
                    {
                        bool isSet = false;
                        FieldInfo field = weapon.GetType().GetField(parameterName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

                        if (field != null)
                        {
                            object fieldValue = field.GetValue(weapon);
                            if (fieldValue is float value)
                            {
                                value += parameterModifier;
                                value *= parameterMultiplier;
                                field.SetValue(weapon, value);
                                isSet = true;
                            } else if (fieldValue is int intValue)
                            {
                                intValue += Mathf.RoundToInt(parameterModifier);
                                intValue = Mathf.RoundToInt(intValue * parameterMultiplier);
                                field.SetValue(weapon, intValue);
                                isSet = true;
                            }
                        } else
                        {
                            PropertyInfo property = weapon.GetType().GetProperty(parameterName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                            if (property != null)
                            {
                                object propertyValue = property.GetValue(weapon);
                                if (propertyValue is float value)
                                {
                                    value += parameterModifier;
                                    value *= parameterMultiplier;
                                    property.SetValue(weapon, value);
                                    isSet = true;
                                } else if (propertyValue is int intValue)
                                {
                                    intValue += Mathf.RoundToInt(parameterModifier);
                                    intValue = Mathf.RoundToInt(intValue * parameterMultiplier);
                                    property.SetValue(weapon, intValue);
                                    isSet = true;
                                }
                            }
                        }

                        if (!isSet)
                        {
                            Debug.LogError($"`{this}` attempted to modify `{parameterName}` in `{passiveWeapon}`, but no such field or property is available.");
                        }
                    }
                }
            }
        }
    }
}