using Codice.CM.Common;
using NeoFPS.SinglePlayer;
using PlasticGui.WorkspaceWindow.Home;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Profiling.Memory.Experimental;
using UnityEngine;

namespace Playground
{
    /// <summary>
    /// This kind of enemy will wait until the player is close enough, then lunge at them.
    /// They will move towards the player slowly, but will not attack until they are close enough.
    /// </summary>
    public class WaitAndLungeEnemyController : BasicEnemyController
    {
        [Header("Behaviour")]
        [SerializeField, Tooltip("Minimum distance to attack from. If the player is further away than this the enemy will not attack.")]
        protected float minAttackDistance = 20f;
        [SerializeField, Tooltip("The multiplier for speed when attacking.")]
        protected float speedMultiplier = 2f;

        protected override void Update()
        {
            if (FpsSoloCharacter.localPlayerCharacter == null)
                return;

            if (shouldAttack)
            {
                Vector3 destination = FpsSoloCharacter.localPlayerCharacter.localTransform.position;
                transform.position = Vector3.MoveTowards(transform.position, destination, speed * speedMultiplier * Time.deltaTime);

                Vector3 directionToTarget = destination - transform.position;
                directionToTarget.y = 0;
                if (directionToTarget != Vector3.zero)
                {
                    Quaternion targetRotation = Quaternion.LookRotation(directionToTarget);
                    transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
                }
            } else
            {
                base.Update();
            }
        }

        protected bool shouldAttack {
            get
            {
                if (Vector3.Distance(transform.position, FpsSoloCharacter.localPlayerCharacter.localTransform.position) < minAttackDistance)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
    }
}