using NeoFPS.SinglePlayer;
using NeoFPS;
using System.ComponentModel;
using UnityEngine;

namespace Playground
{
    [CreateAssetMenu(fileName = "Tool Pickup Recipe", menuName = "Playground/Tool Pickup Recipe")]
    public class ToolPickupRecipe : ItemRecipe<InventoryItemPickup>
    {

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