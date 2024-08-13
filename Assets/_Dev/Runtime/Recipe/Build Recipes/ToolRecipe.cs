using NeoFPS.SinglePlayer;
using NeoFPS;
using UnityEngine;
using System;

namespace RogueWave
{
    [CreateAssetMenu(fileName = "Tool Pickup Recipe", menuName = "Rogue Wave/Recipe/Tool Pickup", order = 105)]
    public class ToolRecipe : ItemRecipe<InventoryItemPickup>
    {
        public override string Category => "Tool";

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

        public override bool ShouldBuild
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
                            return false;
                        }
                    }
                }

                return base.ShouldBuild;
            }
        }
    }
}