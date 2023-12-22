using NeoFPS.CharacterMotion;
using NeoFPS.CharacterMotion.MotionData;
using NeoFPS.CharacterMotion.Parameters;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Playground
{
    [RequireComponent(typeof(MotionController))]
    public class MovementUpgradeManager : MonoBehaviour, IMotionGraphDataOverride
    {
        public FloatValueModifier moveSpeed { get; private set; }

        private SwitchParameter m_CanWallRun = null;
        public bool canWallRun
        {
            get { return m_CanWallRun.on; }
            set { m_CanWallRun.on = value; }
        }

        void Start()
        {
            // Get the persistent player data & motion graph
            var persistent = RogueLiteManager.persistentData;
            var motionGraph = GetComponent<MotionController>().motionGraph;

            // Get the required parameters
            m_CanWallRun = motionGraph.GetSwitchProperty("canWallRun");

            // Set up value modifiers for motion graph data
            moveSpeed = new FloatValueModifier(persistent.moveSpeedMultiplier, persistent.moveSpeedPreAdd, persistent.moveSpeedPostAdd);

            // Connect up the various data overrides
            motionGraph.AddDataOverrides(this);
        }

        public Func<float, float> GetFloatOverride(FloatData data)
        {
            switch (data.name)
            {
                case "moveSpeed":
                    return moveSpeed.GetModifiedValue;
                default:
                    return null;
            }
        }

        #region IGNORE

        public Func<bool, bool> GetBoolOverride(BoolData data)
        {
            return null;
        }

        public Func<int, int> GetIntOverride(IntData data)
        {
            return null;
        }

        #endregion
    }
}