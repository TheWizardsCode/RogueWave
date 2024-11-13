using NeoFPS;
using NeoFPS.ModularFirearms;
using NeoFPS.SinglePlayer;
using System;
using UnityEngine;

namespace RogueWave
{
    [CreateAssetMenu(fileName = "Ammo Pickup Recipe", menuName = "Rogue Wave/Recipe/Ammo Pickup", order = 110)]
    public class AmmoRecipe : GenericItemRecipe<Pickup>
    {
        [SerializeField, Tooltip("The ammo type this recipe creates.")]
        internal SharedAmmoType ammo;

        public override string Category => "Ammunition";

        private FpsInventorySwappable _inventory;
        private FpsInventorySwappable inventory
        {
            get
            {
                if (!_inventory)
                {
                    _inventory = FpsSoloCharacter.localPlayerCharacter.inventory as FpsInventorySwappable;
                }
                return _inventory;
            }
        }

        public override void Reset()
        {
            _inventory = null;
            base.Reset();
        }

        /// <summary>
        /// Get the ammo amount as a percentage of the characters inventory space for this ammo.
        /// </summary>
        public float ammoAmountPerCent
        {
            get
            {
                if (inventory.selected == null)
                {
                    return 1;
                }

                SharedPoolAmmo sharedPoolAmmo = inventory.selected.GetComponent<SharedPoolAmmo>();
                if (sharedPoolAmmo == null)
                {
                    return 1;
                }
                return ((pickup as InventoryItemPickup).GetItemPrefab() as FpsInventoryAmmo).quantity / ((float)ammo.maxQuantity - sharedPoolAmmo.currentAmmo);
            }
        }
    }
}