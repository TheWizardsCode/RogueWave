using NaughtyAttributes;
using NeoFPS.ModularFirearms;
using NeoFPS.SinglePlayer;
using System;
using UnityEditor;
using UnityEngine;

namespace Playground
{
    /// <summary>
    /// An Ammunition Effect Recipe will modify all weapons that use the specified ammunition effect.
    /// </summary>
    public abstract class AmmunitionEffectUpgradeRecipe : AbstractRecipe
    {
        [SerializeField, Tooltip("The ammunition type this upgrade applies to.")]
        internal SharedAmmoType ammoType;

        internal abstract void Apply(RogueWaveBulletAmmoEffect ammoEffect);

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