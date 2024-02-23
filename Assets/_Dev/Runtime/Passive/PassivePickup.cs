using NeoFPS;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RogueWave
{
    public class PassivePickup : Pickup
    {
        [SerializeField, Tooltip("The inventory item prefab to give to the character.")]
        internal PassiveWeapon itemPrefab = null;

        [SerializeField, Tooltip("The display mesh of the pickup. This should not be the same game object as this, so that if this is disabled the pickup will still respawn if required.")]
        private GameObject m_DisplayMesh = null;

        public override void Trigger(ICharacter character)
        {
            base.Trigger(character);

            Debug.LogError($"TODO: Pickup the passive item {itemPrefab}");
        }

    }
}