using System;
using UnityEngine;

namespace RogueWave
{
    public interface IRecipe
    {
        public string UniqueID { get; }
        public string DisplayName { get; }
        public string AnnouncerVoicelineForName { get; }
        public string Description { get; }
        public Sprite HeroImage { get; }
        public string Category { get; }
        public int Level { get; }

        public Sprite Icon { get; }
        public bool IsPowerUp { get; }
        public bool IsConsumable { get; }
        public bool IsStackable { get; }
        public int MaxStack { get; }
        public int BuyCost { get; }
        public int BuildCost { get; }
        public float TimeToBuild { get; }
        public AudioClip NameClip { get; }
        public AudioClip BuildStartedClip { get; }
        public AudioClip BuildCompleteClip { get; }
        public ParticleSystem PickupParticles { get; }

        /// <summary>
        /// This is called whenever the recipe is enabled. 
        /// This is useful for resetting any state that may have been changed, such as a reference to the player or any component on them.
        /// </summary>
        public void Reset();

        /// <summary>
        /// Indicates whether this recipe should be built if enough resources are available.
        /// This will test to see if the item would be useful to the player at this point in time.
        /// For example, if the player already has full health, a health pickup would not be useful.
        /// </summary>
        public bool ShouldBuild { get; }
        bool CanOffer { get; }
        float weight { get; }

        /// <summary>
        /// Called when this recipe has been built.
        /// </summary>
        public void BuildFinished();
    }
}