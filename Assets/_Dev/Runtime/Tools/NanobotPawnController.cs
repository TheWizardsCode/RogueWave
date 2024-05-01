using NeoFPS.SinglePlayer;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RogeWave
{
    /// <summary>
    /// The nanobot pawn is the little dude that the nanobots create in front of the player. This controller is responsible for moving the nanobot pawn, animations and other actions the pawn might take.
    /// </summary>
    public class NanobotPawnController : MonoBehaviour
    {
        [SerializeField, Tooltip("The maximum speed the pawn can move.")]
        float maxSpeed = 10f;
        [SerializeField, Tooltip("The acceleration/deceleration of the pawn.")]
        float acceleration = 10f;
        [SerializeField, Tooltip("The speed the pawn will rotate.")]
        float rotationSpeed = 360f;
        [SerializeField, Tooltip("The distance the pawn will stop from their intended destination.")]
        float arrivalDistance = 0.25f;

        private FpsSoloCharacter player;
        private Vector3 playerOffset;
        private Animator animator;

        private float sqrArrivalDistance;
        private float currentSpeed = 0;

        private void Start()
        {
            playerOffset = transform.localPosition;
            sqrArrivalDistance = arrivalDistance * arrivalDistance;

            // when added to the player by the upgrade system the pawn is added to the players transform, but we want the pawn to move independently of the player.
            transform.SetParent(null);

            player = FpsSoloCharacter.localPlayerCharacter;
            animator = GetComponent<Animator>();
        }

        private void FixedUpdate()
        {   
            Vector3 targetPosition = player.transform.position + (player.transform.forward * playerOffset.magnitude);
            Quaternion targetRotation = Quaternion.LookRotation(player.transform.forward, Vector3.up);

            float sqrDistance = (transform.position - targetPosition).sqrMagnitude;

            if (sqrDistance <= sqrArrivalDistance)
            {
                currentSpeed = Mathf.Clamp(currentSpeed - (acceleration * Time.deltaTime), 0, maxSpeed);
            }
            else
            {
                currentSpeed = Mathf.Clamp(currentSpeed + (acceleration * Time.deltaTime), 0, maxSpeed);

                transform.position = Vector3.MoveTowards(transform.position, targetPosition, currentSpeed * Time.deltaTime);
                transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            }

            animator.SetFloat("Speed", currentSpeed / maxSpeed);
            float angle = Vector3.SignedAngle(transform.position - transform.forward, targetPosition - transform.forward, Vector3.up);
            animator.SetFloat("Direction", angle);
        }
    }
}