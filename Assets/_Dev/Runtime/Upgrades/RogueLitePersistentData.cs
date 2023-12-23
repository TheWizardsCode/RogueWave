using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Playground
{
    [Serializable]
    public class RogueLitePersistentData
    {
        // Movement stat upgrades
        // If you want to modify these during gameplay (in combat scene) use the values
        // on the character's MovementUpgradeManager instead
        public float moveSpeedMultiplier = 1f;
        public float moveSpeedPreAdd = 0f;
        public float moveSpeedPostAdd = 0f;
        public float airbourneSpeedMultiplier = 1f;
        public float airbourneSpeedPreAdd = 0f;
        public float airbourneSpeedPostAdd = 0f;
        public float accelerationMultiplier = 1f;
        public float accelerationdPreAdd = 0f;
        public float accelerationPostAdd = 0f;
        public float maxJumpHeightMultiplier = 1f;
        public float maxJumpHeightPreAdd = 0f;
        public float maxJumpHeightPostAdd = 0f;
        public float jetpackForceMultiplier = 1f;
        public float jetpackForcePreAdd = 0f;
        public float jetpackForcePostAdd = 0f;
        public float dashSpeedMultiplier = 1f;
        public float dashSpeedPreAdd = 0f;
        public float dashSpeedPostAdd = 0f;

        // Add additional persistent data here
        // Can serialize value types, serializable structs and arrays/lists
        // Can NOT(!) serialize UnityEngine.Object references or non-serializable types like dictionary
        // Remember to set isDirty = true when changing a value

        public bool isDirty { get; set; } // TODO: Need to wrap values above to automate setting this on change
    }
}