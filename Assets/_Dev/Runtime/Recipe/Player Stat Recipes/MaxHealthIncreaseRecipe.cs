using NeoFPS;
using NeoFPS.SinglePlayer;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RogueWave { 
    [CreateAssetMenu(fileName = "Max Health Recipe", menuName = "Rogue Wave/Recipe/Maximum Health Recipe", order = 10)]
    // REFACTOR: can we remove this and make it a BaseStatRecipe instead?
    public class MaxHealthIncreaseRecipe : GenericStatRecipe<MonoBehaviour>
    {
        [Header("Stat Modifier")]
        [SerializeField, Tooltip("The amount to add to the current MaxHealth of the player.")]
        int AdditionalMaxHealth = 20;

        public override string Category => "Health";

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