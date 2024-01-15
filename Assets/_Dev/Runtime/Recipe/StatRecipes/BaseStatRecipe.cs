using NaughtyAttributes;
using NeoFPS.SinglePlayer;
using System;
using UnityEditor;
using UnityEngine;

namespace Playground
{
    /// <summary>
    /// A stat recipe will upgrade one or more of the player's stats.
    /// The player need not doing anything once the upgrade has been built, it will be applied automatically.
    /// </summary>
    [CreateAssetMenu(fileName = "Stat Recipe", menuName = "Playground/Recipe/Stat", order = 1)]
    public abstract class BaseStatRecipe : AbstractRecipe
    {
        /*

        Parameters To Test:

        - canGrapple


        Parameters TODO:

        SWITCH -
        - canAimHover
        - canJetpack
        - canWallRun
        - canWallRunUp

        INT -
        - maxAirJumpCount

        FLOAT -
        - minDashInterval
        - minAirJumpInterval

        VECTOR -
        - crouchDash

        FLOAT -
        - moveSpeed
        - acceleration
        - accelerationAirborne
        - deceleration
        - maxJumpHeight
        - jetpackForce
        - dashSpeed

        // Abilities
        MotionGraphMovementCanDash,
        MotionGraphMovementCanGrapple,

        // Parameters
        moveSpeed
    */

        [SerializeField, Tooltip("The name of stat to modify.")]
        internal string statName = string.Empty;

        MovementUpgradeManager _movementUpgradeManager;
        internal MovementUpgradeManager movementUpgradeManager
        {
            get
            {
                if (_movementUpgradeManager == null)
                {
                    _movementUpgradeManager = FpsSoloCharacter.localPlayerCharacter.GetComponent<MovementUpgradeManager>();
                }
                return _movementUpgradeManager;
            }
        }

        public override void BuildFinished()
        {
            Apply();

            base.BuildFinished();
        }

        internal abstract void Apply();

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (string.IsNullOrEmpty(uniqueID))
            {
                uniqueID = Guid.NewGuid().ToString();
                EditorUtility.SetDirty(this);
                AssetDatabase.SaveAssets();
            }

            //TODO: is it possible to check the statName is valid in the motiongraph?
        }
#endif
    }
}