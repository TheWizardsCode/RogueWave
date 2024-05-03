using NeoFPS.SinglePlayer;
using UnityEngine;

namespace RogueWave
{
    /// <summary>
    /// The nanobot pawn is the little dude that the nanobots create in front of the player. This controller is responsible for moving the nanobot pawn, animations and other actions the pawn might take.
    /// </summary>
    public class NanobotPawnController : MonoBehaviour
    {
        [Header("Positioning")]
        [SerializeField, Tooltip("The offset from the player that the pawn will be created.")]
        Vector3 playerOffset = new Vector3(0, 0, 5);

        [Header("Movement")]
        [SerializeField, Tooltip("The maximum speed the pawn can move.")]
        float maxSpeed = 10f;
        [SerializeField, Tooltip("The acceleration/deceleration of the pawn.")]
        float acceleration = 10f;
        [SerializeField, Tooltip("The speed the pawn will rotate.")]
        float rotationSpeed = 180f;
        [SerializeField, Tooltip("The distance the pawn will stop from their intended destination.")]
        float arrivalDistance = 0.25f;

        [Header("Idle Behaviour")]
        [SerializeField, Tooltip("The time the pawn will wait before becoming impatient.")]
        float idleTimeBeforeImpatient = 3f;

        private enum State
        {
            Idle,
            Impatient,
            Moving
        }

        private FpsSoloCharacter player;
        private Animator animator;

        private float sqrArrivalDistance;
        private float currentSpeed = 0;
        private float movementDirection;
        private Vector3 targetPosition;
        private Quaternion targetRotation;

        private State m_currentState = State.Idle;
        private float timeInState = 0;

        private State CurrentState
        {
            get
            {
                return m_currentState;
            }
            set
            {
                if (m_currentState != value)
                {
                    switch (m_currentState)
                    {
                        case State.Idle:
                            animator.SetBool("Impatient", false);
                            targetRotation = Quaternion.LookRotation(-player.transform.forward, Vector3.up);
                            break;
                        case State.Impatient:
                            targetRotation = Quaternion.LookRotation(-player.transform.forward, Vector3.up);
                            animator.SetBool("Impatient", true);
                            break;
                        case State.Moving:
                            animator.SetBool("Impatient", false);
                            break;
                    }

                    m_currentState = value;
                    timeInState = 0;
                }
            }
        }

        private void Start()
        {
            sqrArrivalDistance = arrivalDistance * arrivalDistance;

            // when added by the upgrade system the pawn is added to the players transform, but we want the pawn to move independently of the player.
            transform.SetParent(null);

            player = FpsSoloCharacter.localPlayerCharacter;
            animator = GetComponent<Animator>();
        }

        private void Update()
        {
            timeInState += Time.deltaTime;
            CalculateMovement();

            switch (CurrentState)
            {
                case State.Idle:
                    if (timeInState >= idleTimeBeforeImpatient)
                    {
                        CurrentState = State.Impatient;
                    }
                    break;
                case State.Impatient:
                    break;
                case State.Moving:
                    break;
            }

            transform.position = Vector3.MoveTowards(transform.position, targetPosition, currentSpeed * Time.deltaTime);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);

            ConfigureAnimator();
        }

        private void ConfigureAnimator()
        {
            // Movement Speed (normalized)
            animator.SetFloat("Speed", currentSpeed / maxSpeed);

            // Movement Direction (-1 = hard left, 0 = forward, 1 = hard right)
            animator.SetFloat("Direction", movementDirection);

            // Impatient - frustration in idle animations
            // set in CurrentState property
        }

        private void CalculateMovement()
        {
            targetPosition = Vector3.Lerp(transform.position, player.transform.position + (player.transform.forward * playerOffset.magnitude), Time.deltaTime * acceleration);

            float sqrDistance = (transform.position - targetPosition).sqrMagnitude;

            if (sqrDistance <= sqrArrivalDistance)
            {
                currentSpeed = Mathf.Clamp(currentSpeed - (acceleration * Time.deltaTime), 0, maxSpeed);
                if (currentSpeed <= 0.1)
                {
                    currentSpeed = 0;
                    CurrentState = State.Idle;
                }
            }
            else
            {
                targetRotation = Quaternion.LookRotation(targetPosition - transform.position, Vector3.up);
                currentSpeed = Mathf.Clamp(currentSpeed + (acceleration * Time.deltaTime), 0, maxSpeed);
                CurrentState = State.Moving;
            }

            Vector3 localTarget = transform.InverseTransformPoint(targetPosition);
            movementDirection = Mathf.Clamp(localTarget.x, -1, 1);
        }
    }
}