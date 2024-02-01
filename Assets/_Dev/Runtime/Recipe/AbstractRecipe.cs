using NaughtyAttributes;
using System;
using UnityEditor;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Playground
{
    public abstract class AbstractRecipe : ScriptableObject, IRecipe
    {
        [Header("Metadata")]
        [SerializeField, Tooltip("The name of this recipe. Used in the UI for the player.")]
        string displayName = "TBD";
        [SerializeField, TextArea(1, 4), Tooltip("A short description of this recipe helping the player understand what it is.")]
        string description = "TBD";
        [SerializeField, Tooltip("An image to use as the hero image for this recipe."), ShowAssetPreview]
        Texture2D heroImage;
        [SerializeField, Tooltip("A sprite to use as the icon for this recipe."), ShowAssetPreview]
        Sprite icon;

        [Header("Build")]
        [SerializeField, Tooltip("Powerups are recipes that can be offered between levels and, if purchased, become permanent.")]
        bool isPowerUp = false;
        [SerializeField, Tooltip("Consumables are recipes that are used up when they are built. For example, health. These recipes can be built as many times as needed once they have been learned.")]
        bool isConsumable = false;
        [SerializeField, Tooltip("The cooldown time for this recipe (in seconds). If the nanobots have built this recipe, they cannot build it again until this time has passed.")]
        float cooldown = 10;
        [SerializeField, Tooltip("The recipes that must be built before this recipe can be built.")]
        AbstractRecipe[] dependencies = new AbstractRecipe[0];
        [SerializeField, Tooltip("If true, this recipe can be stacked, that is if the player can hold more than onve of these at a time. Weapons, for example, are not stackable while health boosts are.")]
        bool isStackable = false;
        [SerializeField, ShowIf("isStackable"), Tooltip("The maximum number of this recipe that can be held at once.")]
        int maxStack = 1;
        [SerializeField, Tooltip("The resources required to build this ammo type.")]
        int cost = 10;
        [SerializeField, Tooltip("The time it takes to build this recipe.")]
        float timeToBuild = 5;

        [Header("Feedback")]
        [SerializeField, Tooltip("The sound to play when an announcer needs to provide a name for this recipe.")]
        AudioClip[] nameClips = new AudioClip[0];
        [SerializeField, Tooltip("The sound to play when the build is started. If there are no sounds here then a default will be used.")]
        AudioClip[] buildStartedClips = new AudioClip[0];
        [SerializeField, Tooltip("The sound to play when the build is complete. If there are no sounds here then a default will be used.")]
        AudioClip[] buildCompleteClips = new AudioClip[0];
        [SerializeField, Tooltip("The particle system to play when a pickup is spawned.")]
        ParticleSystem pickupParticles;

        float nextTimeAvailable = 0;

        public string UniqueID => uniqueID;

        public string DisplayName => displayName;

        public string Description => description;

        public Texture2D HeroImage => heroImage;

        public Sprite Icon => icon;

        public bool IsPowerUp => isPowerUp;

        public bool IsConsumable => isConsumable;

        public bool IsStackable => maxStack > 1;

        public int MaxStack => maxStack;

        public int Cost => cost;

        public float TimeToBuild => timeToBuild;

        public AudioClip NameClip {
            get
            {
                if (nameClips.Length == 0)
                {
                    return null;
                }
                else
                {
                    return nameClips[Random.Range(0, nameClips.Length)];
                }
            }
        }

        public AudioClip BuildStartedClip
        {
            get
            {
                if (buildStartedClips.Length == 0)
                {
                    return null;
                } else
                {
                    return buildStartedClips[Random.Range(0, buildStartedClips.Length)];
                }
            }
        }

        public AudioClip BuildCompleteClip
        {
            get
            {
                if (buildCompleteClips.Length == 0)
                {
                    return null;
                }
                else
                {
                    return buildStartedClips[Random.Range(0, buildCompleteClips.Length)];
                }
            }
        }

        public ParticleSystem PickupParticles => pickupParticles;

        public virtual void Reset()
        {
        }

        public virtual bool ShouldBuild
        {
            get
            {
                if (Time.timeSinceLevelLoad < nextTimeAvailable)
                {
                    return false;
                }

                if (isStackable) 
                {
                    if (RogueLiteManager.runData.GetCount(this) >= MaxStack)
                    {
                        return false;
                    }
                }

                foreach (IRecipe dependency in dependencies)
                {
                    if (RogueLiteManager.persistentData.RecipeIds.Contains(dependency.UniqueID) == false && RogueLiteManager.runData.Recipes.Contains(dependency) == false)
                    {
                        //Debug.Log(dependency.DisplayName + " is a dependency of " + DisplayName + " but is not in the player's persistent or run data. Cannot build.");
                        return false;
                    }
                    //Debug.Log(dependency.DisplayName + " is a dependency of " + DisplayName + " and is in the player's persistent or run data. Can build.");
                }
                return true;
            }
        }

        public virtual void BuildFinished()
        {
            nextTimeAvailable = Time.timeSinceLevelLoad + cooldown;
        }


        [SerializeField, Tooltip("DO NOT CHANGE THIS. TODO: Create a custom editor that hides this in case of accidental change."), BoxGroup("Internal"), ReadOnly]
        internal string uniqueID;

#if UNITY_EDITOR
        [Button("ERROR: Need a valid ID."), HideIf("IsValidID")]
        protected void GenerateID()
        {
            uniqueID = Guid.NewGuid().ToString();
            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssets();
        }

        private bool IsValidID
        {
            get { return string.IsNullOrEmpty(uniqueID) == false; }
        }
#endif
    }
}