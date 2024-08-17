using NeoFPS;
using UnityEngine;

namespace RogueWave
{
    public class PassivePickup : Pickup
    {
        [SerializeField, Tooltip("The inventory item prefab to give to the character.")]
        internal GameObject itemPrefab = null;
        [SerializeField, Tooltip("The inventory item recipe to give to the character.")]
        internal PassiveItemRecipe recipe = null;

        public override void Trigger(ICharacter character)
        {
            base.Trigger(character);

            NanobotManager nanobotManager = character.GetComponent<NanobotManager>();
            if (nanobotManager != null)
            {
                RogueLiteManager.runData.Add(recipe);
                nanobotManager.Add(recipe);
            }

            Destroy(gameObject);
        }

    }
}