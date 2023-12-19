using NeoFPS;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static NeoFPS.BasicHealthManager;

namespace Playground
{
    public class NanobotManager : MonoBehaviour
    {
        public delegate void OnResourcesChanged(float from, float to);
        public event OnResourcesChanged onResourcesChanged;

        private int currentResources = 0;

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