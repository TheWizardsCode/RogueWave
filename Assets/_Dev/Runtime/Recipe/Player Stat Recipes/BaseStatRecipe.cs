
using NaughtyAttributes;
using System.Reflection;
using UnityEngine;

namespace RogueWave
{
    /// <summary>
    /// A stat recipe will upgrade an objects stats.
    /// <seealso cref="GenericStatRecipe{T}"/>
    /// 
    /// </summary>
    public abstract class BaseStatRecipe : AbstractRecipe
    {
        [SerializeField, Tooltip("If true, this recipe will set a named parameter modifier for the weapon.")]
        bool isNamedParameterModifier = false;
        [SerializeField, Tooltip("The name of a parameter in the weapon to modify."), ShowIf("isNamedParameterModifier")]
        string parameterName;
        [SerializeField, Tooltip("The modifier to apply to the parameter. Note that while this is a float value, if the target parameter is an integer it will be rounded to int."), ShowIf("isNamedParameterModifier")]
        float parameterModifier = 1f;
        [SerializeField, Tooltip("The multiplier to apply to the parameter. This is applied after any modifier value. If the parameter is an int value the result will be rounded to an int."), ShowIf("isNamedParameterModifier")]
        float parameterMultiplier = 1.10f;
        [SerializeField, Tooltip("The time in seconds to wait before repeating the modifier. A value of 0 will only apply the modifier once."), MinValue(0f), ShowIf("isNamedParameterModifier")]
        internal float repeatEvery = 0f;
        
        public override string Category => "Base Stat";

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
                        value += parameterModifier;
                        value *= parameterMultiplier;
                        field.SetValue(target, value);
                        isSet = true;
                    }
                    else if (fieldValue is int intValue)
                    {
                        intValue += Mathf.RoundToInt(parameterModifier);
                        intValue = Mathf.RoundToInt(intValue * parameterMultiplier);
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
                            value += parameterModifier;
                            value *= parameterMultiplier;
                            property.SetValue(target, value);
                            isSet = true;
                        }
                        else if (propertyValue is int intValue)
                        {
                            intValue += Mathf.RoundToInt(parameterModifier);
                            intValue = Mathf.RoundToInt(intValue * parameterMultiplier);
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