using NeoFPS;
using NeoFPS.ModularFirearms;
using NeoFPS.SinglePlayer;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static NeoFPS.BasicHealthManager;

namespace Playground
{
    public class NanobotManager : MonoBehaviour
    {
        [SerializeField, Tooltip("The recipes available to the Nanobots in order of preference.")]
        private Recipe[] recipes;

        public delegate void OnResourcesChanged(float from, float to);
        public event OnResourcesChanged onResourcesChanged;

        private int currentResources = 0;
        private FpsInventorySwappable inventory;
        private bool isBuilding = false;

        private void Start()
        {
            inventory = FpsSoloCharacter.localPlayerCharacter.inventory as FpsInventorySwappable;
        }

        private void Update()
        {
            // TODO: currently we only enable a single recipe, which is to build 556 ammo. Need to make this more generic
            Recipe recipe = recipes[0];
            
            IQuickSlotItem item = inventory.selected;
            if (item == null)
            {
                return;
            }

            SharedPoolAmmo ammo = item.GetComponent<SharedPoolAmmo>();
            if (ammo == null)
            {
                return;
            }

            if (ammo.ammoType.itemIdentifier != recipe.item.itemIdentifier)
            {
                return;
            }

            if (ammo.atMaximum)
            {
                return;
            }

            if (!isBuilding && currentResources >= recipe.cost)
            {
                StartCoroutine(BuildRecipe(recipe));
            }
        }

        private IEnumerator BuildRecipe(Recipe recipe)
        {
            isBuilding = true;
            resources -= recipe.cost;

            yield return new WaitForSeconds(recipe.timeToBuild);

            // TODO Use the pool manager to create the item
            inventory.AddItem(Instantiate(recipe.item));
            isBuilding = false;
        }

        /// <summary>
        /// The amount of resources the player currently has.
        /// </summary>
        public int resources
        {
            get { return currentResources; }
            set
            {
                if (currentResources == value)
                    return;

                if (onResourcesChanged != null)
                    onResourcesChanged(currentResources, value);

                currentResources = value;
            }
        }
    }

    [Serializable]
    public class Recipe
    {
        [SerializeField, Tooltip("The name of this recipe.")]
        public string name = "TBD";
        [SerializeField, Tooltip("The resources required to build this ammo type.")]
        public int cost = 10;
        [SerializeField, Tooltip("The inventory item this recipe creates.")]
        public FpsInventoryItemBase item;
        [SerializeField, Tooltip("The time it takes to build this recipe.")]
        public float timeToBuild = 5;
    }
}