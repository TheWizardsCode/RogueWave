using NeoFPS;
using UnityEngine;
using NeoFPS.SinglePlayer;
using System;
using NaughtyAttributes;

namespace RogueWave
{
    [CreateAssetMenu(fileName = "Weapon Pickup Recipe", menuName = "Rogue Wave/Recipe/Weapon Pickup", order = 108)]
    public class WeaponRecipe : GenericItemRecipe<InventoryItemPickup>
    {
        [Header("Weapon")]
        [SerializeField, Tooltip("If true then the weapon will be put to the top of the loadout build order when purchased, otherwise it will be put in the second slot.")]
        internal bool overridePrimaryWeapon = false;
        [SerializeField, Tooltip("Does this weapon use ammo? If set to true then an ammoRecipe must also be provided. If set to false then the weapon is consumed when used but it will be possible to build as many of the item as the inventory can hold.")]
        internal bool usesAmmo = true;
        [SerializeField, Tooltip("The Ammo recipe for this weapon. When the weapon is built the player should get this recipe too."), ShowIf("usesAmmo")]
        internal AmmoRecipe ammoRecipe;

        public override string Category => "Weapon";

        [NonSerialized]
        private FpsInventorySwappable _inventory;
        private FpsInventorySwappable inventory
        {
            get
            {
                if (!_inventory && FpsSoloCharacter.localPlayerCharacter != null)
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

        public override void BuildFinished()
        {
            if (ammoRecipe != null)
            {
                RogueLiteManager.runData.Add(ammoRecipe);
                FpsSoloCharacter.localPlayerCharacter.GetComponent<NanobotManager>().Add(ammoRecipe);
            }
            RogueLiteManager.runData.AddToLoadout(pickup.GetItemPrefab() as FpsInventoryItemBase);

            base.BuildFinished();
        }

        public override bool ShouldBuild
        {
            get
            {
                if (InInventory == false || !usesAmmo)
                {
                    IInventoryItem item = pickup.GetItemPrefab();
                    int slot = inventory.GetSlotForItem(item as ISwappable);
                    if (slot == -1 || inventory.GetSlotItem(slot) != null)
                    {
                        return false;
                    }

                    if (!usesAmmo)
                    {
                        FpsInventoryItemBase existing = (FpsInventoryItemBase)inventory.GetItem(item.itemIdentifier);
                        if (existing != null && existing.quantity >= existing.maxQuantity)
                        {
                            return false;
                        }
                    }

                    return base.ShouldBuild;
                }
                else
                {
                    return false;
                }
            }
        }

        public bool InInventory
        {
            get
            {
                if (inventory != null)
                {
                    IInventoryItem[] ownedItems = inventory.GetItems();
                    for (int i = 0; i < ownedItems.Length; i++)
                    {
                        if (ownedItems[i].itemIdentifier == pickup.GetItemPrefab().itemIdentifier)
                        {
                            return true;
                        }
                    }

                    return false;
                }
                else
                {
                     return false;
                }
            }
        }
    }
}
