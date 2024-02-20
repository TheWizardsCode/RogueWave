using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Playground
{
    /// <summary>
    /// The Bool Stat Recipe will upgrade one of the player's bool stats, such as canDash.
    /// </summary>
    [CreateAssetMenu(fileName = "Switch Stat Recipe", menuName = "Playground/Recipe/Switch Stat", order = 1)]
    public class SwitchStatRecipe : BaseStatRecipe
    {
        [SerializeField, Tooltip("The name of the stat to modify.")]
        internal string statName = string.Empty;
        [SerializeField, Tooltip("The value of the stat when this recipe is applied.")]
        bool value = false;

        [Button("Apply Switch (works in game only)")]
        internal override void Apply()
        {
            movementUpgradeManager.SetBoolOverride(statName, value);
        }
    }
}