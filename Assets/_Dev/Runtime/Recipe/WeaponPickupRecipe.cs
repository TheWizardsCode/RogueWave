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
    public class WeaponPickupRecipe : Recipe<InteractivePickup>
    {
        [Header("Weapon")]
        [SerializeField, Tooltip("The Ammo recipe for this weapon. When the weapon is built the player should get this recipe too.")]
        private AmmoPickupRecipe ammoRecipe;
        [SerializeField, Tooltip("The inventory slot this item should be placed in.")]
        internal int inventorySlot = 1;

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
            RogueLiteManager.runData.Add(pickup.GetItem() as FpsInventoryItemBase);

            base.BuildFinished();
        }

        public override bool ShouldBuild
        {
            get
            {
                IInventoryItem[] ownedItems = inventory.GetItems();
                for (int i = 0; i < ownedItems.Length; i++)
                {
                    if (ownedItems[i].itemIdentifier == pickup.GetItem().itemIdentifier)
                    {
                        return false;
                    }
                }

                return true;
            }
        }
    }

    public static class InteractivePickupExtension
    {
        public static FpsInventoryItemBase GetItem(this InteractivePickup instance)
        {
            var fieldInfo = typeof(InteractivePickup).GetField("m_Item", BindingFlags.NonPublic | BindingFlags.Instance);
            return fieldInfo.GetValue(instance) as FpsInventoryItemBase;
        }
    }
}
