using NeoFPS;
using NeoFPS.ModularFirearms;
using NeoFPS.SinglePlayer;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Playground
{
    public class NanobotManager : MonoBehaviour
    {
        [Header("Starting Recipes")]
        [SerializeField, Tooltip("The health recipes available to the Nanobots in order of preference.")]
        private List<HealthPickupRecipe> healthRecipes = new List<HealthPickupRecipe>();
        [SerializeField, Tooltip("The weapon recipes available to the Nanobots in order of preference.")]
        private List<WeaponPickupRecipe> weaponRecipes = new List<WeaponPickupRecipe>();
        [SerializeField, Tooltip("The ammo recipes available to the Nanobots in order of preference.")]
        private List<AmmoPickupRecipe> ammoRecipes = new List<AmmoPickupRecipe>();

        [Header("Building")]
        [SerializeField, Tooltip("Cooldown between recipes.")]
        private float cooldown = 5;

        [Header("Feedback")]
        [SerializeField, Tooltip("The sound to play when the build is started. Note that this can be overridden in the recipe.")]
        private AudioClip buildStartedClip;
        [SerializeField, Tooltip("The sound to play when the build is complete. Note that this can be overridden in the recipe.")]
        private AudioClip buildCompleteClip;
        [SerializeField, Tooltip("The particle system to play when a pickup is spawned.")]
        ParticleSystem pickupSpawnParticlePrefab;

        public delegate void OnResourcesChanged(float from, float to);
        public event OnResourcesChanged onResourcesChanged;

        private int currentResources = 0;
        private bool isBuilding = false;
        private float timeOfNextBuiild = 0;

        private List<FpsInventoryBase> obtainedWeapons = new List<FpsInventoryBase>();

        private void Update()
        {
            if (isBuilding || Time.timeSinceLevelLoad < timeOfNextBuiild)
            {
                return;
            }

            // Prioritize building ammo if the player is low on ammo
            if (TryAmmoRecipes(0.1f))
            {
                return;
            }

            // Health is the next priority, got to stay alive
            if (TryHealthRecipes())
            {
                return;
            }

            // If we can afford a powerup, build it
            if(TryPowerUpRecipes()) {
                return;
            }

            // If we can't afford a powerup, build ammo up to near mazimum (not maximum because there will often be half used clips lying around)
            if (TryAmmoRecipes(0.9f))
            {
                return;
            }
        }

        private bool TryHealthRecipes()
        {
            for (int i = 0; i < healthRecipes.Count; i++)
            {
                if (TryRecipe(healthRecipes[i]))
                {
                    return true;
                }
            }

            return false;
        }

        private bool TryPowerUpRecipes()
        {
            for (int i = 0; i < weaponRecipes.Count; i++)
            {
                if (TryRecipe(weaponRecipes[i]))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// If the ammo available for the currently equipped weapon is below the minimum level, try to build ammo.
        /// </summary>
        /// <param name="minimumAmmoAmount">The % (0-1) of ammo that is the minimum required</param>
        /// <returns></returns>
        private bool TryAmmoRecipes(float minimumAmmoAmount)
        {
            for (int i = 0; i < ammoRecipes.Count; i++)
            {
                if (!ammoRecipes[i].HasAmount(minimumAmmoAmount))
                {
                    return TryRecipe(ammoRecipes[i]);
                }
            }

            return false;
        }

        private bool TryRecipe(IRecipe recipe)
        {
            if (currentResources >= recipe.Cost && recipe.ShouldBuild)
            {
                StartCoroutine(BuildRecipe(recipe));
                return true;
            }

            return false;
        }

        private IEnumerator BuildRecipe(IRecipe recipe)
        {
            isBuilding = true;
            resources -= recipe.Cost;

            if (recipe.BuildStartedClip != null)
            {
                NeoFpsAudioManager.PlayEffectAudioAtPosition(recipe.BuildStartedClip, transform.position, 1);
            } else
            {
                NeoFpsAudioManager.PlayEffectAudioAtPosition(buildStartedClip, transform.position, 1);
            }

            yield return new WaitForSeconds(recipe.TimeToBuild);

            // TODO Use the pool manager to create the item
            GameObject go = Instantiate(recipe.Item.gameObject);
            go.transform.position = transform.position + (transform.forward * 5);
            if (recipe.BuildCompleteClip != null)
            {
                NeoFpsAudioManager.PlayEffectAudioAtPosition(recipe.BuildCompleteClip, go.transform.position, 1);
            } else
            {
                NeoFpsAudioManager.PlayEffectAudioAtPosition(buildCompleteClip, go.transform.position, 1);
            }

            // TODO: Use the pool manager to create the particle system
            if (pickupSpawnParticlePrefab != null)
            {
                ParticleSystem ps = Instantiate(pickupSpawnParticlePrefab, go.transform);
                ps.Play();
            }

            recipe.BuildFinished();

            isBuilding = false;
            timeOfNextBuiild = Time.timeSinceLevelLoad + cooldown;
        }

        /// <summary>
        /// Adds the recipe to the list of starting recipes.
        /// </summary>
        /// <param name="ammoRecipe"></param>
        internal void Add(IRecipe recipe)
        {
            AmmoPickupRecipe ammo = recipe as AmmoPickupRecipe;
            if (recipe != null)
            {
                if (!ammoRecipes.Contains(ammo))
                {
                    ammoRecipes.Add(recipe as AmmoPickupRecipe);
                }
                return;
            }

            HealthPickupRecipe health = recipe as HealthPickupRecipe;
            if (recipe != null)
            {
                if (!healthRecipes.Contains(health))
                {
                    healthRecipes.Add(recipe as HealthPickupRecipe);
                }
                return;
            }

            WeaponPickupRecipe weapon = recipe as WeaponPickupRecipe;
            if (recipe != null)
            {
                if (!weaponRecipes.Contains(weapon))
                {
                    weaponRecipes.Add(recipe as WeaponPickupRecipe);
                }
                return;
            }
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
}