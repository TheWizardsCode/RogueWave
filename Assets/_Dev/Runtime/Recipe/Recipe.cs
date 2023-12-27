using NeoFPS;
using NeoFPS.ModularFirearms;
using NeoFPS.SinglePlayer;
using System;
using Unity.Collections;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;

namespace Playground
{
    public class Recipe<T> : ScriptableObject, IRecipe where T : MonoBehaviour
    {
        [Header("Metadata")]
        [SerializeField, Tooltip("The name of this recipe.")]
        string displayName = "TBD";
        [SerializeField, Tooltip("DO NOT CHANGE THIS. TODO: Create a custom editor that hides this in case of accidental change.")]
        string uniqueID;
        [SerializeField, Tooltip("Powerups are recipes that can be offered between levels and, if purchased, become permanent.")]
        bool isPowerUp = false;

        [Header("Item")]
        [SerializeField, Tooltip("The pickup item this recipe creates.")]
        [FormerlySerializedAs("item")]
        internal T pickup;
        [SerializeField, Tooltip("The resources required to build this ammo type.")]
        int cost = 10;
        [SerializeField, Tooltip("The time it takes to build this recipe.")]
        float timeToBuild = 5;

        [Header("Feedback")]
        [SerializeField, Tooltip("The sound to play when the build is started. If this is null then ")]
        AudioClip buildStartedClip;
        [SerializeField, Tooltip("The sound to play when the build is complete.")]
        AudioClip buildCompleteClip;
        [SerializeField, Tooltip("The particle system to play when a pickup is spawned.")]
        ParticleSystem pickupParticles;

        public virtual bool ShouldBuild
        {
            get
            {
                return true;
            }
        }
        public string UniqueID => uniqueID;

        public string DisplayName => displayName;

        public bool IsPowerUp => isPowerUp;

        public int Cost => cost;

        public float TimeToBuild => timeToBuild;

        public AudioClip BuildStartedClip => buildStartedClip;

        public AudioClip BuildCompleteClip => buildCompleteClip;

        public Component Item => pickup;

        public ParticleSystem PickupParticles => pickupParticles;

        public virtual void BuildFinished()
        {
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (string.IsNullOrEmpty(uniqueID))
            {
                uniqueID = Guid.NewGuid().ToString();
                EditorUtility.SetDirty(this);
                AssetDatabase.SaveAssets();
            }
        }
#endif
    }
}