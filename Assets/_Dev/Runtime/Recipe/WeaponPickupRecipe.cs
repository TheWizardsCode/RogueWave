using NeoFPS.ModularFirearms;
using NeoFPS;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NeoFPS.SinglePlayer;
using System.Reflection;

namespace Playground
{
    [CreateAssetMenu(fileName = "Weapon Pickup Recipe", menuName = "Playground/Weapon Pickup Recipe")]
    public class WeaponPickupRecipe : ItemRecipe<InteractivePickup>
    {
        [Header("Weapon")]
        [SerializeField, Tooltip("The Ammo recipe for this weapon. When the weapon is built the player should get this recipe too.")]
        internal AmmoPickupRecipe ammoRecipe;

        private FpsInventorySwappable _inventory;
        private FpsInventorySwappable inventory
        {
            get
            {
                if (_inventory == null)
                {
                    _inventory = FpsSoloCharacter.localPlayerCharacter.inventory as FpsInventorySwappable;
                }
                return _inventory;
            }
        }

        public override void BuildFinished()
        {
            NanobotManager nanobotManager = FpsSoloCharacter.localPlayerCharacter.GetComponent<NanobotManager>();
            nanobotManager.Add(ammoRecipe);
            RogueLiteManager.runData.AddToLoadout(pickup.GetItemPrefab() as FpsInventoryItemBase);

            base.BuildFinished();
        }

        public override bool ShouldBuild
        {
            get
            {
                IInventoryItem[] ownedItems = inventory.GetItems();
                for (int i = 0; i < ownedItems.Length; i++)
                {
                    if (ownedItems[i].itemIdentifier == pickup.GetItemPrefab().itemIdentifier)
                    {
                        return false;
                    }
                }

                return true;
            }
        }
    }
}
