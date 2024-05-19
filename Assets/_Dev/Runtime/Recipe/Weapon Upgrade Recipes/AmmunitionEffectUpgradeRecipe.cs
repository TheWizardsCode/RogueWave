using NeoFPS;
using NeoFPS.ModularFirearms;
using NeoFPS.SinglePlayer;
using UnityEngine;

namespace RogueWave
{
    /// <summary>
    /// An Ammunition Effect Recipe will modify all weapons that use the specified ammunition effect.
    /// </summary>
    public abstract class AmmunitionEffectUpgradeRecipe : AbstractRecipe
    {
        [SerializeField, Tooltip("The ammunition type this upgrade applies to.")]
        internal SharedAmmoType ammoType;

        public override string Category => "Ammunition";

        internal virtual void Apply()
        {
            IInventoryItem[] items = FpsSoloCharacter.localPlayerCharacter.inventory.GetItems();
            foreach (IInventoryItem item in items)
            {
                RogueWaveBulletAmmoEffect ammoEffect = item.GetComponent<RogueWaveBulletAmmoEffect>();
                if (ammoEffect != null)
                {
                    Apply(ammoEffect);
                }
            }
        }

        internal abstract void Apply(RogueWaveBulletAmmoEffect effect);

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