using NaughtyAttributes;
using System;
using System.Diagnostics.Eventing.Reader;
using UnityEngine;
using Debug = UnityEngine.Debug;
using Random = UnityEngine.Random;

namespace RogueWave
{
    /// <summary>
    /// Basic movement controller for enemies. It handles the most basic movement of the enemy,which is essentially to move towards a position relative to the target, and to rotate towards the target.
    /// Basic Movement Controller is paired with a Basic Enemy Controller, which handles the central coordination of the enemy controllers.
    /// 
    /// Subclasses of this class will implement more complex movement patterns, such as formations, patrolling, or following a path.
    /// </summary>
    [RequireComponent(typeof(BasicEnemyController))]
    public class BasicMovementController : MonoBehaviour
    {
        [Header("Movement")]
        [SerializeField, Tooltip("The minimum speed at which the enemy moves.")]
        internal float minSpeed = 4f;
        [SerializeField, Tooltip("The maximum speed at which the enemy moves.")]
        public float maxSpeed = 6f;
        [SerializeField, Tooltip("The rate at which the enemy accelerates to its maximum speed.")]
        internal float acceleration = 10f;
        [SerializeField, Tooltip("How fast the enemy rotates.")]
        internal float rotationSpeed = 1f;
        [SerializeField, Tooltip("The minimum height the enemy will move to.")]
        internal float minimumHeight = 0.5f;
        [SerializeField, Tooltip("The maximum height the enemy will move to.")]
        internal float maximumHeight = 75f;
        [SerializeField, Tooltip("The distance the enemy will try to avoid other squad members by.")]
        float squadAvoidanceDistance = 10f;

        [Header("Navigation")]
        [SerializeField, Tooltip("The distance the enemy will try to avoid obstacles by.")]
        internal float obstacleAvoidanceDistance = 2f;
        [SerializeField, Tooltip("The distance the enemy needs to be from a target destination for it to be considered as arrived. This is important as large enemies, or ones with a slow trun speed might have difficulty getting to the precise target location. This can result in circular motion around the destination.")]
        internal float arrivalDistance = 1.5f;

        [SerializeField, Tooltip("Enable debuggging for this enemy."), Foldout("Editor Only")]
        bool isDebug;

        enum Direction
        {
            None,
            Left,
            Right,
            BothHorizontal,
            All
        }
        Direction obstacleDirection = Direction.None;
        Quaternion targetRotation = Quaternion.identity;

        private AIDirector aiDirector;

        private float sqrArrivalDistance;
        private float sqrSlowingDistance;
        Vector3 _destination = Vector3.zero;

        internal float currentDesiredSpeed;

        BasicEnemyController enemyController;

        internal Vector3 destination
        {
            get
            {
                return _destination;
            }
            set
            {
                if (_destination != value)
                {
                    value.y = Mathf.Clamp(value.y, minimumHeight, maximumHeight);
                    _destination = value;
                }
            }
        }

        private float currentSpeedMultiplier;
        private BasicEnemyController currentSquadLeader;
        private float currentSqrDistanceToGoal;

