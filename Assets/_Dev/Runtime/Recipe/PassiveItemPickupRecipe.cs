using NeoFPS;
using NeoFPS.SinglePlayer;
using System;
using UnityEngine;

namespace RogueWave
{
    [CreateAssetMenu(fileName = "Passive Item Pickup Recipe", menuName = "Rogue Wave/Recipe/Passive Item Pickup", order = 106)]
    public class PassiveItemPickupRecipe : ItemRecipe<PassivePickup>
    {
        public override string Category => "Passive Item";

        /// <summary>
        /// Apply this recipe if there is not too many applied already.
        /// </summary>
        /// <param name="manager">The Nanobot Manager applying this recipe.</param>
        /// <returns>True if succesfully applied, otherwise false.</returns>
        internal bool Apply(NanobotManager manager)
        {
            int applied = manager.GetAppliedCount(this);

            if (applied < MaxStack)
            {
                GameObject passiveWeapon = Instantiate(pickup.itemPrefab);
                passiveWeapon.transform.SetParent(manager.transform, false);
                return true;
            }

            return false;
        }
    }
}