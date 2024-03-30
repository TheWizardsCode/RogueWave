using NaughtyAttributes;
using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

namespace RogueWave
{
    public abstract class AbstractRecipe : ScriptableObject, IRecipe
    {
        [Header("Description")]
        [SerializeField, Tooltip("The name of this recipe. Used in the UI for the player.")]
        string displayName = "TBD";
        [SerializeField, TextArea(1, 4), Tooltip("A short description of this recipe helping the player understand what it is.")]
        string description = "TBD";
        [SerializeField, Tooltip("An image to use as the hero image for this recipe."), ShowAssetPreview]
        Texture2D heroImage;
        [SerializeField, Tooltip("A sprite to use as the icon for this recipe."), ShowAssetPreview]
        Sprite icon;

        [Header("Selection Criteria")]
        [SerializeField, Tooltip("The zero based level of this recipe. This is used to influence when the recipe should be offered to the player.")]
        int level = 1;
        [SerializeField, Tooltip("The base weight of this recipe. This is used to influence when the recipe should be offered to the player if all prerequisites have been set. Other factors will affect the total weight, such as the number of complements already owned.")]
        float baseWeight = 0.2f;
        [SerializeField, Tooltip("The recipes that must be built before this recipe can be built.")]
        AbstractRecipe[] dependencies = new AbstractRecipe[0];
        [SerializeField, Tooltip("The recipes that complement this one. For each complimentary recipe the player already has this one will be given a higher chance of being offered.")]
        AbstractRecipe[] complements = new AbstractRecipe[0];
        [SerializeField, Tooltip("The resources required to buy this recipe. If the recipe is for a built item then there will be an additional cost to build it (see 'buildCost'), if it is an immediate upgrade then this is the only cost incurred.")]
        [FormerlySerializedAs("cost")]
        int buyCost = 500;


        [Header("Build")]
        [SerializeField, Tooltip("Powerups are recipes that can be offered between levels and, if purchased, become permanent.")]
        bool isPowerUp = false;
        [SerializeField, Tooltip("Consumables are recipes that are used up when they are built. Typically such recipes are only built once. For example, it is unlikely that the player will need two Pistols, so the recipe is marked as consumable. The recipe remains available, but will only come back into use if the item it built is lost for some reason.")]
        bool isConsumable = false;
        [SerializeField, Tooltip("The cooldown time for this recipe (in seconds). If the nanobots have built this recipe, they cannot build it again until this time has passed.")]
        float cooldown = 10;
        [SerializeField, Tooltip("If true, this recipe can be stacked, that is if the player can hold more than onve of these at a time. Weapons, for example, are not stackable while health boosts are.")]
        bool isStackable = false;
        [SerializeField, ShowIf("isStackable"), Tooltip("The maximum number of this recipe that can be held at once.")]
        int maxStack = 1;
        [SerializeField, Tooltip("The resources required to build this recipe. It must first be bought, see 'buyCost'")]
        int buildCost = 50;
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

        public int Level => level;

        public bool IsPowerUp => isPowerUp;

        public bool IsConsumable => isConsumable;

        public bool IsStackable => isStackable;

        public int MaxStack => maxStack;

        public int BuyCost {
            get
            {
                
                if (IsStackable)
                {
                    int ownedCopies = RogueLiteManager.runData.GetCount(this);
                    return Mathf.RoundToInt(buyCost * (1 + (ownedCopies * 0.1f)));
                }
                else
                {
                    return buyCost;
                }
            }
        }

        public int BuildCost => buildCost;

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

        public float weight {
            get
            {
                float adjustedWeight = baseWeight;

                int ownedComplements = 0;
                foreach (IRecipe complement in complements)
                {
                    if (RogueLiteManager.runData.Contains(complement))
                    {
                        ownedComplements++;
                    }
                }
                if (ownedComplements > 0)
                {
                    adjustedWeight += ownedComplements * 0.05f;
                }

                int count = RogueLiteManager.runData.GetCount(this);
                if (IsStackable && count > 0)
                {   
                    adjustedWeight *= 1.1f + ((float)count/maxStack);
                }

                return adjustedWeight;
            }
        }

        public virtual void Reset()
        {
            nextTimeAvailable = 0;
        }

        public virtual bool ShouldBuild
        {
            get
            {
                if (Time.timeSinceLevelLoad < nextTimeAvailable)
                {
                    return false;
                }

                if (isConsumable == false && (RogueLiteManager.runData.GetCount(this) >= MaxStack || RogueLiteManager.persistentData.GetCount(this) >= MaxStack))
                {
                    return false;
                }

                return true;
            }
        }

        /// <summary>
        /// Check to see if this recipe can be offered to the player.
        /// This considers the dependencies of the recipe and whether the player already has the maximum number of instances of the recipe.
        /// </summary>
        public virtual bool CanOffer
        {
            get
            {
                if (IsStackable)
                {
                    if (RogueLiteManager.runData.GetCount(this) >= MaxStack || RogueLiteManager.persistentData.GetCount(this) >= MaxStack)
                    {
                        return false;
                    }
                } 
                else if (RogueLiteManager.runData.Contains(this) || RogueLiteManager.persistentData.Contains(this))
                {
                    return false;
                }

                foreach (IRecipe dependency in dependencies)
                {
                    if (RogueLiteManager.runData.Contains(dependency) == false && RogueLiteManager.persistentData.Contains(dependency) == false)
                    {
#if UNITY_EDITOR
                        Debug.Log(dependency.DisplayName + " is a dependency of " + DisplayName + " but is not in the player's persistent or run data. Cannot build.");
#endif
                        return false;
                    }
#if UNITY_EDITOR
                    Debug.Log(dependency.DisplayName + " is a dependency of " + DisplayName + " and is in the player's persistent or run data. Can build.");
#endif
                }

                return true;
            }
        }

        public virtual void BuildFinished()
        {
            nextTimeAvailable = Time.timeSinceLevelLoad + cooldown;
        }

        [SerializeField, Tooltip("DO NOT CHANGE THIS. Unless you know what you are doing"), BoxGroup("Internal"), ReadOnly]
        internal string uniqueID;

#if UNITY_EDITOR
        [Button("Regenerate ID (use with care)")]
        protected void GenerateID()
        {
            uniqueID = Guid.NewGuid().ToString();
            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssets();
        }

        [Button("Set Build cost to 10% of buy cost")]
        protected void SetBuildCost()
        {
            buildCost = Mathf.RoundToInt(buyCost * 0.1f);
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