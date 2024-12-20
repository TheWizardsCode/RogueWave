﻿using NeoFPS;
using NeoFPS.ModularFirearms;
using NeoFPS.SinglePlayer;
using System;
using UnityEngine;

namespace RogueWave
{
    [CreateAssetMenu(fileName = "Health Pickup Recipe", menuName = "Rogue Wave/Recipe/Health Pickup", order = 115)]
    public class HealthPickupRecipe : GenericItemRecipe<HealthPickup>
    {
        public override string Category => "Health";

        public override string TechnicalSummary
        {
            get
            {
                string summary = string.Empty;

                HealthPickup healthPickup = Instantiate(pickup);
                if (healthPickup.GetHealType() == HealthPickup.HealType.FixedValue)
                {
                    summary = $"Heal {healthPickup.GetHealAmount()} HP";
                }
                else if (healthPickup.GetHealType() == HealthPickup.HealType.Factor)
                {
                    summary = $"Heal {healthPickup.GetHealAmount() * 100}% HP";
                } else if (healthPickup.GetHealType() == HealthPickup.HealType.MissingFactor)
                {
                    summary = $"Heal {healthPickup.GetHealAmount() * 100}% of missing HP";
                }

#if UNITY_EDITOR
                DestroyImmediate(healthPickup.gameObject);
#else
                Destroy(healthPickup.gameObject);
#endif

                return summary;
            }
        }

        [NonSerialized]
        private BasicHealthManager _healthManager;
        private BasicHealthManager healthManager
        {
            get
            {
                if (!_healthManager && FpsSoloCharacter.localPlayerCharacter != null)
                {
                    _healthManager = FpsSoloCharacter.localPlayerCharacter.GetComponent<BasicHealthManager>();
                }
                return _healthManager;
            }
        }

        public override void Reset()
        {
            _healthManager = null;
            base.Reset();
        }

        /// <summary>
        /// Get the heal amount as a percentage of the characters missing health.
        /// </summary>
        public float healAmountPerCent
        {
            get {
                return (pickup as HealthPickup).GetHealAmount() / (_healthManager.healthMax - _healthManager.health);
            }
        }

        public override bool ShouldBuild
        {
            get
            {
                if (healthManager == null)
                {
                    // Character has not been spawned yet and so we must be in the upgrade level menu
                    return base.ShouldBuild;
                }

                float missingHealth = healthManager.healthMax - healthManager.health;

                return missingHealth > 0 && base.ShouldBuild;
            }
        }

        /// <summary>
        /// Test to see if the character currently has at least a minimum % health available to them.
        /// </summary>
        public bool HasAmount(float amountPerCent)
        {
            if (healthManager == null)
            {
                return true;
            }

            return healthManager.health / healthManager.healthMax >= amountPerCent;
        }
    }
}