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
        // Add persistent upgrade data here (no object references, but value types and arrays are fine)
        public float moveSpeedMultiplier = 1f;
        public float moveSpeedPreAdd = 0f;
        public float moveSpeedPostAdd = 0f;

        public bool isDirty { get; set; }

        void OnValueChanged()
        {
            isDirty = true;
        }
    }
}