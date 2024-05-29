using NaughtyAttributes;
using System.Diagnostics;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;
using Debug = UnityEngine.Debug;

namespace RogueWave
{
    /// <summary>
    /// Basic movement controller for enemies. It handles the most basic movement of the enemy,which is essentially to move towards a position relative to the target, and to rotate towards the target.
    /// Basic Movement Controller is paird with a Basic Enemy Controller, which handles the central coordination of the enemy controllers.
    /// 
    /// Subclasses of this class will implement more complex movement patterns, such as formations, patrolling, or following a path.
    /// </summary>
    public class BasicMovementController : MonoBehaviour
    {
        [Header("Movement")]
        [SerializeField, Tooltip("The minimum speed at which the enemy moves.")]
        internal float minSpeed = 4f;
        [SerializeField, Tooltip("The maximum speed at which the enemy moves.")]
        internal float maxSpeed = 6f;
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

        private AIDirector aiDirector;

        private float sqrArrivalDistance;
        Vector3 wanderDestination = Vector3.zero;
        Vector3 _destination = Vector3.zero;

        internal float currentSpeed;

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
                    value.y = Mathf.Max(value.y, minimumHeight);
                    _destination = value;
                }
            }
        }

        internal bool hasArrived
        {
            get
            {
                float distanceToGoal = Vector3.SqrMagnitude(destination - transform.position);
                if (distanceToGoal < sqrArrivalDistance)
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

            enemyController = GetComponent<BasicEnemyController>();
        }

        private void Start()
        {
            currentSpeed = Random.Range(minSpeed, maxSpeed);
            aiDirector = FindAnyObjectByType<AIDirector>();
        }

        internal void MoveUpdateMovement(Vector3 destination, float speedMultiplier, BasicEnemyController squadLeader)
        {
            this.destination = destination;

            // if the distance to the goalDestination is < arrive distance then slow down, eventually stopping
            float distanceToGoal = Vector3.SqrMagnitude(destination - transform.position);
            if (distanceToGoal < sqrArrivalDistance)
            {
                currentSpeed = Mathf.Max(0, currentSpeed - Time.deltaTime * acceleration);
            } else if (currentSpeed < maxSpeed)
            {
                currentSpeed = currentSpeed + Time.deltaTime * acceleration;
            }

            if (currentSpeed == 0)
            {
                return;
            }

            MoveTowards(destination, speedMultiplier, squadLeader);
        }

        internal virtual void MoveTowards(Vector3 destination, float speedMultiplier, BasicEnemyController squadLeader)
        {
            Vector3 centerDirection = destination - transform.position;
            Vector3 avoidanceDirection = Vector3.zero;
            BasicEnemyController[] squadMembers = aiDirector.GetSquadMembers(squadLeader);
            int squadSize = 0;

            if (squadLeader == this)
            {
                Vector3 directionToTarget = (destination - transform.position).normalized;
                float dotProduct = Vector3.Dot(transform.forward, directionToTarget);

                if (dotProduct > 0)
                {
                    transform.rotation = Quaternion.Slerp(transform.rotation, ObstacleAvoidanceRotation(destination, avoidanceDirection), rotationSpeed * Time.deltaTime);
                }
                else
                {
                    transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(directionToTarget), rotationSpeed * Time.deltaTime);
                }
            }
            else
            {
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
                    // TODO: centreDirection is never used!
                    centerDirection /= squadSize;
                    centerDirection = (centerDirection - transform.position).normalized;
                }

                destination.y = Mathf.Clamp(destination.y, minimumHeight, maximumHeight);

                Vector3 directionToTarget = (destination - transform.position).normalized;
                float dotProduct = Vector3.Dot(transform.forward, directionToTarget);

                if (dotProduct > 0)
                {
                    transform.rotation = Quaternion.Slerp(transform.rotation, ObstacleAvoidanceRotation(destination, avoidanceDirection), rotationSpeed * Time.deltaTime);
                }
                else
                {
                    transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(directionToTarget), rotationSpeed * Time.deltaTime);
                }
            }

            transform.position += transform.forward * currentSpeed * speedMultiplier * Time.deltaTime;

            AdjustHeight(destination, speedMultiplier);
        }

        /// <summary>
        /// Get a recommended rotation for the enemy to take in order to avoid the nearest obstacle.
        /// </summary>
        /// <param name="destination">The destination we are trying to reach.</param>
        /// <param name="avoidanceDirection">The optimal direction we are trying to avoid other flock members.</param>
        /// <returns></returns>
        private Quaternion ObstacleAvoidanceRotation(Vector3 destination, Vector3 avoidanceDirection)
        {
            float distanceToObstacle = 0;
            float turnAngle = 0;
            float turnRate = obstacleAvoidanceDistance * 10;

            // Check for obstacle dead ahead
            Ray ray = new Ray(enemyController.sensor.position, transform.forward);
            if (Physics.Raycast(ray, out RaycastHit forwardHit, obstacleAvoidanceDistance, enemyController.sensorMask))
            {
                if (forwardHit.collider.transform.root != enemyController.Target)
                {
                    distanceToObstacle = forwardHit.distance;
                    turnAngle = 90;
                }
            }

            // check for obstacle to the left
            if (turnAngle == 0)
            {
                ray.direction = Quaternion.AngleAxis(-turnRate, transform.transform.up) * transform.forward;
                if (Physics.Raycast(ray, out RaycastHit leftForwardHit, obstacleAvoidanceDistance, enemyController.sensorMask))
                {
                    if (leftForwardHit.collider.transform.root != enemyController.Target)
                    {
                        distanceToObstacle = leftForwardHit.distance;
                        turnAngle = 45;
                    }
                }
            }

            // Check for obstacle to the right
            if (turnAngle == 0)
            {
                ray.direction = Quaternion.AngleAxis(turnRate, transform.transform.up) * transform.forward;
                if (Physics.Raycast(ray, out RaycastHit rightForwardHit, obstacleAvoidanceDistance, enemyController.sensorMask))
                {
                    if (rightForwardHit.collider.transform.root != enemyController.Target)
                    {
                        distanceToObstacle = rightForwardHit.distance;
                        turnAngle = -45;
                    }
                }
            }

#if UNITY_EDITOR
            if (isDebug)
            {
                if (distanceToObstacle > 0)
                {
                    Debug.DrawRay(enemyController.sensor.position, ray.direction * obstacleAvoidanceDistance, Color.red, 2);
                }
            }
#endif

            // Calculate avoidance rotation
            Quaternion targetRotation = Quaternion.identity;
            if (distanceToObstacle > 0) // turn to avoid obstacle
            {
                targetRotation = transform.rotation * Quaternion.Euler(0, turnAngle * (distanceToObstacle / obstacleAvoidanceDistance), 0);
                if (distanceToObstacle < 1f)
                {
                    currentSpeed = 0.1f;
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
            else // no obstacle so rotate towards target
            {
                Vector3 directionToTarget = destination - transform.position;
                directionToTarget.y = 0;
                avoidanceDirection.y = 0;
                Vector3 direction = (directionToTarget + avoidanceDirection).normalized;
                if (direction != Vector3.zero)
                {
                    targetRotation = Quaternion.LookRotation(direction);
                }
            }

            return targetRotation;
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
            float rate = maxSpeed * speedMultiplier * Time.deltaTime * (Mathf.Abs(verticalAngle) / 90);

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
                    if (destination.y > transform.position.y)
                    {
                        transform.position += Vector3.up * currentSpeed * Time.deltaTime;
                    }
                    else
                    {
                        transform.position -= Vector3.up * currentSpeed * Time.deltaTime;
                    }
                }
            }
        }
    }
}