using NeoFPS.SinglePlayer;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace RogueWave
{
    /// <summary>
    /// The nanobot pawn is the little dude that the nanobots create in front of the player. This controller is responsible for moving the nanobot pawn, animations and other actions the pawn might take.
    /// </summary>
    public class NanobotPawnController : MonoBehaviour
    {
        [SerializeField, Tooltip("The offset from the player that the pawn will be created.")]
        Vector3 playerOffset = new Vector3(0, 0, 5);
        [SerializeField, Tooltip("The detection range for enemies and objects of interest.")]
        protected float m_DetectionRange = 15f;
        [SerializeField, Tooltip("The number of items the nanobots will detect and potentially make available to other items.")]
        int detectedObjectsCount = 10;
        [SerializeField, Tooltip("The layer mask for objects that should be detected and tracked.")]
        LayerMask detectionLayerMask;

        [Header("Animation")]
        [SerializeField, Tooltip("The trigger for the attack animation.")]
        string attackTrigger = "Attack";

        [Header("Movement")]
        [SerializeField, Tooltip("The distance the player needs to move to trigger the pawn to move.")]
        float movementDampingDistance = 2f;
        [SerializeField, Tooltip("The rotation the player needs to move through in order to trigger the pawn to move.")]
        float rotationDamping = 20;
        [SerializeField, Tooltip("The distance below which the nearest enemy needs to be for the pawn to become aggro'd.")]
        float aggroDistance = 7.5f;
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

        [Header("Starting Upgrades")]
        [SerializeField, Tooltip("Recipes that should be applied to the player when the pawn is enabled.")]
        AbstractRecipe[] startingRecipes;

        Collider[] colliders;
        float[] colliderDistances;
        private float sqrMovementDampingDistance;
        Queue<KeyValuePair<float, Collider>> m_collidersQueue = new Queue<KeyValuePair<float, Collider>>();
        bool isDetectionQueueInvalid = true;

        public Queue<KeyValuePair<float, Collider>> sortedColliders
        {
            get
            {
                if (isDetectionQueueInvalid)
                {
                    m_collidersQueue.Clear();
                    for (int i = 0; i < detectedObjectsCount; i++)
                    {
                        m_collidersQueue.Enqueue(new KeyValuePair<float, Collider>(colliderDistances[i], colliders[i]));
                    }
                    isDetectionQueueInvalid = false;
                }
                return m_collidersQueue;
            }
        }

        /// <summary>
        /// Peek at the first non-null collider in the queue. If there are no colliders in the queue, this will return a default KeyValuePair (key = 0, collider = null).
        /// 
        /// If there are null colliders in the queue, they will be removed and the next collider will be peeked at.
        /// </summary>
        /// <returns></returns>
        public KeyValuePair<float, Collider> PeekDetectedObject()
        {
            if (sortedColliders.Count > 0 && sortedColliders.Peek().Value == null)
            {
                sortedColliders.Dequeue();
                return PeekDetectedObject();
            }
            return sortedColliders.Count > 0 ? sortedColliders.Peek() : default(KeyValuePair<float, Collider>);
        }

        internal KeyValuePair<float, Collider> ObjectAt(int idx)
        {
            if (idx >= detectedObjectsCount)
            {
                return default(KeyValuePair<float, Collider>);
            }

            return new KeyValuePair<float, Collider>(colliderDistances[idx], colliders[idx]);
        }

        public KeyValuePair<float, Collider> GetNearestObject()
        {
            if (sortedColliders.Peek().Value == null)
            {
                sortedColliders.Dequeue();
                return GetNearestObject();
            }
            return sortedColliders.Dequeue();
        }

        public void Enqueue(float distance, Collider collider)
        {
            sortedColliders.Enqueue(new KeyValuePair<float, Collider>(distance, collider));
        }

        private enum State
        {
            Idle,
            Impatient,
            Moving,
            Aggro
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
        private Collider aggroTarget;

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
                        case State.Aggro:
                            animator.SetBool("Impatient", false);
                            break;
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

        private void Awake()
        {
            colliders = new Collider[detectedObjectsCount];
            colliderDistances = new float[detectedObjectsCount];
            sqrMovementDampingDistance = movementDampingDistance * movementDampingDistance;
        }

        private void Start()
        {
            sqrArrivalDistance = arrivalDistance * arrivalDistance;

            // when added by the upgrade system the pawn is added to the players transform, but we want the pawn to move independently of the player.
            transform.SetParent(null);

            player = FpsSoloCharacter.localPlayerCharacter;
            animator = GetComponent<Animator>();

            NanobotManager nanobotManager = FpsSoloCharacter.localPlayerCharacter.GetComponent<NanobotManager>();
            foreach (IRecipe recipe in startingRecipes)
            {
                RogueLiteManager.persistentData.Add(recipe);
                nanobotManager.Add(recipe);
            }
        }

        private void Update()
        {
            timeInState += Time.deltaTime;
            ManageState();

            CalculateMovement();

            transform.position = Vector3.MoveTowards(transform.position, targetPosition, currentSpeed * Time.deltaTime);
            transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles.x, Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime).eulerAngles.y, transform.rotation.eulerAngles.z);

            ConfigureAnimator();

            DetectObjectsOfInterest();
        }

        /// <summary>
        /// Updates the local collection of known objects of interest within the detection range.
        /// </summary>
        private void DetectObjectsOfInterest()
        {
            Array.Clear(colliders, 0, detectedObjectsCount);
            int count = Physics.OverlapSphereNonAlloc(FpsSoloCharacter.localPlayerCharacter.transform.position, m_DetectionRange, colliders, detectionLayerMask);
            for (int i = 0; i < count; i++)
            {
                colliderDistances[i] = colliders[i] != null ? Vector3.Distance(FpsSoloCharacter.localPlayerCharacter.transform.position, colliders[i].transform.position) : float.MaxValue;
            }

            Array.Sort(colliderDistances, colliders);
            isDetectionQueueInvalid = true;
        }

        /// <summary>
        /// Get the nearest enemy, if there is one.
        /// </summary>
        /// <returns>The nearest Enemy controller or null if thre isn't an enemy nearby.</returns>
        internal BasicEnemyController GetNearestEnemy()
        {
            KeyValuePair<float, Collider> nearest = PeekDetectedObject();
            if (nearest.Value != null)
            {
                return nearest.Value.GetComponentInParent<BasicEnemyController>();
            }
            return null;
        }

        private void ManageState()
        {
            switch (CurrentState)
            {
                case State.Aggro:
                    CheckAggro();
                    break;
                case State.Idle:
                    if (timeInState >= idleTimeBeforeImpatient)
                    {
                        CurrentState = State.Impatient;
                    }
                    CheckAggro();
                    break;
                case State.Impatient:
                    CheckAggro();
                    break;
                case State.Moving:
                    break;
            }
        }

        /// <summary>
        /// Checks to see if the pawn should be in an aggro state and sets the state and target if it should be.
        /// </summary>
        bool CheckAggro()
        {
            KeyValuePair<float, Collider> nearest = PeekDetectedObject();
            if (nearest.Value != null && nearest.Key <= aggroDistance)
            {
                aggroTarget = nearest.Value;
                CurrentState = State.Aggro;
                return true;
            } else
            {
                aggroTarget = null;
                if (CurrentState == State.Aggro)
                {
                    CurrentState = State.Idle;
                }
                return false;
            }
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

        Vector3 lastPlayerPosition = Vector3.zero;
        private Vector3 lastPlayerForward;

        private void CalculateMovement()
        {
            if (CurrentState != State.Moving 
                && (lastPlayerPosition - player.transform.position).sqrMagnitude < sqrMovementDampingDistance
                && Quaternion.Angle(player.transform.rotation, Quaternion.LookRotation(lastPlayerForward)) < rotationDamping
                )
            {

                if (aggroTarget != null)
                {
                    targetRotation = Quaternion.LookRotation(aggroTarget.transform.position - transform.position, Vector3.up);
                }
                else
                {
                    targetRotation = player.transform.rotation;
                }

                return;
            }

            lastPlayerPosition = player.transform.position;
            lastPlayerForward = player.transform.forward;

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

                if (aggroTarget != null)
                {
                    targetRotation = Quaternion.LookRotation(aggroTarget.transform.position - transform.position, Vector3.up);
                }
                else
                {
                    targetRotation = player.transform.rotation;
                }
            }
            else
            {
                targetRotation = Quaternion.LookRotation(targetPosition - transform.position, Vector3.up);
                currentSpeed = Mathf.Clamp(currentSpeed + (acceleration * Time.deltaTime), 0, maxSpeed);
                CurrentState = State.Moving;
            }

            // find the height of the ground at the target position
            float maxTerrrainHeight = 8;
            float yOffset = -1;
            if (Physics.Raycast(targetPosition + (Vector3.up * maxTerrrainHeight), Vector3.down, out RaycastHit hit, maxTerrrainHeight * 1.1f, 1 << 0))
            {
                targetPosition.y = hit.point.y;
            }

            Vector3 localTarget = transform.InverseTransformPoint(targetPosition);
            movementDirection = Mathf.Clamp(localTarget.x, -1, 1);
        }
    }
}