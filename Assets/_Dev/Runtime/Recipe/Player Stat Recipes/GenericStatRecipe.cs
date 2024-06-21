using NaughtyAttributes;
using NeoFPS.SinglePlayer;
using System.Reflection;
using UnityEngine;

namespace RogueWave
{
    /// <summary>
    /// A stat recipe will upgrade an objects stats. The generic type T is the type of object to apply the stat to.
    /// 
    /// </summary>
    public abstract class GenericStatRecipe<T> : BaseStatRecipe
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
        [SerializeField, Tooltip("A prototype used to find the target to apply this stat modifier to.")]
        internal T targetPrototype;

        // REFACTOR: This needs to move up into a Movement Stat Recipe OR we remove the need for it and instead use the reflection approach for names parameters.
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
    }
}