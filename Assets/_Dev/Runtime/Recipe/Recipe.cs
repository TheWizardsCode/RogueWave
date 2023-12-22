﻿using NeoFPS;
using NeoFPS.ModularFirearms;
using NeoFPS.SinglePlayer;
using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace Playground
{
    public class Recipe<T> : ScriptableObject, IRecipe where T : MonoBehaviour
    {
        [Header("Metadata")]
        [SerializeField, Tooltip("The name of this recipe.")]
        string displayName = "TBD";

        [Header("Item")]
        [SerializeField, Tooltip("The pickup item this recipe creates.")]
        [FormerlySerializedAs("item")]
        protected T pickup;
        [SerializeField, Tooltip("The offset from the NanobotManager to spawn the item.")]
        Vector3 spawnOffset = new Vector3(0, 0, 0);
        [SerializeField, Tooltip("The resources required to build this ammo type.")]
        int cost = 10;
        [SerializeField, Tooltip("The time it takes to build this recipe.")]
        float timeToBuild = 5;

        [Header("Feedback")]
        [SerializeField, Tooltip("The sound to play when the build is started.")]
        AudioClip buildStartedClip;
        [SerializeField, Tooltip("The sound to play when the build is complete.")]
        AudioClip buildCompleteClip;

        public virtual bool ShouldBuild
        {
            get
            {
                return true;
            }
        }

        public string DisplayName => displayName;

        public Vector3 SpawnOffset => spawnOffset;

        public int Cost => cost;

        public float TimeToBuild => timeToBuild;

        public AudioClip BuildStartedClip => buildStartedClip;

        public AudioClip BuildCompleteClip => buildCompleteClip;

        public Component Item => pickup;
    }
}