        internal bool hasArrived
        {
            get
            {
                if (currentSqrDistanceToGoal < sqrArrivalDistance)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        protected virtual void Awake()
        {
            sqrArrivalDistance = arrivalDistance * arrivalDistance;
            sqrSlowingDistance = sqrArrivalDistance * 1.5f;
            enemyController = GetComponent<BasicEnemyController>();
        }

        private void Start()
        {
            currentDesiredSpeed = Random.Range(minSpeed, maxSpeed);
            aiDirector = AIDirector.Instance;
        }

        private void Update()
        {
            currentSqrDistanceToGoal = Vector3.SqrMagnitude(destination - transform.position);
            if (!hasArrived)
            {
                if (Time.frameCount % 3 == 0) {
                    MoveTowards(currentSpeedMultiplier, currentSquadLeader);
                } else
                {
                    transform.position += transform.forward * currentDesiredSpeed * currentSpeedMultiplier * Time.deltaTime;
                }
            }
        }

        /// <summary>
        /// Set the movement goals for this object.
        /// </summary>
        /// <param name="destination">The destination to move towards.</param>
        /// <param name="speedMultiplier">How fast to go, as a multiplier of the base speed.</param>
        /// <param name="squadLeader">The squad leader, if one exists, that this object will follow orders from.</param>
        internal void SetMovementGoals(Vector3 destination, float speedMultiplier, BasicEnemyController squadLeader)
        {
            this.destination = destination;
            this.currentSpeedMultiplier = speedMultiplier;
            this.currentSquadLeader = squadLeader;
        }

          internal virtual void MoveTowards(float speedMultiplier, BasicEnemyController squadLeader)
        {
            if (currentSqrDistanceToGoal < sqrSlowingDistance)
            {
                currentDesiredSpeed = Mathf.Max(0, currentDesiredSpeed - Time.deltaTime * acceleration);
            }
            else if (currentDesiredSpeed < maxSpeed)
            {
                currentDesiredSpeed += Time.deltaTime * acceleration;
            }

            if (currentDesiredSpeed == 0)
            {
                return;
            }

            SetRotation(squadLeader);

            if (obstacleDirection == Direction.All)
            {
                transform.position += transform.up * currentDesiredSpeed * Time.deltaTime;
            }
            else if (obstacleDirection != Direction.None)
            {
                transform.position += transform.forward * currentDesiredSpeed * 0.8f * Time.deltaTime;
            }
            else
            {
                transform.position += transform.forward * currentDesiredSpeed * speedMultiplier * Time.deltaTime;
            }

            AdjustHeight(destination, speedMultiplier);
        }

        private void SetRotation(BasicEnemyController squadLeader)
        {
            Vector3 centerDirection = destination - transform.position;
            Vector3 avoidanceDirection = Vector3.zero;
            Span<BasicEnemyController> squadMembers = aiDirector.GetSquadMembers(squadLeader).AsSpan();
            int squadSize = 0;

            // if not the squad leader then avoid other squad members
            if (squadLeader != null && squadLeader.movementController != this) {
                foreach (BasicEnemyController enemy in squadMembers)
                {
                    if (enemy != null && enemy != this)
                    {
                        centerDirection += enemy.transform.position;

                        if (Vector3.Distance(enemy.transform.position, transform.position) < squadAvoidanceDistance)
                        {
                            avoidanceDirection += transform.position - enemy.transform.position;
                        }

                        squadSize++;
                    }
                }

                if (squadSize > 0)
                {
                    // TODO: centreDirection is never used - remove it?
                    centerDirection /= squadSize;
                    centerDirection = (centerDirection - transform.position).normalized;
                }
            }

            Vector3 directionToTarget = (destination - transform.position).normalized;
            float dotProduct = Vector3.Dot(transform.forward, directionToTarget);

            SetObstacleAvoidanceRotation(destination, avoidanceDirection);
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            //if (dotProduct != 1)
            //{
            //    transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            //}
            //else
            //{
            //    transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(directionToTarget), rotationSpeed * Time.deltaTime);
            //}
        }

        /// <summary>
        /// Get a recommended rotation for the enemy to take in order to avoid the nearest obstacle.
        /// </summary>
        /// <param name="destination">The destination we are trying to reach.</param>
        /// <param name="avoidanceDirection">The optimal direction we are trying to avoid other flock members.</param>
        /// <returns></returns>
        private void SetObstacleAvoidanceRotation(Vector3 destination, Vector3 avoidanceDirection)
        {
            // REFACTOR: This code has many side effects, need to rewrite to remove them.
            bool forwardBlocked = false;
            bool forwardRightBlocked = false;
            bool forwardLeftBlocked = false;

            float distanceToObstacle = 0;
            float turnAngle = 0;
            float offsetAngleIncrement = 45;

            // Check for obstacle dead ahead
            Ray ray = new Ray(enemyController.sensor.position, transform.forward);
            if (Physics.Raycast(ray, out RaycastHit forwardHit, obstacleAvoidanceDistance * 2, enemyController.sensorMask))
            {
                forwardBlocked = forwardHit.collider.transform.root != enemyController.Target;
            } 

            // check for obstacle to the forward left
            ray.direction = Quaternion.AngleAxis(-offsetAngleIncrement, transform.transform.up) * transform.forward;
            if (Physics.Raycast(ray, out RaycastHit leftForwardHit, obstacleAvoidanceDistance, enemyController.sensorMask))
            {
                forwardLeftBlocked = leftForwardHit.collider.transform.root != enemyController.Target;
            } else
            {
                forwardLeftBlocked = false;
            }

            // check for obstacle to the forward right
            ray.direction = Quaternion.AngleAxis(offsetAngleIncrement, transform.transform.up) * transform.forward;
            if (Physics.Raycast(ray, out RaycastHit rightForwardHit, obstacleAvoidanceDistance, enemyController.sensorMask))
            {
                forwardRightBlocked = rightForwardHit.collider.transform.root != enemyController.Target;
            } else
            {
                forwardRightBlocked = false;
            }

            if (forwardRightBlocked && forwardLeftBlocked)
            {
                if (forwardBlocked)
                {
                    obstacleDirection = Direction.All;
                    distanceToObstacle = forwardHit.distance;
                }
                else
                {
                    obstacleDirection = Direction.BothHorizontal;

                    turnAngle = 180;

                    if (Vector3.Dot(transform.right, destination - transform.position) > 0)
                    {
                        distanceToObstacle = rightForwardHit.distance;
                        obstacleDirection = Direction.Right;
                    }
                    else
                    {
                        distanceToObstacle = leftForwardHit.distance;
                        obstacleDirection = Direction.Left;
                    }
                }
            }
            else if (forwardRightBlocked)
            {
                turnAngle = -125;
                distanceToObstacle = rightForwardHit.distance;
                obstacleDirection = Direction.Right;
            }
            else if (forwardLeftBlocked)
            {
                turnAngle = 125;
                distanceToObstacle = leftForwardHit.distance;
                obstacleDirection = Direction.Left;
            }
            else
            {
                obstacleDirection = Direction.None;
                Vector3 directionToTarget = destination - transform.position;
                directionToTarget.y = 0;
                avoidanceDirection.y = 0;
                Vector3 direction = (directionToTarget + avoidanceDirection).normalized;
                if (direction != Vector3.zero && direction.sqrMagnitude > 0.0f)
                {
                    targetRotation = Quaternion.LookRotation(direction);
                }

                return;
            }

            // Calculate avoidance rotation
            if (distanceToObstacle > 0)
            {
                if (distanceToObstacle < 1f)
                {
                    targetRotation = transform.rotation * Quaternion.Euler(0, turnAngle * 1.5f, 0);
                }
                else
                {
                    targetRotation = transform.rotation * Quaternion.Euler(0, turnAngle, 0);
                }
#if UNITY_EDITOR
                if (isDebug)
                {
                    if (turnAngle != 0)
                    {
                        Debug.Log($"Rotating {turnAngle * (distanceToObstacle / obstacleAvoidanceDistance)} degrees on the Y axis to avoid an obstacle.");
                    }
                }
#endif
            }
        }

        private void AdjustHeight(Vector3 destination, float speedMultiplier)
        {
            float distanceToObstacle = 0;
            float testingAngle = 12;
            float verticalAngle = 0;

            // check for obstacle to the above/in front
            Ray ray = new Ray(enemyController.sensor.position, Quaternion.AngleAxis(-testingAngle, transform.right) * transform.forward);
            if (Physics.Raycast(ray, out RaycastHit forwardUpHit, obstacleAvoidanceDistance, enemyController.sensorMask))
            {
                if (forwardUpHit.collider.transform.root != enemyController.Target)
                {
                    distanceToObstacle = forwardUpHit.distance;
                    verticalAngle -= 45;

#if UNITY_EDITOR
                    if (isDebug)
                    {
                        if (distanceToObstacle > 0)
                        {
                            Debug.DrawRay(enemyController.sensor.position, ray.direction * obstacleAvoidanceDistance, Color.red, 2);
                        }
                    }
#endif
                }
#if UNITY_EDITOR
                else
                {
                    Debug.DrawRay(enemyController.sensor.position, ray.direction * obstacleAvoidanceDistance, Color.green, 2);
                }
#endif
            }

            // check for obstacle to the below/in front
            ray.direction = Quaternion.AngleAxis(testingAngle, transform.right) * transform.forward;
            if (Physics.Raycast(ray, out RaycastHit forwardDownHit, obstacleAvoidanceDistance, enemyController.sensorMask))
            {
                // TODO: Don't hard code the ground tag
                if (forwardDownHit.collider.transform.root != enemyController.Target && !forwardDownHit.collider.CompareTag("Ground"))
                {
                    distanceToObstacle = forwardDownHit.distance;
                    verticalAngle += 45;

#if UNITY_EDITOR
                    if (isDebug && distanceToObstacle > 0)
                    {
                        Debug.DrawRay(enemyController.sensor.position, ray.direction * obstacleAvoidanceDistance, Color.red, 2);
                    }
#endif
                }
#if UNITY_EDITOR
                else
                {
                    Debug.DrawRay(enemyController.sensor.position, ray.direction * obstacleAvoidanceDistance, Color.green, 2);
                }
#endif
            }

            // check for obstacle below
            ray = new Ray(enemyController.sensor.position, -transform.up);
            if (Physics.Raycast(ray, out RaycastHit downHit, minimumHeight + enemyController.sensor.transform.localPosition.y, enemyController.sensorMask))
            {
                if (downHit.collider.transform.root != enemyController.Target)
                {
                    distanceToObstacle = downHit.distance;
                    verticalAngle += 60;

#if UNITY_EDITOR
                    if (isDebug)
                    {
                        if (distanceToObstacle > 0)
                        {
                            Debug.DrawRay(enemyController.sensor.position, ray.direction * obstacleAvoidanceDistance, Color.red, 2);
                        }
                    }
#endif
                }
#if UNITY_EDITOR
                else
                {
                    Debug.DrawRay(enemyController.sensor.position, ray.direction * obstacleAvoidanceDistance, Color.green, 2);
                }
#endif
            }

            verticalAngle = Mathf.Clamp(verticalAngle, -90, 90);
            float rate = currentDesiredSpeed * Time.deltaTime * (Mathf.Abs(verticalAngle) / 90);

            if (distanceToObstacle > 0)
            {
                // Try to get over or under the obstacle
                if (verticalAngle == 0)
                {
                    transform.position += Vector3.up * rate;
#if UNITY_EDITOR
                    if (isDebug)
                    {
                        Debug.Log("Obstacles above and below in front, moving vertically up in an atttempt to get over it.");
                    }
#endif
                }
                else if (verticalAngle < 0 && transform.position.y > minimumHeight)
                {
                    transform.position -= Vector3.up * rate;
#if UNITY_EDITOR
                    if (isDebug)
                    {
                        Debug.Log($"Moving down to avoid an obstacle.");
                    }
#endif
                }
                else if (verticalAngle > 0 && transform.position.y < maximumHeight)
                {
                    transform.position += Vector3.up * rate;
#if UNITY_EDITOR
                    if (isDebug)
                    {
                        Debug.Log($"Moving up to avoid an obstacle.");
                    }
#endif
                }
            }
            else
            {
                // No obstacle so adjust height to match destination
                float heightDifference = transform.position.y - destination.y;
                if (heightDifference > 0.2f || heightDifference < -0.2f)
                {
                    rate = currentDesiredSpeed * Time.deltaTime * 0.5f;
                    if (destination.y > transform.position.y)
                    {
                        transform.position += Vector3.up * rate;
                    }
                    else
                    {
                        transform.position -= Vector3.up * rate;
                    }
                }
            }
        }
    }
}