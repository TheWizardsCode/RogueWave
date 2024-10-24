using NaughtyAttributes;
using NeoFPS.SinglePlayer;
using NeoFPSEditor.Hub.Pages;
using System;
using System.Collections.Generic;
using System.Linq;
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

        [Header("Scanner")]
        [SerializeField, Tooltip("The scanner that the nanobots will use to detect enemies and objects of interest.  This object creates the center of the scan sphere.")]
        private Transform scanner;
        [SerializeField, Tooltip("The detection range for enemies and objects of interest.")]
        protected float m_DetectionRange = 15f;
        [SerializeField, Tooltip("The number of items the nanobots will detect and potentially make available to other items.")]
        int detectedObjectsCount = 10;
        [SerializeField, Tooltip("The layer mask for objects that should be detected and tracked.")]
        LayerMask detectionLayerMask;

        [Header("Movement")]
        [SerializeField, Tooltip("If set to true this pawn is able to move independently.")]
        bool canMove = true;
        [SerializeField, Tooltip("The distance the player needs to move to trigger the pawn to move."), ShowIf("canMove")]
        float movementDampingDistance = 2f;
        [SerializeField, Tooltip("The rotation the player needs to move through in order to trigger the pawn to move."), ShowIf("canMove")]
        float rotationDamping = 20;
        [SerializeField, Tooltip("The distance below which the nearest enemy needs to be for the pawn to become aggro'd."), ShowIf("canMove")]
        float aggroDistance = 7.5f;
        [SerializeField, Tooltip("The maximum speed the pawn can move."), ShowIf("canMove")]
        float maxSpeed = 10f;
        [SerializeField, Tooltip("The acceleration/deceleration of the pawn."), ShowIf("canMove")]
        float acceleration = 10f;
        [SerializeField, Tooltip("The speed the pawn will rotate."), ShowIf("canMove")]
        float rotationSpeed = 180f;
        [SerializeField, Tooltip("The distance the pawn will stop from their intended destination."), ShowIf("canMove")]
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

        internal FpsSoloCharacter player;
        private Animator animator;

        private float sqrArrivalDistance;
        private float currentSpeed = 0;
        private float movementDirection;
        private Vector3 targetPosition;
        private Quaternion targetRotation;

        private State m_currentState = State.Idle;
        private float timeInState = 0;
        private Collider aggroTarget;

        /// <summary>
        /// Get the colliders that have been detected by the pawn, sorted by distance from the pawn.
        /// This is a queue of key value pairs where the key is the distance from the pawn and the value is the collider.
        /// The queue is updated whenever the queue is deemed invalid, which is usually when 
        /// </summary>
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
        /// <returns>The object at the front of the queue.</returns>
        /// <seealso cref="GetNearestObject"/>
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

        /// <summary>
        /// Get the nearest object of interest to the pawn.
        /// </summary>
        /// <returns>The object that is nearest to the pawn in world space.</returns>
        public KeyValuePair<float, Collider> GetNearestObject()
        {
            if (sortedColliders.Count == 0)
            {
                return default(KeyValuePair<float, Collider>);
            }

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
                            if (canMove)
                            {
                                targetRotation = Quaternion.LookRotation(-player.transform.forward, Vector3.up);
                            }
                            break;
                        case State.Impatient:
                            animator.SetBool("Impatient", true);
                            if (canMove)
                            {
                                targetRotation = Quaternion.LookRotation(-player.transform.forward, Vector3.up);
                            }
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

            if (FpsSoloCharacter.localPlayerCharacter != null)
            {
                NanobotManager nanobotManager = FpsSoloCharacter.localPlayerCharacter.GetComponent<NanobotManager>();
                foreach (IRecipe recipe in startingRecipes)
                {
                    RogueLiteManager.persistentData.Add(recipe);
                    nanobotManager.Add(recipe);
                }
            }
        }

        private void Update()
        {
            timeInState += Time.deltaTime;
            ManageState();

            if (canMove)
            {
                CalculateMovement();

                transform.position = Vector3.MoveTowards(transform.position, targetPosition, currentSpeed * Time.deltaTime);
                transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles.x, Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime).eulerAngles.y, transform.rotation.eulerAngles.z);
            }

            ConfigureAnimator();

            DetectObjectsOfInterest();
        }

        /// <summary>
        /// Updates the local collection of known objects of interest within the detection range.
        /// The data is stored internally in a sorted Array of Colliders (`colliders`) and an Array of distances from the pawn to the collider (`colliderDistances`).
        /// The index for each array is the same, so the distance to the collider at `colliders[i]` is `colliderDistances[i]`.
        /// </summary>
        /// <seealso cref="GetNearestObject"/>
        private void DetectObjectsOfInterest()
        {
            Array.Clear(colliders, 0, detectedObjectsCount);
            int count = Physics.OverlapSphereNonAlloc(scanner.position, m_DetectionRange, colliders, detectionLayerMask);
            for (int i = 0; i < count; i++)
            {
                colliderDistances[i] = colliders[i] != null ? Vector3.Distance(scanner.position, colliders[i].transform.position) : float.MaxValue;
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
            if (canMove)
            {
                // Movement Speed (normalized)
                animator.SetFloat("Speed", currentSpeed / maxSpeed);
                // Movement Direction (-1 = hard left, 0 = forward, 1 = hard right)
                animator.SetFloat("Direction", movementDirection);
            }

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
            if (Physics.Raycast(targetPosition + (Vector3.up * maxTerrrainHeight), Vector3.down, out RaycastHit hit, maxTerrrainHeight * 1.1f, 1 << 0))
            {
                targetPosition.y = hit.point.y;
            }

            Vector3 localTarget = transform.InverseTransformPoint(targetPosition);
            movementDirection = Mathf.Clamp(localTarget.x, -1, 1);
        }
    }
}