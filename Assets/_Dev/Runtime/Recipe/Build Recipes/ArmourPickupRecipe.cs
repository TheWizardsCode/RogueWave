using NeoFPS;
using NeoFPS.SinglePlayer;
using UnityEngine;

namespace RogueWave
{
    [CreateAssetMenu(fileName = "Armour Pickup Recipe", menuName = "Rogue Wave/Recipe/Armour Pickup", order = 110)]
    public class ArmourPickupRecipe : ItemRecipe<InventoryItemPickup>
    {
        [SerializeField, FpsInventoryKey, Tooltip("The inventory ID of the armour type")]
        private int m_InventoryID = 0;

        IInventory _Inventory;
        IInventoryItem _ArmourItem;

        private IInventoryItem armourItem
        {
            get
            {
                if (FpsSoloCharacter.localPlayerCharacter == null)
                {
                    return null;
                }

                if (_Inventory == null)
                {
                    _Inventory = FpsSoloCharacter.localPlayerCharacter.inventory;
                    if (_Inventory == null)
                    {
                        return null;
                    }
                }

                _ArmourItem = _Inventory.GetItem(m_InventoryID);
                if (_ArmourItem == null || _ArmourItem.quantity == 0)
                {
                    return null;
                }

                return _ArmourItem;
            }
        }

        public override void Reset()
        {
            _Inventory = null;
            _ArmourItem = null;
            base.Reset();
        }

        public override bool ShouldBuild
        {
            get
            {
                if (_ArmourItem == null)
                {
                    return base.ShouldBuild;
                }

                return armourItem.quantity < 100
                    && base.ShouldBuild;
            }
        }

        /// <summary>
        /// Check to see if the current shield amount is at least a minimum % of the maximum shield capacity.
        /// </summary>
        /// <param name="amountPerCent">Minimum amount we are checking for.</param>
        /// <returns></returns>
        public bool HasAmount(float amountPerCent)
        {
            if (armourItem == null)
            {
                return false;
            }   

            return armourItem.quantity >= 100 * amountPerCent;
        }
    }
}