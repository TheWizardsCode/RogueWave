using NeoFPS;
using NeoFPS.SinglePlayer;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Playground { 
    [CreateAssetMenu(fileName = "Max Health Recipe", menuName = "Rogue Wave/Recipe/Maximum Health Recipe", order = 10)]
    public class MaxHealthIncreaseRecipe : BaseStatRecipe
    {
        [SerializeField, Tooltip("The amount to add to the current MaxHealth of the player.")]
        int AdditionalMaxHealth = 20;

        internal override void Apply()
        {
            BasicHealthManager healthManager = FpsSoloCharacter.localPlayerCharacter.GetComponent<BasicHealthManager>();
            if (healthManager != null)
            {
                healthManager.healthMax += AdditionalMaxHealth;
            }
        }
    }
}