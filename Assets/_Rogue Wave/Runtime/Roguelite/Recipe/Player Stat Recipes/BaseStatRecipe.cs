
using NaughtyAttributes;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace RogueWave
{
    /// <summary>
    /// A stat recipe will upgrade an objects stats.
    /// </summary>
    public abstract class BaseStatRecipe : AbstractRecipe
    {
        [SerializeField, Tooltip("If true, this recipe will set a named parameter modifier for the weapon.")]
        bool isNamedParameterModifier = false;
        [SerializeField, Tooltip("The name of a parameter in the weapon to modify."), ShowIf("isNamedParameterModifier")]
        string parameterName;
        [SerializeField, Tooltip("Apply a additive/subractive modifier (this will be applied BEFORE any multiplier)."), ShowIf("isNamedParameterModifier")]
        bool hasAdditiveModifier = false;
        [SerializeField, Tooltip("The modifier to apply to the parameter. Note that while this is a float value, if the target parameter is an integer it will be rounded to int."), ShowIf(EConditionOperator.And, "isNamedParameterModifier", "hasAdditiveModifier")]
        float parameterModifier = 1f;
        [SerializeField, Tooltip("Apply a multiplier (this will be applied AFTER any additive/subtractive modifier)."), ShowIf("isNamedParameterModifier")]
        bool hasMultiplier = false;
        [SerializeField, Tooltip("The multiplier to apply to the parameter. This is applied after any modifier value. If the parameter is an int value the result will be rounded to an int."), ShowIf(EConditionOperator.And, "isNamedParameterModifier", "hasMultiplier")]
        float parameterMultiplier = 1f;
        [SerializeField, Tooltip("Is this a One Shot modifier or should it be repeated."), ShowIf("isNamedParameterModifier")]
        internal bool isRepeating = true;
        [SerializeField, Tooltip("The time in seconds to wait before repeating the modifier. A value of 0 will only apply the modifier once."), MinValue(0f), ShowIf(EConditionOperator.And, "isNamedParameterModifier", "isRepeating")]
        internal float repeatEvery = 0f;

        public override string Category => "Base Stat";

        public override string TechnicalSummary {
            get
            {
                if (isNamedParameterModifier)
                {
                    StringBuilder sb = new StringBuilder();
                    sb.Append(ConvertToReadableString(parameterName));
                    if (hasAdditiveModifier && parameterModifier != 0)
                    {
                        sb.Append(" + ");
                        sb.Append(parameterModifier);
                    }
                    if (hasMultiplier && parameterMultiplier != 1)
                    {
                        sb.Append(" * ");
                        sb.Append(parameterMultiplier);
                    }
                    if (isRepeating && repeatEvery > 0)
                    {
                        sb.Append(" every ");
                        sb.Append(repeatEvery);
                        sb.Append("s");
                    }

                    return sb.ToString();
                } else
                {
                    return string.Empty;
                }
            }
        }

        public override void BuildFinished()
        {
            Apply();

            base.BuildFinished();
        }

        /// <summary>
        /// Apply the modifier to the target object.
        /// The implementation of this should get the target object this recipe is modifying 
        /// and call `Apply(MonoBehaviour target)` with that target object.
        /// 
        /// For example: `Apply(FpsSoloCharacter.localPlayerCharacter.GetComponentInChildren<BasicHealthManager>() as MonoBehaviour);`
        /// </summary>
        internal abstract void Apply();

        /// <summary>
        /// Apply the named modifier to the target object.
        /// </summary>
        /// <param name="target">The target object which holds the stat to be modified.</param>
        internal virtual void Apply(MonoBehaviour target)
        {
            if (isNamedParameterModifier)
            {
                bool isSet = false;
                FieldInfo field = target.GetType().GetField(parameterName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

                if (field != null)
                {
                    object fieldValue = field.GetValue(target);
                    if (fieldValue is float value)
                    {
                        if (hasAdditiveModifier)
                        {
                            value += parameterModifier;
                        }
                        if (hasMultiplier)
                        {
                            value *= parameterMultiplier;
                        }
                        field.SetValue(target, value);
                        isSet = true;
                    }
                    else if (fieldValue is int intValue)
                    {
                        if (hasAdditiveModifier)
                        {
                            intValue += Mathf.RoundToInt(parameterModifier);
                        }
                        if (hasMultiplier)
                        {
                            intValue = Mathf.RoundToInt(intValue * parameterMultiplier);
                        }
                        field.SetValue(target, intValue);
                        isSet = true;
                    }
                }
                else
                {
                    PropertyInfo property = target.GetType().GetProperty(parameterName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                    if (property != null)
                    {
                        object propertyValue = property.GetValue(target);
                        if (propertyValue is float value)
                        {
                            if (hasAdditiveModifier)
                            {
                                value += parameterModifier;
                            }
                            if (hasMultiplier)
                            {
                                value *= parameterMultiplier;
                            }
                            property.SetValue(target, value);
                            isSet = true;
                        }
                        else if (propertyValue is int intValue)
                        {
                            if (hasAdditiveModifier)
                            {
                                intValue += Mathf.RoundToInt(parameterModifier);
                            }
                            if (hasMultiplier)
                            {
                                intValue = Mathf.RoundToInt(intValue * parameterMultiplier);
                            }
                            property.SetValue(target, intValue);
                            isSet = true;
                        }
                    }
                }

                if (!isSet)
                {
                    Debug.LogError($"`{this}` attempted to modify `{parameterName}` in `{target}`, but no such field or property is available.");
                }
            }
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (string.IsNullOrEmpty(uniqueID))
            {
                GenerateID();
            }

            //TODO: is it possible to check the parameterName is valid in the motiongraph?
        }
#endif
    }
}