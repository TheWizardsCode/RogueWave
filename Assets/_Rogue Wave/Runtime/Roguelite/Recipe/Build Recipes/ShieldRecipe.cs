﻿using NeoFPS;
using NeoFPS.SinglePlayer;
using UnityEngine;

namespace RogueWave
{
    /// <summary>
    /// This will add a shield manager to the player if they don't have one, and if they do, it will recharge a depleted shield.
    /// </summary>
    [CreateAssetMenu(fileName = "Shield Pickup Recipe", menuName = "Rogue Wave/Recipe/Shield Pickup", order = 110)]
    public class ShieldRecipe : GenericItemRecipe<ShieldPickup>
    {
        private ShieldManager _shieldMgr;

        public override string Category => "Shield";

        public override string TechnicalSummary
        {
            get
            {
                string summary = string.Empty;

                ShieldPickup itemPickup = Instantiate(pickup);

                summary = $"{Category} {itemPickup.stepCount}";

#if UNITY_EDITOR
                DestroyImmediate(itemPickup.gameObject);
#else
                Destroy(itemPickup.gameObject);
#endif

                return summary;
            }
        }

        private ShieldManager shieldManager
        {
            get
            {
                if (!_shieldMgr && FpsSoloCharacter.localPlayerCharacter != null)
                {
                    _shieldMgr = FpsSoloCharacter.localPlayerCharacter.GetComponent<ShieldManager>();
                }
                return _shieldMgr;
            }
        }

        public override void Reset()
        {
            _shieldMgr = null;
            base.Reset();
        }

        public override bool ShouldBuild
        {
            get
            {
                if (shieldManager == null)
                {
                    // Character has not been spawned yet and so we must be in the upgrade level menu
                    return base.ShouldBuild;
                }

                return shieldManager.shieldState != ShieldState.Recharging
                    && shieldManager.shield < (shieldManager.shieldStepCount * shieldManager.shieldStepCapacity)
                    && base.ShouldBuild;
            }
        }

        /// <summary>
        /// Check to see if the current shield amount is at least a minimum % of the maximum shield capacity.
        /// </summary>
        /// <param name="amountPerCent">Minimum amount we are checking for.</param>
        /// <returns></returns>
        public bool HasAmount(float amountPerCent)
        {
            return shieldManager.shield >= (shieldManager.shieldStepCount * shieldManager.shieldStepCapacity) * amountPerCent;
        }
    }
}