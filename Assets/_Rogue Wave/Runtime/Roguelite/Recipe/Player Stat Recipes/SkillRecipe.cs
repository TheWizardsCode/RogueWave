using NaughtyAttributes;
using NeoFPS.SinglePlayer;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RogueWave
{
    /// <summary>
    /// The Bool Stat Recipe will upgrade one of the player's bool stats, such as canDash.
    /// </summary>
    [CreateAssetMenu(fileName = "Switch Stat Recipe", menuName = "Rogue Wave/Recipe/Switch Stat", order = 1)]
    public class SkillRecipe : BaseStatRecipe
    {
        [SerializeField, Tooltip("The name of the stat to modify.")]
        internal string statName = string.Empty;
        [SerializeField, Tooltip("The value of the stat when this recipe is applied.")]
        bool value = false;

        public override string Category => "Stat";

        MovementUpgradeManager _movementUpgradeManager;
        internal MovementUpgradeManager movementUpgradeManager
        {
            get
            {
                if (_movementUpgradeManager == null)
                {
                    _movementUpgradeManager = FpsSoloCharacter.localPlayerCharacter.GetComponent<MovementUpgradeManager>();
                }
                return _movementUpgradeManager;
            }
        }

        [Button("Apply Switch (works in game only)")]
        internal override void Apply()
        {
            movementUpgradeManager.SetBoolOverride(statName, value);
        }
    }
}