using NaughtyAttributes;
using NeoFPS;
using UnityEngine;

namespace RogueWave {
    public class WeaponPickup : InventoryItemPickup
    {
        [SerializeField, Tooltip("The pickup recipe for this weapon. When the player picks up this weapon, the recipe will be used to add the weapon to the Nanobot Manager for this run."), Required]
        WeaponPickupRecipe weaponPickupRecipe;
        [SerializeField, Tooltip("The pickup recipe for the ammo. When the player picks up this weapon, the recipe will be used to add the ammo to the Nanobot Manager for this run. " +
            "Note that the default recipe will be provided by the weapon itself. This allows an alternative recipe to be provided.")]
        AmmoPickupRecipe ammoPickupRecipe;

        public override void Trigger(ICharacter character)
        {
            base.Trigger(character);

            NanobotManager nanobotManager = character.GetComponent<NanobotManager>();
            if (nanobotManager != null)
            {
                RogueLiteManager.runData.Add(weaponPickupRecipe);
                nanobotManager.Add(weaponPickupRecipe);
                if (ammoPickupRecipe != null)
                {
                    RogueLiteManager.runData.Add(ammoPickupRecipe);
                    nanobotManager.Add(ammoPickupRecipe);
                }
            }
        }
    }
}
