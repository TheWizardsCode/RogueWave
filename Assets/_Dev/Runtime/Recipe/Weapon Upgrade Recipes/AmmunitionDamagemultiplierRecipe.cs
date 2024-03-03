using NeoFPS;
using NeoFPS.ModularFirearms;
using NeoFPS.SinglePlayer;
using UnityEngine;

namespace RogueWave
{
    /// <summary>
    /// Multiply the damage of the ammunition by a certain value. This effect will be applied to all weapons that use the specified ammunition type.
    /// </summary>
    [CreateAssetMenu(fileName = "Ammunition Damage Multiplier Recipe", menuName = "Rogue Wave/Recipe/Ammunition Damage Multiplier", order = 10)]
    public class AmmunitionDamageMultiplierRecipe : AmmunitionEffectUpgradeRecipe
    {
        [Header("Upgrade")]
        [SerializeField, Tooltip("The damage multiplier to apply to this ammo type.")]
        float multiplier = 1.1f;

        internal override void Apply(RogueWaveBulletAmmoEffect effect)
        {
            effect.damage *= multiplier;
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (string.IsNullOrEmpty(uniqueID))
            {
                GenerateID();
            }
        }
#endif
    }
}